namespace BlogCore.DAL.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Post
{
    //  [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
    [Key]
    public int Id { get; set; }
    [Required]
    public string Author { get; set; } = string.Empty;
    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public ICollection<Comment> Comments { get; } = new List<Comment>(); // Collection navigation containing dependents
}
