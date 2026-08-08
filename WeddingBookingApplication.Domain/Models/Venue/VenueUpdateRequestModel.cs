namespace WeddingBookingApplication.Domain.Models.Venue;

public class VenueUpdateRequestModel
{
    public string VenueName { get; set; } = null!;
    public string Location { get; set; } = null!;
    public int Capacity { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
