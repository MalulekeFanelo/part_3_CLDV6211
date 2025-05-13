using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace EventEase1.Models

{
    public class Booking
    {
        
              
        public int BookingId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        public int VenueId { get; set; }

        [Required]
        public DateTime BookingDate { get; set; }

        // Navigation properties
        public Event? Event  { get; set; } = null!;
        public Venue? Venue  { get; set; }= null!;
    }
}