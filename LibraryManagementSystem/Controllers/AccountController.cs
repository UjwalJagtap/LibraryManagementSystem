using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Models; // Include ViewModel namespaces
using LibraryManagementSystem.Data; // Include your DbContext namespace
using BCrypt.Net; // For password hashing
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace LibraryManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly LibraryContext _context;

        public AccountController(LibraryContext context)
        {
            _context = context;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check if the username or email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "Username or Email already exists.");
                return View(model);
            }

            // Hash the password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            // Create the new user
            var newUser = new User
            {
                Username = model.Username,
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Role = model.Role,
                PasswordHash = hashedPassword
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration successful! You can now log in.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("Index", "Home");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Role == model.Role);

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                TempData["ErrorMessage"] = "Invalid username, password, or role.";
                return RedirectToAction("Index", "Home");
            }
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role)
    };

            // Create claims identity
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // Sign in the user
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // Store Role and UserId in the session on successful login
            HttpContext.Session.SetString("Role", user.Role); // Storing Role
            HttpContext.Session.SetInt32("UserId", user.UserId); // Storing UserId


            // Redirect based on role
            if (user.Role == "Student")
                return RedirectToAction("Dashboard", "Student");
            else if (user.Role == "Librarian")
                return RedirectToAction("Dashboard", "Admin");

            TempData["ErrorMessage"] = "Unknown role.";
            return RedirectToAction("Index", "Home");
        }
        
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Clear session data
            return RedirectToAction("Index", "Home"); // Redirect to Home page
        }


    }
}
