namespace BlogCore.DAL.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Comment
{
    [Key]
   // [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string Content { get; set; } = string.Empty;
    [Required]
    public int PostId { get; set; }

    [Required]
    public required Post Post { get; set; }
}
