using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismManagementSystem.Models
{
    public partial class Feedback
    {
        [Key]
        public int Id { get; set; }

        
        [Required]
        public string TouristId { get; set; } = string.Empty;

        [Required]
        public int BookingId { get; set; }

        [Required]
        public int TourId { get; set; }

        [Required]
        public string AgencyUserId { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        [Required, StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(TourId))]
        public Tour? Tour { get; set; }

        [ForeignKey(nameof(BookingId))]
        public Booking? Booking { get; set; }
    }
}
