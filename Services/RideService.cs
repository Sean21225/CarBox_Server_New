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
            car.Status = CarStatus.Occupied;

            await _carRepository.UpdateCarAsync(car);
            return rideOrder;
        }

        public async Task<RideOrder> SearchCarToRide(int rideOrderId)
{
    Console.WriteLine($"SearchCarToRide called with rideOrderId={rideOrderId}");

    // 1️⃣ Get the ride order
    var rideOrder = await _rideOrderRepository.GetRideByIdAsync(rideOrderId);
    if (rideOrder == null)
        throw new InvalidOperationException("Ride order not found");

    if (rideOrder.Status != RideOrderStatus.Open)
        throw new InvalidOperationException("Ride order is not open");

    Console.WriteLine($"Ride found: Id={rideOrderId}, source={rideOrder.source?.Id}, destination={rideOrder.Destination?.Id}, RideTime={rideOrder.RideTime}");

    // 2️⃣ Get available cars
    var availableCars = await _carRepository.GetAvailableCarsAsync();
    if (!availableCars.Any())
        throw new InvalidOperationException("No available cars at the moment");

    Console.WriteLine($"Total available cars: {availableCars.Count}");

    // 3️⃣ Filter cars with battery > 40%
    var carsWithSufficientBattery = availableCars.Where(car => car.BatteryLevel > 40).ToList();
    if (!carsWithSufficientBattery.Any())
        throw new InvalidOperationException("No cars with sufficient battery available");

    Console.WriteLine("Cars with sufficient battery:");
    foreach (var car in carsWithSufficientBattery)
    {
        Console.WriteLine($"Car Id: {car.Id}, Status: {car.Status}, Battery: {car.BatteryLevel}%, LastStation: {car.LastStation?.Id}");
    }

    // 4️⃣ Sort cars by circular distance from start station
    int startStation = rideOrder.source?.Id ?? 0;
    Console.WriteLine($"Sorting cars relative to startStation={startStation}");
    List<Car> sortedCars = CircularSortByStartNumber(carsWithSufficientBattery, startStation);
    if (!sortedCars.Any())
        throw new InvalidOperationException("No suitable cars near the requested station");

    // 5️⃣ Get route
    var route = (await _routeRepository.GetAllRoutesAsync()).FirstOrDefault();
    if (route == null)
        throw new InvalidOperationException("No routes found in the system");

    Console.WriteLine("Route found, calculating travel time...");

    // 6️⃣ Calculate travel time safely
    var selectedCar = sortedCars.First();
    int lastStationId = selectedCar.LastStation?.Id ?? -1;
    int sourceId = rideOrder.source?.Id ?? -1;

    if (lastStationId == -1 || sourceId == -1)
        throw new InvalidOperationException("Invalid last station or ride source");

    int travelTime = route.GetTravelTime(lastStationId, sourceId);
    Console.WriteLine($"Selected Car Id: {selectedCar.Id}, LastStation: {lastStationId}, TravelTime to source: {travelTime} minutes");

    // 7️⃣ Check if car can arrive in time
    if (DateTime.Now.AddMinutes(travelTime) > rideOrder.RideTime)
        throw new InvalidOperationException("No CARBOX was found that could arrive at the desired time");

    // 8️⃣ Assign car to ride
    await AssignCarToRide(selectedCar, rideOrder);

    // 9️⃣ Set future ride status
    if (rideOrder.RideTime > DateTime.Now.AddMinutes(15))
        selectedCar.Status = CarStatus.Waiting;

    Console.WriteLine($"Car {selectedCar.Id} assigned to ride {rideOrderId}");
    return rideOrder;
}


        public async Task<List<RideOrder>> GetAllRideOrdersAsync()
        {
            return await _rideOrderRepository.GetAllRidesAsync();
        }


        // Function for circular sorting with a start number, filtering out the start number itself
        public static List<Car> CircularSortByStartNumber(List<Car> cars, int startNumber)
        {
            return cars
                .Where(c => c.LastStation.Id != startNumber)  // Filter out the start number
                .OrderByDescending(c =>
                    c.LastStation.Id < startNumber ?
                    c.LastStation.Id :
                    c.LastStation.Id - int.MaxValue / 2
                ).ToList();
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






