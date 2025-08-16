using System;
using System.Text;
using System.Threading.Tasks;
using CarboxBackend.Models;
using CarboxBackend.Repositories;
using MQTTnet;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Buffers;

namespace CarboxBackend.Services
{
    public class MqttService : BackgroundService
    {
        private readonly IMqttClient _mqttClient;
        private readonly MqttClientOptions _mqttClientOptions;
        private readonly CarRepository _carRepository;
        private readonly StationRepository _stationRepository;
        private readonly CarService _carService;

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Guid _instanceId = Guid.NewGuid();

        public MqttService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;

            var mqttFactory = new MqttClientFactory();
            _mqttClient = mqttFactory.CreateMqttClient();

            _mqttClientOptions = new MqttClientOptionsBuilder()
                .WithClientId($"MQTTServer-{_instanceId}")
                .WithTcpServer("test.mosquitto.org", 1883) // Eclipse Mosquitto public broker
                .Build();

            _mqttClient.ApplicationMessageReceivedAsync += OnMessageReceived;
            _mqttClient.DisconnectedAsync += async e =>
            {
                Console.WriteLine($"[MqttService {_instanceId}] MQTT client disconnected. Reason: {e.Reason}");
                if (e.Exception != null)
                {
                    Console.WriteLine($"[MqttService {_instanceId}] Exception: {e.Exception.Message}");
                }
                // Optionally, try to reconnect
                await Task.Delay(TimeSpan.FromSeconds(5));
                try
                {
                    await _mqttClient.ConnectAsync(_mqttClientOptions);
                    Console.WriteLine($"[MqttService {_instanceId}] Reconnected to MQTT broker.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MqttService {_instanceId}] Reconnection failed: {ex.Message}");
                }
            };
            Console.WriteLine($"[MqttService {_instanceId}] Constructor called.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _mqttClient.ConnectAsync(_mqttClientOptions, stoppingToken);
                Console.WriteLine("Connected to MQTT broker.");

                var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                    .WithTopicFilter(f => f.WithTopic("carbox/data/#"))
                    .WithTopicFilter(f => f.WithTopic("carbox/ride/+/end"))
                    .Build();

                await _mqttClient.SubscribeAsync(subscribeOptions, stoppingToken);
                Console.WriteLine("Subscribed to topics: carbox/data/# and carbox/ride/+/end");

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

        public async Task SubscribeToEndRideTopicAsync(string rideId, CancellationToken cancellationToken = default)
        {
            var topic = $"carbox/ride/{rideId}/end";
            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter(f => f.WithTopic(topic))
                .Build();
            await _mqttClient.SubscribeAsync(subscribeOptions, cancellationToken);
            Console.WriteLine($"[MqttService {_instanceId}] Subscribed to end ride topic: {topic}");
        }

        public async Task UnsubscribeFromEndRideTopicAsync(string rideId, CancellationToken cancellationToken = default)
        {
            var topic = $"carbox/ride/{rideId}/end";
            var unsubscribeOptions = new MqttClientUnsubscribeOptionsBuilder()
                .WithTopicFilter(topic)
                .Build();
            await _mqttClient.UnsubscribeAsync(unsubscribeOptions, cancellationToken);
            Console.WriteLine($"[MqttService {_instanceId}] Unsubscribed from end ride topic: {topic}");
        }

        private async Task CompleteRideAsync(int rideId, RideOrderRepository rideOrderRepository, CarRepository carRepository)
        {
            try
            {
                Console.WriteLine($"[MqttService {_instanceId}] Starting ride completion process for ride ID: {rideId}");
                
                // Get the ride order
                var rideOrder = await rideOrderRepository.GetRideByIdAsync(rideId);
                if (rideOrder == null)
                {
                    Console.WriteLine($"[MqttService {_instanceId}] No ride order found with ID: {rideId}");
                    return;
                }

                Console.WriteLine($"[MqttService {_instanceId}] Found ride order: ID={rideOrder.Id}, Status={rideOrder.Status}, AssignedCarId={rideOrder.AssignedCarId}");

                // Check if ride is already completed
                if (rideOrder.Status == RideOrderStatus.Completed)
                {
                    Console.WriteLine($"[MqttService {_instanceId}] Ride {rideId} was already completed.");
                    return;
                }

                // Update ride status to completed
                rideOrder.Status = RideOrderStatus.Completed;
                await rideOrderRepository.UpdateRideAsync(rideOrder);
                Console.WriteLine($"[MqttService {_instanceId}] Ride {rideId} marked as completed.");

                // Update car status to Available if a car is assigned
                if (!string.IsNullOrEmpty(rideOrder.AssignedCarId))
                {
                    var car = await carRepository.GetCarByIdAsync(rideOrder.AssignedCarId);
                    if (car != null)
                    {
                        Console.WriteLine($"[MqttService {_instanceId}] Found assigned car: ID={car.Id}, Status={car.Status}");
                        
                        // Set car status to Available
                        car.Status = CarStatus.Available;
                        await carRepository.UpdateCarAsync(car);
                        Console.WriteLine($"[MqttService {_instanceId}] Car {car.Id} status updated to Available.");
                    }
                    else
                    {
                        Console.WriteLine($"[MqttService {_instanceId}] Assigned car {rideOrder.AssignedCarId} not found.");
                    }
                }

                // Unsubscribe from the specific ride's MQTT topic
                await UnsubscribeFromEndRideTopicAsync(rideId.ToString());
                Console.WriteLine($"[MqttService {_instanceId}] Completed ride termination process for ride {rideId}. No further MQTT communication needed for this ride.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MqttService {_instanceId}] Error completing ride {rideId}: {ex.Message}");
            }
        }

        private async Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var carRepository = scope.ServiceProvider.GetRequiredService<CarRepository>(); 
                var carService = scope.ServiceProvider.GetRequiredService<CarService>(); 
                var rideOrderRepository = scope.ServiceProvider.GetRequiredService<RideOrderRepository>();

                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload.ToArray());
                Console.WriteLine($"[MqttService {_instanceId}] Received message from topic '{topic}': {payload}");

                // Handle end ride message
                if (topic.StartsWith("carbox/ride/") && topic.EndsWith("/end"))
                {
                    Console.WriteLine($"[MqttService {_instanceId}] Processing end ride message for topic: {topic}");
                    // Extract rideId from topic
                    var parts = topic.Split('/');
                    Console.WriteLine($"[MqttService {_instanceId}] Topic parts: {string.Join(", ", parts)}");
                    if (parts.Length >= 4)
                    {
                        var rideIdStr = parts[2];
                        Console.WriteLine($"[MqttService {_instanceId}] Extracted ride ID string: {rideIdStr}");
                        if (int.TryParse(rideIdStr, out int rideId))
                        {
                            Console.WriteLine($"[MqttService {_instanceId}] Parsed ride ID: {rideId}");
                            await CompleteRideAsync(rideId, rideOrderRepository, carRepository);
                        }
                        else
                        {
                            Console.WriteLine($"[MqttService {_instanceId}] Failed to parse ride ID from string: {rideIdStr}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[MqttService {_instanceId}] Invalid topic format. Expected at least 4 parts, got {parts.Length}");
                    }
                    return;
                }

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

        public async Task PublishMessageAsync(string topic, string message)
        {
            try
            {
                if (_mqttClient.IsConnected)
                {
                    var mqttMessage = new MqttApplicationMessageBuilder()
                        .WithTopic(topic)
                        .WithPayload(message)
                        .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                        .Build();

                    await _mqttClient.PublishAsync(mqttMessage);
                    Console.WriteLine($"[MqttService {_instanceId}] Published message to topic '{topic}': {message}");
                }
                else
                {
                    Console.WriteLine($"[MqttService {_instanceId}] MQTT client is not connected. Cannot publish message.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[MqttService {_instanceId}] Error publishing MQTT message: {ex.Message}");
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