using System;
using System.Collections.Generic;

namespace HabitTracker.Areas.Admin.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalHabits { get; set; }
        public int ActiveHabits { get; set; }
        public int CompletedHabits { get; set; }
        public int NewUsersToday { get; set; }
        public int NewHabitsToday { get; set; }
        public List<RecentUserViewModel> RecentUsers { get; set; } = new List<RecentUserViewModel>();
        public List<RecentHabitViewModel> RecentHabits { get; set; } = new List<RecentHabitViewModel>();
    }

    public class RecentUserViewModel
    {
        public string UserId { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public bool EmailConfirmed { get; set; }
    }

    public class RecentHabitViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}
