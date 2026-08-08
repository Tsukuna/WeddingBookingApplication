namespace WeddingBookingApplication.Domain.Models.Venue;

public class VenueResponseModel
{
    public int VenueId { get; set; }
    public int VendorId { get; set; }
    public string VenueName { get; set; } = null!;
    public string Location { get; set; } = null!;
    public int Capacity { get; set; }
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
