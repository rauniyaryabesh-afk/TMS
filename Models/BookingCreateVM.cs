using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TourismManagementSystem.Models.ViewModels
{
    public class BookingCreateVM
    {
        [Required]
        public int TourId { get; set; }

        public string TourName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an available date.")]
        [DataType(DataType.Date)]
        public DateTime? SelectedDate { get; set; }

        [Range(1, 9999)]
        public int ParticipantsCount { get; set; } = 1;

        public List<SelectListItem> AvailableDates { get; set; } = new();
    }
}
