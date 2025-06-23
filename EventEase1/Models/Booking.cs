using System.ComponentModel.DataAnnotations;

namespace EventEase1.Models
{
    public class Booking
    {
        public int BookingId { get; set; }

        [Required]
        public int? EventId { get; set; }

        [Required]
        public int? VenueId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        // Correct navigation properties
        public Event? Event { get; set; }
        public Venue? Venue { get; set; }
    }
}
