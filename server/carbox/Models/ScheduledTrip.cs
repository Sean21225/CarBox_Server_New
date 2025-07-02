namespace carbox.Models
{
    public class ScheduledTrip
    {
        public int Id { get; set; } // מזהה ייחודי לנסיעה
        public int RideOrderId { get; set; } // מזהה ההזמנה הקשורה
        public DateTime StartTime { get; set; } // זמן התחלה מתוכנן
        public DateTime EndTime { get; set; } // זמן סיום משוער
        public List<Station> RouteStations { get; set; } = new List<Station>(); // מסלול הנסיעה
        public TripStatus Status { get; set; } = TripStatus.Scheduled; // סטטוס הנסיעה
    }

    public enum TripStatus
    {
        Scheduled,   // מתוזמן
        InProgress,  // בתהליך
        Completed,   // הושלם
        Cancelled    // בוטל
    }
}
