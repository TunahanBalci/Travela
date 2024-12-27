using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelApp.Data;
using TravelApp.Models.Entities;
using TravelApp.Models.ViewModels;

namespace TravelApp.Controllers
{
    public class CreateDestinationController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<CreateDestinationController> _logger;

        public CreateDestinationController(AppDBContext context, ILogger<CreateDestinationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> CreateDestination()
        {
            try
            {
                var model = new CreateDestinationViewModel
                {
                    Cities = await _context.Cities.ToListAsync(),
                    Activities = await _context.Activities.ToListAsync(),
                    Attractions = await _context.Preferences
                        .Where(p => !string.IsNullOrEmpty(p.Content))
                        .ToListAsync() ?? new List<Preference>()
                };

                if (!model.Cities.Any())
                {
                    ModelState.AddModelError("", "No cities available. Please create a city first.");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading the Create Destination page.");
                ModelState.AddModelError("", "An error occurred while loading the page. Please try again later.");
                return View(new CreateDestinationViewModel
                {
                    Cities = new List<City>(),
                    Activities = new List<Activity>(),
                    Attractions = new List<Preference>()
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDestination(CreateDestinationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Cities = await _context.Cities.ToListAsync();
                model.Activities = await _context.Activities.ToListAsync();
                model.Attractions = await _context.Preferences
                    .Where(p => !string.IsNullOrEmpty(p.Content))
                    .ToListAsync() ?? new List<Preference>();
                return View(model);
            }

            try
            {
                var destination = new Destination
                {
                    ID = Guid.NewGuid(),
                    Name = model.Name,
                    Description = model.Description,
                    Location = model.Location,
                    CityID = model.CityID,
                    Image_Path = $"/images/destinations/{model.Name}.png"
                };

                if (model.SelectedPreferencesIds != null && model.SelectedPreferencesIds.Any())
                {
                    var selectedPreferences = await _context.Preferences
                        .Where(p => model.SelectedPreferencesIds.Contains(p.ID))
                        .ToListAsync();
                    destination.Attractions = selectedPreferences;
                }

                if (model.SelectedActivityIds != null && model.SelectedActivityIds.Any())
                {
                    var selectedActivities = await _context.Activities
                        .Where(a => model.SelectedActivityIds.Contains(a.ID))
                        .ToListAsync();
                    destination.Activities = selectedActivities;
                }

                _context.Destinations.Add(destination);
                await _context.SaveChangesAsync();

                return RedirectToAction("ListDestinations", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating the destination.");
                ModelState.AddModelError("", $"Error: {ex.Message}");
                model.Cities = await _context.Cities.ToListAsync();
                model.Activities = await _context.Activities.ToListAsync();
                model.Attractions = await _context.Preferences
                    .Where(p => !string.IsNullOrEmpty(p.Content))
                    .ToListAsync() ?? new List<Preference>();
                return View(model);
            }
        }

        [HttpGet]
        [Route("CreateDestination/EditDestination/{id}")]
        public async Task<IActionResult> EditDestination(Guid id)
        {
            try
            {
                var destination = await _context.Destinations
                    .Include(d => d.Activities)
                    .Include(d => d.Attractions)
                    .FirstOrDefaultAsync(d => d.ID == id);

                if (destination == null)
                {
                    return NotFound("Destination not found.");
                }

                var viewModel = new CreateDestinationViewModel
                {
                    Name = destination.Name,
                    Description = destination.Description,
                    Location = destination.Location,
                    CityID = destination.CityID,
                    SelectedActivityIds = destination.Activities.Select(a => a.ID).ToList(),
                    Activities = await _context.Activities.ToListAsync(),
                    SelectedPreferencesIds = destination.Attractions.Select(a => a.ID).ToList(),
                    Attractions = await _context.Preferences.ToListAsync(),
                    Cities = await _context.Cities.ToListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading the Edit Destination page.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDestination(Guid id, CreateDestinationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Cities = await _context.Cities.ToListAsync();
                model.Activities = await _context.Activities.ToListAsync();
                model.Attractions = await _context.Preferences.ToListAsync();
                return View(model);
            }

            try
            {
                var destination = await _context.Destinations
                    .Include(d => d.Activities)
                    .Include(d => d.Attractions)
                    .FirstOrDefaultAsync(d => d.ID == id);

                if (destination == null)
                {
                    return NotFound("Destination not found.");
                }

                destination.Name = model.Name;
                destination.Description = model.Description;
                destination.Location = model.Location;
                destination.CityID = model.CityID;
                destination.Image_Path = $"/images/accommodations/{model.Name}.png"; // Set Image_Path dynamically

                var selectedActivities = await _context.Activities
                    .Where(a => model.SelectedActivityIds.Contains(a.ID))
                    .ToListAsync();
                destination.Activities = selectedActivities;

                var selectedPreferences = await _context.Preferences
                    .Where(p => model.SelectedPreferencesIds.Contains(p.ID))
                    .ToListAsync();
                destination.Attractions = selectedPreferences;

                await _context.SaveChangesAsync();

                return RedirectToAction("ListDestinations", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while editing the destination.");
                ModelState.AddModelError("", $"Error: {ex.Message}");
                model.Cities = await _context.Cities.ToListAsync();
                model.Activities = await _context.Activities.ToListAsync();
                model.Attractions = await _context.Preferences.ToListAsync();
                return View(model);
            }
        }
    }
}
