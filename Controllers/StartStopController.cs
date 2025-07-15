using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CarboxBackend.Date;
using MongoDB.Driver;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson.Serialization.Attributes;
using CarboxBackend.Models;
using CarboxBackend.Services;
using Newtonsoft.Json;

namespace carbox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class StartStopController : ControllerBase
    {
        private readonly IMongoCollection<Car> cars;
        private readonly MqttService _mqttService;

        public StartStopController(MongoDBService mongoDBService, MqttService mqttService)
        {
            cars = mongoDBService.Database?.GetCollection<Car>("Cars");
            _mqttService = mqttService;
        }



        // GET: api/StartStop
        [HttpGet]
        public IActionResult Get()
        {
            var car_list = cars.Find(car => true).ToList();
            return Ok(car_list);
        }

        // POST: api/StartStop
        [HttpPost]
        public IActionResult UpdateCarStatus([FromBody] CarStatusRequest request)
        {
            Console.WriteLine($"[DEBUG] Received UpdateCarStatus request: CarId={request?.CarId}, status={request?.status}");
            if (request == null)
            {
                Console.WriteLine("[DEBUG] Request is null");
                return BadRequest(new { message = "Invalid status request." });
            }
            Console.WriteLine("hey Ron");
            // Print all cars to the console
            var allCars = cars.Find(car => true).ToList();
            Console.WriteLine($"[DEBUG] All cars in collection (count: {allCars.Count}):");
            foreach (var c in allCars)
            {
                Console.WriteLine($"  Id: {c.Id}, Status: {c.Status}");
            }

            var car = cars.Find(car => car.Id == request.CarId).FirstOrDefault();
            if (car == null)
            {
                Console.WriteLine($"[DEBUG] No car found with Id={request.CarId}");
                return NotFound(new { message = $"No car with Id {request.CarId} available to update." });
            }
            Console.WriteLine($"[DEBUG] Found car: Id={car.Id}, Status(before)={car.Status}");

            car.Status = (CarStatus)int.Parse(request.status);
            cars.ReplaceOne(c => c.Id == car.Id, car);
            Console.WriteLine($"[DEBUG] Updated car: Id={car.Id}, Status(after)={car.Status}");

            // Publish MQTT message to notify the car about status change
            var carCommand = new
            {
                CarId = car.Id,
                Command = "STATUS_UPDATE",
                NewStatus = car.Status,
                Timestamp = DateTime.UtcNow
            };

            string mqttTopic = $"carbox/commands/{car.Id}";
            string mqttMessage = JsonConvert.SerializeObject(carCommand);

            // Publish the message (fire and forget - don't await to avoid blocking the API response)
            _ = Task.Run(async () => await _mqttService.PublishMessageAsync(mqttTopic, mqttMessage));

            return Ok(new { message = $"Status updated to: {car.Status}", car });
        }
    }

    public class CarStatusRequest
    {
        public string CarId { get; set; }
        public string status { get; set; }
    }

    public class carboxCollection
    {
        // CarId is optional, but Id is the MongoDB _id as string
        [BsonId]
        public string Id { get; set; }

        public int Status { get; set; }

        public carboxCollection(int CarId, int status)
        {
            CarId = CarId;
            Status = status;
        }
    }
};