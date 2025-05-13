using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase1.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        public string EventName { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        public string Description { get; set; }

        [Required]
        public int? VenueId { get; set; }

        public Venue? Venue { get; set; }
        
        // ✅ Navigation property to Bookings
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
