using Microsoft.EntityFrameworkCore;

namespace EventEase1.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<EventType> EventTypes { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>().ToTable("Event");
            modelBuilder.Entity<Venue>().ToTable("Venue");
            modelBuilder.Entity<Booking>().ToTable("Booking");
            modelBuilder.Entity<EventType>().ToTable("EventType");

        }
    }
}
    