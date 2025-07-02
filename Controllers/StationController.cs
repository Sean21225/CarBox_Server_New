using CarboxBackend.Models;
using CarboxBackend.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarboxBackend.Controllers
{
    [ApiController]
    [Route("api/stations")]
    public class StationController : ControllerBase
    {
        private readonly StationRepository _stationRepository;

        public StationController(StationRepository stationRepository)
        {
            _stationRepository = stationRepository;
        }

        [HttpGet]
        public async Task<ActionResult<List<Station>>> GetAllStations()
        {
            var stations = await _stationRepository.GetAllStationsAsync();
            return Ok(stations);
        }

        [HttpPost]
        public async Task<ActionResult> AddStation([FromBody] Station station)
        {
            if (station == null)
            {
                return BadRequest("Invalid station data.");
            }

            await _stationRepository.AddStationAsync(station);
            return CreatedAtAction(nameof(GetAllStations), new { id = station.Id }, station);
        }
    }
}
