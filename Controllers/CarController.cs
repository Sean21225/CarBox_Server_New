using Microsoft.AspNetCore.Mvc;
using CarboxBackend.Models;
using CarboxBackend.Repositories;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CarboxBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : ControllerBase
    {
        private readonly CarRepository _carRepository;

        // Constructor: Inject the repository
        public CarController(CarRepository carRepository)
        {
            _carRepository = carRepository;
        }

        // Add a new car
        [HttpPost("add")]
        public async Task<IActionResult> AddCar([FromBody] Car newCar)
        {
            if (newCar == null || string.IsNullOrEmpty(newCar.Id))
            {
                return BadRequest("Invalid car data.");
            }

            await _carRepository.AddCarAsync(newCar);
            return Ok(newCar);
        }

        // Get a specific car by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCarById(string id)
        {
            var car = await _carRepository.GetCarByIdAsync(id);
            if (car == null)
            {
                return NotFound("Car not found.");
            }
            return Ok(car);
        }

        // Get all available cars
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableCars()
        {
            var availableCars = await _carRepository.GetAvailableCarsAsync();
            return Ok(availableCars);
        }

        // Update car details
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCar([FromBody] Car updatedCar)
        {
            if (updatedCar == null || string.IsNullOrEmpty(updatedCar.Id))
            {
                return BadRequest("Invalid car data.");
            }

            await _carRepository.UpdateCarAsync(updatedCar);
            return Ok(new { message = "Car updated successfully", updatedCar });
        }

        //// Update battery level
        //[HttpPut("updateBattery/{id}")]
        //public async Task<IActionResult> UpdateBatteryLevel(string id, [FromBody] int batteryLevel)
        //{
        //    var car = await _carRepository.GetCarByIdAsync(id);
        //    if (car == null)
        //    {
        //        return NotFound("Car not found.");
        //    }

        //    car.BatteryLevel = batteryLevel;
        //    await _carRepository.UpdateCarAsync(car);
        //    return Ok(new { message = "Battery level updated successfully", car });
        //}

        //// Update station list
        //[HttpPut("updateStations/{id}")]
        //public async Task<IActionResult> UpdateStations(string id, [FromBody] List<string> stations)
        //{
        //    var car = await _carRepository.GetCarByIdAsync(id);
        //    if (car == null)
        //    {
        //        return NotFound("Car not found.");
        //    }

        //    car.StopStations = stations;
        //    await _carRepository.UpdateCarAsync(car);
        //    return Ok(new { message = "Station list updated successfully", car });
        //}

        // Get battery level
        [HttpGet("battery/{id}")]
        public async Task<IActionResult> GetBatteryLevel(string id)
        {
            var car = await _carRepository.GetCarByIdAsync(id);
            if (car == null)
            {
                return NotFound("Car not found.");
            }
            return Ok(new { batteryLevel = car.BatteryLevel });
        }

        // Get station list
        [HttpGet("stations/{id}")]
        public async Task<IActionResult> GetStations(string id)
        {
            var car = await _carRepository.GetCarByIdAsync(id);
            if (car == null)
            {
                return NotFound("Car not found.");
            }
            return Ok(new { stations = car.StopStations });
        }
    }
}
