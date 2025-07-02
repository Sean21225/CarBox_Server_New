using System;
using System.Text;
using System.Threading.Tasks;
using carbox.Models;
using carbox.Repositories;
using MQTTnet;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Buffers;

namespace carbox.Services
{
    public class MqttService : BackgroundService
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _mqttClientOptions;
        private readonly CarRepository _carRepository;
        private readonly StationRepository _stationRepository;
        private readonly CarService _carService;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        public MqttService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;

            var mqttFactory = new MqttClientFactory();
            _mqttClient = mqttFactory.CreateMqttClient();

            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithClientId("MQTTServer")
                .WithTcpServer("127.0.0.1", 1883)
                .Build();

            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceived;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _mqttClient.ConnectAsync(_mqttClientOptions, stoppingToken);
                Console.WriteLine("Connected to MQTT broker.");

                var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(f => f.WithTopic("carbox/data/#"))
                    .Build();

                await _mqttClient.SubscribeAsync(subscribeOptions, stoppingToken);
                Console.WriteLine("Subscribed to topic: carbox/data/#");

                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MQTT service: {ex.Message}");
            }
        }

        private async Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var carRepository = scope.ServiceProvider.GetRequiredService<CarRepository>(); 
                var carService = scope.ServiceProvider.GetRequiredService<CarService>(); 

                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload.ToArray());
                Console.WriteLine($"Received message from topic '{topic}': {payload}");

                try
                {
                    var carMassage = JsonConvert.DeserializeObject<CarMassage>(payload);
                    if (carMassage != null)
                    {
                        await UpdateCarLocation(carMassage); // UpdateCarLocation כבר מטפל ב- Scoped Services כמו שצריך
                    }
                    else
                    {
                        Console.WriteLine("Warning: Received an empty or invalid CarMassage object.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing payload: {ex.Message}");
                }
            }
        }


        private async Task UpdateCarLocation(CarMassage update)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var carRepository = scope.ServiceProvider.GetRequiredService<CarRepository>();
                var stationRepository = scope.ServiceProvider.GetRequiredService<StationRepository>();
                var carService = scope.ServiceProvider.GetRequiredService<CarService>();

                var car = await carRepository.GetCarByIdAsync(update.Id);
                if (car != null)
                {
                    car.Location = new Location { Latitude = update.Latitude, Longitude = update.Longitude };

                    var station = await stationRepository.GetStationByIdAsync(update.LastStationID);
                    if (station != null)
                    {
                        car.LastStation = station;
                    }
                    else
                    {
                        Console.WriteLine($"Station ID {update.LastStationID} not found.");
                    }

                    if (Enum.IsDefined(typeof(CarStatus), update.Status))
                    {
                        car.Status = (CarStatus)update.Status;
                    }
                    else
                    {
                        Console.WriteLine($"Invalid status value: {update.Status}");
                    }

                    car.BatteryLevel = update.BatteryLevel;

                    await carRepository.UpdateCarAsync(car);
                    Console.WriteLine($"Updated location for car ID {update.Id}");

                    await carService.UpdateLastStationAsync(update.Id);
                }
                else
                {
                    Console.WriteLine($"Car ID {update.Id} not found in repository.");
                }
            }
        }

        public class CarMassage
        {
            public string Id { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public int LastStationID { get; set; }
            public int Status { get; set; }
            public int BatteryLevel { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}
