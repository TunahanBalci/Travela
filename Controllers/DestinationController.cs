using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelApp.Data;
using TravelApp.Models.Entities;
using TravelApp.Models.ViewModels;

namespace TravelApp.Controllers
{
    public class DestinationController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<DestinationController> _logger;
        public DestinationController(AppDBContext context, ILogger<DestinationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Destination/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var destination = await _context.Destinations
                .Include(d => d.City)
                .Include(d => d.Reviews)
                    .ThenInclude(r => r.User)
                .Include(d => d.Activities)
                .Include(d => d.Attractions)
                .FirstOrDefaultAsync(d => d.ID == id);

            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = Guid.Parse(userIdClaim);
            var user = await _context.Users
                .Include(u => u.Favorited)
                .FirstOrDefaultAsync(u => u.ID == userId);

            var isVisited = _context.Visits.Any(v => v.UserID == userId && v.DestinationID == id);
            var hasReview = _context.Reviews.Any(r => r.UserID == userId && r.DestinationID == id);
            bool isFavorited = user.Favorited.Any(f => f.ID == id);

            if (destination == null)
            {
                return NotFound("Destination not found.");
            }

            ViewBag.Visited = isVisited;
            ViewBag.HasReviewed = hasReview; // Assign the ViewBag.HasReviewed explicitly
            ViewBag.CanLeaveReview = isVisited && !hasReview;
            ViewBag.IsFavorited = isFavorited;


            return View(destination);
        }

        [HttpGet]
        public IActionResult Search(string query)
        {
            var destinations = _context.Destinations
                .Include(query => query.City)
                .Where(d => d.Name.Contains(query) || d.Description.Contains(query))
                .OrderBy(d => d.Name)
                .ToList();

            var accommodations = _context.Accommodations
                .Where(a => a.Name.Contains(query) || a.Location.Contains(query))
                .OrderBy(a => a.Name)
                .ToList();

            var model = new SearchResultsViewModel
            {
                Query = query,
                Destinations = destinations,
                Accommodations = accommodations
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsVisited(Guid DestinationID)
        {

            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = Guid.Parse(userIdClaim);

            var travelHistory = new Visit
            {
                UserID = userId,
                DestinationID = DestinationID,
                Visit_Date = DateTime.Now
            };

            _context.Visits.Add(travelHistory);
            await _context.SaveChangesAsync();

            TempData["FeedbackMessage"] = "Successfully added to your travel history.";
            return RedirectToAction("Details", new { id = DestinationID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(Guid destinationId, int rating, string comment)
        {
            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = Guid.Parse(userIdClaim);

            // Check if user has already reviewed
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.DestinationID == destinationId && r.UserID == userId);

            if (existingReview != null)
            {
                TempData["FeedbackMessage"] = "You have already reviewed this destination.";
                return RedirectToAction("Details", "Destination", new { id = destinationId });
            }

            // Create new review
            var review = new Review
            {
                ID = Guid.NewGuid(),
                DestinationID = destinationId,
                UserID = userId,
                Rating = rating,
                Comment = comment,
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["FeedbackMessage"] = "Review submitted successfully!";
            return RedirectToAction("Details", "Destination", new { id = destinationId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToFavorites(Guid destinationId)
        {
            if (destinationId == Guid.Empty)
            {
                TempData["FeedbackMessage"] = "Invalid destination ID.";
                _logger.LogWarning($"Inıtial DestinationID is wrong: ID = {destinationId}");
                return RedirectToAction("Details", new { id = destinationId });
            }

            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = Guid.Parse(userIdClaim);

            // Debugging: Log the incoming destinationId
            Console.WriteLine($"Adding to favorites: Destination ID = {destinationId}");

            var destination = await _context.Destinations.FirstOrDefaultAsync(d => d.ID == destinationId);

            if (destination == null)
            {
                TempData["FeedbackMessage"] = "Destination not found.";
                _logger.LogWarning($"Destination not found: ID = {destination.ID}");
                return RedirectToAction("Details", new { id = destinationId });
            }

            var user = await _context.Users
                .Include(u => u.Favorited)
                .FirstOrDefaultAsync(u => u.ID == userId);

            if (user == null)
            {
                TempData["FeedbackMessage"] = "User not found.";
                _logger.LogWarning($"User not found: ID = {userId}");
                return RedirectToAction("Details", new { id = destinationId });
            }

            if (!user.Favorited.Any(f => f.ID == destinationId))
            {
                user.Favorited.Add(destination);
                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = "Destination added to favorites!";
            }
            else
            {
                TempData["FeedbackMessage"] = "This destination is already in your favorites.";
            }

            return RedirectToAction("Details", new { id = destinationId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromFavorites(Guid destinationId)
        {
            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = Guid.Parse(userIdClaim);

            var user = await _context.Users
                .Include(u => u.Favorited)
                .FirstOrDefaultAsync(u => u.ID == userId);

            if (user == null)
            {
                TempData["FeedbackMessage"] = "User not found.";
                return RedirectToAction("Details", new { id = destinationId });
            }

            var destination = user.Favorited.FirstOrDefault(f => f.ID == destinationId);
            if (destination != null)
            {
                user.Favorited.Remove(destination);
                await _context.SaveChangesAsync();
                TempData["FeedbackMessage"] = "Destination removed from favorites!";
            }
            else
            {
                TempData["FeedbackMessage"] = "This destination is not in your favorites.";
            }

            return RedirectToAction("Details", new { id = destinationId });
        }



    }
}
