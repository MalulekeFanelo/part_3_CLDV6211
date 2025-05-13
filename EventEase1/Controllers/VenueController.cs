using EventEase1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Identity.Client.Extensions.Msal;
using Azure.Storage.Blobs.Models;

namespace EventEase1.Controllers
{
    public class VenueController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VenueController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var venues = await _context.Venues.ToListAsync();
            return View(venues);
        }

        // GET: Venues/Create
        public IActionResult Create()
        {
            var venue = new Venue(); // Create an empty venue object to pass to the view
            return View(venue);      // Same as in Edit, except you're not retrieving an existing record
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue)
        {
            if (ModelState.IsValid)
            {


                // Handle image upload to Azure Blob Storage if an image file was provided
                // This is Step 4C: Modify Controller to receive ImageFile from View (user upload)
                // This is Step 5: Upload selected image to Azure Blob Storage
                if (venue.ImageFile != null)
                {

                    // Upload image to Blob Storage (Azure)
                    var blobUrl = await UploadImageToBlobAsync(venue.ImageFile); //Main part of Step 5 B (upload image to Azure Blob Storage)

                    // Step 6: Save the Blob URL into ImageUrl property (the database)
                    venue.ImageUrl = blobUrl;
                }

                _context.Add(venue);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Venue created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.FirstOrDefaultAsync(v => v.VenueId == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue)
        {
            if (id != venue.VenueId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Load the original venue from the database
                    var existingVenue = await _context.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.VenueId == id);

                    if (existingVenue == null) return NotFound();

                    if (venue.ImageFile != null)
                    {
                        // Upload new image
                        var blobUrl = await UploadImageToBlobAsync(venue.ImageFile);
                        venue.ImageUrl = blobUrl;
                    }
                    else
                    {
                        // Keep the original image URL
                        venue.ImageUrl = existingVenue.ImageUrl;
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Venue updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error has occurred while updating the venue.");
                }
            }

            return View(venue);
        }



        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var venue = await _context.Venues.FirstOrDefaultAsync(v => v.VenueId == id);
            if (venue == null) return NotFound();

            return View(venue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venues
                .Include(v => v.Bookings)
                .FirstOrDefaultAsync(v => v.VenueId == id);

            if (venue == null)
            {
                return NotFound();
            }

            if (venue.Bookings.Any())
            {
                TempData["ErrorMessage"] = "Cannot delete this venue because it has active bookings.";
                return RedirectToAction(nameof(Index));                
            }

            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // This is Step 5 (C): Upload selected image to Azure Blob Storage.
        // It completes the entire uploading process inside Step 5 â€” from connecting to Azure to returning the Blob URL after upload.
        // This will upload the Image to Blob Storage Account
        // Uploads an image to Azure Blob Storage and returns the Blob URL
        private async Task<string> UploadImageToBlobAsync(IFormFile imageFile)
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=eventease1storage;AccountKey=DL9YevqFzikUkBqGmYf1oQpFTfY5ZuRXtGeYl9YuiPCHoSGd+GvpWa61SRnHlw24Tu0WoX69id1T+AStNxfp0g==;EndpointSuffix=core.windows.net";
            var containerName = "cldv6211project";

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(Guid.NewGuid() + Path.GetExtension(imageFile.FileName));

            var blobHttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders
            {
                ContentType = imageFile.ContentType
            };

            using (var stream = imageFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                });
            }

            return blobClient.Uri.ToString();
        }

    }
}

