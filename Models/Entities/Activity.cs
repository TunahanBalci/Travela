using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelApp.Models.Entities
{
    public class Activity
    {
        [Key]
        public Guid ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; }

        public DateTime Date { get; set; }

        [Range(0.00, 99999999.99, ErrorMessage = "Invalid value (cannot be negative or too big).")]
        public decimal Price { get; set; }

        [Required]
        public bool Requires_Reservation { get; set; } = false;

        public ICollection<Destination> Destinations { get; set; } = new List<Destination>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        private double? _averageRating;

        [Column(TypeName = "float")]
        public double? Average_Rating
        {
            get => _averageRating;
            set => _averageRating = value; // Allow setting from code
        }

        public ICollection<Accommodation> Accommodations { get; set; } = new List<Accommodation>();
    }
}
