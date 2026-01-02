using System;
using System.ComponentModel.DataAnnotations;

namespace HabitTracker.Models
{
    public class CreateHabitViewModel
    {
        [Required(ErrorMessage = "Habit name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Habit name must be between 1 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_.,!?()]+$", ErrorMessage = "Habit name contains invalid characters")]
        [Display(Name = "Habit Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Frequency is required")]
        [Display(Name = "Frequency")]
        public string Frequency { get; set; } = "Daily";

        [Required(ErrorMessage = "Target days is required")]
        [Range(1, 365, ErrorMessage = "Target days must be between 1 and 365")]
        [Display(Name = "Target Days")]
        public int TargetDays { get; set; } = 30;

        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_]*$", ErrorMessage = "Category contains invalid characters")]
        [Display(Name = "Category")]
        public string Category { get; set; } = "Other";
    }

    public class EditHabitViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Habit name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Habit name must be between 1 and 100 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_.,!?()]+$", ErrorMessage = "Habit name contains invalid characters")]
        [Display(Name = "Habit Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Frequency is required")]
        [Display(Name = "Frequency")]
        public string Frequency { get; set; } = string.Empty;

        [Required(ErrorMessage = "Target days is required")]
        [Range(1, 365, ErrorMessage = "Target days must be between 1 and 365")]
        [Display(Name = "Target Days")]
        public int TargetDays { get; set; } = 30;

        [Range(0, 100, ErrorMessage = "Progress must be between 0 and 100")]
        [Display(Name = "Progress (%)")]
        public int Progress { get; set; } = 0;

        [StringLength(50, ErrorMessage = "Category cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_]*$", ErrorMessage = "Category contains invalid characters")]
        [Display(Name = "Category")]
        public string Category { get; set; } = "Other";
    }

    public class HabitListViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Frequency { get; set; } = string.Empty;

        [Range(1, 365)]
        public int TargetDays { get; set; } = 30;

        [StringLength(50)]
        public string Category { get; set; } = "Other";

        [Range(0, 100)]
        public int Progress { get; set; } = 0;

        public bool IsCompleted { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public int CurrentStreak { get; set; } = 0;

        public int LongestStreak { get; set; } = 0;

        public DateTime CreatedDate { get; set; }

        public DateTime? LastCompletedDate { get; set; }

        public bool IsAchieved { get; set; } = false;

        public DateTime? AchievedDate { get; set; }
    }
}
