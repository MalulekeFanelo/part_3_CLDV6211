using EventEase1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EventEase1.Controllers
{
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? eventTypeId, DateTime? startDate, DateTime? endDate, bool? venueAvailable)
        {
            var query = _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .AsQueryable();

            if (eventTypeId.HasValue)
            {
                query = query.Where(e => e.EventTypeID == eventTypeId);
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.EventDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(e => e.EventDate <= endDate.Value);
            }

            if (venueAvailable == true)
            {
                query = query.Where(e => e.Venue != null && e.Venue.Availability == true);
            }

            ViewBag.EventTypes = new SelectList(await _context.EventTypes.ToListAsync(), "EventTypeID", "Name");

            var filteredEvents = await query.ToListAsync();

            return View(filteredEvents);
        }

        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName");
            ViewData["EventTypeID"] = new SelectList(_context.EventTypes, "EventTypeID", "Name"); // ✅ only addition
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create(Event @event)
        {
            if (ModelState.IsValid)
            {
                _context.Events.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
            ViewData["EventTypeID"] = new SelectList(_context.EventTypes, "EventTypeID", "Name", @event.EventTypeID); // ✅ MOVED ABOVE return
            return View(@event);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.EventId == id);
            if (@event == null) return NotFound();

            return View(@event);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (@event == null)
            {
                return NotFound();
            }

            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
            ViewData["EventTypeID"] = new SelectList(_context.EventTypes, "EventTypeID", "TypeName", @event.EventTypeID);
            return View(@event);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Event @event)
        {
            if (id != @event.EventId) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
            ViewData["EventTypeID"] = new SelectList(_context.EventTypes, "EventTypeID", "Name", @event.EventTypeID);
            return View(@event);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventEntity = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (eventEntity == null)
            {
                return NotFound();
            }

            return View(eventEntity);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (eventEntity == null)
            {
                return NotFound();
            }

            if (eventEntity.Bookings != null && eventEntity.Bookings.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete this event because it has active bookings.";
                return RedirectToAction(nameof(Index));
            }

            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
