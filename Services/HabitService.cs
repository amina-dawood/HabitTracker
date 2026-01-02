using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HabitTracker.Models;
using HabitTracker.Models.Interface;
using Microsoft.Extensions.Logging;

namespace HabitTracker.Services
{
    public class HabitService : IHabitService
    {
        private readonly IHabitRepository _habitRepo;
        private readonly ILogger<HabitService> _logger;

        public HabitService(
            IHabitRepository habitRepo,
            ILogger<HabitService> logger)
        {
            _habitRepo = habitRepo;
            _logger = logger;
        }

        public async Task<Habit?> GetHabitByIdAsync(int id, string userId)
        {
            return await _habitRepo.GetHabitByIdAsync(id, userId);
        }

        public async Task<List<Habit>> GetAllHabitsAsync(string userId)
        {
            var habits = await _habitRepo.GetAllHabitsAsync(userId);
            var today = DateTime.Today;

            foreach (var habit in habits)
            {
                // Logic to reset streak if missed
                if (habit.LastCompletedDate.HasValue)
                {
                    var daysSinceLastCompletion = (today - habit.LastCompletedDate.Value.Date).Days;
                    
                    // Determine allowed gap based on frequency
                    int maxGap = 1; // Default for Daily
                    if (habit.Frequency == "Weekly") maxGap = 8; // Allow up to 8 days for weekly (7 + 1 buffer)
                    
                    // If gap is larger than allowed, streak is broken
                    if (daysSinceLastCompletion > maxGap)
                    {
                        if (habit.CurrentStreak > 0)
                        {
                            habit.CurrentStreak = 0;
                            // Update database to persist the reset
                            await _habitRepo.UpdateHabitAsync(habit);
                        }
                    }
                }
            }

            return habits;
        }

        public async Task<int> CreateHabitAsync(Habit habit)
        {
            try
            {
                _logger.LogInformation("Creating new habit for user {UserId}: {HabitName}", habit.UserId, habit.Name);

                // Validate habit data
                if (string.IsNullOrWhiteSpace(habit.Name))
                {
                    _logger.LogWarning("Habit creation failed: Name is required for user {UserId}", habit.UserId);
                    throw new ArgumentException("Habit name is required");
                }

                if (string.IsNullOrWhiteSpace(habit.Description))
                    habit.Description = "";

                if (string.IsNullOrWhiteSpace(habit.Category))
                    habit.Category = "Other";

                // Validate target days
                if (habit.TargetDays <= 0)
                    habit.TargetDays = 30;

                // Set default values
                habit.CreatedDate = DateTime.Now;
                habit.CurrentStreak = 0;
                habit.LongestStreak = 0;
                habit.IsActive = true;

                int habitId = await _habitRepo.AddHabitAsync(habit);
                _logger.LogInformation("Successfully created habit {HabitId} for user {UserId}", habitId, habit.UserId);

                return habitId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating habit for user {UserId}: {HabitName}", habit.UserId, habit.Name);
                throw new ApplicationException("Failed to create habit. Please try again.", ex);
            }
        }

        public async Task<bool> UpdateHabitAsync(Habit habit)
        {
            // Validate habit data
            if (string.IsNullOrWhiteSpace(habit.Name))
                throw new ArgumentException("Habit name is required");

            // Ensure required fields are not null
            habit.Description ??= "";
            habit.Category ??= "Other";

            return await _habitRepo.UpdateHabitAsync(habit);
        }

        public async Task<bool> DeleteHabitAsync(int id, string userId)
        {
            return await _habitRepo.DeleteHabitAsync(id, userId);
        }

        public async Task<bool> MarkHabitCompleteAsync(int id, string userId)
        {
            try
            {
                _logger.LogInformation("Marking habit {HabitId} as complete for user {UserId}", id, userId);

                var habit = await _habitRepo.GetHabitByIdAsync(id, userId);
                if (habit == null)
                {
                    _logger.LogWarning("Habit {HabitId} not found for user {UserId}", id, userId);
                    return false;
                }

                // Check if already completed today
                if (habit.IsCompletedToday)
                {
                    _logger.LogInformation("Habit {HabitId} already completed today for user {UserId}", id, userId);
                    return false;
                }

                var today = DateTime.Today;
                var yesterday = today.AddDays(-1);

                // Calculate new streak based on LastCompletedDate
                int maxGap = 1;
                if (habit.Frequency == "Weekly") maxGap = 8;

                int newStreak;
                if (habit.LastCompletedDate.HasValue)
                {
                    var daysSinceLastCompletion = (today - habit.LastCompletedDate.Value.Date).Days;
                    if (daysSinceLastCompletion <= maxGap)
                    {
                         // Continued streak
                         newStreak = habit.CurrentStreak + 1;
                    }
                    else
                    {
                        // Broken streak
                        newStreak = 1;
                    }
                }
                else
                {
                    // First time
                    newStreak = 1;
                }

                // Update habit
                habit.LastCompletedDate = today;
                habit.CurrentStreak = newStreak;

                // Update longest streak if current is better
                if (newStreak > habit.LongestStreak)
                {
                    habit.LongestStreak = newStreak;
                }

                var success = await _habitRepo.UpdateHabitAsync(habit);

                if (success)
                {
                    _logger.LogInformation("Successfully marked habit {HabitId} as complete for user {UserId}, current streak: {Streak}", id, userId, newStreak);
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking habit {HabitId} as complete for user {UserId}", id, userId);
                throw new ApplicationException("Failed to mark habit as complete. Please try again.", ex);
            }
        }

        public async Task<bool> IsCompletedTodayAsync(int habitId, string userId)
        {
            var habit = await _habitRepo.GetHabitByIdAsync(habitId, userId);
            return habit?.IsCompletedToday ?? false;
        }

        public async Task<int> GetCurrentStreakAsync(int habitId, string userId)
        {
            var habit = await _habitRepo.GetHabitByIdAsync(habitId, userId);
            return habit?.CurrentStreak ?? 0;
        }

        public async Task<int> GetLongestStreakAsync(int habitId, string userId)
        {
            var habit = await _habitRepo.GetHabitByIdAsync(habitId, userId);
            return habit?.LongestStreak ?? 0;
        }

        public async Task<List<Habit>> GetActiveHabitsAsync(string userId)
        {
            return await _habitRepo.GetActiveHabitsAsync(userId);
        }

        public async Task<Dictionary<string, int>> GetCategoryStatsAsync(string userId)
        {
            return await _habitRepo.GetCategoryStatsAsync(userId);
        }

        public async Task<List<Habit>> GetHabitsByCategoryAsync(string userId, string category)
        {
            return await _habitRepo.GetHabitsByCategoryAsync(userId, category);
        }

        public async Task<bool> UpdateProgressAsync(int id, string userId, int progress)
        {
            // Validate progress range
            progress = Math.Max(0, Math.Min(100, progress));
            return await _habitRepo.UpdateProgressAsync(id, userId, progress);
        }

        public async Task<HabitTracker.Models.ReportViewModel> GetDashboardDataAsync(string userId)
        {
            var habits = await _habitRepo.GetAllHabitsAsync(userId);

            // Calculate today's completion stats
            int completedToday = habits.Count(h => h.IsCompletedToday);

            // Calculate streaks
            int totalCurrentStreak = habits.Sum(h => h.CurrentStreak);
            int longestStreak = habits.Any() ? habits.Max(h => h.LongestStreak) : 0;
            int averageStreak = habits.Any() ? (int)Math.Round((double)totalCurrentStreak / habits.Count) : 0;

            // Get recent completions (habits completed in last 30 days)
            var recentCompletions = new List<HabitCompletionViewModel>();
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            foreach (var habit in habits.Where(h => h.LastCompletedDate >= thirtyDaysAgo))
            {
                if (habit.LastCompletedDate.HasValue)
                {
                    recentCompletions.Add(new HabitCompletionViewModel
                    {
                        HabitName = habit.Name,
                        CompletionDate = habit.LastCompletedDate.Value,
                        Category = "General" // Simplified - no category field
                    });
                }
            }

            recentCompletions = recentCompletions
                .OrderByDescending(c => c.CompletionDate)
                .Take(10)
                .ToList();

            var dashboardData = new HabitTracker.Models.ReportViewModel
            {
                TotalHabits = habits.Count,
                CompletedHabits = completedToday,
                ActiveHabits = habits.Count,
                AverageStreak = averageStreak,
                LongestStreak = longestStreak,
                CategoryStats = new Dictionary<string, int> { { "All Habits", habits.Count } }, // Simplified
                RecentCompletions = recentCompletions,
                PendingHabits = habits.Count - completedToday,
                CompletionRate = habits.Any() ?
                    Math.Round((decimal)completedToday / habits.Count * 100, 1) : 0
            };

            return dashboardData;
        }
    }

}
