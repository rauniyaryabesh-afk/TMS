using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TourismManagementSystem.Models;
using TourismManagementSystem.Models.ViewModels;

namespace TourismManagementSystem.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        // GET: /Reports
        public async Task<IActionResult> Index()
        {
            IQueryable<Booking> bookingsQuery = _context.Bookings
                .Include(b => b.Tour);

            string role;

            if (User.IsInRole("Agency"))
            {
                role = "Agency";
                bookingsQuery = bookingsQuery.Where(b => b.Tour != null && b.Tour.AgencyUserId == CurrentUserId);
            }
            else if (User.IsInRole("Tourist"))
            {
                role = "Tourist";
                bookingsQuery = bookingsQuery.Where(b => b.TouristId == CurrentUserId);
            }
            else
            {
                return Forbid();
            }

            var bookings = await bookingsQuery.ToListAsync();

            // Summary counts
            int total = bookings.Count;
            int paid = bookings.Count(b => b.PaymentStatus == PaymentStatus.Paid);
            int unpaid = bookings.Count(b => b.PaymentStatus == PaymentStatus.Unpaid);
            int cancelled = bookings.Count(b => b.Status == BookingStatus.Cancelled);
            int confirmed = bookings.Count(b => b.Status == BookingStatus.Confirmed);
            int pending = bookings.Count(b => b.Status == BookingStatus.Pending);
            int completed = bookings.Count(b => b.Status == BookingStatus.Completed);

            // Revenue = sum of paid bookings * tour price * participants
            decimal revenue = bookings
                .Where(b => b.PaymentStatus == PaymentStatus.Paid && b.Tour != null)
                .Sum(b => (b.Tour!.Price * b.ParticipantsCount));

            // Top tours (by booking count)
            var topTours = bookings
                .Where(b => b.Tour != null)
                .GroupBy(b => new { b.TourId, Name = b.Tour!.Name })
                .Select(g => new TopTourRow
                {
                    TourId = g.Key.TourId,
                    TourName = g.Key.Name,
                    BookingsCount = g.Count(),
                    PaidRevenue = g.Where(x => x.PaymentStatus == PaymentStatus.Paid && x.Tour != null)
                                  .Sum(x => x.Tour!.Price * x.ParticipantsCount)
                })
                .OrderByDescending(x => x.BookingsCount)
                .ThenByDescending(x => x.PaidRevenue)
                .Take(5)
                .ToList();

            // Recent bookings
            var recent = bookings
                .OrderByDescending(b => b.CreatedAt)
                .Take(10)
                .Select(b => new RecentBookingRow
                {
                    BookingId = b.BookingId,
                    TourName = b.Tour?.Name ?? "Tour",
                    TourDate = b.TourDate,
                    Participants = b.ParticipantsCount,
                    Status = b.Status.ToString(),
                    PaymentStatus = b.PaymentStatus.ToString(),
                    CreatedAt = b.CreatedAt
                })
                .ToList();

            var vm = new ReportsVM
            {
                Role = role,
                GeneratedAt = DateTime.Now,

                TotalBookings = total,
                PaidBookings = paid,
                UnpaidBookings = unpaid,
                CancelledBookings = cancelled,
                ConfirmedBookings = confirmed,
                PendingBookings = pending,
                CompletedBookings = completed,

                TotalRevenue = revenue,

                TopTours = topTours,
                RecentBookings = recent
            };

            return View(vm);
        }
    }
}
