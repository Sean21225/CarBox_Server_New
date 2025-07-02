using System.Collections.Generic;
using System.Linq;

namespace carbox.Models
{
    public class Route
    {
        public int Id { get; set; }  // Unique identifier for the route
        public string Name { get; set; }  // Route name
        public List<Station> Stations { get; set; } = new List<Station>(); // List of stations in the route
        public Station ChargingStation { get; set; }  // Charging station (Station 0)
        public Dictionary<int, double> DistancesFromChargingStation { get; set; } = new Dictionary<int, double>(); // Distances from the charging station
        public double TotalDistance { get; set; } // Total route distance
        public Dictionary<(int, int), int> travelTimeMatrix { get; set; }

        public int GetTravelTime(int sourceStationId, int destinationStationId)
        {
            return travelTimeMatrix[(sourceStationId,destinationStationId)];
        }
    }
}
