using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using HabitTracker.Models;
using HabitTracker.Models.Interface;
using HabitTracker.Services;

namespace HabitTracker.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IHabitService _habitService;

        public ReportsController(IHabitService habitService)
        {
            _habitService = habitService;
        }

        // GET: Reports Dashboard
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Redirect("/Identity/Account/Login");
            }

            // Use the service to get dashboard data
            var reportData = await _habitService.GetDashboardDataAsync(userId);

            return View(reportData);
        }

        // GET: Category-wise report
        public async Task<IActionResult> Categories()
        {
            var userId = GetCurrentUserId();
            var categoryStats = await _habitService.GetCategoryStatsAsync(userId);
            return View(categoryStats);
        }

        // GET: Streak report
        public async Task<IActionResult> Streaks()
        {
            var userId = GetCurrentUserId();

            var habits = await _habitService.GetAllHabitsAsync(userId);

            Console.WriteLine($"Streaks page - User {userId} has {habits.Count} habits for streak analysis");

            // Include all habits, not just active ones, for complete streak analysis
            var streakData = new List<StreakViewModel>();

            foreach (var habit in habits)
            {
                streakData.Add(new StreakViewModel
                {
                    HabitName = habit.Name,
                    CurrentStreak = habit.CurrentStreak,
                    LongestStreak = habit.LongestStreak,
                    LastCompletedDate = habit.LastCompletedDate
                });
            }

            // Simple sort by current streak (descending), then by longest streak (descending)
            for (int i = 0; i < streakData.Count - 1; i++)
            {
                for (int j = 0; j < streakData.Count - i - 1; j++)
                {
                    bool shouldSwap = false;

                    if (streakData[j].CurrentStreak < streakData[j + 1].CurrentStreak)
                    {
                        shouldSwap = true;
                    }
                    else if (streakData[j].CurrentStreak == streakData[j + 1].CurrentStreak &&
                             streakData[j].LongestStreak < streakData[j + 1].LongestStreak)
                    {
                        shouldSwap = true;
                    }

                    if (shouldSwap)
                    {
                        var temp = streakData[j];
                        streakData[j] = streakData[j + 1];
                        streakData[j + 1] = temp;
                    }
                }
            }

            return View(streakData);
        }

        // Helper method to get current user ID
        private string GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return userId;
        }

    }
}