using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TravelApp.Data;
using TravelApp.Models.Entities;
using TravelApp.Models.ViewModels;

namespace TravelApp.Controllers
{
    [Authorize]
    public class CreateActivityController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<CreateActivityController> _logger;

        public CreateActivityController(AppDBContext context, ILogger<CreateActivityController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> CreateActivity()
        {
            try
            {
                var destinations = await _context.Destinations
                    .Select(d => new SelectListItem
                    {
                        Value = d.ID.ToString(),
                        Text = $"{d.Name} (ID: {d.ID})"
                    })
                    .ToListAsync();

                var accommodations = await _context.Accommodations
                    .Select(a => new SelectListItem
                    {
                        Value = a.ID.ToString(),
                        Text = $"{a.Name} (ID: {a.ID})"
                    })
                    .ToListAsync();

                var model = new CreateActivityViewModel
                {
                    Destinations = destinations,
                    Accommodations = accommodations
                };

                if (!model.Destinations.Any())
                {
                    ModelState.AddModelError("", "No destinations available. Please create a destination first.");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Create Activity page.");
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateActivity(CreateActivityViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Destinations = await _context.Destinations
                    .Select(d => new SelectListItem
                    {
                        Value = d.ID.ToString(),
                        Text = $"{d.Name} (ID: {d.ID})"
                    })
                    .ToListAsync();

                model.Accommodations = await _context.Accommodations
                    .Select(a => new SelectListItem
                    {
                        Value = a.ID.ToString(),
                        Text = $"{a.Name} (ID: {a.ID})"
                    })
                    .ToListAsync();

                return View(model);
            }

            try
            {
                var selectedDestinations = await _context.Destinations
                    .Where(d => model.DestinationIDs.Contains(d.ID))
                    .ToListAsync();

                var selectedAccommodations = await _context.Accommodations
                    .Where(a => model.AccommodationIDs.Contains(a.ID))
                    .ToListAsync();

                var activity = new Activity
                {
                    ID = Guid.NewGuid(),
                    Name = model.Name,
                    Type = model.Type,
                    Date = model.Date,
                    Price = model.Price,
                    Requires_Reservation = model.Requires_Reservation,
                    Destinations = selectedDestinations,
                    Accommodations = selectedAccommodations
                };

                _context.Activities.Add(activity);
                await _context.SaveChangesAsync();
                return RedirectToAction("ListActivities", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity.");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again.");

                model.Destinations = await _context.Destinations
                    .Select(d => new SelectListItem
                    {
                        Value = d.ID.ToString(),
                        Text = $"{d.Name} (ID: {d.ID})"
                    })
                    .ToListAsync();

                model.Accommodations = await _context.Accommodations
                    .Select(a => new SelectListItem
                    {
                        Value = a.ID.ToString(),
                        Text = $"{a.Name} (ID: {a.ID})"
                    })
                    .ToListAsync();

                return View(model);
            }
        }

        [HttpGet]
        [Route("CreateActivity/EditActivity/{id}")]
        public async Task<IActionResult> EditActivity(Guid id)
        {
            try
            {
                var activity = await _context.Activities
                    .Include(a => a.Destinations)
                    .Include(a => a.Accommodations)
                    .FirstOrDefaultAsync(a => a.ID == id);

                if (activity == null)
                {
                    _logger.LogError("Activity with ID {ID} not found.", id);
                    return NotFound("Activity not found.");
                }

                var viewModel = new CreateActivityViewModel
                {
                    Name = activity.Name,
                    Type = activity.Type,
                    Date = activity.Date,
                    Price = activity.Price,
                    Requires_Reservation = activity.Requires_Reservation,
                    DestinationIDs = activity.Destinations?.Select(d => d.ID).ToList() ?? new List<Guid>(),
                    AccommodationIDs = activity.Accommodations?.Select(a => a.ID).ToList() ?? new List<Guid>(),
                    Destinations = await _context.Destinations
                        .Select(d => new SelectListItem
                        {
                            Value = d.ID.ToString(),
                            Text = $"{d.Name} (ID: {d.ID})"
                        })
                        .ToListAsync(),
                    Accommodations = await _context.Accommodations
                        .Select(a => new SelectListItem
                        {
                            Value = a.ID.ToString(),
                            Text = $"{a.Name} (ID: {a.ID})"
                        })
                        .ToListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while editing activity with ID {ID}.", id);
                return StatusCode(500, "An unexpected error occurred. Please try again.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditActivity(Guid id, CreateActivityViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Destinations = await _context.Destinations
                    .Select(d => new SelectListItem
                    {
                        Value = d.ID.ToString(),
                        Text = $"{d.Name} (ID: {d.ID})"
                    })
                    .ToListAsync();

                viewModel.Accommodations = await _context.Accommodations
                    .Select(a => new SelectListItem
                    {
                        Value = a.ID.ToString(),
                        Text = $"{a.Name} (ID: {a.ID})"
                    })
                    .ToListAsync();

                return View(viewModel);
            }

            try
            {
                var existingActivity = await _context.Activities
                    .Include(a => a.Destinations)
                    .Include(a => a.Accommodations)
                    .FirstOrDefaultAsync(a => a.ID == id);

                if (existingActivity == null)
                {
                    _logger.LogError("Activity with ID {ID} not found.", id);
                    ModelState.AddModelError("", "Activity not found.");
                    return View(viewModel);
                }

                existingActivity.Name = viewModel.Name;
                existingActivity.Type = viewModel.Type;
                existingActivity.Date = viewModel.Date;
                existingActivity.Price = viewModel.Price;
                existingActivity.Requires_Reservation = viewModel.Requires_Reservation;

                var selectedDestinations = await _context.Destinations
                    .Where(d => viewModel.DestinationIDs.Contains(d.ID))
                    .ToListAsync();
                existingActivity.Destinations = selectedDestinations;

                var selectedAccommodations = await _context.Accommodations
                    .Where(a => viewModel.AccommodationIDs.Contains(a.ID))
                    .ToListAsync();
                existingActivity.Accommodations = selectedAccommodations;

                await _context.SaveChangesAsync();
                return RedirectToAction("ListActivities", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while editing activity.");
                ModelState.AddModelError("", "An unexpected error occurred. Please contact support.");

                viewModel.Destinations = await _context.Destinations
                    .Select(d => new SelectListItem
                    {
                        Value = d.ID.ToString(),
                        Text = $"{d.Name} (ID: {d.ID})"
                    })
                    .ToListAsync();

                viewModel.Accommodations = await _context.Accommodations
                    .Select(a => new SelectListItem
                    {
                        Value = a.ID.ToString(),
                        Text = $"{a.Name} (ID: {a.ID})"
                    })
                    .ToListAsync();

                return View(viewModel);
            }
        }
    }
}
