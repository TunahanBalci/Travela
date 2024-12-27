using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TravelApp.Models.Entities;

namespace TravelApp.Models.ViewModels
{
    public class DestinationViewModel
    {
        public string ID { get; set; }
        public string Image_Path { get; set; }
        public string Name { get; set; }
        public string CityName { get; set; }
        public double AverageRating { get; set; }
        public string Description { get; set; }
        public bool IsFavorited { get; set; }
        public string Location { get; set; }

    }

}
