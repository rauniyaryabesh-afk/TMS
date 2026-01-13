using System.ComponentModel.DataAnnotations;

namespace TourismManagementSystem.Models
{
    public class Destination
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Country { get; set; }
    }
}
