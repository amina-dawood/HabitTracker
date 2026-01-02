using HabitTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace HabitTracker.Models.Interface
{
    public interface IHabitRepository
    {
        // Basic CRUD operations - Async versions
        Task<List<Habit>> GetAllHabitsAsync(string userId);
        Task<Habit?> GetHabitByIdAsync(int id, string userId);
        Task<int> AddHabitAsync(Habit habit);
        Task<bool> UpdateHabitAsync(Habit habit);
        Task<bool> DeleteHabitAsync(int id, string userId);
        Task<bool> UpdateProgressAsync(int id, string userId, int progress);
        // Status update
        Task<bool> MarkAsCompleteAsync(int id, string userId);

        // Additional methods for enhanced features
        Task<List<Habit>> GetHabitsByCategoryAsync(string userId, string category);
        Task<List<Habit>> GetActiveHabitsAsync(string userId);
        Task<bool> UpdateStreakAsync(int id, string userId);
        Task<Dictionary<string, int>> GetCategoryStatsAsync(string userId);

        // Keep sync versions for backward compatibility (optional)
        List<Habit> GetAllHabits(string userId);
        Habit? GetHabitById(int id, string userId);
        int AddHabit(Habit habit);
        bool UpdateHabit(Habit habit);
        bool DeleteHabit(int id, string userId);
        bool UpdateProgress(int id, string userId, int progress);
        bool MarkAsComplete(int id, string userId);
    }
}