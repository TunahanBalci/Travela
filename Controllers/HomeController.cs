using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelApp.Data;
using TravelApp.Models;
using TravelApp.Models.Entities;
using TravelApp.Models.ViewModels;

namespace TravelApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDBContext _context;

        public HomeController(ILogger<HomeController> logger, AppDBContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = Guid.Parse(userIdClaim);
            var user = await _context.Users
                .Include(u => u.Preferences)
                .Include(u => u.Favorited)
                .FirstOrDefaultAsync(u => u.ID == userId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Fetch and calculate priority for destinations
            var destinations = await _context.Destinations
                .Include(d => d.Reviews)
                .Include(d => d.City)
                .Include(d => d.Attractions)
                .Include(d => d.Activities)
                .ToListAsync();

            var topDestinations = destinations
                .Select(d =>
                {
                    int priority = d.Attractions.Count(a => user.Preferences.Any(p => p.Content == a.Content));
                    if (user.Favorited.Any(f => f.ID == d.ID))
                    {
                        priority += 5;
                    }
                    return new
                    {
                        Destination = d,
                        AverageRating = d.Reviews.Any() ? d.Reviews.Average(r => r.Rating) : 0,
                        Priority = priority
                    };
                })
                .OrderByDescending(d => d.Priority)
                .ThenByDescending(d => d.AverageRating)
                .Take(10)
                .Select(d => d.Destination)
                .ToList();

            // Fetch and calculate priority for accommodations
            var accommodations = await _context.Accommodations
                .Include(a => a.City)
                .Include(a => a.Amenities)
                .ToListAsync();

            var sortedAccommodations = accommodations
                .Select(a =>
                {
                    int priority = a.Amenities.Count(am => user.Preferences.Any(p => p.Content == am.Name));
                    return new
                    {
                        Accommodation = a,
                        Priority = priority
                    };
                })
                .OrderByDescending(a => a.Priority)
                .Select(a => a.Accommodation)
                .ToList();

            // Gather favorited destinations
            var favoritedDestinationIds = user.Favorited.Select(f => f.ID.ToString()).ToList();

            // Statistics for destinations by city
            var destinationStats = await _context.Destinations
                .Include(d => d.City)
                .GroupBy(d => d.City.Name)
                .Select(group => new
                {
                    CityName = group.Key,
                    Count = group.Count()
                })
                .ToListAsync();

            // Top users by number of reviews
            var topUsersByReviews = await _context.Users
                .Select(u => new
                {
                    UserName = u.Name,
                    ReviewCount = u.Review_History.Count
                })
                .OrderByDescending(u => u.ReviewCount)
                .Take(10)
                .ToListAsync();

            // Destination average ratings
            var destinationRatings = await _context.Destinations
                .Include(d => d.Reviews)
                .Select(d => new
                {
                    Name = d.Name,
                    AverageRating = d.Reviews.Any() ? d.Reviews.Average(r => r.Rating) : 0
                })
                .OrderByDescending(d => d.AverageRating)
                .ToListAsync();

            // Average ratings by city
            var cityRatings = await _context.Destinations
                .Include(d => d.City)
                .GroupBy(d => d.City.Name)
                .Select(group => new
                {
                    City = group.Key,
                    AverageRating = group.Any(g => g.Reviews.Any()) ? group.SelectMany(g => g.Reviews).Average(r => r.Rating) : 0
                })
                .OrderByDescending(c => c.AverageRating)
                .ToListAsync();

            // Prepare ViewModel
            var viewModel = new HomeViewModel
            {
                Destinations = topDestinations.Select(d => new DestinationViewModel
                {
                    ID = d.ID.ToString(),
                    Image_Path = string.IsNullOrEmpty(d.Image_Path) ? "/images/fallback_destination.png" : d.Image_Path,
                    Name = d.Name,
                    CityName = d.City.Name,
                    AverageRating = d.Average_Rating ?? 0,
                    Description = d.Description,
                    Location = d.Location,
                }).ToList(),
                Accommodations = sortedAccommodations.Select(a => new AccommodationViewModel
                {
                    ID = a.ID.ToString(),
                    Image_Path = string.IsNullOrEmpty(a.Image_Path) ? "/images/fallback_accommodation.png" : a.Image_Path,
                    Name = a.Name,
                    CityName = a.City.Name,
                    Price = a.Price,
                    AverageRating = a.Average_Rating ?? 0
                }).ToList(),
                DestinationStats = destinationStats,
                TopUsersByReviews = topUsersByReviews,
                DestinationRatings = destinationRatings,
                CityRatings = cityRatings,
                FavoritedDestinationIds = favoritedDestinationIds
            };

            ViewBag.CityRatings = cityRatings;
            ViewBag.DestinationRatings = destinationRatings;
            ViewBag.TopUsersByReviews = topUsersByReviews;
            ViewBag.DestinationStats = destinationStats;
            ViewBag.Cities = _context.Cities.Select(c => new { c.ID, c.Name }).ToList();


            return View(viewModel);
        }





        public IActionResult Details(Guid id)
        {
            var destination = _context.Destinations
                .Include(d => d.Reviews)
                .FirstOrDefault(d => d.ID == id);

            if (destination == null)
            {
                return NotFound();
            }

            return View(destination);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
