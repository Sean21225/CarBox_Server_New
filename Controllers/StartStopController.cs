using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CarboxBackend.Date;
using MongoDB.Driver;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using CarboxBackend.Models;

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
            cars = mongoDBService.Database?.GetCollection<Car>("Cars");
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
            Console.WriteLine("in updateCarStatus");
            if (request == null || string.IsNullOrEmpty(request.status))
            {
                return BadRequest("Invalid status request.");
            }

            var car = cars.Find(car => true).FirstOrDefault();
            if (car == null)
            {
                return NotFound("No cars available to update.");
            }

            car.Status = request.status;
            cars.ReplaceOne(c => c.CarId == car.CarId, car);

            return Ok(new { message = $"Status updated to: {car.Status}", car });
        }
    }

    public class StatusRequest
    {
        public string CarId { get; set; }
        public string status { get; set; }
    }

    public class carboxCollection
    {
        public int CarId { get; set; }

        [BsonId]
        public ObjectId Id { get; set; }

        public string Status { get; set; }

        public carboxCollection(int CarId, string status)
        {
            CarId = CarId;
            Status = status;
        }
    }
};



