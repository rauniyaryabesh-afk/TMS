namespace TourismManagementSystem.Models
{
    public class TourDestination
    {
        public int Id { get; set; }

        public int TourId { get; set; }
        public Tour? Tour { get; set; }

        public int DestinationId { get; set; }
        public Destination? Destination { get; set; }
    }
}
