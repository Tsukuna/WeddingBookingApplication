namespace WeddingBookingApplication.Domain.Models.Booking;

public class BookingCreateResponseModel
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = null!;
    public int? BookingId { get; set; }
}
