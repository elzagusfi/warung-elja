using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarungElja.Data;
using WarungElja.Models;
using WarungElja.Utilities;

namespace WarungElja.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,IT")]
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        [Authorize(Roles = "Admin,IT")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [Authorize(Roles = "Admin,IT")]
        public IActionResult Create()
        {
            var viewModel = new ChangeUserInfoViewModel
            {
                Role = "Kasir"
            };
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,IT")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChangeUserInfoViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == viewModel.Username);
                
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Username already exists.");
                    return View(viewModel);
                }

                if (string.IsNullOrEmpty(viewModel.NewPassword))
                {
                    ModelState.AddModelError("NewPassword", "Password is required.");
                    return View(viewModel);
                }

                if (viewModel.NewPassword != viewModel.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                    return View(viewModel);
                }

                var user = new User
                {
                    Username = viewModel.Username,
                    Name = viewModel.Name,
                    Role = viewModel.Role,
                    PasswordHash = PasswordHasher.HashPassword(viewModel.NewPassword),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Add(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "User created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        [Authorize(Roles = "Admin,IT")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new ChangeUserInfoViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.Name,
                Role = user.Role ?? "Kasir"
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,IT")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ChangeUserInfoViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.Users.FindAsync(id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username == viewModel.Username && u.Id != id);
                    
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Username", "Username already exists.");
                        return View(viewModel);
                    }

                    user.Username = viewModel.Username;
                    user.Name = viewModel.Name;
                    user.Role = viewModel.Role;
                    user.UpdatedAt = DateTime.UtcNow;

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        [Authorize(Roles = "Admin,IT")]
        public async Task<IActionResult> ResetPassword(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new ChangeUserInfoViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.Name
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,IT")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id, ChangeUserInfoViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(viewModel.NewPassword))
            {
                ModelState.AddModelError("", "Password field is required.");
                return View(viewModel);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.Users.FindAsync(id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    user.PasswordHash = PasswordHasher.HashPassword(viewModel.NewPassword);
                    user.UpdatedAt = DateTime.UtcNow;

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["SuccessMessage"] = "Password reset successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        [Authorize(Roles = "Admin,IT")]
        public async Task<IActionResult> Deactivate(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("Deactivate")]
        [Authorize(Roles = "Admin,IT")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                if (user.Role == "Admin")
                {
                    var adminCount = await _context.Users.CountAsync(u => u.Role == "Admin" && u.IsActive);
                    if (adminCount <= 1)
                    {
                        TempData["ErrorMessage"] = "Cannot deactivate the last admin user.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;
                
                _context.Users.Update(user);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "User deactivated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var user = await _context.Users.FindAsync(userId);
            
            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new ChangeUserInfoViewModel
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.Name,
                Role = user.Role ?? "User"
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ChangeUserInfoViewModel viewModel)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            if (userId != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Username == viewModel.Username && u.Id != userId);
                    
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Username", "Username already exists.");
                        return View(viewModel);
                    }

                    if (!string.IsNullOrEmpty(viewModel.NewPassword))
                    {
                        if (string.IsNullOrEmpty(viewModel.CurrentPassword))
                        {
                            ModelState.AddModelError("CurrentPassword", "Current password is required to change password.");
                            return View(viewModel);
                        }

                        if (!PasswordHasher.VerifyPassword(viewModel.CurrentPassword, user.PasswordHash))
                        {
                            ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                            return View(viewModel);
                        }

                        if (viewModel.NewPassword != viewModel.ConfirmPassword)
                        {
                            ModelState.AddModelError("ConfirmPassword", "New password and confirmation do not match.");
                            return View(viewModel);
                        }

                        user.PasswordHash = PasswordHasher.HashPassword(viewModel.NewPassword);
                    }

                    user.Username = viewModel.Username;
                    user.Name = viewModel.Name;
                    user.UpdatedAt = DateTime.UtcNow;

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Profile updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Profile));
            }
            return View(viewModel);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}