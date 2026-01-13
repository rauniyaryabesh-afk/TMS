using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TourismManagementSystem.Models
{
    public class TourDate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int TourId { get; set; }

        [ForeignKey(nameof(TourId))]
        public Tour? Tour { get; set; }
    }
}
