using BugBuddy.Data;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugBuddy.Models
{
    public class Bug
    {
        [Key] // ✅ Ensure this is present
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string Priority { get; set; } // Low, Medium, High

        [Required]
        public string Status { get; set; } // Open, In Progress, Resolved

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [ForeignKey("User")]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();

    }
}
