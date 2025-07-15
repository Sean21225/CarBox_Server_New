
import paho.mqtt.client as mqtt
import json
import time
from datetime import datetime, timezone

# Connection settings for Broker (same as subscriber)
BROKER_IP = "test.mosquitto.org"
BROKER_PORT = 1883

# Test data - simulating what the backend would publish
test_car_commands = [
    {
        "CarId": "car123",
        "Command": "STATUS_UPDATE",
        "NewStatus": "Occupied"
        # Timestamp will be added at publish time
    },
    {
        "CarId": "car456", 
        "Command": "STATUS_UPDATE",
        "NewStatus": "Available"
        # Timestamp will be added at publish time
    },
    {
        "CarId": "car789",
        "Command": "STATUS_UPDATE", 
        "NewStatus": "InMaintenance"
        # Timestamp will be added at publish time
    }
]

# Function called when publisher connects successfully to broker
def on_connect(client, userdata, flags, rc):
    if rc == 0:
        print("Publisher successfully connected to MQTT Broker!")
        print(f"Connected to broker at {BROKER_IP}:{BROKER_PORT}")
    else:
        print(f"Connection failed with code {rc}")

# Function called when message is published
def on_publish(client, userdata, mid):
    print(f"Message published successfully (Message ID: {mid})")

# Create MQTT client instance
client = mqtt.Client(client_id="TestPublisher")

# Set callback functions
client.on_connect = on_connect
client.on_publish = on_publish

# Try to connect to broker
try:
    print(f"Attempting to connect to MQTT broker at {BROKER_IP}:{BROKER_PORT}")
    client.connect(BROKER_IP, BROKER_PORT, 60)
    client.loop_start()  # Start background loop for processing network traffic
    
    # Wait a moment for connection to establish
    time.sleep(2)
    
    # Publish test messages
    print("\n--- Publishing Test Messages ---")
    
    for i, command in enumerate(test_car_commands):
        # Add a fresh timestamp for each message
        command_to_send = command.copy()
        command_to_send["Timestamp"] = datetime.now(timezone.utc).isoformat()
        # If your backend expects different fields or topic structure, adjust here
        topic = f"carbox/commands/{command_to_send['CarId']}"
        message = json.dumps(command_to_send)
        
        print(f"\nTest {i+1}:")
        print(f"Topic: {topic}")
        print(f"Message: {message}")
        
        # Publish the message
        result = client.publish(topic, message, qos=0)
        
        if result.rc == mqtt.MQTT_ERR_SUCCESS:
            print("✓ Message queued for publishing")
        else:
            print(f"✗ Failed to queue message: {result.rc}")
        
        # Wait between messages
        time.sleep(2)
    
    # Also test publishing to the original robot/status topic for compatibility
    print(f"\n--- Testing Legacy Topic ---")
    legacy_message = {
        "status": "test_from_publisher",
        "timestamp": datetime.now(timezone.utc).isoformat(),
        "message": "Testing MQTT publishing functionality"
    }
    
    topic = "robot/status"
    message = json.dumps(legacy_message)
    
    print(f"Topic: {topic}")
    print(f"Message: {message}")
    
    result = client.publish(topic, message, qos=0)
    if result.rc == mqtt.MQTT_ERR_SUCCESS:
        print("✓ Legacy message queued for publishing")
    else:
        print(f"✗ Failed to queue legacy message: {result.rc}")
    
    # Wait for all messages to be sent
    print("\nWaiting for messages to be sent...")
    time.sleep(3)
    
    print("\n--- Test Complete ---")
    print("If you have the subscriber script running, you should see these messages.")
    print("To test with your backend, trigger a status update via the /api/StartStop endpoint.")
    
except Exception as e:
    print(f"Error connecting to MQTT broker: {e}")
    exit()

finally:
    client.loop_stop()
    client.disconnect()
    print("Disconnected from MQTT broker.")
