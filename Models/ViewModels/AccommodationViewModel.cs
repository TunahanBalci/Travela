using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TravelApp.Models.Entities;

namespace TravelApp.Models.ViewModels
{
    public class AccommodationViewModel
    {
        public string ID { get; set; }
        public string Image_Path { get; set; }
        public string Name { get; set; }
        public string CityName { get; set; }
        public decimal Price { get; set; }
        public double AverageRating { get; set; }
    }

}
