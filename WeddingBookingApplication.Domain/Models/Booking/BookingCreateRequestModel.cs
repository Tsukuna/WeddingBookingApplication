using System;
using System.Collections.Generic;

namespace WeddingBookingApplication.Domain.Models.Booking;

public class BookingCreateRequestModel
{
    public string CustomerName { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public string? CustomerEmail { get; set; }
    public int VendorId { get; set; }
    public int VenueId { get; set; }
    public DateOnly BookingDate { get; set; }
    public int GuestCount { get; set; }
    public List<int> DecorationPackageIds { get; set; } = new();
    public List<int> ServicePackageIds { get; set; } = new();
}
