using CarboxBackend.Date;
using CarboxBackend.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CarboxBackend.Repositories
{
    public class RideOrderRepository
    {
        private readonly IMongoCollection<RideOrder> _rideOrdersCollection;

        public RideOrderRepository(MongoDBService mongoDBService)
        {
            _rideOrdersCollection = mongoDBService.Database?.GetCollection<RideOrder>("Orders");
        }

        // Inserts a new ride order into the database
        public async Task CreateRideOrderAsync(RideOrder rideOrder)
        {
            await _rideOrdersCollection.InsertOneAsync(rideOrder);
        }

        // Retrieves a ride order by ID
        public async Task<RideOrder> GetRideByIdAsync(int id)
        {
            return await _rideOrdersCollection.Find(ride => ride.Id == id).FirstOrDefaultAsync();
        }

        // Retrieves all ride orders
        public async Task<List<RideOrder>> GetAllRidesAsync()
        {
            return await _rideOrdersCollection.Find(ride => true).ToListAsync();
        }

        // Updates a ride order (e.g., assigning a car)
        public async Task UpdateRideAsync(RideOrder rideOrder)
        {
            await _rideOrdersCollection.ReplaceOneAsync(r => r.Id == rideOrder.Id, rideOrder);
        }
    }
}
