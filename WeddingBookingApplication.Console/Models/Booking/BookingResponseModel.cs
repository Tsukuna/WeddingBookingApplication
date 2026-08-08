using System;
using System.Collections.Generic;
using WeddingBookingApplication.Domain.Models.Decoration;
using WeddingBookingApplication.Domain.Models.Service;

namespace WeddingBookingApplication.Domain.Models.Booking;

public class BookingResponseModel
{
    public int BookingId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string CustomerPhone { get; set; } = null!;
    public string? CustomerEmail { get; set; }
    public int VendorId { get; set; }
    public string VendorName { get; set; } = null!;
    public int VenueId { get; set; }
    public string VenueName { get; set; } = null!;
    public DateOnly BookingDate { get; set; }
    public int GuestCount { get; set; }
    public decimal TotalAmount { get; set; }
    public byte Status { get; set; }
    public string StatusName { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public List<DecorationResponseModel> Decorations { get; set; } = new();
    public List<ServiceResponseModel> Services { get; set; } = new();
}
