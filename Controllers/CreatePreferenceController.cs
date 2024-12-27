using Microsoft.AspNetCore.Mvc;
using TravelApp.Data;
using TravelApp.Models.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Threading.Tasks;

namespace TravelApp.Controllers
{
    [Authorize] // Ensure only admins can access these actions
    public class CreatePreferenceController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<CreatePreferenceController> _logger;

        public CreatePreferenceController(AppDBContext context, ILogger<CreatePreferenceController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult CreatePreference()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePreference(Preference preference)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    preference.ID = Guid.NewGuid();
                    _context.Preferences.Add(preference);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("ListPreferences", "AdminPanel");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating Preference.");
                    ModelState.AddModelError("", "An error occurred while creating the Preference. Please try again.");
                }
            }

            return View(preference);
        }

        [HttpGet]
        [Route("CreatePreference/EditPreference/{id}")]
        public async Task<IActionResult> EditPreference(Guid id)
        {
            try
            {
                var preference = await _context.Preferences.FindAsync(id);

                if (preference == null)
                {
                    return NotFound("Preference not found.");
                }

                return View(preference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Preference for editing with ID {ID}.", id);
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPreference(Guid id, Preference preference)
        {
            if (id != preference.ID)
            {
                return BadRequest("Preference ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return View(preference);
            }

            try
            {
                var existingPreference = await _context.Preferences.FindAsync(id);

                if (existingPreference == null)
                {
                    return NotFound("Preference not found.");
                }

                existingPreference.Content = preference.Content;

                await _context.SaveChangesAsync();

                return RedirectToAction("ListPreferences", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing Preference with ID {ID}.", id);
                ModelState.AddModelError("", "An error occurred while editing the Preference. Please try again.");
                return View(preference);
            }
        }
    }
}
