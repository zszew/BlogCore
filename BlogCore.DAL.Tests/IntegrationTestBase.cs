using BlogCore.DAL.Data;
using BlogCore.DAL.Models;
using BlogCore.DAL.Repositories;
using Bogus;
using Bogus.DataSets;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Respawn.Graph;
using System;
using System.Collections.Generic;
using System.Text;
using Testcontainers.MsSql;

namespace BlogCore.DAL.Tests
{
    [TestClass]
    [DoNotParallelize]
    public sealed class IntegrationTestBase
    {
        protected static readonly MsSqlContainer _dbContainer = new
        MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .WithPassword("StrongPassword123!")
        .Build();
        protected BlogContext _context = null!;
        protected BlogRepository _repository = null!;
        private Respawner _respawner = null!;

        [AssemblyInitialize]
        public static async Task AssemblyInit(TestContext context)
        {
            // Uruchomienie kontenera raz dla wszystkich testów w projekcie
            await _dbContainer.StartAsync();
        }

        [TestInitialize]
        public async Task Setup()
        {
            var connectionString = _dbContainer.GetConnectionString();
            // 1. Konfiguracja EF Core
            var options = new DbContextOptionsBuilder<BlogContext>()
            .UseSqlServer(connectionString)
            .Options;
            _context = new BlogContext(options);
            await _context.Database.EnsureCreatedAsync(); // Tworzy schemat
            _repository = new BlogRepository(_context);
            // 2. Inicjalizacja Respawn przy użyciu AKTYWNEGO połączenia
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
             //   var query = "SET IDENTITY_INSERT Posts ON); SET IDENTITY_INSERT Comments ON;";

              //  await _context.Database.ExecuteSqlRawAsync(query, CancellationToken.None);
                

                _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
                {
                    TablesToIgnore = new Table[]
                    {
                new Table("__EFMigrationsHistory")
                    }
                });
            }
            // 3. Pierwszy reset bazy
            await ResetDatabaseAsync();
        }

        // Metoda resetująca - czyści tabele bazy danych
        async Task ResetDatabaseAsync()
        {
            if (_respawner != null)
            {
                var connectionString = _dbContainer.GetConnectionString();
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                  //  var query = "SET IDENTITY_INSERT Posts ON); SET IDENTITY_INSERT Comments ON;";

                  //  await _context.Database.ExecuteSqlRawAsync(query, CancellationToken.None);
                    // Resetowanie danych przy użyciu obiektu połączenia
                    await _respawner.ResetAsync(connection);
                }
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Zwalnianie zasobów po każdym teście
            _context.Dispose();
        }
        [AssemblyCleanup]
        public static async Task AssemblyCleanup()
        {
            // Zatrzymanie kontenera po zakończeniu wszystkich testów
            await _dbContainer.StopAsync();
        }

        
    //    [TestMethod]
        public async Task GetAllPosts_WhenDatabaseHasData_ReturnsAllRecords()
        {

            // cleanup
          //  await ResetDatabaseAsync(); // Pobranie danych przezrepozytorium
         //   await _context.SaveChangesAsync(); // Zapis do kontenera Docker

            var result1 = _repository.GetAllPosts(); // Pobranie danych przezrepozytorium

            Assert.AreEqual(0, result1.Count());

            // Arrange: Generujemy i zapisujemy 5 losowych postów
            var fakePosts = DataGenerator.GetPostFaker().Generate(5);
            await _context.Posts.AddRangeAsync(fakePosts);
            await _context.SaveChangesAsync(); // Zapis do kontenera Docker

            // Act
            var result = _repository.GetAllPosts(); // Pobranie danych przezrepozytorium
            // Assert
            Assert.AreEqual(5, result.Count());

            
        }
        
        
        // Functional tests

        [TestMethod]
        public async Task AddOnePost_WhenDatabaseHasData_PostCountIncrements()
        {

            // Arrange: Generujemy i zapisujemy 5 losowych postów
            var fakePosts = DataGenerator.GetPostFaker().Generate(5);
            var postToAdd = fakePosts[0];
            _context.Posts.AddRange(fakePosts.GetRange(1,4));
            await _context.SaveChangesAsync(); // Zapis do kontenera Docker

            // Act
            _repository.AddPost(postToAdd);


            // Assert
            Assert.AreEqual(5, _context.Posts.Count());
        }

         [TestMethod]
       // [ExpectedException(typeof(Microsoft.EntityFrameworkCore.DbUpdateException))]
        public async Task AddPost_NullContent_ThrowsDbUpdateException()
        {

            // Arrange: Tworzymy post bez wymaganej treści (Content)
            var invalidPost = new Post
            {
                Author = "Jan Kowalski",
                Content = null! // Celowe złamanie reguły [Required]
            };
            // Act: Próba dodania rekordu do bazy w kontenerze
           // _repository.AddPost(invalidPost);
            
            // Assert: Atrybut ExpectedException automatycznie zweryfikuje wynik DEPRECATED
            // Act
            Assert.ThrowsExactly<Microsoft.EntityFrameworkCore.DbUpdateException>(() => _repository.AddPost(invalidPost));
        }

        [TestMethod]
        public async Task GetCommentsForOnePostId_WhenDatabaseHasData_CorrectCommentCount()
        {
            // Arrrange
            var fakePost = DataGenerator.GetPostFaker().Generate(1);
            _context.Posts.AddRange(fakePost);
            await _context.SaveChangesAsync(); // Zapis do kontenera Docker
            var postId = _repository.GetAllPosts().First().Id; // Pobranie danych przezrepozytorium


            var fakeComments = DataGenerator.GetCommentFaker(postId).Generate(3);

            
            _context.Comments.AddRange(fakeComments);
            await _context.SaveChangesAsync(); // Zapis do kontenera Docker

            // Act
            var retrievedComments = _repository.GetCommentsByPostId(postId);

            // Assert
            Assert.AreEqual(3, retrievedComments.Count());
        }

        // Post tests
        [TestMethod]
        public async Task GetAllPosts_EmptyDb_ReturnsZero()
        {
            // Arrrange
            await ResetDatabaseAsync();

            // Act
            var result = _repository.GetAllPosts();

            // Assert
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        public async Task AddPost_LongContent_SavesCorrectly()
        {
            // Arrrange
            var fakePost = DataGenerator.GetLongPostFaker().Generate(1).First();
            var data = fakePost.Content;

            // Act
            _repository.AddPost(fakePost);

            // Assert
            Assert.AreEqual(data, _context.Posts.First().Content);
        }

        [TestMethod]
        public async Task AddPost_SpecialCharactersInAuthor_SavesCorrectly()
        {
            // Arrrange
            var fakePost = DataGenerator.GetPostFaker().Generate(1).First();
            var specialString = "Zażółć Gęślą Jaźń 123!";
            fakePost.Author = specialString;

            // Act
            _repository.AddPost(fakePost);

            // Assert
            Assert.AreEqual(specialString, _context.Posts.First().Author);
        }

        // Comments & relations tests
        [TestMethod]
        public async Task AddComment_ValidData_IncreasesCountForPost()
        {
            // Arrrange
            var fakePost = DataGenerator.GetPostFaker().Generate(1);
            _context.Posts.AddRange(fakePost);
            await _context.SaveChangesAsync(); // Zapis do kontenera Docker
            var postId = _repository.GetAllPosts().First().Id; // Pobranie danych przezrepozytorium

            var fakeComments = DataGenerator.GetCommentFaker(postId).Generate(1);


            _context.Comments.AddRange(fakeComments);
            await _context.SaveChangesAsync(); // Zapis do kontenera Docker

            // Act
            var retrievedComments = _repository.GetCommentsByPostId(postId);

            // Assert
            Assert.AreEqual(1, retrievedComments.Count());
        }

        [TestMethod]
        public async Task GetCommentsByPostId_NonExistentPost_ReturnsEmpty()
        {
            // Arrrange

            // Act
            var retrievedComments = _repository.GetCommentsByPostId(123);

            // Assert
            Assert.IsNotNull(retrievedComments);
        }

        [TestMethod]
        public async Task MultipleComments_DifferentPosts_ReturnsOnlyCorrectOnes()
        {
            // Arrrange
            var fakePosts = DataGenerator.GetPostFaker().Generate(2);
            _context.Posts.AddRange(fakePosts);
            await _context.SaveChangesAsync(); // Zapis do kontenera Docker
            var postIds = _repository.GetAllPosts().ToArray().Select(post => post.Id).ToArray(); // Pobranie danych przezrepozytorium

            var fakeComments1 = DataGenerator.GetCommentFaker(postIds[0]).Generate(5);
            var fakeComments2 = DataGenerator.GetCommentFaker(postIds[1]).Generate(2);


            _context.Comments.AddRange(fakeComments1.Concat(fakeComments2));
            await _context.SaveChangesAsync(); // Zapis do kontenera Docker

            // Act
            var retrievedComments = _repository.GetCommentsByPostId(postIds[0]);

            // Assert
            Assert.AreEqual(5, retrievedComments.Count());
        }


        // Validation (negative) tests
        [TestMethod]
        public async Task AddComment_OrphanComment_ThrowsException()
        {
            // Arrrange
            var invalidComment = DataGenerator.GetCommentFaker(123).Generate(1).First();

            // Act & Assert
            Assert.ThrowsExactly<Microsoft.EntityFrameworkCore.DbUpdateException>(() => _repository.AddComment(invalidComment));
        }


        // Validation (negative) tests
        [TestMethod]
        public async Task AddPost_NullAuthor_ThrowsDbUpdateException()
        {
            // Arrrange
            var invalidPost = DataGenerator.GetPostFaker().Generate(1).First();
            invalidPost.Author = null;

            // Act & Assert
            Assert.ThrowsExactly<Microsoft.EntityFrameworkCore.DbUpdateException>(() => _repository.AddPost(invalidPost));
        }

        [TestMethod]
        public async Task AddComment_NullContent_ThrowsDbUpdateException()
        {
            // Arrange
            var fakePost = DataGenerator.GetPostFaker().Generate(1);
            _context.Posts.AddRange(fakePost);
            await _context.SaveChangesAsync(); // Zapis do kontenera Docker
            var postId = _repository.GetAllPosts().First().Id; // Pobranie danych przezrepozytorium


            var invalidComment = DataGenerator.GetCommentFaker(postId).Generate(1).First();
            invalidComment.Content = null;

            // Act & Assert
            Assert.ThrowsExactly<Microsoft.EntityFrameworkCore.DbUpdateException>(() => _repository.AddComment(invalidComment));
        }


        // Integration test


        [TestMethod]
        public async Task DeletePost_CascadeDeleteComments()
        {
            // Arrange
            var fakePost = DataGenerator.GetPostFaker().Generate(1);
            _context.Posts.AddRange(fakePost);
            await _context.SaveChangesAsync(); // Zapis do kontenera Docker
            var postId = _repository.GetAllPosts().First().Id; // Pobranie danych przezrepozytorium

            var fakeComments = DataGenerator.GetCommentFaker(postId).Generate(1);


            _context.Comments.AddRange(fakeComments);
            await _context.SaveChangesAsync(); // Zapis do kontenera Docker

            // Act
            var retrievedComments = _repository.GetCommentsByPostId(postId);
            _repository.DeletePost(postId);

            var retrievedCommentsShouldBeEmpty = _repository.GetCommentsByPostId(postId);


            // Assert
            Assert.AreEqual(1, retrievedComments.Count());
            Assert.AreEqual(0, retrievedCommentsShouldBeEmpty.Count());
        }
    }
}
