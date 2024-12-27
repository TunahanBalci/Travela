using System.ComponentModel.DataAnnotations;

namespace TravelApp.Models.Entities
{
    public class City
    {
        [Key]
        public Guid ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Location { get; set; }

        [MaxLength(50)]
        public string Climate { get; set; }

        [MaxLength(50)]
        public string Terrain { get; set; }


        [Range(0.00, 99999999.99, ErrorMessage = "Invalid value (cannot be negative or too big).")]
        public decimal Cost_Of_Living { get; set; }



        public ICollection<Destination> Destinations { get; set; } = new List<Destination>();

        public ICollection<Accommodation> Accommodations { get; set; } = new List<Accommodation>();

        public ICollection<Activity> Activities { get; set; } = new List<Activity>();

    }
}
