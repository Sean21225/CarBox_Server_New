import paho.mqtt.client as mqtt

# הגדרות חיבור ל-Broker (השרת עצמו)
BROKER_IP = "216.24.57.252"
BROKER_PORT = 1883
TOPIC = "robot/status" # אותו נושא כמו ב-Publisher

# פונקציה שתופעל כאשר ה-Subscriber מתחבר בהצלחה ל-Broker
def on_connect(client, userdata, flags, rc):
    if rc == 0:
        print("The Subscriber has successfully connected to the MQTT Broker!")
        # נרשם לנושא הרצוי
        client.subscribe(TOPIC)
        print(f"נרשם לנושא: {TOPIC}")
    else:
        print(f"חיבור נכשל עם קוד {rc}")

# פונקציה שתופעל כאשר מתקבלת הודעה
def on_message(client, userdata, msg):
    print(f"הודעה התקבלה בנושא '{msg.topic}': {msg.payload.decode()}")

# יצירת מופע של לקוח MQTT
client = mqtt.Client(client_id="ServerSubscriber") # שם ייחודי ללקוח

# הגדרת פונקציות Callback
client.on_connect = on_connect
client.on_message = on_message

# ניסיון התחברות ל-Broker
try:
    
    client.connect(BROKER_IP, BROKER_PORT, 60)
except Exception as e:
    print(f"Error connecting to MQTT broker: {e}")
    exit()

# התחלת לולאת ה-MQTT באופן בלוקי (חוסמת)
# זה ישאיר את התוכנית רצה ומקשיבה להודעות
client.loop_forever()