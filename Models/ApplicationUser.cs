using Microsoft.AspNetCore.Identity;

namespace TourismManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        // One-to-one: Agency user -> AgencyProfile
        public AgencyProfile? AgencyProfile { get; set; }
    }
}
