using System.Collections.Generic;
using System.Threading.Tasks;
using HabitTracker.Models;

namespace HabitTracker.Models.Interface
{
    public interface IHabitService
    {
        // Habit CRUD operations
        Task<Habit?> GetHabitByIdAsync(int id, string userId);
        Task<List<Habit>> GetAllHabitsAsync(string userId);
        Task<int> CreateHabitAsync(Habit habit);
        Task<bool> UpdateHabitAsync(Habit habit);
        Task<bool> DeleteHabitAsync(int id, string userId);

        // Habit completion operations
        Task<bool> MarkHabitCompleteAsync(int id, string userId);
        Task<bool> IsCompletedTodayAsync(int habitId, string userId);

        // Streak calculations
        Task<int> GetCurrentStreakAsync(int habitId, string userId);
        Task<int> GetLongestStreakAsync(int habitId, string userId);

        // Statistics and reporting
        Task<List<Habit>> GetActiveHabitsAsync(string userId);
        Task<Dictionary<string, int>> GetCategoryStatsAsync(string userId);
        Task<List<Habit>> GetHabitsByCategoryAsync(string userId, string category);

        // Progress operations
        Task<bool> UpdateProgressAsync(int id, string userId, int progress);

        // Dashboard data
        Task<ReportViewModel> GetDashboardDataAsync(string userId);
    }
}
