namespace CarboxBackend.Date
{
    using MongoDB.Driver;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    

    public class MongoDBService
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoDatabase? _database;

        public MongoDBService(IConfiguration config)
        {
            _configuration = config;

            var connectionString = _configuration.GetConnectionString("DbConnection");
            var mongoUrl = MongoUrl.Create(connectionString);
            var mongoClient = new MongoClient(mongoUrl);
            _database = mongoClient.GetDatabase("carboxDB");
            Console.WriteLine("_database: " + _database);
        }

        public IMongoDatabase? Database => _database;
    }

}
