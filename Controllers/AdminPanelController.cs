using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelApp.Data;
using TravelApp.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            var isAdmin = User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value;
            if (!(isAdmin != null && bool.TryParse(isAdmin, out var isAdminBool) && isAdminBool))
            {
                return Forbid();
            }

            return View();
        }

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
            return View(entityData);
        }

        public async Task<IActionResult> List()
        {
            var isAdmin = User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value;
            if (isAdmin != null && bool.TryParse(isAdmin, out var isAdminBool) && isAdminBool)
            {
                return View();
            }

            return Forbid();
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
                .Include(d => d.Attractions)
                .Include(d => d.Activities)
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
            var amenities = await _context.Amenities
                .Include(a => a.Accommodations)
                .ToListAsync();

            return View(amenities);
        }

        public async Task<IActionResult> ListPreferences()
        {
            var preferences = await _context.Preferences.ToListAsync();
            return View(preferences);
        }

        public async Task<IActionResult> ListCities()
        {
            var cities = await _context.Cities
                .Include(c => c.Destinations)
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
                    .Include(a => a.Reviews)
                    .Include(a => a.Activities)
                    .FirstOrDefaultAsync(a => a.ID == id);

                if (accommodation == null)
                {
                    return NotFound("Accommodation not found.");
                }

                if (accommodation.Reviews.Any())
                {
                    _context.Reviews.RemoveRange(accommodation.Reviews);
                }

                accommodation.Activities.Clear();
                _context.Accommodations.Remove(accommodation);

                await _context.SaveChangesAsync();
                return RedirectToAction("ListAccommodations");
            }
            catch
            {
                return RedirectToAction("ListAccommodations");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteActivity(Guid id)
        {
            try
            {
                var activity = await _context.Activities
                    .Include(a => a.Accommodations)
                    .FirstOrDefaultAsync(a => a.ID == id);

                if (activity == null)
                {
                    return NotFound("Activity not found.");
                }

                var relatedAccommodations = await _context.Set<Dictionary<string, object>>("AccommodationActivity")
                    .Where(ac => EF.Property<Guid>(ac, "ActivitiesID") == id)
                    .ToListAsync();

                _context.RemoveRange(relatedAccommodations);
                _context.Activities.Remove(activity);

                await _context.SaveChangesAsync();
                return RedirectToAction("ListActivities");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAmenity(Guid id)
        {
            try
            {
                var amenity = await _context.Amenities
                    .Include(a => a.Accommodations)
                    .FirstOrDefaultAsync(a => a.ID == id);

                if (amenity == null)
                {
                    return NotFound("Amenity not found.");
                }

                amenity.Accommodations.ToList().ForEach(a => a.Amenities.Remove(amenity));
                _context.Amenities.Remove(amenity);

                await _context.SaveChangesAsync();
                return RedirectToAction("ListAmenities");
            }
            catch
            {
                return StatusCode(500);
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
                    .Include(c => c.Accommodations)
                    .Include(c => c.Activities)
                    .FirstOrDefaultAsync(c => c.ID == id);

                if (city == null)
                {
                    return NotFound("City not found.");
                }

                foreach (var destination in city.Destinations)
                {
                    _context.Reviews.RemoveRange(destination.Reviews);
                    _context.Destinations.Remove(destination);
                }

                foreach (var accommodation in city.Accommodations)
                {
                    _context.Reviews.RemoveRange(accommodation.Reviews);
                    _context.Accommodations.Remove(accommodation);
                }

                _context.Activities.RemoveRange(city.Activities);
                _context.Cities.Remove(city);

                await _context.SaveChangesAsync();
                return RedirectToAction("ListCities");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDestination(Guid id)
        {
            try
            {
                var destination = await _context.Destinations
                    .Include(d => d.Reviews)
                    .FirstOrDefaultAsync(d => d.ID == id);

                if (destination == null)
                {
                    return NotFound("Destination not found.");
                }

                _context.Destinations.Remove(destination);
                await _context.SaveChangesAsync();
                return RedirectToAction("ListDestinations");
            }
            catch
            {
                return StatusCode(500);
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
                    .FirstOrDefaultAsync(u => u.ID == id);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("ListUsers");
            }
            catch
            {
                return StatusCode(500);
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
