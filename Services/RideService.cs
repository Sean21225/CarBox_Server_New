using CarboxBackend.Models;
using CarboxBackend.Repositories;
using System;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;

namespace CarboxBackend.Services
{
    // Service class responsible for handling ride order logic, such as assigning a car
    public class RideService
    {
        private readonly RideOrderRepository _rideOrderRepository; // Database repository for ride orders
        private readonly CarRepository _carRepository; // Database repository for cars
        private readonly StationRepository _stationRepository;
        private readonly RouteRepository _routeRepository;
        Random rnd = new Random();

        // Constructor - injects repositories
        public RideService(RideOrderRepository rideOrderRepository, CarRepository carRepository, RouteRepository routeRepository)
        {
            _rideOrderRepository = rideOrderRepository;
            _carRepository = carRepository;
            _routeRepository = routeRepository;
        }

        // Adds a new ride order to the database
        public async Task<RideOrder> CreateRideOrderAsync(RideOrder rideOrder)
        {
            if (rideOrder == null)
                throw new ArgumentNullException(nameof(rideOrder));


            rideOrder.Id = rnd.Next(); // Ensure ID is set
            rideOrder.CreatedAt = DateTime.UtcNow;
            rideOrder.Status = RideOrderStatus.Open; // Default status

            await _rideOrderRepository.CreateRideOrderAsync(rideOrder);
            return rideOrder;
        }

        // Arrival at a station:
        // 1. Update the car's station list
        // 2. Decrease battery percentage
        public async Task ArriveAtStationAsync(string carId, Station station)
        {
            var car = await _carRepository.GetCarByIdAsync(carId);
            if (car == null) return;

            car.StopStations.Remove(station);

            if (station.Id == 0)
                car.BatteryLevel = 100;
            else 
                car.BatteryLevel -= 10; // Example battery consumption
            await _carRepository.UpdateCarAsync(car);
        }
       
        public async Task<RideOrder> AssignCarToRide(Car car, RideOrder rideOrder)
        {
            // Assign the car to the ride order
            rideOrder.AssignedCarId = car.Id;
            rideOrder.Status = RideOrderStatus.Assigned;

            // Save rideOrder updates to the database
            await _rideOrderRepository.UpdateRideAsync(rideOrder);


            Station newStation = rideOrder.Destination;


            // Add station to list, sort and update
            car.StopStations.Add(newStation);
            car.StopStations = car.StopStations.OrderBy(s => s.Id).ToList(); // Example sorting logic

            // Update car status to "Occupied"
            car.Status = CarStatus.Waiting;

            await _carRepository.UpdateCarAsync(car);
            return rideOrder;
        }

        public async Task<RideOrder> SearchCarToRide(int rideOrderId)
        {
            // Helper: convert Israel local to UTC
            static DateTime IsraelToUtc(DateTime ilTime)
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
                return TimeZoneInfo.ConvertTimeToUtc(ilTime, tz);
            }

            // Helper: convert any RideTime to UTC (assume IL if unspecified)
            static DateTime ToUtc(DateTime dt)
            {
                if (dt.Kind == DateTimeKind.Utc) return dt;
                if (dt.Kind == DateTimeKind.Local) return dt.ToUniversalTime();
                var tz = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
                return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(dt, DateTimeKind.Unspecified), tz);
            }

            var rideOrder = await _rideOrderRepository.GetRideByIdAsync(rideOrderId);
            if (rideOrder == null || rideOrder.Status != RideOrderStatus.Open)
                throw new InvalidOperationException("Ride order not found or not open");

            rideOrder.RideTime = ToUtc(rideOrder.RideTime);

            var candidateCars = (await _carRepository.GetAvailableCarsAsync())
                .Where(c => c.BatteryLevel > 40)
                .Where(c => c.LastStation != null)
                .ToList();

            if (!candidateCars.Any())
                throw new InvalidOperationException("No cars meet the availability criteria");

            var startStation = rideOrder.source.Id;
            var sortedCars = CircularSortByStartNumber(candidateCars, startStation);
            var selectedCar = sortedCars.First();

            int travelMinutes = StationDurations.Matrix[selectedCar.LastStation.Id - 1, startStation - 1];

            // ✅ Use Israel local current time first, then convert to UTC
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
            var nowIL = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            var carArrivalIL = nowIL.AddMinutes(travelMinutes);
            var carArrivalUtc = IsraelToUtc(carArrivalIL); // true UTC equivalent

            Console.WriteLine($"[IL] now={nowIL:HH:mm}, carArrival={carArrivalIL:HH:mm}");
            Console.WriteLine($"[UTC] carArrivalUtc={carArrivalUtc:O}");

            // Compare using UTC
            if (carArrivalUtc > rideOrder.RideTime)
            {
                rideOrder.RideTime = carArrivalUtc;
                await _rideOrderRepository.UpdateRideAsync(rideOrder);
                Console.WriteLine($"[Adjusted RideTime UTC] -> {rideOrder.RideTime:O}");
            }

            await AssignCarToRide(selectedCar, rideOrder);

            if (rideOrder.RideTime > DateTime.UtcNow.AddMinutes(15))
                selectedCar.Status = CarStatus.Waiting;

            return rideOrder;
        }


        public async Task<List<RideOrder>> GetAllRideOrdersAsync()
        {
            return await _rideOrderRepository.GetAllRidesAsync();
        }


        // Function for circular sorting with a start number, filtering out the start number itself
        // Function for circular sorting with a start number, putting same-station cars last
public static List<Car> CircularSortByStartNumber(List<Car> cars, int startNumber)
{
    return cars
        .OrderBy(c =>
            c.LastStation.Id == startNumber      // cars at source get 'true' -> sorted last
                ? int.MaxValue                   // ensures they're last
                : c.LastStation.Id < startNumber
                    ? -(startNumber - c.LastStation.Id) // sort by proximity "before" start
                    : (c.LastStation.Id - startNumber)) // sort by proximity "after" start
        .ToList();
}













        //// Get the route information
        //var route = await _routeRepository.GetRouteByIdAsync(rideOrder.RouteId);
        //        if (route == null || !route.Stations.Any())
        //            throw new InvalidOperationException("Route not found or has no stations");

        //// Get the requested station index
        //int requestedStationIndex = route.Stations.FindIndex(s => s.Id == rideOrder.PickupStationId);
        //        if (requestedStationIndex == -1)
        //            throw new InvalidOperationException("Requested station not found in the route");

        //int totalStations = route.Stations.Count;
        //    var sortedCars = carsWithSufficientBattery
        //    .Select(car => {
        //    int carStationIndex = route.Stations.FindIndex(s => s.Id == car.LastStationId);
        //    if (carStationIndex == -1) return null; // Skip cars not on this route

        //    // Calculate the circular distance from car's station to the requested station
        //    int distance;
        //    if (carStationIndex <= requestedStationIndex)
        //    {
        //        distance = requestedStationIndex - carStationIndex;
        //    }
        //    else
        //    {
        //        distance = requestedStationIndex + (totalStations - carStationIndex);
        //    }

        //    return new { Car = car, Distance = distance };
        //})
        //.Where(item => item != null)
        //.OrderBy(item => item.Distance)
        //.Select(item => item.Car)
        //.ToList();



        //private async Task<Car> FindNearestCarInCircularRoute(List<Car> availableCars, Station source)
        //{
        //    return await CalculateDistanceInCircularRoute(availableCars, source);

        //}

        //private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        //{
        //    double R = 6371; 
        //    double dLat = (lat2 - lat1) * Math.PI / 180;
        //    double dLon = (lon2 - lon1) * Math.PI / 180;
        //    double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
        //               Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
        //               Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        //    double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        //    return R * c; // מרחק בקילומטרים
        //}


        //private async Task<Car> CalculateDistanceInCircularRoute(List<Car> availableCars, Station source)
        //{
        //    //רשימת הרכבים לפי הקרבה לתחנת המוצא
        //    availableCars = (List<Car>)availableCars.OrderBy(car => CalculateDistance(car.Location.Latitude, car.Location.Longitude, source.Location.Latitude, source.Location.Longitude));

        //    // הגדרת סדר התחנות במסלול
        //    //var stations = new[] { Station.A, Station.B, Station.C, Station.D };
        //    var stations_json = await _stationRepository.GetAllStationsAsync();
        //    List<Station> stations = (List<Station>)stations_json.OrderBy(station => station.Id);

        //    return availableCars.FirstOrDefault();

        //}

    }
}






