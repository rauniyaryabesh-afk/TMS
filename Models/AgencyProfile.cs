using System.ComponentModel.DataAnnotations;

namespace TourismManagementSystem.Models
{
    public class AgencyProfile
    {
        [Key]
        public int Id { get; set; }

        
        public string AgencyUserId { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string AgencyName { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [StringLength(2000)]
        public string ServicesOffered { get; set; } = string.Empty;

        [StringLength(2000)]
        public string TourGuideInfo { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(120)]
        public string? ContactEmail { get; set; }

        [StringLength(30)]
        public string? ContactPhone { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        // Optional image
        [StringLength(500)]
        public string? ImageUrl { get; set; }
    }
}
