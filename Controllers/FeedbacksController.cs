using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TourismManagementSystem.Models;

namespace TourismManagementSystem.Controllers
{
    [Authorize]
    public class FeedbacksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbacksController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

    
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> MyFeedback()
        {
            var items = await _context.Feedbacks
                .Include(f => f.Tour)
                .Where(f => f.TouristId == CurrentUserId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(items);
        }

        
        [Authorize(Roles = "Agency")]
        public async Task<IActionResult> Index()
        {
            var items = await _context.Feedbacks
                .Include(f => f.Tour)
                .Where(f => f.AgencyUserId == CurrentUserId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            return View(items);
        }

        // Tourist: Create feedback from booking
       
        [Authorize(Roles = "Tourist")]
        public async Task<IActionResult> Create(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Tour)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null) return NotFound();
            if (booking.TouristId != CurrentUserId) return Forbid();

            //Only allow feedback if Paid or Completed, and not Cancelled
            if (booking.Status == BookingStatus.Cancelled)
            {
                TempData["Error"] = "You cannot leave feedback for a cancelled booking.";
                return RedirectToAction("Index", "Bookings");
            }

            if (booking.PaymentStatus != PaymentStatus.Paid &&
                booking.Status != BookingStatus.Completed)
            {
                TempData["Error"] = "Please complete payment before leaving feedback.";
                return RedirectToAction("Index", "Bookings");
            }

            //Prevent duplicate feedback
            bool already = await _context.Feedbacks.AnyAsync(f =>
                f.BookingId == bookingId && f.TouristId == CurrentUserId);

            if (already)
            {
                TempData["Error"] = "You already submitted feedback for this booking.";
                return RedirectToAction("Index", "Bookings");
            }

            var fb = new Feedback
            {
                BookingId = booking.BookingId,
                TourId = booking.TourId,
                AgencyUserId = booking.Tour?.AgencyUserId ?? "",
                TouristId = CurrentUserId,
                Rating = 5,
                Comment = ""
            };

            ViewBag.TourName = booking.Tour?.Name ?? "Tour";
            return View(fb);
        }

        [HttpPost]
        [Authorize(Roles = "Tourist")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Feedback feedback)
        {
            var touristId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            //Re-load booking again (never trust hidden fields)
            var booking = await _context.Bookings
                .Include(b => b.Tour)
                .FirstOrDefaultAsync(b => b.BookingId == feedback.BookingId);

            if (booking == null) return NotFound();
            if (booking.TouristId != touristId) return Forbid();

            //lock values
            feedback.TouristId = touristId;
            feedback.TourId = booking.TourId;
            feedback.AgencyUserId = booking.Tour?.AgencyUserId ?? "";
            feedback.CreatedAt = DateTime.Now;

            //basic rule: must be paid or completed
            if (booking.Status == BookingStatus.Cancelled)
                ModelState.AddModelError("", "Cannot leave feedback for a cancelled booking.");

            if (booking.PaymentStatus != PaymentStatus.Paid && booking.Status != BookingStatus.Completed)
                ModelState.AddModelError("", "Please pay (or complete tour) before leaving feedback.");

            //block duplicates
            bool already = await _context.Feedbacks.AnyAsync(f =>
                f.BookingId == booking.BookingId && f.TouristId == touristId);

            if (already)
                ModelState.AddModelError("", "Feedback already submitted for this booking.");

            if (!ModelState.IsValid)
            {
                ViewBag.TourName = booking.Tour?.Name ?? "Tour";
                return View(feedback); // returns same page with errors shown
            }

            _context.Feedbacks.Add(feedback);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Feedback submitted successfully!";
            return RedirectToAction(nameof(MyFeedback));
        }

    }
}
