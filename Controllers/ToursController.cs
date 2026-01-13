using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TourismManagementSystem.Models;

namespace TourismManagementSystem.Controllers
{
    [Authorize]
    public class ToursController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ToursController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        // Agency sees ONLY their tours
        // Tourist sees ALL tours
        public async Task<IActionResult> Index()
        {
            IQueryable<Tour> query = _context.Tours
                .Include(t => t.AvailableDates);

            if (User.IsInRole("Agency"))
            {
                query = query.Where(t => t.AgencyUserId == CurrentUserId);
            }

            var tours = await query.ToListAsync();

            // Load agency profiles for displaying agency info in the list
            var agencyIds = tours.Select(t => t.AgencyUserId).Distinct().ToList();
            var profiles = await _context.AgencyProfiles
                .Where(p => agencyIds.Contains(p.AgencyUserId))
                .ToListAsync();

            ViewBag.AgencyProfiles = profiles.ToDictionary(p => p.AgencyUserId, p => p);

            return View(tours);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var tour = await _context.Tours
                .Include(t => t.AvailableDates)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tour == null) return NotFound();

            // show agency info to tourist
            var profile = await _context.AgencyProfiles
                .FirstOrDefaultAsync(p => p.AgencyUserId == tour.AgencyUserId);

            ViewBag.AgencyProfile = profile;

            // stop agencies from viewing details of other agencies (optional, but matches your requirement)
            if (User.IsInRole("Agency") && tour.AgencyUserId != CurrentUserId)
                return Forbid();

            return View(tour);
        }

        // ONLY Agency can create
        [Authorize(Roles = "Agency")]
        public async Task<IActionResult> Create()
        {
            // require profile first (good for assignment)
            bool hasProfile = await _context.AgencyProfiles.AnyAsync(p => p.AgencyUserId == CurrentUserId);
            if (!hasProfile)
            {
                TempData["Error"] = "Please create your Agency Profile before creating tours.";
                return RedirectToAction("Create", "AgencyProfiles");
            }

            return View();
        }

        // POST: Tours/Create
        [HttpPost]
        [Authorize(Roles = "Agency")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tour tour)
        {
            // require profile first
            bool hasProfile = await _context.AgencyProfiles.AnyAsync(p => p.AgencyUserId == CurrentUserId);
            if (!hasProfile)
            {
                TempData["Error"] = "Please create your Agency Profile before creating tours.";
                return RedirectToAction("Create", "AgencyProfiles");
            }

            if (!ModelState.IsValid) return View(tour);

            tour.AgencyUserId = CurrentUserId; // ✅ owner set correctly

            _context.Tours.Add(tour);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ONLY Agency can edit their own
        [Authorize(Roles = "Agency")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var tour = await _context.Tours.FirstOrDefaultAsync(t => t.Id == id);
            if (tour == null) return NotFound();

            if (tour.AgencyUserId != CurrentUserId) return Forbid();

            return View(tour);
        }

        // POST: Tours/Edit/5
        [HttpPost]
        [Authorize(Roles = "Agency")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tour tour)
        {
            if (id != tour.Id) return NotFound();

            var existing = await _context.Tours.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (existing == null) return NotFound();

            if (existing.AgencyUserId != CurrentUserId) return Forbid();

            if (!ModelState.IsValid) return View(tour);

            // keep owner locked
            tour.AgencyUserId = existing.AgencyUserId;

            _context.Update(tour);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ONLY Agency can delete their own
        [Authorize(Roles = "Agency")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var tour = await _context.Tours.FirstOrDefaultAsync(t => t.Id == id);
            if (tour == null) return NotFound();

            if (tour.AgencyUserId != CurrentUserId) return Forbid();

            return View(tour);
        }

        // POST: Tours/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Agency")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tour = await _context.Tours.FirstOrDefaultAsync(t => t.Id == id);
            if (tour == null) return NotFound();

            if (tour.AgencyUserId != CurrentUserId) return Forbid();

            // Don’t delete if bookings exist
            bool hasBookings = await _context.Bookings.AnyAsync(b => b.TourId == id);
            if (hasBookings)
            {
                TempData["Error"] = "Cannot delete this tour because bookings already exist.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Tours.Remove(tour);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
