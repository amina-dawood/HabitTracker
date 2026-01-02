using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HabitTracker.Models;

namespace HabitTracker.Data
{
    public class HabitTrackerContext : IdentityDbContext
    {
        public HabitTrackerContext(DbContextOptions<HabitTrackerContext> options)
            : base(options)
        {
        }

        public DbSet<Habit> Habits { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Habit configuration
            builder.Entity<Habit>()
                .Property(h => h.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Entity<Habit>()
                .Property(h => h.Description)
                .HasMaxLength(500);

            builder.Entity<Habit>()
                .Property(h => h.Frequency)
                .IsRequired();

            builder.Entity<Habit>()
                .Property(h => h.UserId)
                .IsRequired()
                .HasMaxLength(450);

            // Index for performance
            builder.Entity<Habit>()
                .HasIndex(h => h.UserId);
        }
    }
}