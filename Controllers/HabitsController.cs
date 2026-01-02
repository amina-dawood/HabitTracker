
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using HabitTracker.Models;
using HabitTracker.Models.Interface;
using System.Threading.Tasks;
using HabitTracker.Services;
using System.Linq;

namespace HabitTracker.Controllers
{
    [Authorize]
    public class HabitsController : Controller
    {
        private readonly IHabitService _habitService;
        private readonly ILogger<HabitsController> _logger;

        public HabitsController(IHabitService habitService, ILogger<HabitsController> logger)
        {
            _habitService = habitService;
            _logger = logger;
        }

        // GET: View all habits
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> ViewAll(string? category = null)
        {
            string userId = GetCurrentUserId();
            var habits = await _habitService.GetAllHabitsAsync(userId);

            // Calculate statistics for ViewBag
            ViewBag.TotalHabits = habits.Count;
            ViewBag.ActiveHabits = habits.Count(h => h.IsActive);
            ViewBag.CompletedToday = habits.Count(h => h.IsCompletedToday);
            ViewBag.TodayCompletionRate = habits.Any(h => h.IsActive) ?
                (int)Math.Round((double)ViewBag.CompletedToday / habits.Count(h => h.IsActive) * 100) : 0;
            ViewBag.Categories = habits.Select(h => h.Category).Distinct().ToList();

            return View(habits);
        }

        // GET: Show create form
        public IActionResult Create()
        {
            return View();
        }

        // POST: Mark habit as complete for today (daily tracking)
        [HttpPost]
        public async Task<IActionResult> MarkComplete(int id)
        {
            var userId = GetCurrentUserId();

            try
            {
                _logger.LogInformation("User {UserId} attempting to mark habit {HabitId} as complete", userId, id);

                // Check if habit exists and belongs to user
                var habit = await _habitService.GetHabitByIdAsync(id, userId);
                if (habit == null)
                {
                    _logger.LogWarning("User {UserId} attempted to complete non-existent habit {HabitId}", userId, id);
                    return Json(new { success = false, message = "Habit not found or access denied" });
                }

                // Check if already completed today
                if (habit.IsCompletedToday)
                {
                    _logger.LogInformation("User {UserId} attempted to complete habit {HabitId} that was already completed today", userId, id);
                    return Json(new { success = false, message = "Habit already completed today!" });
                }

                // Mark habit as completed using service
                bool completionSuccess = await _habitService.MarkHabitCompleteAsync(id, userId);

                if (completionSuccess)
                {
                    // Get updated habit data
                    var updatedHabit = await _habitService.GetHabitByIdAsync(id, userId);

                    _logger.LogInformation("User {UserId} successfully completed habit {HabitId}", userId, id);

                    return Json(new {
                        success = true,
                        message = "Habit completed for today! 🎉",
                        currentStreak = updatedHabit?.CurrentStreak ?? 0,
                        longestStreak = updatedHabit?.LongestStreak ?? 0,
                        targetDays = updatedHabit?.TargetDays ?? 0,
                        progressPercentage = updatedHabit?.ProgressPercentage ?? 0,
                        todayCompleted = true
                    });
                }

                _logger.LogWarning("Failed to mark habit {HabitId} as complete for user {UserId}", id, userId);
                return Json(new { success = false, message = "Failed to mark habit as complete. Please try again." });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error marking habit {HabitId} complete for user {UserId}: {Message}", id, userId, ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Application error marking habit {HabitId} complete for user {UserId}", id, userId);
                return Json(new { success = false, message = "Unable to complete habit right now. Please try again in a moment." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error marking habit {HabitId} complete for user {UserId}", id, userId);
                return Json(new { success = false, message = "An unexpected error occurred. Please try again." });
            }
        }

        // POST: Delete habit (simple)
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                string userId = GetCurrentUserId();
                bool success = await _habitService.DeleteHabitAsync(id, userId);

                if (success)
                {
                    return Json(new { success = true, message = "Habit deleted successfully!" });
                }

                return Json(new { success = false, message = "Habit not found" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while deleting the habit. Please try again." });
            }
        }

        // POST: Update progress (simple)
        [HttpPost]
        public async Task<IActionResult> UpdateProgress(int id, int progress)
        {
            try
            {
                string userId = GetCurrentUserId();
                bool success = await _habitService.UpdateProgressAsync(id, userId, progress);

                if (success)
                {
                    return Json(new { success = true, message = "Progress updated to " + progress + "%" });
                }

                return Json(new { success = false, message = "Failed to update progress" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while updating progress. Please try again." });
            }
        }

        // POST: Create habit (AJAX submission)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAjax([FromForm] CreateHabitViewModel model)
        {
            _logger.LogInformation("User {UserId} attempting to create habit: {HabitName}", GetCurrentUserId(), model?.Name);

            // Model validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Habit creation validation failed for user {UserId}: {Errors}", GetCurrentUserId(), string.Join(", ", errors));
                return Json(new { success = false, message = "Validation failed: " + string.Join(", ", errors) });
            }

            if (model == null)
            {
                _logger.LogWarning("Habit creation failed: Model is null for user {UserId}", GetCurrentUserId());
                return Json(new { success = false, message = "Invalid habit data" });
            }

            try
            {
                // Create habit object
                var habit = new Habit
                {
                    Name = model.Name.Trim(),
                    Description = model.Description?.Trim() ?? "",
                    Frequency = model.Frequency,
                    TargetDays = model.TargetDays,
                    Category = model.Category?.Trim() ?? "Other",
                    UserId = GetCurrentUserId(),
                    CreatedDate = DateTime.Now,
                    CurrentStreak = 0,
                    LongestStreak = 0,
                    IsActive = true
                };

                // Save to database using service
                int habitId = await _habitService.CreateHabitAsync(habit);
                _logger.LogInformation("Successfully created habit {HabitId} for user {UserId}", habitId, GetCurrentUserId());

                return Json(new { success = true, message = "Habit created successfully!" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error creating habit for user {UserId}: {Message}", GetCurrentUserId(), ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Application error creating habit for user {UserId}", GetCurrentUserId());
                return Json(new { success = false, message = "Unable to create habit right now. Please try again." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating habit for user {UserId}", GetCurrentUserId());
                return Json(new { success = false, message = "An unexpected error occurred. Please try again." });
            }
        }

        // POST: Create habit (form submission - fallback)
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateHabitViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Create habit object
                var habit = new Habit
                {
                    Name = model.Name.Trim(),
                    Description = model.Description?.Trim() ?? "",
                    Frequency = model.Frequency,
                    TargetDays = model.TargetDays,
                    Category = model.Category?.Trim() ?? "Other",
                    UserId = GetCurrentUserId(),
                    CreatedDate = DateTime.Now,
                    CurrentStreak = 0,
                    LongestStreak = 0,
                    IsActive = true
                };

                // Save to database using service
                int habitId = await _habitService.CreateHabitAsync(habit);

                TempData["Success"] = "Habit created successfully!";
                return RedirectToAction("ViewAll");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating habit");
                // Show specific error for debugging
                TempData["Error"] = $"Error: {ex.Message}";
                return View(model);
            }
        }

        // GET: Show edit form
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                string userId = GetCurrentUserId();
                var habit = await _habitService.GetHabitByIdAsync(id, userId);

                if (habit == null)
                {
                    _logger.LogWarning("User {UserId} attempted to edit non-existent habit {HabitId}", userId, id);
                    TempData["Error"] = "Habit not found";
                    return RedirectToAction("ViewAll");
                }

                var model = new EditHabitViewModel
                {
                    Id = habit.Id,
                    Name = habit.Name,
                    Description = habit.Description,
                    Frequency = habit.Frequency,
                    TargetDays = habit.TargetDays,
                    Category = habit.Category,
                    Progress = habit.Progress
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading habit {HabitId} for editing by user {UserId}", id, GetCurrentUserId());
                TempData["Error"] = "An error occurred while loading the habit";
                return RedirectToAction("ViewAll");
            }
        }

        // POST: Save habit changes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditHabitViewModel model)
        {
            _logger.LogInformation("User {UserId} attempting to update habit {HabitId}", GetCurrentUserId(), model?.Id);

            // Model validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                _logger.LogWarning("Habit update validation failed for user {UserId}: {Errors}", GetCurrentUserId(), string.Join(", ", errors));
                TempData["Error"] = "Validation failed: " + string.Join(", ", errors);
                return View(model);
            }

            if (model == null)
            {
                _logger.LogWarning("Habit update failed: Model is null for user {UserId}", GetCurrentUserId());
                TempData["Error"] = "Invalid habit data";
                return RedirectToAction("ViewAll");
            }

            try
            {
                string userId = GetCurrentUserId();
                var existingHabit = await _habitService.GetHabitByIdAsync(model.Id, userId);

                if (existingHabit == null)
                {
                    _logger.LogWarning("User {UserId} attempted to update non-existent habit {HabitId}", userId, model.Id);
                    TempData["Error"] = "Habit not found";
                    return RedirectToAction("ViewAll");
                }

                // Update habit properties
                existingHabit.Name = model.Name.Trim();
                existingHabit.Description = model.Description?.Trim() ?? "";
                existingHabit.Frequency = model.Frequency;
                existingHabit.TargetDays = model.TargetDays;
                existingHabit.Category = model.Category?.Trim() ?? "Other";
                existingHabit.Progress = Math.Max(0, Math.Min(100, model.Progress));

                // Save changes
                bool success = await _habitService.UpdateHabitAsync(existingHabit);

                if (success)
                {
                    _logger.LogInformation("Successfully updated habit {HabitId} for user {UserId}", model.Id, userId);
                    TempData["Success"] = "Habit updated successfully!";
                    return RedirectToAction("ViewAll");
                }
                else
                {
                    _logger.LogWarning("Failed to update habit {HabitId} for user {UserId}", model.Id, userId);
                    TempData["Error"] = "Failed to update habit";
                    return View(model);
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Validation error updating habit {HabitId} for user {UserId}: {Message}", model.Id, GetCurrentUserId(), ex.Message);
                TempData["Error"] = ex.Message;
                return View(model);
            }
            catch (ApplicationException ex)
            {
                _logger.LogError(ex, "Application error updating habit {HabitId} for user {UserId}", model.Id, GetCurrentUserId());
                TempData["Error"] = "Unable to update habit right now. Please try again.";
                return RedirectToAction("ViewAll");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating habit {HabitId} for user {UserId}", model.Id, GetCurrentUserId());
                TempData["Error"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction("ViewAll");
            }
        }

        // GET HABITS as JSON for AJAX
        [HttpGet]
        public async Task<IActionResult> GetHabits()
        {
            string userId = GetCurrentUserId();
            var habits = await _habitService.GetAllHabitsAsync(userId);
            return Json(habits);
        }

        // POST: Check if habit is completed today
        [HttpPost]
        public async Task<IActionResult> IsCompletedToday(int habitId)
        {
            try
            {
                string userId = GetCurrentUserId();
                bool isCompleted = await _habitService.IsCompletedTodayAsync(habitId, userId);

                return Json(new { isCompleted });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking completion status: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { isCompleted = false, error = ex.Message });
            }
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

