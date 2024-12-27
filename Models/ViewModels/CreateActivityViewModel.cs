using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TravelApp.Models.ViewModels
{
    public class CreateActivityViewModel
    {

        public Guid ID { get; set; }

        [Required(ErrorMessage = "Activity name is required.")]
        [MaxLength(100, ErrorMessage = "Activity name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        [MaxLength(50, ErrorMessage = "Type cannot exceed 50 characters.")]
        public string Type { get; set; }

        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Range(0.00, 99999999.99, ErrorMessage = "Price must be a valid number.")]
        public decimal Price { get; set; }

        [Display(Name = "Requires Reservation")]
        public bool Requires_Reservation { get; set; }

        [Display(Name = "Destinations")]
        public List<Guid>? DestinationIDs { get; set; } = new List<Guid>();

        public List<SelectListItem> Destinations { get; set; } = new List<SelectListItem>();

        [Display(Name = "Accommodations")]
        public List<Guid>? AccommodationIDs { get; set; } = new List<Guid>();

        public List<SelectListItem> Accommodations { get; set; } = new List<SelectListItem>();
    }
}
