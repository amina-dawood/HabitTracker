using System;
using System.Collections.Generic;

namespace HabitTracker.Areas.Admin.Models
{
    public class UserViewModel
    {
        public string Id { get; set; } = "";
        public string Email { get; set; } = "";
        public string UserName { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsActive { get; set; }
        public int TotalHabits { get; set; }
        public int CompletedHabits { get; set; }
    }

    public class UserListViewModel
    {
        public List<UserViewModel> Users { get; set; } = new List<UserViewModel>();
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int UsersWithEmailConfirmed { get; set; }
    }
}
