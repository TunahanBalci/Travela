using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TravelApp.Models.ViewModels
{
    public class EditCityViewModel
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Climate { get; set; }
        public string Terrain { get; set; }
        public decimal Cost_Of_Living { get; set; }
        public List<Guid> SelectedActivityIds { get; set; } = new List<Guid>();
        public List<SelectListItem> Activities { get; set; } = new List<SelectListItem>();
    }

}
