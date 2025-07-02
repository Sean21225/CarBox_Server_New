namespace carbox.Models
{
    public class RideOrder
    {
        public int Id { get; set; }  // מזהה הזמנה
        public int UserId { get; set; }
        public Station source { get; set; }
        public Station Destination { get; set; }
        public DateTime RideTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public RideOrderStatus Status { get; set; } = RideOrderStatus.Open;
        public string? AssignedCarId { get; set; } // מזהה הרכב שהוקצה
    }

    public enum RideOrderStatus
    {
        Open = 0,
        Assigned = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4
    }
}
