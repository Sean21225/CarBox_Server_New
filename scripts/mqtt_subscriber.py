import paho.mqtt.client as mqtt

# Use Eclipse Mosquitto public broker
BROKER_IP = "test.mosquitto.org"
BROKER_PORT = 1883
# Subscribe to all carbox topics and the legacy topic
CARBOX_TOPIC = "carbox/#"
LEGACY_TOPIC = "robot/status"

# Rest of your code remains the same...
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
    print(f"הודעה התקבלה בנושא '{msg.topic}': {msg.payload.decode()}")

client = mqtt.Client(client_id="ServerSubscriber")
client.on_connect = on_connect
client.on_message = on_message

try:
    client.connect(BROKER_IP, BROKER_PORT, 60)
except Exception as e:
    print(f"Error connecting to MQTT broker: {e}")
    exit()

client.loop_forever()