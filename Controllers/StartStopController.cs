using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CarboxBackend.Date;
using MongoDB.Driver;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CarboxBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class StartStopController : ControllerBase
    {
        //Car car1 = new Car(1, "stop");
        private readonly IMongoCollection<carboxCollection> cars;

        public StartStopController(MongoDBService mongoDBService)
        {
            cars = mongoDBService.Database?.GetCollection<carboxCollection>("carboxCollection");
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
        public IActionResult UpdateCarStatus([FromBody] StatusRequest request)
        {
            Console.WriteLine($"[DEBUG] Received UpdateCarStatus request: CarId={request?.CarId}, status={request?.status}");
            if (request == null)
            {
                Console.WriteLine("[DEBUG] Request is null");
                return BadRequest(new { message = "Invalid status request." });
            }

            Console.WriteLine($"[DEBUG] Using database: {cars.Database.DatabaseNamespace.DatabaseName}");
            Console.WriteLine($"[DEBUG] Using collection: {cars.CollectionNamespace.CollectionName}");

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

            car.Status = (CarStatus)request.status;
            cars.ReplaceOne(c => c.Id == car.Id, car);
            Console.WriteLine($"[DEBUG] Updated car: Id={car.Id}, Status(after)={car.Status}");

            return Ok(new { message = $"Status updated to: {car.Status}", car });
        }
    }

    public class StatusRequest
    {
        public string CarId { get; set; } // Now matches MongoDB _id
        public int status { get; set; }
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



