using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace EventEase1.Models
{
    public class Venue
    {
        
            public int VenueId { get; set; }

        [Required]
        public string VenueName { get; set; }

        [Required]
        public string Location { get; set; }


        public int Capacity { get; set; }
             public string ImageUrl { get; set; } = "https://via.placeholder.com/150";
        [NotMapped]

        public IFormFile? ImageFile { get; set; }


        public List<Event>? Events { get; set; } // Relationship with Event

        public ICollection<Booking>? Bookings { get; set; }
    }

    }

