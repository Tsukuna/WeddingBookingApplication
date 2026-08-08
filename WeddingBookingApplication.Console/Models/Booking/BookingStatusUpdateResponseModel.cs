namespace WeddingBookingApplication.Domain.Models.Booking;

public class BookingStatusUpdateResponseModel
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = null!;
}
