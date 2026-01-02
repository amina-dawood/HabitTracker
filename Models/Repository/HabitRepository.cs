using Dapper;
using HabitTracker.Models;
using HabitTracker.Models.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace HabitTracker.Models.Repository
{
    public class HabitRepository : IHabitRepository
    {
        private readonly IConfiguration _config;

        public HabitRepository(IConfiguration config)
        {
            _config = config;
        }  

        // Get connection string
        private string GetConnectionString()
        {
            return _config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string not found in configuration");
        }


        // 2. Get single habit by ID
        public Habit? GetHabitById(int id, string userId)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                string sql = "SELECT * FROM Habits WHERE Id = @Id AND UserId = @UserId";
                return connection.QueryFirstOrDefault<Habit>(sql, new { Id = id, UserId = userId });
            }
        }

        //// 3. Add new habit
        //public int AddHabit(Habit habit)
        //{
        //    using (var connection = new SqlConnection(GetConnectionString()))
        //    {
        //        string sql = @"
        //            INSERT INTO Habits (Name, Frequency, Progress, IsCompleted, UserId, CreatedDate) 
        //            VALUES (@Name, @Frequency, @Progress, @IsCompleted, @UserId, @CreatedDate);
        //            SELECT CAST(SCOPE_IDENTITY() as int)";

        //        return connection.ExecuteScalar<int>(sql, habit);
        //    }
        //}
        // Add new habit
        public int AddHabit(Habit habit)
        {
            // Ensure required fields are not null
            if (habit.Description == null) habit.Description = "";
            if (habit.Category == null) habit.Category = "Other";

            using (var connection = new SqlConnection(GetConnectionString()))
            {
                // Insert habit with all fields
                string sql = @"
                INSERT INTO Habits
                (Name, Description, Frequency, TargetDays, Category, UserId, CreatedDate, CurrentStreak, LongestStreak, IsActive, Progress, IsCompleted)
                VALUES
                (@Name, @Description, @Frequency, @TargetDays, @Category, @UserId, @CreatedDate, @CurrentStreak, @LongestStreak, @IsActive, 0, 0);

                SELECT CAST(SCOPE_IDENTITY() as int)";

                return connection.ExecuteScalar<int>(sql, habit);
            }
        }

        // 4. Update habit
        public bool UpdateHabit(Habit habit)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                string sql = @"
                    UPDATE Habits
                    SET Name = @Name,
                        Description = @Description,
                        Frequency = @Frequency,
                        TargetDays = @TargetDays,
                        Category = @Category,
                        IsActive = @IsActive,
                        LastCompletedDate = @LastCompletedDate,
                        CurrentStreak = @CurrentStreak,
                        LongestStreak = @LongestStreak,
                        Progress = @Progress
                    WHERE Id = @Id AND UserId = @UserId";

                int rowsAffected = connection.Execute(sql, habit);
                return rowsAffected > 0;
            }
        }

        // 5. Delete habit
        public bool DeleteHabit(int id, string userId)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                string sql = "DELETE FROM Habits WHERE Id = @Id AND UserId = @UserId";
                int rowsAffected = connection.Execute(sql, new { Id = id, UserId = userId });
                return rowsAffected > 0;
            }
        }

        // 6. Mark habit as complete
        public bool MarkAsComplete(int id, string userId)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                string sql = @"
                    UPDATE Habits
                    SET IsCompleted = 1,
                        Progress = 100,
                        LastCompletedDate = GETDATE()
                    WHERE Id = @Id AND UserId = @UserId";

                int rowsAffected = connection.Execute(sql, new { Id = id, UserId = userId });
                return rowsAffected > 0;
            }
        }

        // Update progress (legacy method - habits are now daily recurring)
        public bool UpdateProgress(int id, string userId, int progress)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                string sql = @"
                    UPDATE Habits
                    SET Progress = @Progress
                    WHERE Id = @Id AND UserId = @UserId";

                int rowsAffected = connection.Execute(sql, new
                {
                    Id = id,
                    UserId = userId,
                    Progress = progress
                });

                return rowsAffected > 0;
            }
        }

        // ASYNC METHODS

        // 1. Get all habits for a user - Async
        public async Task<List<Habit>> GetAllHabitsAsync(string userId)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                string sql = "SELECT * FROM Habits WHERE UserId = @UserId ORDER BY CreatedDate DESC";
                var habits = await connection.QueryAsync<Habit>(sql, new { UserId = userId });
                return habits.ToList();
            }
        }

        // Helper method to ensure all streaks are updated
        // private async Task EnsureStreaksAreUpdatedAsync(string userId)
        // {
        //    Legacy method removed for simplicity
        // }

        // Sync version for backward compatibility
        public List<Habit> GetAllHabits(string userId)
        {
            return GetAllHabitsAsync(userId).Result;
        }

        // 2. Get single habit by ID - Async
        public async Task<Habit?> GetHabitByIdAsync(int id, string userId)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                string sql = "SELECT * FROM Habits WHERE Id = @Id AND UserId = @UserId";
                return await connection.QueryFirstOrDefaultAsync<Habit>(sql, new { Id = id, UserId = userId });
            }
        }

        // 3. Add new habit - Async
        public async Task<int> AddHabitAsync(Habit habit)
        {
            // Ensure required fields are not null
            if (habit.Description == null) habit.Description = "";
            if (habit.Category == null) habit.Category = "Other";

            using (var connection = new SqlConnection(GetConnectionString()))
            {
                // Insert habit with all fields
                string sql = @"
                INSERT INTO Habits
                (Name, Description, Frequency, TargetDays, Category, UserId, CreatedDate, CurrentStreak, LongestStreak, IsActive, Progress, IsCompleted)
                VALUES
                (@Name, @Description, @Frequency, @TargetDays, @Category, @UserId, @CreatedDate, @CurrentStreak, @LongestStreak, @IsActive, 0, 0);

                SELECT CAST(SCOPE_IDENTITY() as int)";

                return await connection.ExecuteScalarAsync<int>(sql, habit);
            }
        }

        // 4. Update habit - Async
        public async Task<bool> UpdateHabitAsync(Habit habit)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                string sql = @"
                    UPDATE Habits
                    SET Name = @Name,
                        Description = @Description,
                        Frequency = @Frequency,
                        TargetDays = @TargetDays,
                        Category = @Category,
                        IsActive = @IsActive,
                        LastCompletedDate = @LastCompletedDate,
                        CurrentStreak = @CurrentStreak,
                        LongestStreak = @LongestStreak,
                        Progress = @Progress
                    WHERE Id = @Id AND UserId = @UserId";

                int rowsAffected = await connection.ExecuteAsync(sql, habit);
                return rowsAffected > 0;
            }
        }

        // 5. Delete habit - Async
        public async Task<bool> DeleteHabitAsync(int id, string userId)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                string sql = "DELETE FROM Habits WHERE Id = @Id AND UserId = @UserId";
                int rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, UserId = userId });
                return rowsAffected > 0;
            }
        }

        // 6. Mark habit as complete - Async
        public async Task<bool> MarkAsCompleteAsync(int id, string userId)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                string sql = @"
                    UPDATE Habits
                    SET IsCompleted = 1,
                        Progress = 100
                    WHERE Id = @Id AND UserId = @UserId";

                int rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, UserId = userId });
                return rowsAffected > 0;
            }
        }

        // Update progress - Async (legacy method - habits are now daily recurring)
        public async Task<bool> UpdateProgressAsync(int id, string userId, int progress)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                string sql = @"
                    UPDATE Habits
                    SET Progress = @Progress
                    WHERE Id = @Id AND UserId = @UserId";

                int rowsAffected = await connection.ExecuteAsync(sql, new
                {
                    Id = id,
                    UserId = userId,
                    Progress = progress
                });

                return rowsAffected > 0;
            }
        }

        // Get habits by category - Async
        public async Task<List<Habit>> GetHabitsByCategoryAsync(string userId, string category)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                string sql = "SELECT * FROM Habits WHERE UserId = @UserId AND Category = @Category AND IsActive = 1 ORDER BY CreatedDate DESC";
                var habits = await connection.QueryAsync<Habit>(sql, new { UserId = userId, Category = category });
                return habits.ToList();
            }
        }

        // Get active habits - Async
        public async Task<List<Habit>> GetActiveHabitsAsync(string userId)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                string sql = "SELECT * FROM Habits WHERE UserId = @UserId AND IsActive = 1 ORDER BY CreatedDate DESC";
                var habits = await connection.QueryAsync<Habit>(sql, new { UserId = userId });
                return habits.ToList();
            }
        }

        // Update streak when habit is completed
        public async Task<bool> UpdateStreakAsync(int id, string userId)
        {
             // Legacy method - functionality moved to Service layer
             return await Task.FromResult(true);
        }

        /* Legacy helper methods removed
        private async Task<int> CalculateCurrentStreakFromCompletionsAsync(int habitId, string userId) ...
        private async Task<int> CalculateLongestStreakFromCompletionsAsync(int habitId, string userId) ...
        private async Task<DateTime?> GetLastCompletionDateAsync(int habitId, string userId) ...
        private async Task<int> FixExistingCompletedHabitsAsync(string userId) ...
        */

        // Simple class for category statistics query results
        private class CategoryStatResult
        {
            public string Category { get; set; } = string.Empty;
            public int Count { get; set; }
        }

        // Get category statistics
        public async Task<Dictionary<string, int>> GetCategoryStatsAsync(string userId)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                try
                {
                    // Try with new columns first (after migration)
                    string sql = @"
                        SELECT Category, COUNT(*) as Count
                        FROM Habits
                        WHERE UserId = @UserId AND IsActive = 1
                        GROUP BY Category
                        ORDER BY Count DESC";

                    var results = await connection.QueryAsync<CategoryStatResult>(sql, new { UserId = userId });
                    var dict = new Dictionary<string, int>();
                    foreach (var result in results)
                    {
                        dict[result.Category] = result.Count;
                    }
                    return dict;
                }
                catch
                {
                    // Fallback for old database schema (before migration)
                    string sql = @"
                        SELECT 'All Habits' as Category, COUNT(*) as Count
                        FROM Habits
                        WHERE UserId = @UserId";

                    var results = await connection.QueryAsync<CategoryStatResult>(sql, new { UserId = userId });
                    var dict = new Dictionary<string, int>();
                    foreach (var result in results)
                    {
                        dict[result.Category] = result.Count;
                    }
                    return dict;
                }
            }
        }

    }  
}