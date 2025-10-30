using Microsoft.AspNetCore.Mvc;
using CarboxBackend.Models;
using CarboxBackend.Repositories;
using CarboxBackend.Services;
using System;
using System.Threading.Tasks;

namespace CarboxBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RideOrdersController : ControllerBase
    {
        private readonly RideService _rideService;

        public RideOrdersController(RideService rideService)
        {
            _rideService = rideService;
        }

        private DateTime GetIsraelDateTime()
        {
            TimeZoneInfo israelTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, israelTimeZone);
        }

        // POST: /api/RideOrders
        [HttpPost]
        public async Task<IActionResult> CreateRideOrder([FromBody] RideOrderRequest rideOrderRequest)
        {
            if (rideOrderRequest == null || !ModelState.IsValid)
                return BadRequest("Invalid ride order request.");

            if (rideOrderRequest.source == null || rideOrderRequest.Destination == null)
                return BadRequest("Source and destination must be provided.");

            if (rideOrderRequest.source.Id == rideOrderRequest.Destination.Id)
                return BadRequest(new { message = "Source and destination stations must be different." });

            Console.WriteLine("Creating ride order");
            Console.WriteLine($"Source: {rideOrderRequest.source.Id}, Destination: {rideOrderRequest.Destination.Id}");

            var rideOrder = new RideOrder
            {
                UserId = rideOrderRequest.UserId,
                source = rideOrderRequest.source,
                Destination = rideOrderRequest.Destination,
                RideTime = rideOrderRequest.RideTime,
                CreatedAt = GetIsraelDateTime()
            };

            var createdRide = await _rideService.CreateRideOrderAsync(rideOrder);

            return Ok(new { Message = "Ride order created successfully", Ride = createdRide });
        }

        // POST: /api/RideOrders/{rideOrderId}/assign
        [HttpPost("{rideOrderId}/assign")]
        public async Task<IActionResult> AssignCar(int rideOrderId)
        {
            try
            {
                Console.WriteLine($"AssignCar called with rideOrderId={rideOrderId}");
                var ride = await _rideService.SearchCarToRide(rideOrderId);
                Console.WriteLine($"new departure: {ride.RideTime}");

                if (ride == null)
                    return BadRequest(new { message = "No ride found or no suitable cars available." });

                // Calculate arrival time safely
                int travelMinutes = 0;
                if (StationDurations.Matrix != null &&
                    ride.source != null &&
                    ride.Destination != null)
                {
                    travelMinutes = StationDurations.Matrix[ride.source.Id - 1, ride.Destination.Id - 1];
                    Console.WriteLine($"travelMinutes: {travelMinutes}");
                }

                return Ok(new
                {
                    ride,
                    
                    arrival = ride.RideTime.AddMinutes(travelMinutes),
                    travelMinutes
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AssignCar: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        // DTO for incoming ride order request
        public class RideOrderRequest
        {
            public int UserId { get; set; }
            public Station source { get; set; }
            public Station Destination { get; set; }
            public DateTime RideTime { get; set; }
        }
    }
}
