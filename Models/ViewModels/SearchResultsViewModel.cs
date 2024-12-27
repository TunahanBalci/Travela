using TravelApp.Models.Entities;

namespace TravelApp.Models.ViewModels
{
    public class SearchResultsViewModel
    {
        public string Query { get; set; }
        public IEnumerable<Destination> Destinations { get; set; }
        public IEnumerable<Accommodation> Accommodations { get; set; }
    }
}
