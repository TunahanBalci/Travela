using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TravelApp.Models.Entities;

namespace TravelApp.Models.ViewModels
{
    public class CreateDestinationViewModel
    {
        [Required(ErrorMessage = "Destination name is required.")]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Location is required.")]
        [MaxLength(100)]
        public string Location { get; set; }

        [Required(ErrorMessage = "Please select a city.")]
        public Guid CityID { get; set; }

        public ICollection<Guid> SelectedActivityIds { get; set; } = new List<Guid>();
        public ICollection<Activity> Activities { get; set; } = new List<Activity>();

        public ICollection<Guid> SelectedPreferencesIds { get; set; } = new List<Guid>();
        public ICollection<Preference> Attractions { get; set; } = new List<Preference>();

        public ICollection<City> Cities { get; set; } = new List<City>();
    }
}
