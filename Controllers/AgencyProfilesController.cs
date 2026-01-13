using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TourismManagementSystem.Models;

namespace TourismManagementSystem.Controllers
{
    [Authorize(Roles = "Agency")]
    public class AgencyProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AgencyProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public async Task<IActionResult> MyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = await _context.AgencyProfiles
                .FirstOrDefaultAsync(a => a.AgencyUserId == userId);

            if (profile == null)
            {
               
                return RedirectToAction(nameof(Create));
            }

            return View(profile);
        }


        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

          
            bool exists = await _context.AgencyProfiles
                .AnyAsync(a => a.AgencyUserId == userId);

            if (exists)
            {
                return RedirectToAction(nameof(MyProfile));
            }

            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AgencyProfile profile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            
            bool exists = await _context.AgencyProfiles
                .AnyAsync(a => a.AgencyUserId == userId);

            if (exists)
            {
                return RedirectToAction(nameof(MyProfile));
            }

            if (!ModelState.IsValid)
            {
                return View(profile);
            }

           
            profile.AgencyUserId = userId!;

            _context.AgencyProfiles.Add(profile);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Agency profile created successfully.";
            return RedirectToAction(nameof(MyProfile));
        }

       
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = await _context.AgencyProfiles
                .FirstOrDefaultAsync(a => a.AgencyUserId == userId);

            if (profile == null)
            {
                return RedirectToAction(nameof(Create));
            }

            return View(profile);
        }

        
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AgencyProfile profile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existing = await _context.AgencyProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == profile.Id);

            if (existing == null)
            {
                return NotFound();
            }

            if (existing.AgencyUserId != userId)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return View(profile);
            }

      
            profile.AgencyUserId = existing.AgencyUserId;

            _context.Update(profile);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Agency profile updated successfully.";
            return RedirectToAction(nameof(MyProfile));
        }
    }
}
