using EventEase1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;

namespace EventEase1.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var bookings = from b in _context.Bookings
                           .Include(b => b.Event)
                           .Include(b => b.Venue)
                           select b;

            if (!String.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b => b.BookingId.ToString().Contains(searchString) ||
                                                b.Event.EventName.Contains(searchString));
            }

            return View(await bookings.ToListAsync());
        }


        public IActionResult Create()
        {
            ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName");
            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking booking)
        {
            var selectedEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == booking.EventId);

            if (selectedEvent == null)
            {
                ModelState.AddModelError("", "Selected event not found.");
                ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }

            var conflict = await _context.Bookings
                .Include(b => b.Event)
                .AnyAsync(b => b.VenueId == booking.VenueId &&
                               b.Event.EventDate.Date == selectedEvent.EventDate.Date);

            if (conflict)
            {
                ModelState.AddModelError("", "This venue is already booked for that date.");
                ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
                return View(booking);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "This venue is already booked for that date.");
                }
            }

            ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();

            ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);

            return View(booking);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Booking booking)
        {
            if (id != booking.BookingId) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.EventId = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            ViewBag.VenueId = new SelectList(_context.Venues, "VenueId", "VenueName", booking.VenueId);
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
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
