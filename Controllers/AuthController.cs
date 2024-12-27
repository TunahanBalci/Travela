using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using TravelApp.Data;
using TravelApp.Models.Entities;

namespace TravelApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDBContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDBContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            user.ID = Guid.NewGuid();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            ViewData["SuccessMessage"] = "Registration successful! You can now log in.";
            return View();
        }

        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("IsAdmin", user.IsAdmin.ToString())
            };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("CookieAuth", principal);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("User initiated logout.");

            await HttpContext.SignOutAsync("CookieAuth");

            _logger.LogInformation("User successfully logged out.");

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
