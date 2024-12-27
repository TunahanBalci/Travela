using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TravelApp.Models.Entities;

namespace TravelApp.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<DestinationViewModel> Destinations { get; set; }
        public IEnumerable<AccommodationViewModel> Accommodations { get; set; }
        public IEnumerable<dynamic> DestinationStats { get; set; }
        public IEnumerable<dynamic> TopUsersByReviews { get; set; }
        public IEnumerable<dynamic> DestinationRatings { get; set; }
        public IEnumerable<dynamic> CityRatings { get; set; }
        public List<string> FavoritedDestinationIds { get; set; } // List of string IDs

    }


}
