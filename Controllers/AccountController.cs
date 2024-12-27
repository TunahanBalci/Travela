using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TravelApp.Data;
using TravelApp.Models.DTOs;
using TravelApp.Models.Entities;
using System.ComponentModel.DataAnnotations;
using TravelApp.Utils;

namespace TravelApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AppDBContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region User Settings Actions

        public IActionResult Settings()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null)
                {
                    return NotFound();
                }
                return View(user);
            }
            catch
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateName([FromForm] UpdateRequest request)
        {
            if (string.IsNullOrEmpty(request.Value))
            {
                return BadRequest("Name cannot be empty.");
            }

            try
            {
                var user = GetCurrentUser();
                if (user == null)
                {
                    return Unauthorized();
                }

                user.Name = request.Value;
                _context.Entry(user).State = EntityState.Modified;
                _context.SaveChanges();
                return Ok(new { success = true, message = "Name updated successfully." });
            }
            catch
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmail([FromForm] UpdateRequest request)
        {
            if (string.IsNullOrEmpty(request.Value) || !new EmailAddressAttribute().IsValid(request.Value))
            {
                return BadRequest("Invalid email format.");
            }

            try
            {
                var user = GetCurrentUser();
                if (user == null)
                {
                    return Unauthorized();
                }

                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Value);
                if (existingUser != null)
                {
                    return BadRequest("This email is already in use.");
                }

                user.Email = request.Value;
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("IsAdmin", user.IsAdmin.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                return Json(new { success = true, message = "Email updated successfully." });
            }
            catch
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword([FromForm] UpdatePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid password data." });
            }

            try
            {
                var user = GetCurrentUser();
                if (user == null || user.Password != request.CurrentPassword)
                {
                    return BadRequest(new { message = "Incorrect current password." });
                }

                user.Password = request.NewPassword;
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Password updated successfully." });
            }
            catch
            {
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        public async Task<IActionResult> Preferences()
        {
            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = Guid.Parse(userIdClaim);
            var user = await _context.Users.Include(u => u.Preferences).FirstOrDefaultAsync(u => u.ID == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var allPreferences = await _context.Preferences.ToListAsync();
            ViewBag.UserPreferences = user.Preferences ?? new List<Preference>();
            return View(allPreferences);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPreference([FromBody] PreferenceRequest request)
        {
            if (request == null || request.PreferenceId == Guid.Empty)
            {
                return BadRequest(new { success = false, message = "Invalid preference data." });
            }

            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            var userId = Guid.Parse(userIdClaim);
            var user = await _context.Users.Include(u => u.Preferences).FirstOrDefaultAsync(u => u.ID == userId);
            if (user == null || user.Preferences.Any(p => p.ID == request.PreferenceId))
            {
                return Json(new { success = false, message = "Preference already added." });
            }

            var preference = await _context.Preferences.FirstOrDefaultAsync(p => p.ID == request.PreferenceId);
            if (preference == null)
            {
                return Json(new { success = false, message = "Preference not found." });
            }

            user.Preferences.Add(preference);
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Preference added successfully." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePreference([FromBody] RemovePreferenceRequest request)
        {
            if (request == null || request.PreferenceId == Guid.Empty)
            {
                return BadRequest(new { success = false, message = "Invalid preference data." });
            }

            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Json(new { success = false, message = "User not authenticated." });
            }

            var userId = Guid.Parse(userIdClaim);
            var user = await _context.Users.Include(u => u.Preferences).FirstOrDefaultAsync(u => u.ID == userId);
            var preference = user?.Preferences.FirstOrDefault(p => p.ID == request.PreferenceId);
            if (preference == null)
            {
                return Json(new { success = false, message = "Preference not found or already removed." });
            }

            user.Preferences.Remove(preference);
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Preference removed successfully." });
        }

        public async Task<IActionResult> PastTravels()
        {
            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userId = Guid.Parse(userIdClaim);
            var visitedDestinations = await _context.Destinations.Include(d => d.City)
                .Where(d => _context.Visits.Any(v => v.UserID == userId && v.DestinationID == d.ID)).ToListAsync();

            var userReviewedDestinationIds = await _context.Reviews
                .Where(r => r.UserID == userId && r.DestinationID.HasValue)
                .Select(r => r.DestinationID.Value).ToListAsync();

            ViewBag.ReviewedDestinations = userReviewedDestinationIds;
            return View(visitedDestinations);
        }

        public async Task<IActionResult> FavoritedDestinations()
        {
            var user = GetCurrentUser();
            if (user == null)
            {
                return Unauthorized();
            }

            var favoritedDestinations = await _context.Users
                .Where(u => u.ID == user.ID)
                .Include(u => u.Favorited).ThenInclude(d => d.City)
                .Include(u => u.Favorited).ThenInclude(d => d.Reviews)
                .FirstOrDefaultAsync();

            return favoritedDestinations == null ? NotFound() : View(favoritedDestinations.Favorited);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFavoritedDestination([FromBody] RemoveFavoritedDestinationRequest request)
        {
            if (request == null || request.DestinationId == Guid.Empty)
            {
                return BadRequest(new { success = false, message = "Invalid request data." });
            }

            try
            {
                var user = GetCurrentUser();
                if (user == null)
                {
                    return Unauthorized(new { success = false, message = "User not authenticated." });
                }

                var destination = await _context.Destinations.FirstOrDefaultAsync(d => d.ID == request.DestinationId);
                if (destination == null || !user.Favorited.Contains(destination))
                {
                    return BadRequest(new { success = false, message = "Destination not in your favorites." });
                }

                user.Favorited.Remove(destination);
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Destination removed from your favorites." });
            }
            catch
            {
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }

        private User GetCurrentUser()
        {
            var userIDClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIDClaim, out var userID)
                ? _context.Users.FirstOrDefault(u => u.ID == userID)
                : null;
        }

        #endregion

        #region DTOs

        public class RemoveFavoritedDestinationRequest
        {
            public Guid DestinationId { get; set; }
        }

        public class UpdateRequest
        {
            [Required]
            public string Value { get; set; }
        }

        public class UpdatePasswordRequest
        {
            [Required(ErrorMessage = "Current password is required.")]
            public string CurrentPassword { get; set; }

            [Required(ErrorMessage = "New password is required.")]
            [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters long.")]
            public string NewPassword { get; set; }

            [Required(ErrorMessage = "Confirm password is required.")]
            [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public class PreferenceRequest
        {
            public Guid PreferenceId { get; set; }
        }

        public class RemovePreferenceRequest
        {
            public Guid PreferenceId { get; set; }
        }

        #endregion
    }
}
