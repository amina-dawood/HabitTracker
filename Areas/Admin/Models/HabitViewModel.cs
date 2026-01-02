using System;
using System.Collections.Generic;

namespace HabitTracker.Areas.Admin.Models
{
    public class HabitViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Frequency { get; set; } = "";
        public string Category { get; set; } = "";
        public int Progress { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsActive { get; set; }
        public string UserId { get; set; } = "";
        public string UserEmail { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public DateTime? LastCompletedDate { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
    }

    public class HabitListViewModel
    {
        public List<HabitViewModel> Habits { get; set; } = new List<HabitViewModel>();
        public int TotalHabits { get; set; }
        public int ActiveHabits { get; set; }
        public int CompletedHabits { get; set; }
        public Dictionary<string, int> CategoryStats { get; set; } = new Dictionary<string, int>();
    }
}
