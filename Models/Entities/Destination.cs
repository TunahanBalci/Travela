using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace TravelApp.Models.Entities
{
    public class Destination
    {
        [Key]
        public Guid ID { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        public Guid CityID { get; set; } // Foreign Key

        [ForeignKey("CityID")]
        [ValidateNever] // Prevent validation on the navigation property
        public City City { get; set; }

        [Required]
        [MaxLength(100)]
        public string Location { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
        public ICollection<Preference> Attractions { get; set; } = new List<Preference>();

        public string Image_Path { get; set; } = string.Empty;

        private double? _averageRating;

        [Column(TypeName = "float")]
        public double? Average_Rating
        {
            get => _averageRating;
            set => _averageRating = value; // Allow setting from code
        }

    }
}
