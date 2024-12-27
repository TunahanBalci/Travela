using Microsoft.EntityFrameworkCore;
using TravelApp.Models.Entities;

namespace TravelApp.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<Accommodation> Accommodations { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Booking> Bookings_History { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Visit> Visits { get; set; }
        public DbSet<Preference> Preferences { get; set; }

        public override int SaveChanges()
        {
            UpdateAverageRatings();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAverageRatings();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAverageRatings()
        {
            var updatedReviews = ChangeTracker.Entries<Review>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
                .Select(e => e.Entity);

            foreach (var review in updatedReviews)
            {
                if (review.DestinationID.HasValue)
                {
                    var destination = Destinations.Include(d => d.Reviews)
                        .FirstOrDefault(d => d.ID == review.DestinationID.Value);

                    if (destination != null)
                    {
                        destination.Average_Rating = destination.Reviews
                            .Where(r => r.Rating.HasValue)
                            .Average(r => r.Rating.Value);
                    }
                }

                if (review.AccommodationID.HasValue)
                {
                    var accommodation = Accommodations.Include(a => a.Reviews)
                        .FirstOrDefault(a => a.ID == review.AccommodationID.Value);

                    if (accommodation != null)
                    {
                        accommodation.Average_Rating = accommodation.Reviews
                            .Where(r => r.Rating.HasValue)
                            .Average(r => r.Rating.Value);
                    }
                }

                if (review.ActivityID.HasValue)
                {
                    var activity = Activities.Include(a => a.Reviews)
                        .FirstOrDefault(a => a.ID == review.ActivityID.Value);

                    if (activity != null)
                    {
                        activity.Average_Rating = activity.Reviews
                            .Where(r => r.Rating.HasValue)
                            .Average(r => r.Rating.Value);
                    }
                }
            }
        }

        public async Task MoveExpiredBookingsToHistory()
        {
            var expiredBookings = Bookings
                .Where(b => b.End_Date < DateTime.Now)
                .ToList();

            foreach (var booking in expiredBookings)
            {
                var history = new Booking
                {
                    ID = booking.ID,
                    User = booking.User,
                    Accommodation = booking.Accommodation,
                    Start_Date = booking.Start_Date,
                    End_Date = booking.End_Date,
                    Booking_Date = booking.Booking_Date
                };

                booking.Accommodation.Availability = true;
                Bookings_History.Add(history);
                Bookings.Remove(booking);
            }

            await SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Booking>(entity =>
            {
                entity.ToTable(tb =>
                {
                    tb.HasCheckConstraint("CK_Booking_StartBeforeEnd", "Start_Date < End_Date");
                });
            });

            modelBuilder.Entity<Accommodation>()
                .HasMany(a => a.Activities)
                .WithMany(b => b.Accommodations)
                .UsingEntity<Dictionary<string, object>>(
                    "AccommodationActivity",
                    j => j
                        .HasOne<Activity>()
                        .WithMany()
                        .HasForeignKey("ActivitiesID")
                        .OnDelete(DeleteBehavior.NoAction),
                    j => j
                        .HasOne<Accommodation>()
                        .WithMany()
                        .HasForeignKey("AccommodationsID")
                        .OnDelete(DeleteBehavior.Cascade)
                );

            modelBuilder.Entity<Visit>()
                .HasKey(uth => new { uth.UserID, uth.DestinationID });

            modelBuilder.Entity<Destination>()
                .HasOne(d => d.City)
                .WithMany(c => c.Destinations)
                .HasForeignKey(d => d.CityID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Destination)
                .WithMany(d => d.Reviews)
                .HasForeignKey(r => r.DestinationID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Accommodation)
                .WithMany(a => a.Reviews)
                .HasForeignKey(r => r.AccommodationID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Activity)
                .WithMany(a => a.Reviews)
                .HasForeignKey(r => r.ActivityID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Activity>()
                .HasMany(a => a.Destinations)
                .WithMany(d => d.Activities)
                .UsingEntity<Dictionary<string, object>>(
                    "ActivityDestination",
                    j => j.HasOne<Destination>().WithMany().HasForeignKey("DestinationID"),
                    j => j.HasOne<Activity>().WithMany().HasForeignKey("ActivityID"),
                    j =>
                    {
                        j.HasKey("ActivityID", "DestinationID");
                        j.ToTable("ActivityDestinations");
                    }
                );
        }
    }
}
