using System;
using System.Collections.Generic;

namespace TourismManagementSystem.Models.ViewModels
{
    public class ReportsVM
    {
        public string Role { get; set; } = "";
        public DateTime GeneratedAt { get; set; } = DateTime.Now;

        // Summary
        public int TotalBookings { get; set; }
        public int PaidBookings { get; set; }
        public int UnpaidBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int PendingBookings { get; set; }
        public int CompletedBookings { get; set; }

        public decimal TotalRevenue { get; set; } // Paid only

        // Charts/Tables
        public List<TopTourRow> TopTours { get; set; } = new();
        public List<RecentBookingRow> RecentBookings { get; set; } = new();
    }

    public class TopTourRow
    {
        public int TourId { get; set; }
        public string TourName { get; set; } = "";
        public int BookingsCount { get; set; }
        public decimal PaidRevenue { get; set; }
    }

    public class RecentBookingRow
    {
        public int BookingId { get; set; }
        public string TourName { get; set; } = "";
        public DateTime? TourDate { get; set; }
        public int Participants { get; set; }
        public string Status { get; set; } = "";
        public string PaymentStatus { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
