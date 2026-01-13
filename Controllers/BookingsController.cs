using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TourismManagementSystem.Models;
using TourismManagementSystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TourismManagementSystem.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        private string CurrentEmail => User.FindFirstValue(ClaimTypes.Email) ?? "";

      
        // INDEX (Tourist: own bookings, Agency: bookings for their tours)
 
        public async Task<IActionResult> Index()
        {
            IQueryable<Booking> query = _context.Bookings.Include(b => b.Tour);

            if (User.IsInRole("Tourist"))
            {
                query = query.Where(b => b.TouristId == CurrentUserId);

                // used in view to hide "Leave Feedback" if already submitted
                var myBookingIds = await _context.Bookings
                    .Where(b => b.TouristId == CurrentUserId)
                    .Select(b => b.BookingId)
                    .ToListAsync();

                var feedbackBookingIds = await _context.Feedbacks
                    .Where(f => f.TouristId == CurrentUserId && myBookingIds.Contains(f.BookingId))
                    .Select(f => f.BookingId)
                    .ToListAsync();

                ViewBag.FeedbackBookingIds = feedbackBookingIds;
            }
            else if (User.IsInRole("Agency"))
            {
                query = query.Where(b => b.Tour != null && b.Tour.AgencyUserId == CurrentUserId);
            }
            else
            {
                return Forbid();
            }

            var list = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return View(list);
        }

     
        // CREATE (Tourist only)  /Bookings/Create?tourId=#
       
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> Create(int tourId)
        {
            var tour = await _context.Tours
                .Include(t => t.AvailableDates)
                .FirstOrDefaultAsync(t => t.Id == tourId);

            if (tour == null) return NotFound();

            var vm = new BookingCreateVM
            {
                TourId = tour.Id,
                TourName = tour.Name,
                ParticipantsCount = 1,
                AvailableDates = (tour.AvailableDates ?? new List<TourDate>())
                    .OrderBy(d => d.Date)
                    .Select(d => new SelectListItem
                    {
                        Value = d.Date.ToString("yyyy-MM-dd"),
                        Text = d.Date.ToString("dd MMM yyyy")
                    })
                    .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize(Roles = "Tourist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingCreateVM vm)
        {
            var tour = await _context.Tours
                .Include(t => t.AvailableDates)
                .FirstOrDefaultAsync(t => t.Id == vm.TourId);

            if (tour == null) return NotFound();

            vm.TourName = tour.Name;
            vm.AvailableDates = (tour.AvailableDates ?? new List<TourDate>())
                .OrderBy(d => d.Date)
                .Select(d => new SelectListItem
                {
                    Value = d.Date.ToString("yyyy-MM-dd"),
                    Text = d.Date.ToString("dd MMM yyyy")
                })
                .ToList();

            if (vm.ParticipantsCount < 1)
                ModelState.AddModelError(nameof(vm.ParticipantsCount), "Participants must be at least 1.");

            if (vm.ParticipantsCount > tour.MaxGroupSize)
                ModelState.AddModelError(nameof(vm.ParticipantsCount),
                    $"Maximum group size for this tour is {tour.MaxGroupSize}.");

            var selectedDate = vm.SelectedDate?.Date;
            bool isAllowed = selectedDate.HasValue &&
                             (tour.AvailableDates ?? new List<TourDate>())
                             .Any(d => d.Date.Date == selectedDate.Value);

            if (!isAllowed)
                ModelState.AddModelError(nameof(vm.SelectedDate), "Selected date is not available.");

            if (!ModelState.IsValid)
                return View(vm);

            var booking = new Booking
            {
                TourId = tour.Id,
                TouristId = CurrentUserId,
                TouristEmail = CurrentEmail,
                TouristName = User.Identity?.Name ?? "Tourist",
                ParticipantsCount = vm.ParticipantsCount,
                TourDate = selectedDate!.Value,
                CreatedAt = DateTime.Now,
                Status = BookingStatus.Pending,
                PaymentStatus = PaymentStatus.Unpaid
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking created successfully!";
            return RedirectToAction(nameof(Index));
        }

        
        // CANCEL (Tourist only)
       
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Tour)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();
            if (booking.TouristId != CurrentUserId) return Forbid();

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
                return RedirectToAction(nameof(Index));

            return View(booking);
        }

        [HttpPost]
        [Authorize(Roles = "Tourist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Tour)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();
            if (booking.TouristId != CurrentUserId) return Forbid();

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
                return RedirectToAction(nameof(Index));

            booking.Status = BookingStatus.Cancelled;

            if (booking.PaymentStatus == PaymentStatus.Paid)
                booking.PaymentStatus = PaymentStatus.Refunded;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking cancelled successfully.";
            return RedirectToAction(nameof(Index));
        }

       
        // PAYMENT (Tourist only)
       
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> Pay(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Tour)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();
            if (booking.TouristId != CurrentUserId) return Forbid();

            if (booking.Status == BookingStatus.Cancelled)
                return RedirectToAction(nameof(Index));

            if (booking.PaymentStatus == PaymentStatus.Paid)
                return RedirectToAction(nameof(Index));

            return View(booking);
        }

        [HttpPost]
        [Authorize(Roles = "Tourist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();
            if (booking.TouristId != CurrentUserId) return Forbid();

            if (booking.Status == BookingStatus.Cancelled)
                return RedirectToAction(nameof(Index));

            booking.PaymentStatus = PaymentStatus.Paid;
            booking.Status = BookingStatus.Confirmed;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment successful!";
            return RedirectToAction(nameof(Index));
        }
    }
}
