using Microsoft.AspNetCore.Mvc;
using NoteList.ViewModels;
using NoteList.Data;
using NoteList.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace NoteList.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if username or email already exists
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Email);

                    if (existingUser != null)
                    {
                        if (existingUser.Username == model.Username)
                            ModelState.AddModelError("Username", "Username is already taken.");
                        if (existingUser.Email == model.Email)
                            ModelState.AddModelError("Email", "Email is already registered.");

                        return View(model);
                    }

                    var user = new User
                    {
                        Username = model.Username,
                        Email = model.Email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password)
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Registration successful! You can now log in.";
                    return RedirectToAction("Login");
                }
                catch (Exception)
                {
                    // TODO: Log the exception (ex) here
                    ModelState.AddModelError(string.Empty, "An error occurred while processing your registration. Please try again later.");
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            // If user is already authenticated, redirect to Home
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username == model.Username);

                    if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                    {
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.Username),
                            new Claim(ClaimTypes.Email, user.Email)
                        };

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                        return RedirectToAction("Index", "Home");
                    }

                    ModelState.AddModelError(string.Empty, "Invalid username or password.");
                }
                catch (Exception)
                {
                    // TODO: Log error
                    ModelState.AddModelError(string.Empty, "An error occurred during log in. Please try again later.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
