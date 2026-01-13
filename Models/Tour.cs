using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismManagementSystem.Models
{
    public partial class Tour
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Range(1, 10000)]
        public decimal Price { get; set; }

        [Range(1, 500)]
        public int MaxGroupSize { get; set; }

        public int DurationDays { get; set; }

        public string AgencyUserId { get; set; } = string.Empty;

        public ICollection<TourDate> AvailableDates { get; set; } = new List<TourDate>();
        public ICollection<TourDestination> TourDestinations { get; set; } = new List<TourDestination>();

        [NotMapped]
        public int TourId => Id;

        [NotMapped]
        public string TourName
        {
            get => Name;
            set => Name = value;
        }

        [NotMapped]
        public int GroupSizeLimit
        {
            get => MaxGroupSize;
            set => MaxGroupSize = value;
        }

        [NotMapped]
        public IEnumerable<DateTime> Dates
        {
            get
            {
                foreach (var d in AvailableDates)
                    yield return d.Date;
            }
        }
    }
}
