import paho.mqtt.client as mqtt
import requests
import re

# Use Eclipse Mosquitto public broker
BROKER_IP = "test.mosquitto.org"
BROKER_PORT = 1883
# Subscribe to all carbox topics and the legacy topic
CARBOX_TOPIC = "carbox/#"
LEGACY_TOPIC = "robot/status"

# API endpoint for ride orders (adjust if needed)
API_BASE_URL = "http://localhost:5000/api"

def on_connect(client, userdata, flags, rc):
    if rc == 0:
        print("The Subscriber has successfully connected to the MQTT Broker!")
        client.subscribe(CARBOX_TOPIC)
        print(f"נרשם לנושא: {CARBOX_TOPIC}")
        client.subscribe(LEGACY_TOPIC)
        print(f"נרשם לנושא: {LEGACY_TOPIC}")
    else:
        print(f"חיבור נכשל עם קוד {rc}")

def on_message(client, userdata, msg):
    topic = msg.topic
    payload = msg.payload.decode()
    print(f"הודעה התקבלה בנושא '{topic}': {payload}")
    
    # Handle ride end messages
    if topic.startswith("carbox/ride/") and topic.endswith("/end"):
        # Extract ride ID from topic: carbox/ride/{ride_id}/end
        match = re.match(r"carbox/ride/(\d+)/end", topic)
        if match:
            ride_id = match.group(1)
            print(f"Processing ride end for ID: {ride_id}")
            terminate_ride(ride_id)
        else:
            print(f"Could not extract ride ID from topic: {topic}")

def terminate_ride(ride_id):
    """Terminate a ride by marking it as completed"""
    try:
        # This is a simplified approach - in a real implementation, you'd need to:
        # 1. Get the current ride order
        # 2. Update its status to completed
        # 3. Handle any additional cleanup
        
        print(f"Attempting to terminate ride {ride_id}...")
        print("Note: This Python subscriber is for testing. The C# backend service should handle this in production.")
        
        # You could make an API call here if there was a specific endpoint for terminating rides
        # For now, we'll just log the action
        
    except Exception as e:
        print(f"Error terminating ride {ride_id}: {e}")

client = mqtt.Client(client_id="ServerSubscriber")
client.on_connect = on_connect
client.on_message = on_message

try:
    client.connect(BROKER_IP, BROKER_PORT, 60)
except Exception as e:
    print(f"Error connecting to MQTT broker: {e}")
    exit()

client.loop_forever()