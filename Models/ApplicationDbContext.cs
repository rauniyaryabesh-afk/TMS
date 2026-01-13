using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TourismManagementSystem.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Tour> Tours => Set<Tour>();
        public DbSet<TourDate> TourDates => Set<TourDate>();
        public DbSet<Destination> Destinations => Set<Destination>();
        public DbSet<TourDestination> TourDestinations => Set<TourDestination>();

        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<Feedback> Feedbacks => Set<Feedback>();
        public DbSet<AgencyProfile> AgencyProfiles => Set<AgencyProfile>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Tour>()
                .HasMany(t => t.AvailableDates)
                .WithOne(d => d.Tour!)
                .HasForeignKey(d => d.TourId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Tour>()
                .HasMany(t => t.TourDestinations)
                .WithOne(td => td.Tour!)
                .HasForeignKey(td => td.TourId)
                .OnDelete(DeleteBehavior.Cascade);

           
            builder.Entity<Feedback>()
                .HasOne(f => f.Tour)
                .WithMany() 
                .HasForeignKey(f => f.TourId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Feedback>()
    .HasOne(f => f.Tour)
    .WithMany()
    .HasForeignKey(f => f.TourId)
    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Feedback>()
                .HasOne(f => f.Booking)
                .WithMany()
                .HasForeignKey(f => f.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            
            builder.Entity<Feedback>()
                .HasIndex(f => new { f.BookingId, f.TouristId })
                .IsUnique();

        }
    }
}
