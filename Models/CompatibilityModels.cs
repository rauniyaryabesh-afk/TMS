using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TourismManagementSystem.Models
{

    public partial class Tour
    {
        
        [NotMapped]
        public DateTime? AvailableDate
        {
            get => AvailableDates?.FirstOrDefault()?.Date;
            set
            {
                if (value == null) return;

                AvailableDates ??= new List<TourDate>();

                if (!AvailableDates.Any())
                    AvailableDates.Add(new TourDate { Date = value.Value });
                else
                    AvailableDates.First().Date = value.Value;
            }
        }
    }


    public partial class Booking
    {
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

        [NotMapped]
        public Feedback? Feedback => Feedbacks.FirstOrDefault();
    }

    public partial class Feedback
    {
        public string? AgencyReply { get; set; }
        public DateTime? ReplyAt { get; set; }
    }
}
