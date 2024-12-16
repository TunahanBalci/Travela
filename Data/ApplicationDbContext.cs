using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TravelApp.Models;

namespace TravelApp.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet tanımlamaları
        public DbSet<Accommodation> Accommodations { get; set; }
        public DbSet<AccommodationActivity> AccommodationActivities { get; set; }
        public DbSet<Activities> Activities { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<CityCommonActivity> CityCommonActivities { get; set; }
        public DbSet<Destination> Destinations { get; set; }
        public DbSet<DestinationAttraction> DestinationAttractions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewComment> ReviewComments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserDestination> UserDestinations { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<UserTravelHistory> UserTravelHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Bileşik anahtarlar için yapılandırma
            modelBuilder.Entity<AccommodationActivity>()
                .HasKey(aa => new { aa.Accommodation_ID, aa.Activity_ID });

            // Foreign key ilişkisi için OnDelete davranışını düzenle
            modelBuilder.Entity<AccommodationActivity>()
                .HasOne(aa => aa.Accommodation)
                .WithMany(a => a.AccommodationActivities)
                .HasForeignKey(aa => aa.Accommodation_ID)
                .OnDelete(DeleteBehavior.Restrict); // Cascade delete'i kapatır

            modelBuilder.Entity<AccommodationActivity>()
                .HasOne(aa => aa.Activity)
                .WithMany(act => act.AccommodationActivities)
                .HasForeignKey(aa => aa.Activity_ID)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete'i kapatır

            modelBuilder.Entity<UserDestination>()
                .HasKey(ud => new { ud.User_ID, ud.Destination_ID });

            modelBuilder.Entity<UserTravelHistory>()
                .HasKey(uth => new { uth.User_ID, uth.Destination_ID });

            modelBuilder.Entity<DestinationAttraction>()
                .HasKey(da => new { da.Destination_ID, da.Attraction });

            modelBuilder.Entity<CityCommonActivity>()
                .HasKey(cca => new { cca.City_ID, cca.Activity });

            modelBuilder.Entity<UserPreference>()
                .HasKey(up => new { up.User_ID, up.Preference });

            // Diğer ilişkiler ve yapılandırmalar gerektiğinde burada tanımlanabilir.
        }
    }
}
