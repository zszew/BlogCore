namespace BlogCore.DAL.Repositories;

using BlogCore.DAL.Data;
using BlogCore.DAL.Models;
using Microsoft.EntityFrameworkCore;

public class BlogRepository
{
    private readonly BlogContext _context;

    public BlogRepository(BlogContext context)
    {
        _context = context;
    }

    public void AddPost(Post post)
    {
        _context.Posts.Add(post);
        _context.SaveChanges(); // Kluczowe dla utrwalenia danych w kontenerze
    }
    public IEnumerable<Post> GetAllPosts()
    {
        return _context.Posts.ToList();
    }

    public void AddComment(Comment comment)
    {
        _context.Comments.Add(comment);
        _context.SaveChanges(); // Kluczowe dla utrwalenia danych w kontenerze
    }
    public IEnumerable<Comment> GetCommentsByPostId(int postId)
    {
        var comments = _context.Posts.Where(p => p.Id == postId).ToArray().Select(p => p.Comments);
        return comments.SelectMany(item => item).ToList(); // C#
      //  return _context.Posts.Find(postId).Comments.ToList();
    }

    public void DeletePost(int postId)
    {
        _context.Posts.Where(p => p.Id == postId).ExecuteDelete();
    }
}
