using Microsoft.AspNetCore.Mvc;
using carbox.Models;
using carbox.Repositories;
using carbox.Services;
using System;
using System.Threading.Tasks;
using TimeZoneConverter;
using Microsoft.AspNetCore.Http.HttpResults;

namespace carbox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RideOrdersController : ControllerBase
    {
        private readonly RideService _rideService;
        private readonly RideOrderRepository _rideOrderRepository;

        public RideOrdersController(RideService rideService)
        {
            _rideService = rideService;
        }

        private DateTime GetIsraelDateTime()
        {
            TimeZoneInfo israelTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, israelTimeZone);
        }


        // Creates a new ride order
        // POST: /api/RideOrders
        [HttpPost]
        public async Task<IActionResult> CreateRideOrder([FromBody] RideOrderRequest rideOrderRequest)
        {
            if (rideOrderRequest == null || !ModelState.IsValid)
            {
                return BadRequest("Invalid ride order request.");
            }

            //// Get current UTC time
            //DateTime createdAtUtc = DateTime.UtcNow;

            //// Convert to Israel time using the new function
            //DateTime createdAtIsraelTime = ConvertUtcToIsraelTime(createdAtUtc);
            


            var rideOrder = new RideOrder
            {
                UserId = rideOrderRequest.UserId,
                source = rideOrderRequest.source,
                Destination = rideOrderRequest.Destination,
                RideTime = rideOrderRequest.RideTime,
                CreatedAt = GetIsraelDateTime()
            };

            var createdRide = await _rideService.CreateRideOrderAsync(rideOrder);

            return Ok(new {
                Message = "Ride order created successfully",
                Ride = createdRide
            });

        }

        // Assigns a car to a ride order
        // POST: /api/RideOrders/{rideOrderId}/assign
        [HttpPost("{rideOrderId}/assign")]
        public async Task<IActionResult> AssignCar(int rideOrderId)
        {
            try
            {
                var ride = await _rideService.SearchCarToRide(rideOrderId);
                Console.WriteLine($"departure at: {ride.RideTime}, ride duration: {StationDurations.Matrix[ride.source.Id - 1, ride.Destination.Id - 1]}");
                return Ok(new { ride, arrival = ride.RideTime.AddMinutes(StationDurations.Matrix[ride.source.Id - 1, ride.Destination.Id - 1])});
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // DTO for incoming ride order request
        public class RideOrderRequest
        {
            public int UserId { get; set; }
            public required Station source { get; set; }
            public required Station Destination { get; set; }
            public DateTime RideTime { get; set; }
        }
    }
    
    public class DepartureBody
    {
        public int departure { get; set; }
    }
}
