
using System;
using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Models
{
    public class Habit
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Habit name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Habit name must be between 1 and 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Frequency is required")]
        [RegularExpression(@"^(Daily|Weekly|Monthly)$", ErrorMessage = "Frequency must be Daily, Weekly, or Monthly")]
        public string Frequency { get; set; } = "Daily";

        [Range(1, 365, ErrorMessage = "Target days must be between 1 and 365")]
        public int TargetDays { get; set; } = 30;

        [Required]
        public string UserId { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public DateTime? LastCompletedDate { get; set; }

    [Range(0, int.MaxValue)]
    public int CurrentStreak { get; set; } = 0;

    [Range(0, int.MaxValue)]
    public int LongestStreak { get; set; } = 0;

    // Additional properties for compatibility
    public string Category { get; set; } = "General";

    public bool IsActive { get; set; } = true;

    [Range(0, 100)]
    public int Progress { get; set; } = 0;

    // Legacy properties for backward compatibility
    public bool IsCompleted => IsCompletedToday;

    public bool IsAchieved => CurrentStreak >= TargetDays;

    // Computed properties
    public bool IsCompletedToday => LastCompletedDate?.Date == DateTime.Today;

    public double ProgressPercentage => TargetDays > 0 ? (CurrentStreak * 100.0) / TargetDays : 0;

    public string ProgressColor => ProgressPercentage >= 75 ? "success" :
                                  ProgressPercentage >= 50 ? "warning" : "danger";
    }
}