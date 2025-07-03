using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BugBuddy.Models
{
    public class Note
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key to Bug
        public int BugId { get; set; }

        [ForeignKey("BugId")]
        public virtual Bug Bug { get; set; }
    }
}
