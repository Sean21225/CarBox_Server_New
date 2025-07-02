using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using CarboxBackend.Models;
using CarboxBackend.Repositories;

namespace CarboxBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoutesController : ControllerBase
    {
        private readonly RouteRepository _routeRepository;

        public RoutesController(RouteRepository routeRepository)
        {
            _routeRepository = routeRepository;
        }

        [HttpPost("add")]
        public IActionResult AddRoute([FromBody] Models.Route route)
        {
            if (route == null || string.IsNullOrEmpty(route.Name) || route.Stations == null || !route.Stations.Any())
            {
                return BadRequest("Invalid route data. Route must have a name and at least one station.");
            }

            var addedRoute = _routeRepository.AddRouteAsync(route);
            return CreatedAtAction(nameof(GetRouteById), new { id = addedRoute.Id }, addedRoute);
        }

        [HttpGet("{id}")]
        public IActionResult GetRouteById(int id)
        {
            var route = _routeRepository.GetRouteByIdAsync(id);
            if (route == null)
            {
                return NotFound("Route not found.");
            }
            return Ok(route);
        }

        [HttpGet]
        public IActionResult GetAllRoutes()
        {
            return Ok(_routeRepository.GetAllRoutesAsync());
        }
    }
}
