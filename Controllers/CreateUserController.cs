using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelApp.Data;
using TravelApp.Models.Entities;
using TravelApp.Models.ViewModels;

namespace TravelApp.Controllers
{
    public class CreateUserController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<CreateUserController> _logger;

        public CreateUserController(AppDBContext context, ILogger<CreateUserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult CreateUser()
        {
            var isAdmin = User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value;
            if (!(isAdmin != null && bool.TryParse(isAdmin, out var isAdminBool) && isAdminBool))
            {
                return Forbid();
            }

            var preferences = _context.Preferences
                .Select(p => new SelectListItem
                {
                    Value = p.ID.ToString(),
                    Text = p.Content
                })
                .ToList();

            var model = new CreateUserViewModel
            {
                Preferences = preferences
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUser(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Preferences = _context.Preferences
                    .Select(p => new SelectListItem
                    {
                        Value = p.ID.ToString(),
                        Text = p.Content
                    })
                    .ToList();

                return View(model);
            }

            try
            {
                var user = new User
                {
                    ID = Guid.NewGuid(),
                    Name = model.Name,
                    Email = model.Email,
                    Password = model.Password,
                    IsAdmin = false
                };

                if (model.SelectedPreferenceIds != null && model.SelectedPreferenceIds.Any())
                {
                    var selectedPreferences = _context.Preferences
                        .Where(p => model.SelectedPreferenceIds.Contains(p.ID))
                        .ToList();

                    user.Preferences = selectedPreferences;
                }

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("ListUsers", "AdminPanel");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                model.Preferences = _context.Preferences
                    .Select(p => new SelectListItem
                    {
                        Value = p.ID.ToString(),
                        Text = p.Content
                    })
                    .ToList();

                return View(model);
            }
        }

        [HttpGet]
        [Route("CreateUser/EditUser/{id}")]
        public async Task<IActionResult> EditUser(Guid id)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.ID == id);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                if (user.IsAdmin)
                {
                    ModelState.AddModelError("", "Admin users cannot be edited.");
                    return RedirectToAction("ListUsers", "AdminPanel");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user for editing.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(Guid id, User user)
        {
            if (id != user.ID)
            {
                return BadRequest("User ID mismatch.");
            }

            var existingUser = await _context.Users.FindAsync(id);

            if (existingUser == null)
            {
                return NotFound("User not found.");
            }

            if (existingUser.IsAdmin)
            {
                ModelState.AddModelError("", "Admin users cannot be edited.");
                return RedirectToAction("ListUsers", "AdminPanel");
            }

            if (!ModelState.IsValid)
            {
                return View(user);
            }

            try
            {
                existingUser.Name = user.Name;
                existingUser.Email = user.Email;

                if (!string.IsNullOrEmpty(user.Password))
                {
                    existingUser.Password = user.Password;
                }

                existingUser.IsAdmin = user.IsAdmin;

                await _context.SaveChangesAsync();

                return RedirectToAction("ListUsers", "AdminPanel");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing user.");
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View(user);
            }
        }
    }
}
