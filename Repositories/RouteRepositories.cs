using CarboxBackend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarboxBackend.Repositories
{
    public class RouteRepository
    {
        private readonly IMongoCollection<Models.Route> _routes;
        public RouteRepository(IMongoDatabase database)
        {
            _routes = database.GetCollection<Models.Route>("Routes");
        }


        public async Task<Models.Route> AddRouteAsync(Models.Route route)
        {
            await _routes.InsertOneAsync(route);
            return route;
        }

        public async Task<List<Models.Route>> GetAllRoutesAsync()
        {
            return await _routes.Find(route => true).ToListAsync();
        }

        public async Task<Models.Route> GetRouteByIdAsync(int id)
        {
            return await _routes.Find(route => route.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateRouteAsync(Models.Route route)
        {
            await _routes.ReplaceOneAsync(r => r.Id == route.Id, route);
        }
    }
}

public static class StationDurations
{
    // Matrix[from, to] = travel time in minutes
    public static readonly int[,] Matrix = new int[,]
    {
        // A    B    C    D
        {  0,  14,  30,  47 }, // A
        { 38,   0,  16,  33 }, // B
        { 22,  36,   0,  17 }, // C
        {  5,  19,  35,   0 }  // D
    };
}
