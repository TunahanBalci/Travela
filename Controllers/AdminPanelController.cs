using Microsoft.AspNetCore.Mvc;
using TravelApp.Data;
using TravelApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using TravelApp.Models.ViewModels;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;

namespace TravelApp.Controllers
{
    [Authorize]
    public class AdminPanelController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<AdminPanelController> _logger;

        public AdminPanelController(AppDBContext context, ILogger<AdminPanelController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Entity/Create
        public IActionResult Create()
        {

            var isAdmin = User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value;
            if (!(isAdmin != null && bool.TryParse(isAdmin, out var isAdminBool) && isAdminBool))
            {
                return Forbid();
            }


            return View();
        }

        // POST: Entity/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string entityType, object entityData)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    switch (entityType)
                    {
                        case "Activity":
                            _context.Activities.Add((Activity)entityData);
                            break;
                        case "Accommodation":
                            _context.Accommodations.Add((Accommodation)entityData);
                            break;
                        case "Destination":
                            _context.Destinations.Add((Destination)entityData);
                            break;
                        case "City":
                            _context.Cities.Add((City)entityData);
                            break;
                        case "User":
                            _context.Users.Add((User)entityData);
                            break;
                        case "Review":
                            _context.Reviews.Add((Review)entityData);
                            break;
                        default:
                            throw new Exception("Unsupported entity type.");
                    }
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            ViewBag.EntityType = entityType;
            if (entityType == "Review")
            {
                ViewBag.EntityTypes = new[] { "Destination", "Accommodation", "Activity" };
            }

            return View(entityData);
        }
        public async Task<IActionResult> List()
        {
            var isAdmin = User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value;
            if (isAdmin != null && bool.TryParse(isAdmin, out var isAdminBool) && isAdminBool)
            {
                return View();
            }

            return Forbid(); // Restrict access if not an admin
        }










        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ListUsers()
        {
            var users = await _context.Users
                .Include(u => u.Review_History)
                .Include(u => u.Visited)
                .Include(u => u.Favorited)
                .Include(u => u.Preferences)
                .Include(u => u.Booking_History)
                .ToListAsync();
            return View(users);
        }
        public async Task<IActionResult> ListReviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Destination)
                .Include(r => r.Accommodation)
                .Include(r => r.Activity)
                .ToListAsync();

            return View(reviews);
        }
        public async Task<IActionResult> ListDestinations()
        {
            var destinations = await _context.Destinations
                .Include(d => d.City)
                .Include(d => d.Attractions)    // Eagerly load Attractions
                .Include(d => d.Activities)     // Eagerly load Activities
                .ToListAsync();

            return View(destinations);
        }

        public async Task<IActionResult> ListActivities()
        {
            var activities = await _context.Activities
                .Include(a => a.Destinations)
                .Include(a => a.Accommodations)
                    .ThenInclude(a => a.City)
                .Include(a => a.Reviews)
                .ToListAsync();
            return View(activities);
        }
        public async Task<IActionResult> ListAmenities()
        {
            try
            {
                var amenities = await _context.Amenities
                    .Include(a => a.Accommodations) // Eagerly load related Accommodations
                    .ToListAsync();

                return View(amenities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Amenities with related Accommodations.");
                // Optionally, redirect to an error page or display an error message
                return View("Error");
            }
        }

        // GET: AdminPanel/ListPreferences
        public async Task<IActionResult> ListPreferences()
        {
            try
            {
                var preferences = await _context.Preferences.ToListAsync();
                return View(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching Preferences.");
                // Optionally, redirect to an error page or display an error message
                return View("Error");
            }
        }
        public async Task<IActionResult> ListCities()
        {
            var cities = await _context.Cities
                .Include(c =>c.Destinations)
                .Include(c => c.Activities)
                .Include(c => c.Accommodations)
                .ToListAsync();
            return View(cities);
        }

        public async Task<IActionResult> ListAccommodations()
        {
            var accommodations = await _context.Accommodations
                .Include(a => a.Activities)
                .Include(a => a.Amenities)
                .Include(a => a.City)
                .ToListAsync();
            return View(accommodations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccommodation(Guid id)
        {
            try
            {
                var accommodation = await _context.Accommodations
                    .Include(a => a.Reviews) // Include related reviews
                    .Include(a => a.Activities) // Include related activities
                    .FirstOrDefaultAsync(a => a.ID == id);

                if (accommodation == null)
                {
                    return NotFound("Accommodation not found.");
                }

                // Delete related reviews
                if (accommodation.Reviews.Any())
                {
                    _context.Reviews.RemoveRange(accommodation.Reviews);
                }

                // Remove associations with activities
                if (accommodation.Activities.Any())
                {
                    accommodation.Activities.Clear(); // Clear the relationships
                }

                // Remove the accommodation
                _context.Accommodations.Remove(accommodation);

                await _context.SaveChangesAsync();

                return RedirectToAction("ListAccommodations", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting accommodation with ID {ID}.", id);
                return RedirectToAction("ListAccommodations", "AdminPanel", new { error = "An error occurred while deleting the accommodation." });
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteActivity(Guid id)
        {
            try
            {
                var activity = await _context.Activities
                    .Include(a => a.Accommodations) // Include related accommodations
                    .FirstOrDefaultAsync(a => a.ID == id);

                if (activity == null)
                {
                    return NotFound("Activity not found.");
                }

                // Manually remove associations
                var relatedAccommodations = await _context.Set<Dictionary<string, object>>("AccommodationActivity")
                    .Where(ac => EF.Property<Guid>(ac, "ActivitiesID") == id)
                    .ToListAsync();

                _context.RemoveRange(relatedAccommodations); // Remove from intermediate table

                _context.Activities.Remove(activity); // Remove the activity itself
                await _context.SaveChangesAsync();

                return RedirectToAction("ListActivities", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting activity with ID {ID}.", id);
                return StatusCode(500, "An error occurred while deleting the activity.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAmenity(Guid id)
        {
            try
            {
                var amenity = await _context.Amenities
                    .Include(a => a.Accommodations) // Include related accommodations
                    .FirstOrDefaultAsync(a => a.ID == id);

                if (amenity == null)
                {
                    return NotFound("Amenity not found.");
                }

                // Remove the association with accommodations
                foreach (var accommodation in amenity.Accommodations)
                {
                    accommodation.Amenities.Remove(amenity);
                }

                _context.Amenities.Remove(amenity); // Remove the amenity itself
                await _context.SaveChangesAsync();

                return RedirectToAction("ListAmenities", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Amenity with ID {ID}.", id);
                return StatusCode(500, "An error occurred while deleting the amenity.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCity(Guid id)
        {
            try
            {
                var city = await _context.Cities
                    .Include(c => c.Destinations)
                        .ThenInclude(d => d.Activities)
                    .Include(c => c.Destinations)
                        .ThenInclude(d => d.Attractions)
                    .Include(c => c.Destinations)
                        .ThenInclude(d => d.Reviews)
                    .Include(c => c.Accommodations)
                        .ThenInclude(a => a.Activities)
                    .Include(c => c.Accommodations)
                        .ThenInclude(a => a.Reviews)
                    .Include(c => c.Activities)
                    .FirstOrDefaultAsync(c => c.ID == id);

                if (city == null)
                {
                    return NotFound("City not found.");
                }

                // Delete reviews associated with destinations
                foreach (var destination in city.Destinations)
                {
                    if (destination.Reviews.Any())
                    {
                        _context.Reviews.RemoveRange(destination.Reviews);
                    }

                    if (destination.Activities.Any())
                    {
                        _context.Activities.RemoveRange(destination.Activities);
                    }

                    if (destination.Attractions.Any())
                    {
                        _context.Preferences.RemoveRange(destination.Attractions);
                    }

                    _context.Destinations.Remove(destination);
                }

                // Delete reviews associated with accommodations
                foreach (var accommodation in city.Accommodations)
                {
                    if (accommodation.Reviews.Any())
                    {
                        _context.Reviews.RemoveRange(accommodation.Reviews);
                    }

                    if (accommodation.Activities.Any())
                    {
                        _context.Activities.RemoveRange(accommodation.Activities);
                    }

                    _context.Accommodations.Remove(accommodation);
                }

                // Delete activities directly associated with the city
                if (city.Activities.Any())
                {
                    _context.Activities.RemoveRange(city.Activities);
                }

                // Finally, delete the city
                _context.Cities.Remove(city);

                await _context.SaveChangesAsync();

                return RedirectToAction("ListCities", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting city with ID {ID}.", id);
                return StatusCode(500, "An error occurred while deleting the city.");
            }
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDestination(Guid id)
        {
            try
            {
                var destination = await _context.Destinations
                    .Include(d => d.Activities)
                    .Include(d => d.Attractions)
                    .Include(d => d.Reviews)
                    .FirstOrDefaultAsync(d => d.ID == id);

                if (destination == null)
                {
                    return NotFound("Destination not found.");
                }

                // Ensure no related reviews or entities block deletion
                if (destination.Reviews.Any())
                {
                    ModelState.AddModelError("", "Destination cannot be deleted because it has related reviews.");
                    return RedirectToAction("ListDestinations", "AdminPanel");
                }

                _context.Destinations.Remove(destination);
                await _context.SaveChangesAsync();

                return RedirectToAction("ListDestinations", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting destination with ID {ID}.", id);
                return StatusCode(500, "An error occurred while deleting the destination.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePreference(Guid id)
        {
            try
            {
                var preference = await _context.Preferences.FindAsync(id);

                if (preference == null)
                {
                    return NotFound("Preference not found.");
                }

                _context.Preferences.Remove(preference);
                await _context.SaveChangesAsync();

                return RedirectToAction("ListPreferences", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Preference with ID {ID}.", id);
                return StatusCode(500, "An error occurred while deleting the preference.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            try
            {
                var review = await _context.Reviews
                    .Include(r => r.Destination)
                    .Include(r => r.Accommodation)
                    .Include(r => r.Activity)
                    .FirstOrDefaultAsync(r => r.ID == id);

                if (review == null)
                {
                    return NotFound("Review not found.");
                }

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                return RedirectToAction("ListReviews", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting review with ID {ID}.", id);
                return StatusCode(500, "An error occurred while deleting the review.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Review_History)
                    .Include(u => u.Visited)
                    .Include(u => u.Favorited)
                    .Include(u => u.Preferences)
                    .Include(u => u.Booking_History)
                    .FirstOrDefaultAsync(u => u.ID == id);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                if (user.Review_History.Any() || user.Visited.Any() || user.Favorited.Any() || user.Booking_History.Any())
                {
                    ModelState.AddModelError("", "Cannot delete user with associated data.");
                    return RedirectToAction("ListUsers", "AdminPanel");
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("ListUsers", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with ID {ID}.", id);
                return StatusCode(500, "An error occurred while deleting the user.");
            }
        }


        public IActionResult Test()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Test(IFormCollection collection)
        {


            try
            {

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

    }
}
