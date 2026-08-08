namespace WeddingBookingApplication.Domain.Models.Booking;

public enum BookingStatus : byte
{
    Pending = 1,      // Waiting for vendor approval
    Approved = 2,     // Approved by vendor, event is pending
    Rejected = 3,     // Rejected by vendor
    Cancelled = 4,    // Cancelled by customer
    Completed = 5     // Event completed
}
