using System;
using System.Collections.Generic;

namespace HabitTracker.Models
{
    public class ReportViewModel
    {
        public int TotalHabits { get; set; }
        public int CompletedHabits { get; set; }
        public int ActiveHabits { get; set; }
        public int AverageStreak { get; set; }
        public int LongestStreak { get; set; }
        public Dictionary<string, int> CategoryStats { get; set; } = new Dictionary<string, int>();
        public List<HabitCompletionViewModel> RecentCompletions { get; set; } = new List<HabitCompletionViewModel>();
        public int PendingHabits { get; set; }
        public decimal CompletionRate { get; set; }
    }

    public class HabitCompletionViewModel
    {
        public string? HabitName { get; set; }
        public DateTime CompletionDate { get; set; }
        public string? Category { get; set; }
    }

    public class StreakViewModel
    {
        public string? HabitName { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public DateTime? LastCompletedDate { get; set; }
    }
}