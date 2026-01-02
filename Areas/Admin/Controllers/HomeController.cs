using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HabitTracker.Areas.Admin.Models;
using HabitTracker.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace HabitTracker.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize("AdminOnly")]
    public class HomeController : Controller
    {
        private readonly HabitTrackerContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(HabitTrackerContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin Dashboard
        [HttpGet("/Admin/Admin")]
        [HttpGet("/Admin/Admin/Index")]
        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel();

            // Get basic statistics
            var allUsers = await _userManager.Users.ToListAsync();
            var allHabits = await _context.Habits.ToListAsync();

            model.TotalUsers = allUsers.Count;
            model.TotalHabits = allHabits.Count;
            model.ActiveHabits = 0;
            model.CompletedHabits = 0;

            foreach (var habit in allHabits)
            {
                if (habit.IsCompleted)
                {
                    model.CompletedHabits++;
                }
                if (habit.IsActive)
                {
                    model.ActiveHabits++;
                }
            }

            // Get today's statistics
            var today = DateTime.Today;
            model.NewUsersToday = 0;
            model.NewHabitsToday = 0;

            foreach (var user in allUsers)
            {
                // Note: CreatedDate might not be available in IdentityUser
                // We'll count users registered recently
            }

            foreach (var habit in allHabits)
            {
                if (habit.CreatedDate.Date == today)
                {
                    model.NewHabitsToday++;
                }
            }

            // Get recent users (last 5)
            var recentUsers = allUsers.OrderByDescending(u => u.Id).Take(5);
            foreach (var user in recentUsers)
            {
                model.RecentUsers.Add(new RecentUserViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? "",
                    CreatedDate = DateTime.Now, // Placeholder since IdentityUser doesn't have CreatedDate
                    EmailConfirmed = user.EmailConfirmed
                });
            }

            // Get recent habits (last 5)
            var recentHabits = allHabits.OrderByDescending(h => h.CreatedDate).Take(5);
            foreach (var habit in recentHabits)
            {
                var user = allUsers.FirstOrDefault(u => u.Id == habit.UserId);
                model.RecentHabits.Add(new RecentHabitViewModel
                {
                    Id = habit.Id,
                    Name = habit.Name,
                    UserEmail = user?.Email ?? "Unknown",
                    CreatedDate = habit.CreatedDate,
                    IsCompleted = habit.IsCompleted
                });
            }

            return View(model);
        }

        // GET: All Users
        [HttpGet("/Admin/Admin/Users")]
        public async Task<IActionResult> Users()
        {
            var model = new UserListViewModel();

            var allUsers = await _userManager.Users.ToListAsync();
            var allHabits = await _context.Habits.ToListAsync();

            model.TotalUsers = allUsers.Count;
            model.Users = new List<UserViewModel>();

            foreach (var user in allUsers)
            {
                var userHabits = new List<HabitTracker.Models.Habit>();
                foreach (var habit in allHabits)
                {
                    if (habit.UserId == user.Id)
                    {
                        userHabits.Add(habit);
                    }
                }

                int completedHabits = 0;
                foreach (var habit in userHabits)
                {
                    if (habit.IsCompleted)
                    {
                        completedHabits++;
                    }
                }

                model.Users.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    CreatedDate = DateTime.Now, // Placeholder
                    EmailConfirmed = user.EmailConfirmed,
                    IsActive = true, // Placeholder
                    TotalHabits = userHabits.Count,
                    CompletedHabits = completedHabits
                });

                if (user.EmailConfirmed)
                {
                    model.UsersWithEmailConfirmed++;
                }
            }

            model.ActiveUsers = model.Users.Count(u => u.IsActive);

            return View(model);
        }

        // GET: All Habits
        [HttpGet("/Admin/Admin/Habits")]
        public async Task<IActionResult> Habits()
        {
            var model = new HabitListViewModel();

            var allHabits = await _context.Habits.ToListAsync();
            var allUsers = await _userManager.Users.ToListAsync();

            model.TotalHabits = allHabits.Count;
            model.Habits = new List<HabitViewModel>();

            foreach (var habit in allHabits)
            {
                var user = allUsers.FirstOrDefault(u => u.Id == habit.UserId);

                model.Habits.Add(new HabitViewModel
                {
                    Id = habit.Id,
                    Name = habit.Name,
                    Description = habit.Description,
                    Frequency = habit.Frequency,
                    Category = habit.Category,
                    Progress = habit.Progress,
                    IsCompleted = habit.IsCompleted,
                    IsActive = habit.IsActive,
                    UserId = habit.UserId,
                    UserEmail = user?.Email ?? "Unknown User",
                    CreatedDate = habit.CreatedDate,
                    LastCompletedDate = habit.LastCompletedDate,
                    CurrentStreak = habit.CurrentStreak,
                    LongestStreak = habit.LongestStreak
                });

                if (habit.IsCompleted)
                {
                    model.CompletedHabits++;
                }
                if (habit.IsActive)
                {
                    model.ActiveHabits++;
                }

                // Count categories
                if (model.CategoryStats.ContainsKey(habit.Category))
                {
                    model.CategoryStats[habit.Category]++;
                }
                else
                {
                    model.CategoryStats[habit.Category] = 1;
                }
            }

            return View(model);
        }

        // GET: User Details
        public async Task<IActionResult> UserDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userHabits = await _context.Habits.Where(h => h.UserId == id).ToListAsync();

            ViewBag.UserId = user.Id;
            ViewBag.Email = user.Email ?? "";
            ViewBag.UserName = user.UserName ?? "";
            ViewBag.EmailConfirmed = user.EmailConfirmed;
            ViewBag.IsActive = true; // Placeholder
            ViewBag.CreatedDate = DateTime.Now; // Placeholder
            ViewBag.TotalHabits = userHabits.Count;
            ViewBag.CompletedHabits = userHabits.Count(h => h.IsCompleted);
            ViewBag.ActiveHabits = userHabits.Count(h => h.IsActive);
            ViewBag.Habits = userHabits;

            return View();
        }

        // POST: Toggle User Active Status
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "User ID is required" });
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            // Toggle suspension using Identity lockout
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.Now)
            {
                user.LockoutEnd = null; // Activate user
            }
            else
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue; // Suspend user indefinitely
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                return Json(new { success = false, message = "Failed to update user status" });
            }

            return Json(new { success = true, message = "User status updated successfully" });
        }

        // POST: Verify User (admin confirms email)
        [HttpPost]
        public async Task<IActionResult> VerifyUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "User ID is required" });
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "User verified successfully" });
            }

            return Json(new { success = false, message = "Failed to verify user" });
        }

        // POST: Unverify User (block login until verified again)
        [HttpPost]
        public async Task<IActionResult> UnverifyUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "User ID is required" });
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            user.EmailConfirmed = false;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "User unverified successfully" });
            }

            return Json(new { success = false, message = "Failed to unverify user" });
        }

        // GET: Edit User
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.UserId = user.Id;
            ViewBag.Email = user.Email ?? "";
            ViewBag.UserName = user.UserName ?? "";
            ViewBag.EmailConfirmed = user.EmailConfirmed;

            return View();
        }

        // POST: Edit User
        [HttpPost]
        public async Task<IActionResult> EditUser(string userId, string email, string userName, bool emailConfirmed)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.Email = email;
            user.UserName = userName;
            user.EmailConfirmed = emailConfirmed;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "User updated successfully";
                return RedirectToAction("Users");
            }

            TempData["Error"] = "Failed to update user";
            return RedirectToAction("Users");
        }

    }
}
