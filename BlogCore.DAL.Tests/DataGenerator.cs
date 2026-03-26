using BlogCore.DAL.Models;
using Bogus;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using static Mono.CompilerServices.SymbolWriter.CodeBlockEntry;

namespace BlogCore.DAL.Tests
{
    public static class DataGenerator
    {
        // Szablon dla modelu Post
        public static Faker<Post> GetPostFaker() => new Faker<Post>()
      //  .RuleFor(p => p.Id, f => f.UniqueIndex) // Zapewnia unikalne ID
        .RuleFor(p => p.Author, f => f.Name.FullName()) // Generuje realistyczne imię i nazwisko
        .RuleFor(p => p.Content, f => f.Lorem.Paragraph()); // Generuje tekst typu Lorem Ipsum

        // Szablon dla modelu Long Post
        public static Faker<Post> GetLongPostFaker() => new Faker<Post>()
        //  .RuleFor(p => p.Id, f => f.UniqueIndex) // Zapewnia unikalne ID
        .RuleFor(p => p.Author, f => f.Name.FullName()) // Generuje realistyczne imię i nazwisko
        .RuleFor(p => p.Content, f => f.Lorem.Paragraphs(5)); // Generuje tekst typu Lorem Ipsum

        // Szablon dla modelu Post with null author
        public static Faker<Post> GetPostWithNullAuthorFaker() => new Faker<Post>()
        //  .RuleFor(p => p.Id, f => f.UniqueIndex) // Zapewnia unikalne ID
       // .RuleFor(p => p.Author, f => f.Name.FullName()) // Generuje realistyczne imię i nazwisko
        .RuleFor(p => p.Content, f => f.Lorem.Paragraph()); // Generuje tekst typu Lorem Ipsum



        // Szablon dla modelu Comment
        public static Faker<Comment> GetCommentFaker(int postId) => new Faker<Comment>()
     //   .RuleFor(c => c.Id, f => f.UniqueIndex)
        .RuleFor(c => c.PostId, _ => postId) // Wiąże komentarz z konkretnym postem
        .RuleFor(c => c.Content, f => f.Lorem.Sentence()); // Generuje treść komentarza




    }
}
