using Microsoft.EntityFrameworkCore;
using TTA_API.Models;

namespace TTA_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // These DbSets represent the tables in your database
        public DbSet<User> Users { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<PlanSelectedDay> PlanSelectedDays { get; set; }
        public DbSet<Allocation> Allocations { get; set; }
        public DbSet<AllocationTraveler> AllocationTravelers { get; set; }
        public DbSet<SystemSettings> SystemSettings { get; set; }
        public DbSet<Holiday> Holidays { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite primary keys for linking tables
            modelBuilder.Entity<PlanSelectedDay>().HasKey(psd => new { psd.PlanId, psd.Day });
            modelBuilder.Entity<AllocationTraveler>().HasKey(at => new { at.AllocationId, at.UserId });

            // *** THIS IS THE FIX FOR THE CASCADE PATH ERROR ***
            // Define the relationship between AllocationTravelers and Users
            modelBuilder.Entity<AllocationTraveler>()
                .HasOne(at => at.User) // An AllocationTraveler has one User
                .WithMany() // A User can be in many AllocationTravelers
                .HasForeignKey(at => at.UserId) // The foreign key is UserId
                .OnDelete(DeleteBehavior.Restrict); // Tell EF Core NOT to cascade delete from this path        
            // *************************************************

            // Ensure only one row can exist for SystemSettings
            modelBuilder.Entity<SystemSettings>().HasData(new SystemSettings
            {
                SettingsId = 1, // Seed the initial settings row
                DepartureLabel = "Departure",
                ArrivalLabel = "Arrival",
                TripPrice = 240.00m,
                AllocateForCurrentMonth = true,
                UserListViewEnabled = true
            });

            // Seed initial users
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Name = "Allocation Admin", Role = Role.AllocationAdmin, Email = "alloc-admin@trip.com", SendEmail = false, Pin = "0000" },
                new User { Id = 2, Name = "System Admin", Role = Role.SystemAdmin, Email = "sys-admin@trip.com", SendEmail = false, Pin = "0000" }
                //new User { Id = 3, Name = "Gayathri", Role = Role.User, Email = "gayathri@trip.com", SendEmail = true, Pin = "1001" },
                //new User { Id = 4, Name = "Gokul", Role = Role.User, Email = "gokul@trip.com", SendEmail = true, Pin = "1001" },
                //new User { Id = 5, Name = "Kiruthika", Role = Role.User, Email = "kiruthika@trip.com", SendEmail = true, Pin = "1001" },
                //new User { Id = 6, Name = "Narendran", Role = Role.User, Email = "narendran@trip.com", SendEmail = true, Pin = "1001" },
                //new User { Id = 7, Name = "Navin", Role = Role.User, Email = "navin@trip.com", SendEmail = true, Pin = "1001" },
                //new User { Id = 8, Name = "Sangeetha", Role = Role.User, Email = "sangeetha@trip.com", SendEmail = true, Pin = "1001" },
                //new User { Id = 9, Name = "Sarath", Role = Role.User, Email = "sarath@trip.com", SendEmail = true, Pin = "1001" },
                //new User { Id = 10, Name = "Shalini", Role = Role.User, Email = "shalini@trip.com", SendEmail = true, Pin = "1001" }
            );
        }
    }
}