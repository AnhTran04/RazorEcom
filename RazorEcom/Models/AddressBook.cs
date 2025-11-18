using System.ComponentModel.DataAnnotations;

namespace RazorEcom.Models
{
    public class AddressBook
    {
        public int Id { get; set; }

        [Required]
        public required string UserId { get; set; } // Khóa ngoại tới ApplicationUser
        public ApplicationUser User { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string AddressLine { get; set; } = string.Empty;

        [Required]
        public string Ward { get; set; } = string.Empty;

        [Required]
        public string District { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}