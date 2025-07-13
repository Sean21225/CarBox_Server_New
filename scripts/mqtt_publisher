import paho.mqtt.client as mqtt
import time

# --- MQTT Broker Details ---
BROKER_ADDRESS = "test.mosquitto.org"  # Mosquitto public broker
PORT = 1883                            # Unencrypted standard port
TOPIC = "fleet/123/command"                # Topic to publish robot data

# --- Connection Callback ---
def on_connect(client, userdata, flags, rc):
    if rc == 0:
        print("Connected to MQTT Broker successfully!")
    else:
        print(f"Failed to connect, return code {rc}")

# --- MQTT Client Setup ---
client = mqtt.Client("RobotPublisher")
client.on_connect = on_connect

# --- Attempt Connection ---
try:
    client.connect(BROKER_ADDRESS, PORT, 60)
except Exception as e:
    print(f"Error connecting to broker: {e}")
    exit()

# --- Start MQTT Background Loop ---
client.loop_start()

# --- Periodic Publishing Loop ---
message_counter = 0
while True:
    message = f"Hello from Robot! Message {message_counter}"
    client.publish(TOPIC, message)
    print(f"Published message: '{message}' to topic '{TOPIC}'")
    message_counter += 1
    time.sleep(5)
