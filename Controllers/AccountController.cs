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

        /// <summary>
        /// Displays the user settings page.
        /// </summary>
        public IActionResult Settings()
        {
            _logger.LogInformation("Accessing Settings page");

            // Retrieve the logged-in user's email from claims
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("Settings access attempt by an unauthenticated user.");
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
                if (user == null)
                {
                    _logger.LogWarning("User not found in database. Email: {Email}", userEmail);
                    return NotFound();
                }

                _logger.LogInformation("User settings successfully retrieved for Email: {Email}", userEmail);
                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving user settings for Email: {Email}", userEmail);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Updates the user's name.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateName([FromForm] UpdateRequest request)
        {
            _logger.LogInformation("UpdateName request received with value: {Value}", request.Value);

            if (string.IsNullOrEmpty(request.Value))
            {
                _logger.LogWarning("UpdateName request has an empty value.");
                return BadRequest("Name cannot be empty.");
            }

            try
            {
                var user = GetCurrentUser();
                if (user == null)
                {
                    _logger.LogWarning("Unauthorized attempt to update name.");
                    return Unauthorized();
                }

                _logger.LogInformation("Updating name for user with Email: {Email}", user.Email);
                user.Name = request.Value;

                // Ensure EF Core is tracking changes
                _context.Entry(user).State = EntityState.Modified;
                _context.SaveChanges();

                _logger.LogInformation("Name successfully updated for user with Email: {Email}", user.Email);
                return Ok(new { success = true, message = "Name updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the user's name.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Updates the user's email.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmail([FromForm] UpdateRequest request)
        {
            _logger.LogInformation("UpdateEmail request received with value: {Value}", request.Value);

            if (string.IsNullOrEmpty(request.Value))
            {
                _logger.LogWarning("UpdateEmail request has an empty value.");
                return BadRequest("Email cannot be empty.");
            }

            // Validate the email format
            if (!new EmailAddressAttribute().IsValid(request.Value))
            {
                _logger.LogWarning("UpdateEmail request has an invalid email format: {Value}", request.Value);
                return BadRequest("Invalid email format.");
            }

            try
            {
                var user = GetCurrentUser();
                if (user == null)
                {
                    _logger.LogWarning("Unauthorized attempt to update email.");
                    return Unauthorized();
                }

                // Check if the new email is already taken
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Value);
                if (existingUser != null)
                {
                    _logger.LogWarning("Attempt to update email to an already existing email: {Email}", request.Value);
                    return BadRequest("This email is already in use.");
                }

                // Update email in the database
                _logger.LogInformation("Updating email for user with current Email: {Email}", user.Email);
                user.Email = request.Value;
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Email successfully updated for user with Email: {Email}", request.Value);

                // Create new claims with updated email
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("IsAdmin", user.IsAdmin.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Re-issue the authentication cookie with updated claims
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                _logger.LogInformation("Authentication cookie refreshed with new email.");

                return Json(new { success = true, message = "Email updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the user's email.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Updates the user's password.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword([FromForm] UpdatePasswordRequest request)
        {
            _logger.LogInformation("UpdatePassword request received.");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("UpdatePassword request failed validation.");
                return BadRequest(new { message = "Invalid password data." });
            }

            try
            {
                var user = GetCurrentUser();
                if (user == null)
                {
                    _logger.LogWarning("Unauthorized attempt to update password.");
                    return Unauthorized();
                }

                // TODO: Implement password hashing. For demonstration, assuming plain text.
                if (user.Password != request.CurrentPassword)
                {
                    _logger.LogWarning("Incorrect current password for user with Email: {Email}", user.Email);
                    return BadRequest(new { message = "Incorrect current password." });
                }

                // Update password (ideally hash it)
                user.Password = request.NewPassword; // Replace with hashed password in production

                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Password successfully updated for user with Email: {Email}", user.Email);
                return Json(new { success = true, message = "Password updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the user's password.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Displays the preferences management page.
        /// </summary>
        public async Task<IActionResult> Preferences()
        {
            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = Guid.Parse(userIdClaim);
            var user = await _context.Users
                .Include(u => u.Preferences)
                .FirstOrDefaultAsync(u => u.ID == userId);

            if (user == null)
            {
                _logger.LogWarning("User not found in database. ID: {ID}", userId);
                return RedirectToAction("Login", "Account");
            }

            var allPreferences = await _context.Preferences
                .Where(p => !user.Preferences.Contains(p))
                .ToListAsync();

            ViewBag.UserPreferences = user.Preferences ?? new List<Preference>();
            return View(allPreferences);
        }

        /// <summary>
        /// Adds a preference to the user's preferences.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPreference([FromBody] PreferenceRequest request)
        {
            if (request == null || request.PreferenceId == Guid.Empty)
            {
                _logger.LogWarning("AddPreference received invalid request data.");
                return BadRequest(new { success = false, message = "Invalid preference data." });
            }

            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("AddPreference: User not authenticated.");
                return Json(new { success = false, message = "User not authenticated." });
            }

            var userId = Guid.Parse(userIdClaim);
            var user = await _context.Users
                .Include(u => u.Preferences)
                .FirstOrDefaultAsync(u => u.ID == userId);

            if (user == null)
            {
                _logger.LogWarning("AddPreference: User not found. UserID: {UserID}", userId);
                return Json(new { success = false, message = "User not found." });
            }

            if (user.Preferences.Any(p => p.ID == request.PreferenceId))
            {
                _logger.LogWarning("AddPreference: Preference already added. PreferenceID: {PreferenceID}", request.PreferenceId);
                return Json(new { success = false, message = "Preference already added." });
            }

            var preference = await _context.Preferences.FirstOrDefaultAsync(p => p.ID == request.PreferenceId);
            if (preference == null)
            {
                _logger.LogWarning("AddPreference: Preference not found. PreferenceID: {PreferenceID}", request.PreferenceId);
                return Json(new { success = false, message = "Preference not found." });
            }

            user.Preferences.Add(preference);
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            _logger.LogInformation("AddPreference: Preference successfully added. PreferenceID: {PreferenceID}", request.PreferenceId);
            return Json(new { success = true, message = "Preference added successfully." });
        }

        /// <summary>
        /// Removes a preference from the user's preferences.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemovePreference([FromBody] RemovePreferenceRequest request)
        {
            if (request == null || request.PreferenceId == Guid.Empty)
            {
                _logger.LogWarning("RemovePreference received invalid request data.");
                return BadRequest(new { success = false, message = "Invalid preference data." });
            }

            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogWarning("RemovePreference: User not authenticated.");
                return Json(new { success = false, message = "User not authenticated." });
            }

            var userId = Guid.Parse(userIdClaim);
            var user = await _context.Users
                .Include(u => u.Preferences)
                .FirstOrDefaultAsync(u => u.ID == userId);

            if (user == null)
            {
                _logger.LogWarning("RemovePreference: User not found. UserID: {UserID}", userId);
                return Json(new { success = false, message = "User not found." });
            }

            var preference = user.Preferences.FirstOrDefault(p => p.ID == request.PreferenceId);
            if (preference == null)
            {
                _logger.LogWarning("RemovePreference: Preference not found in user's preferences. PreferenceID: {PreferenceID}", request.PreferenceId);
                return Json(new { success = false, message = "Preference not found or already removed." });
            }

            user.Preferences.Remove(preference);
            _context.Entry(user).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            _logger.LogInformation("RemovePreference: Preference successfully removed from user. PreferenceID: {PreferenceID}", request.PreferenceId);

            return Json(new { success = true, message = "Preference removed successfully." });
        }

        /// <summary>
        /// Displays the user's past travels.
        /// </summary>
        public async Task<IActionResult> PastTravels()
        {
            var userIdClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("PastTravels: User not authenticated.");
                return RedirectToAction("Login", "Auth");
            }

            var userId = Guid.Parse(userIdClaim);

            // Fetch visited destinations
            var visitedDestinations = await _context.Destinations
                .Include(d => d.City)
                .Where(d => _context.Visits.Any(v => v.UserID == userId && v.DestinationID == d.ID))
                .ToListAsync();

            // Fetch user-reviewed destinations
            var userReviewedDestinationIds = await _context.Reviews
                .Where(r => r.UserID == userId && r.DestinationID.HasValue)
                .Select(r => r.DestinationID.Value)
                .ToListAsync();

            ViewBag.ReviewedDestinations = userReviewedDestinationIds;

            return View(visitedDestinations);
        }




        /// <summary>
        /// Displays the user's favorited destinations.
        /// </summary>
        public async Task<IActionResult> FavoritedDestinations()
        {
            _logger.LogInformation("FavoritedDestinations action invoked.");

            try
            {
                var user = GetCurrentUser();
                if (user == null)
                {
                    _logger.LogWarning("FavoritedDestinations: User not found or not authenticated.");
                    return Unauthorized();
                }

                // Load the user's favorited destinations, including related data if necessary
                var favoritedDestinations = await _context.Users
                    .Where(u => u.ID == user.ID)
                    .Include(u => u.Favorited)
                        .ThenInclude(d => d.City) // Include related City data
                    .Include(u => u.Favorited)
                        .ThenInclude(d => d.Reviews) // Include related Reviews
                    .FirstOrDefaultAsync();

                if (favoritedDestinations == null)
                {
                    _logger.LogWarning("FavoritedDestinations: No favorited destinations found for user with ID: {UserID}", user.ID);
                    return NotFound("No favorited destinations found.");
                }

                _logger.LogInformation("FavoritedDestinations: Retrieved {Count} favorited destinations for user with ID: {UserID}", favoritedDestinations.Favorited.Count, user.ID);

                return View(favoritedDestinations.Favorited);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FavoritedDestinations: An error occurred while retrieving favorited destinations.");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Removes a destination from the user's favorited destinations.
        /// </summary>
        /// <param name="request">The request containing the Destination ID.</param>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFavoritedDestination([FromBody] RemoveFavoritedDestinationRequest request)
        {
            _logger.LogInformation("RemoveFavoritedDestination request received for DestinationID: {DestinationID}", request.DestinationId);

            if (request == null || request.DestinationId == Guid.Empty)
            {
                _logger.LogWarning("RemoveFavoritedDestination: Invalid request data.");
                return BadRequest(new { success = false, message = "Invalid request data." });
            }

            try
            {
                var user = GetCurrentUser();
                if (user == null)
                {
                    _logger.LogWarning("RemoveFavoritedDestination: User not authenticated or not found.");
                    return Unauthorized(new { success = false, message = "User not authenticated." });
                }

                var destination = await _context.Destinations.FirstOrDefaultAsync(d => d.ID == request.DestinationId);
                if (destination == null)
                {
                    _logger.LogWarning("RemoveFavoritedDestination: Destination not found. DestinationID: {DestinationID}", request.DestinationId);
                    return NotFound(new { success = false, message = "Destination not found." });
                }

                if (!user.Favorited.Contains(destination))
                {
                    _logger.LogWarning("RemoveFavoritedDestination: Destination not in user's favorites. DestinationID: {DestinationID}", request.DestinationId);
                    return BadRequest(new { success = false, message = "Destination not in your favorites." });
                }

                user.Favorited.Remove(destination);
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.LogInformation("RemoveFavoritedDestination: Destination removed from user's favorites. DestinationID: {DestinationID}", request.DestinationId);
                return Json(new { success = true, message = "Destination removed from your favorites." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RemoveFavoritedDestination: An error occurred while removing the destination from favorites.");
                return StatusCode(500, new { success = false, message = "An error occurred while processing your request." });
            }
        }

        /// <summary>
        /// DTO for removing a favorited destination.
        /// </summary>

        /// <summary>
        /// Retrieves the current authenticated user from the database.
        /// </summary>
        private User GetCurrentUser()
        {
            var userIDClaim = User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var userID = Guid.Parse(userIDClaim);
            var user = _context.Users.FirstOrDefault(u => u.ID == userID);

            _logger.LogInformation("Retrieved user: {User}", user != null ? user.Name : "null");

            return user;
        }



        #endregion

        #region DTOs
        public class RemoveFavoritedDestinationRequest
        {
            public Guid DestinationId { get; set; }
        }

        /// <summary>
        /// DTO for updating name and email.
        /// </summary>
        public class UpdateRequest
        {
            [Required]
            public string Value { get; set; }
        }

        /// <summary>
        /// DTO for updating password.
        /// </summary>
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

        /// <summary>
        /// DTO for adding a preference.
        /// </summary>
        public class PreferenceRequest
        {
            public Guid PreferenceId { get; set; }
        }

        /// <summary>
        /// DTO for removing a preference.
        /// </summary>
        public class RemovePreferenceRequest
        {
            public Guid PreferenceId { get; set; }
        }

        #endregion


    }
}
