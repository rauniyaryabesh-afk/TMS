using System.ComponentModel.DataAnnotations;

namespace TourismManagementSystem.Models
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Required, DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; } = "";

        [Required]
        public string Role { get; set; } = "Tourist"; // Tourist or Agency
    }
}
