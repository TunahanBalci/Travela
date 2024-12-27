using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TravelApp.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TravelApp.Models.ViewModels
{
    public class CreateAmenityViewModel
    {
        [Required(ErrorMessage = "Amenity name is required.")]
        [MaxLength(100, ErrorMessage = "Amenity name cannot exceed 100 characters.")]
        public string Name { get; set; }

        // List of all available accommodations to select from
        public List<Accommodation> Accommodations { get; set; } = new List<Accommodation>();

        // List of selected accommodation IDs
        [Display(Name = "Select Accommodations")]
        public List<Guid> SelectedAccommodationIds { get; set; } = new List<Guid>();
    }
}
