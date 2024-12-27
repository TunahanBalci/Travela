using Microsoft.AspNetCore.Mvc;
using TravelApp.Data;
using TravelApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using TravelApp.Models.ViewModels;

namespace TravelApp.Controllers
{
    public class CreateCityController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<CreateCityController> _logger;

        public CreateCityController(AppDBContext context, ILogger<CreateCityController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult CreateCity()
        {
            var isAdmin = User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value;
            if (!(isAdmin != null && bool.TryParse(isAdmin, out var isAdminBool) && isAdminBool))
            {
                return Forbid();
            }
            return View();
        }

        [HttpPost]
        public IActionResult CreateCity(City city)
        {
            if (!ModelState.IsValid)
            {
                return View(city);
            }

            try
            {
                city.ID = Guid.NewGuid();
                _context.Cities.Add(city);
                _context.SaveChanges();
                return RedirectToAction("ListCities", "AdminPanel");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(city);
            }
        }

        [HttpGet]
        [Route("CreateCity/EditCity/{id}")]
        public async Task<IActionResult> EditCity(Guid id)
        {
            try
            {
                var city = await _context.Cities
                    .Include(c => c.Activities)
                    .FirstOrDefaultAsync(c => c.ID == id);

                if (city == null)
                {
                    return NotFound("City not found.");
                }

                var selectedActivityIds = city.Activities.Select(a => a.ID).ToList();

                var activities = await _context.Activities
                    .Select(a => new SelectListItem
                    {
                        Value = a.ID.ToString(),
                        Text = a.Name,
                        Selected = selectedActivityIds.Contains(a.ID)
                    })
                    .ToListAsync();

                var viewModel = new EditCityViewModel
                {
                    ID = city.ID,
                    Name = city.Name,
                    Location = city.Location,
                    Climate = city.Climate,
                    Terrain = city.Terrain,
                    Cost_Of_Living = city.Cost_Of_Living,
                    SelectedActivityIds = selectedActivityIds,
                    Activities = activities
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading city for editing with ID {ID}.", id);
                return StatusCode(500, "An unexpected error occurred. Please try again.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCity(Guid id, EditCityViewModel viewModel)
        {
            if (id != viewModel.ID)
            {
                return BadRequest("City ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                viewModel.Activities = await _context.Activities
                    .Select(a => new SelectListItem
                    {
                        Value = a.ID.ToString(),
                        Text = a.Name,
                        Selected = viewModel.SelectedActivityIds.Contains(a.ID)
                    })
                    .ToListAsync();
                return View(viewModel);
            }

            try
            {
                var existingCity = await _context.Cities
                    .Include(c => c.Activities)
                    .FirstOrDefaultAsync(c => c.ID == id);

                if (existingCity == null)
                {
                    return NotFound("City not found.");
                }

                existingCity.Name = viewModel.Name;
                existingCity.Location = viewModel.Location;
                existingCity.Climate = viewModel.Climate;
                existingCity.Terrain = viewModel.Terrain;
                existingCity.Cost_Of_Living = viewModel.Cost_Of_Living;

                var selectedActivities = await _context.Activities
                    .Where(a => viewModel.SelectedActivityIds.Contains(a.ID))
                    .ToListAsync();

                existingCity.Activities.Clear();
                foreach (var activity in selectedActivities)
                {
                    existingCity.Activities.Add(activity);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("ListCities", "AdminPanel");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error updating city with ID {ID}.", id);
                ModelState.AddModelError("", "Database update failed. Please try again.");
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while editing city.");
                ModelState.AddModelError("", "An unexpected error occurred. Please contact support.");
                return View(viewModel);
            }
        }
    }
}
