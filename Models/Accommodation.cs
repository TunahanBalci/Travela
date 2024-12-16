using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelApp.Models
{
    public class Accommodation
    {
        [Key]
        public int Accommodation_ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public int Availability { get; set; }

        [Column(TypeName = "decimal(3,2)")]
        public decimal Average_Review { get; set; }

        [ForeignKey("City")]
        public int City_ID { get; set; }

        // Navigation properties
        public City City { get; set; }
        public ICollection<AccommodationActivity> AccommodationActivities { get; set; }
        // Add other navigation properties as needed
    }
}
