using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CodeFirstApi.Models
{
    [Index("Username", IsUnique = true)]
    [Index("Email", IsUnique =true)]
    [Index("Mobile", IsUnique =true)]
    public class User
    {
        public int UserId { get; set; }

        [StringLength(50, MinimumLength = 4)]
        public required string Name { get; set; }

        [StringLength(20, MinimumLength = 8)]
        public required string Username { get; set; }
        
        [Required]
        [StringLength(10, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
        
        [EmailAddress]
        public string? Email { get; set; }
        
        [Phone]
        public required string Mobile { get; set; }

        public bool IsEmployee { get; set; } = false;
    }
}
