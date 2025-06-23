using EventEase1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase1.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
    string searchString,
    int? eventTypeId,
    DateTime? startDate,
    DateTime? endDate,
    bool? venueAvailable)
        {
            var bookings = _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e.EventType) // Include EventType
                .Include(b => b.Venue)
                .AsQueryable();

            // Existing search
            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b =>
                    b.Event.EventName.Contains(searchString) ||
                    b.BookingId.ToString().Contains(searchString));
            }

            // New filters
            if (eventTypeId.HasValue)
            {
                bookings = bookings.Where(b => b.Event.EventTypeID == eventTypeId);
            }

            if (startDate != null)
            {
                bookings = bookings.Where(b => b.Event.EventDate >= startDate);
            }

            if (endDate != null)
            {
                bookings = bookings.Where(b => b.Event.EventDate <= endDate);
            }

            if (venueAvailable != null)
            {
                bookings = bookings.Where(b => b.Venue.Availability == venueAvailable);
            }

            // Get event types for dropdown
            ViewBag.EventTypes = new SelectList(_context.EventTypes, "EventTypeID", "Name");

            return View(await bookings.ToListAsync());
        }


        public IActionResult Create()
        {
            // Defensive filtering to prevent null values in dropdowns

            var events = _context.Events
    .Where(e => e.EventName != null && e.EventId != null && e.EventDate != null)
    .Select(e => new { e.EventId, e.EventName }) // Only get what's needed
    .ToList();


            var venues = _context.Venues
                .Where(v => v.VenueName != null)
                .ToList();

            ViewBag.EventId = new SelectList(events, "EventId", "EventName");
            ViewBag.VenueId = new SelectList(venues, "VenueId", "VenueName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            // Null check for EventId and VenueId
            if (booking.EventId == null || booking.VenueId == null)
            {
                ModelState.AddModelError("", "Event and Venue are required.");
                return ReloadFormData(booking); // Make sure ReloadFormData has the correct data
            }

            var selectedEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == booking.EventId);

            if (selectedEvent == null)
            {
                ModelState.AddModelError("", "Selected event not found.");
                return ReloadFormData(booking);
            }

            // Check for booking conflict
            var conflict = await _context.Bookings
                .Include(b => b.Event)
                .AnyAsync(b => b.VenueId == booking.VenueId &&
                               b.Event.EventDate.Date == selectedEvent.EventDate.Date);

            if (conflict)
            {
                ModelState.AddModelError("", "This venue is already booked for that event date.");
                return ReloadFormData(booking);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ModelState.AddModelError("", "Failed to save booking.");
                }
            }

            return ReloadFormData(booking);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            var eventsList = _context.Events.Where(e => e.EventName != null).ToList();
            var venuesList = _context.Venues.Where(v => v.VenueName != null).ToList();

            ViewBag.EventId = new SelectList(eventsList, "EventId", "EventName", booking.EventId ?? 0);
            ViewBag.VenueId = new SelectList(venuesList, "VenueId", "VenueName", booking.VenueId ?? 0);

            return View(booking);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    ModelState.AddModelError("", "Error updating booking.");
                }
            }

            ViewBag.EventId = new SelectList(
                _context.Events.Where(e => e.EventName != null).ToList(),
                "EventId", "EventName", booking.EventId);

            ViewBag.VenueId = new SelectList(
                _context.Venues.Where(v => v.VenueName != null).ToList(),
                "VenueId", "VenueName", booking.VenueId);

            return View(booking);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null) return NotFound();

            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private IActionResult ReloadFormData(Booking booking)
        {
            var events = _context.Events
                .Where(e => e.EventName != null && e.EventId != null && e.EventDate != null)
                .Select(e => new { e.EventId, e.EventName })
                .ToList();

            var venues = _context.Venues
                .Where(v => v.VenueName != null && v.VenueId != null)
                .Select(v => new { v.VenueId, v.VenueName })
                .ToList();

            ViewBag.EventId = new SelectList(events, "EventId", "EventName", booking.EventId);
            ViewBag.VenueId = new SelectList(venues, "VenueId", "VenueName", booking.VenueId);

            return View(booking);
        }

    }
}
