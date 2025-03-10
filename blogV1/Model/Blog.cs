using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable
public class Blog
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    [ForeignKey("Author")]
    public string AuthorId { get; set; }
    public User Author;
}