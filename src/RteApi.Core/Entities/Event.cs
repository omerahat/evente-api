namespace RteApi.Core.Entities;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OrganizerName { get; set; } = string.Empty;
    public DateTime EventTime { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public decimal? LocationLat { get; set; }
    public decimal? LocationLon { get; set; }
    
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public int? CreatedByAdminId { get; set; }
    public User? CreatedByAdmin { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
    public ICollection<EventReview> Reviews { get; set; } = new List<EventReview>();
}

