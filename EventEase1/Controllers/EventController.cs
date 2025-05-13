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

        public async Task<IActionResult> Index()
        {
            var events = await _context.Events.Include(e => e.Venue).ToListAsync();
            return View(events);
        }

        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName");
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
            return View(@event);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.Include(e => e.Venue).FirstOrDefaultAsync(e => e.EventId == id);
            if (@event == null) return NotFound();

            return View(@event);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();

            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
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
            return View(@event);
        }

        // GET: Event/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventEntity = await _context.Events
                .Include(e => e.Venue) // Needed to show venue name on the view
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (eventEntity == null)
            {
                return NotFound();
            }

            return View(eventEntity);
        }

        // POST: Event/Delete/5
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
