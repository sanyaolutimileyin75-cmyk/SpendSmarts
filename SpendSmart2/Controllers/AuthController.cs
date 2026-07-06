using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendSmart2.Data;
using SpendSmart2.Models;
using System.Security.Claims;

namespace SpendSmart2.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Auth/Login
        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users
                .FirstOrDefault(u => u.Email == model.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.FullName!),
        new Claim(ClaimTypes.Email, user.Email!),
        new Claim("ProfilePicture", user.ProfilePicture ?? "")
    };

            var identity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            return RedirectToAction("Index", "Dashboard");
        }


        // GET: /Auth/Register
        public IActionResult Register()
        {
            if (User.Identity!.IsAuthenticated)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check if email already exists
            bool emailExists = _context.Users
                .Any(u => u.Email == model.Email);

            if (emailExists)
            {
                ModelState.AddModelError("", "Email already registered");
                return View(model);
            }

            // Hash the password before saving
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Password = hashedPassword  // ✅ Save hashed password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        // GET: /Auth/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        // GET: /Auth/Profile
        [Authorize]
        public IActionResult Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return RedirectToAction("Login");

            return View(user);
        }

        // POST: /Auth/Profile
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(User model, string? NewPassword, IFormFile? ProfileImage)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return RedirectToAction("Login");

            // Update name
            user.FullName = model.FullName;

            // Update password only if provided
            if (!string.IsNullOrEmpty(NewPassword))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            }

            // ✅ Handle profile image upload
            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                // Validate file size (max 2MB)
                if (ProfileImage.Length > 2 * 1024 * 1024)
                {
                    TempData["Error"] = "Image size must be less than 2MB";
                    return RedirectToAction("Profile");
                }

                // Validate file type
                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                if (!allowedTypes.Contains(ProfileImage.ContentType.ToLower()))
                {
                    TempData["Error"] = "Only JPG, PNG, and GIF images are allowed";
                    return RedirectToAction("Profile");
                }

                // Convert image to Base64 (stored in database)
                using (var ms = new MemoryStream())
                {
                    await ProfileImage.CopyToAsync(ms);
                    var fileBytes = ms.ToArray();
                    var base64 = Convert.ToBase64String(fileBytes);
                    user.ProfilePicture = $"data:{ProfileImage.ContentType};base64,{base64}";
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction("Profile");



        }
    }
}    