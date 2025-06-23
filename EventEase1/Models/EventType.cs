namespace EventEase1.Models
{
    public class EventType
    {
        public int EventTypeID { get; set; }
        public string Name { get; set; }

        // Optional: Add reverse navigation if needed
        public ICollection<Event>? Events { get; set; }
    }
}
