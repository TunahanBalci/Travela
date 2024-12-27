using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelApp.Data;
using TravelApp.Models.Entities;

namespace TravelApp.Controllers
{
    public class AccommodationController : Controller
    {
        private readonly AppDBContext _context;

        public AccommodationController(AppDBContext context)
        {
            _context = context;
        }

        public IActionResult Details(Guid id)
        {
            var accommodation = _context.Accommodations
                .Include(a => a.City)
                .Include(a => a.Amenities)
                .Include(a => a.Reviews)
                    .ThenInclude(r => r.User)
                .Include(a => a.Bookings)
                    .ThenInclude(b => b.User)
                .FirstOrDefault(a => a.ID == id);

            if (accommodation == null)
            {
                return NotFound();
            }

            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = Guid.Parse(userIdClaim);

            var hasReview = _context.Reviews.Any(r => r.UserID == userId && r.AccommodationID == id);
            var activeBooking = _context.Bookings
                .FirstOrDefault(b => b.User.ID == userId && b.Accommodation.ID == id);

            var remainingTime = activeBooking != null
                ? (activeBooking.End_Date - DateTime.Now).TotalDays.ToString("0") + " days"
                : null;

            ViewBag.Visited = activeBooking != null;
            ViewBag.HasReviewed = hasReview;
            ViewBag.RemainingTime = remainingTime;

            ViewBag.AverageRating = accommodation.Reviews.Any()
                ? accommodation.Reviews.Average(r => r.Rating)
                : 0;

            return View(accommodation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitReview(Guid accommodationId, int rating, string comment)
        {
            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = Guid.Parse(userIdClaim);

            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.AccommodationID == accommodationId && r.UserID == userId);

            if (existingReview != null)
            {
                TempData["FeedbackMessage"] = "You have already reviewed this accommodation.";
                return RedirectToAction("Details", "Accommodation", new { id = accommodationId });
            }

            var review = new Review
            {
                ID = Guid.NewGuid(),
                AccommodationID = accommodationId,
                UserID = userId,
                Rating = rating,
                Comment = comment,
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["FeedbackMessage"] = "Review submitted successfully!";
            return RedirectToAction("Details", "Accommodation", new { id = accommodationId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeBooking(Guid accommodationId, DateTime startDate, int duration)
        {
            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = Guid.Parse(userIdClaim);

            var existingBooking = await _context.Bookings
                .FirstOrDefaultAsync(r => r.Accommodation.ID == accommodationId && r.User.ID == userId);

            if (existingBooking != null)
            {
                TempData["FeedbackMessage"] = "You have already booked this accommodation.";
                return RedirectToAction("Details", "Accommodation", new { id = accommodationId });
            }

            var accommodation = await _context.Accommodations.FirstOrDefaultAsync(a => a.ID == accommodationId);

            if (accommodation == null || !accommodation.Availability)
            {
                TempData["FeedbackMessage"] = "This accommodation is not available.";
                return RedirectToAction("Details", "Accommodation", new { id = accommodationId });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.ID == userId);

            var booking = new Booking
            {
                ID = Guid.NewGuid(),
                Accommodation = accommodation,
                User = user,
                Booking_Date = DateTime.Now,
                Start_Date = startDate,
                End_Date = startDate.AddDays(duration - 1)
            };

            accommodation.Availability = false;
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            TempData["FeedbackMessage"] = "Booking made successfully!";
            return RedirectToAction("Details", "Accommodation", new { id = accommodationId });
        }
    }
}
