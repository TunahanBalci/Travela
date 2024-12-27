using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TravelApp.Models.Entities;

namespace TravelApp.Models.ViewModels
{
    public class CreateAccommodationViewModel
    {
        // Accommodation ID (Required for Edit operations)
        public Guid ID { get; set; }

        [Required(ErrorMessage = "Accommodation name is required.")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        [MaxLength(50)]
        public string Type { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [MaxLength(100)]
        public string Location { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.00, 99999999.99, ErrorMessage = "Invalid value (cannot be negative or too big).")]
        public decimal Price { get; set; }

        [Display(Name = "Availability")]
        public bool Availability { get; set; } = true;

        [Required(ErrorMessage = "Please select a city.")]
        public Guid CityID { get; set; }

        public string Image_Path { get; set; } = string.Empty; // New property for image path

        // List of all available cities
        public List<City> Cities { get; set; } = new List<City>();

        // Properties for Activities and Amenities
        public List<Activity> Activities { get; set; } = new List<Activity>();
        public List<Amenity> Amenities { get; set; } = new List<Amenity>();

        // Selected Activity and Amenity IDs
        public List<Guid> SelectedActivityIds { get; set; } = new List<Guid>();
        public List<Guid> SelectedAmenityIds { get; set; } = new List<Guid>();
    }
}
