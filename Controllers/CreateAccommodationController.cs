using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelApp.Data;
using TravelApp.Models.Entities;
using TravelApp.Models.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace TravelApp.Controllers
{
    [Authorize]
    public class CreateAccommodationController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<CreateAccommodationController> _logger;

        public CreateAccommodationController(AppDBContext context, ILogger<CreateAccommodationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: CreateAccommodation/CreateAccommodation
        public async Task<IActionResult> CreateAccommodation()
        {
            try
            {
                var model = new CreateAccommodationViewModel
                {
                    Cities = await _context.Cities.ToListAsync(),
                    Activities = await _context.Activities.ToListAsync(),
                    Amenities = await _context.Amenities.ToListAsync()
                };

                if (!model.Cities.Any())
                {
                    ModelState.AddModelError("", "No cities available. Please create a city first.");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading the Create Accommodation page.");
                ModelState.AddModelError("", "An error occurred while loading the page. Please try again later.");
                return View(new CreateAccommodationViewModel
                {
                    Cities = new List<City>(),
                    Activities = new List<Activity>(),
                    Amenities = new List<Amenity>()
                });
            }
        }

        // POST: CreateAccommodation/CreateAccommodation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAccommodation(CreateAccommodationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Cities = await _context.Cities.ToListAsync();
                model.Activities = await _context.Activities.ToListAsync();
                model.Amenities = await _context.Amenities.ToListAsync();
                return View(model);
            }

            try
            {
                var accommodation = new Accommodation
                {
                    ID = Guid.NewGuid(),
                    Name = model.Name,
                    Type = model.Type,
                    Location = model.Location,
                    Price = model.Price,
                    Availability = model.Availability,
                    CityID = model.CityID,
                    Image_Path = $"/images/accommodations/{model.Name}.png" // Set Image_Path dynamically
                };

                if (model.SelectedActivityIds != null && model.SelectedActivityIds.Any())
                {
                    var selectedActivities = await _context.Activities
                        .Where(a => model.SelectedActivityIds.Contains(a.ID))
                        .ToListAsync();
                    accommodation.Activities = selectedActivities;
                }

                if (model.SelectedAmenityIds != null && model.SelectedAmenityIds.Any())
                {
                    var selectedAmenities = await _context.Amenities
                        .Where(a => model.SelectedAmenityIds.Contains(a.ID))
                        .ToListAsync();
                    accommodation.Amenities = selectedAmenities;
                }

                _context.Accommodations.Add(accommodation);
                await _context.SaveChangesAsync();

                return RedirectToAction("ListAccommodations", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating the accommodation.");
                ModelState.AddModelError("", $"Error: {ex.Message}");
                model.Cities = await _context.Cities.ToListAsync();
                model.Activities = await _context.Activities.ToListAsync();
                model.Amenities = await _context.Amenities.ToListAsync();
                return View(model);
            }
        }



        [HttpGet]
        [Route("CreateAccommodation/EditAccommodation/{id}")]
        public async Task<IActionResult> EditAccommodation(Guid id)
        {
            try
            {
                var accommodation = await _context.Accommodations
                    .Include(a => a.City)
                    .Include(a => a.Activities)
                    .Include(a => a.Amenities)
                    .FirstOrDefaultAsync(a => a.ID == id);

                if (accommodation == null)
                {
                    _logger.LogError("Accommodation with ID {ID} not found.", id);
                    return NotFound("Accommodation not found.");
                }

                var viewModel = new CreateAccommodationViewModel
                {
                    ID = accommodation.ID,
                    Name = accommodation.Name,
                    Type = accommodation.Type,
                    Location = accommodation.Location,
                    Price = accommodation.Price,
                    Availability = accommodation.Availability,
                    CityID = accommodation.City?.ID ?? Guid.Empty, // Handle null City
                    SelectedActivityIds = accommodation.Activities?.Select(a => a.ID).ToList() ?? new List<Guid>(),
                    SelectedAmenityIds = accommodation.Amenities?.Select(a => a.ID).ToList() ?? new List<Guid>(),
                    Cities = await _context.Cities.ToListAsync(),
                    Activities = await _context.Activities.ToListAsync(),
                    Amenities = await _context.Amenities.ToListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while editing accommodation with ID {ID}.", id);
                return StatusCode(500, "An unexpected error occurred. Please try again.");
            }
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAccommodation(Guid id, CreateAccommodationViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Cities = await _context.Cities.ToListAsync();
                viewModel.Activities = await _context.Activities.ToListAsync();
                viewModel.Amenities = await _context.Amenities.ToListAsync();
                return View(viewModel);
            }

            try
            {
                var existingAccommodation = await _context.Accommodations
                    .Include(a => a.Activities)
                    .Include(a => a.Amenities)
                    .FirstOrDefaultAsync(a => a.ID == id);

                if (existingAccommodation == null)
                {
                    ModelState.AddModelError("", "Accommodation not found.");
                    return View(viewModel);
                }

                // Update fields
                existingAccommodation.Name = viewModel.Name;
                existingAccommodation.Type = viewModel.Type;
                existingAccommodation.Location = viewModel.Location;
                existingAccommodation.Price = viewModel.Price;
                existingAccommodation.Availability = viewModel.Availability;
                existingAccommodation.City = await _context.Cities.FindAsync(viewModel.CityID);
                existingAccommodation.Image_Path = $"/images/accommodations/{viewModel.Name}.png"; // Set Image_Path dynamically

                // Update Activities
                if (viewModel.SelectedActivityIds != null)
                {
                    var selectedActivities = await _context.Activities
                        .Where(a => viewModel.SelectedActivityIds.Contains(a.ID))
                        .ToListAsync();
                    existingAccommodation.Activities = selectedActivities;
                }

                // Update Amenities
                if (viewModel.SelectedAmenityIds != null)
                {
                    var selectedAmenities = await _context.Amenities
                        .Where(a => viewModel.SelectedAmenityIds.Contains(a.ID))
                        .ToListAsync();
                    existingAccommodation.Amenities = selectedAmenities;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("ListAccommodations", "AdminPanel");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while updating the accommodation.");
                ModelState.AddModelError("", "Database update failed. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred.");
                ModelState.AddModelError("", "An unexpected error occurred. Please contact support.");
            }

            viewModel.Cities = await _context.Cities.ToListAsync();
            viewModel.Activities = await _context.Activities.ToListAsync();
            viewModel.Amenities = await _context.Amenities.ToListAsync();
            return View(viewModel);
        }

    }
}
