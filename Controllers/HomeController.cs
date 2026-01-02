using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HabitTracker.Models;
using HabitTracker.Models.Interface;

namespace HabitTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHabitService _habitService;

        public HomeController(IHabitService habitService)
        {
            _habitService = habitService;
        }

        // Homepage - everyone can access
        public IActionResult Index()
        {
            return View();
        }

        // Dashboard - only logged-in users
        [Authorize]
        //public IActionResult Dashboard()
        //{
        //    string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //    // Get user's habits
        //    var habits = _habitRepo.GetAllHabits(userId);

        //    // Calculate stats
        //    int completed = habits.Count(h => h.IsCompleted);
        //    int pending = habits.Count(h => !h.IsCompleted);
        //    int streak = CalculateStreak(habits);

        //    // Pass data to view
        //    ViewBag.UserName = User.Identity?.Name?.Split('@')[0] ?? "User";
        //    ViewBag.CompletedHabits = completed;
        //    ViewBag.PendingHabits = pending;
        //    ViewBag.Streak = streak;
        //    ViewBag.RecentHabits = habits.Take(5).ToList(); // Last 5 habits

        //    return View();
        //}
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Redirect("/Identity/Account/Login");
            }

            var habits = await _habitService.GetAllHabitsAsync(userId);

            // Calculate statistics
            ViewBag.TotalHabits = habits.Count;
            ViewBag.ActiveHabits = habits.Count(h => h.IsActive);
            ViewBag.CompletedToday = habits.Count(h => h.IsCompletedToday);
            ViewBag.CompletedHabits = ViewBag.CompletedToday; // Alias for dashboard view
            ViewBag.TotalStreaks = habits.Count(h => h.CurrentStreak > 0);
            ViewBag.AchievedHabits = habits.Count(h => h.IsAchieved);
            
            ViewBag.SuccessRate = habits.Any(h => h.IsActive) ?
                (int)Math.Round((double)ViewBag.CompletedToday / habits.Count(h => h.IsActive) * 100) : 0;

            return View(habits);
        }
        // Simple streak calculation
        private int CalculateStreak(List<Habit> habits)
        {
            // For simplicity, count completed habits in last 7 days
            int recentCompleted = 0;
            var sevenDaysAgo = DateTime.Now.AddDays(-7);

            foreach (var habit in habits)
            {
                if (habit.IsCompleted && habit.CreatedDate >= sevenDaysAgo)
                {
                    recentCompleted++;
                }
            }

            return Math.Min(recentCompleted, 7); // Max 7 day streak
        }
    }
}