using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismManagementSystem.Models
{
    public partial class Booking
    {
        [Key]
        public int BookingId { get; set; }

        // Link Booking -> Tour
        [Required]
        public int TourId { get; set; }

        [ForeignKey(nameof(TourId))]
        public Tour? Tour { get; set; }

        // Tourist (Identity user id)
        public string TouristId { get; set; } = string.Empty;

        [NotMapped]
        public string TouristUserId
        {
            get => TouristId;
            set => TouristId = value;
        }

        public string TouristName { get; set; } = string.Empty;
        public string TouristEmail { get; set; } = string.Empty;

        [Range(1, 9999)]
        public int ParticipantsCount { get; set; } = 1;

        public DateTime? TourDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    }

    public enum BookingStatus
    {
        Pending = 0,
        Confirmed = 1,
        Cancelled = 2,
        Completed = 3
    }

    public enum PaymentStatus
    {
        Unpaid = 0,
        Paid = 1,
        Refunded = 2
    }
}
