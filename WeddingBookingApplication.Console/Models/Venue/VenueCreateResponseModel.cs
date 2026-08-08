namespace WeddingBookingApplication.Domain.Models.Venue;

public class VenueCreateResponseModel
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int VenueId { get; set; }
}
