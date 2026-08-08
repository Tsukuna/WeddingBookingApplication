using System;
using System.Collections.Generic;

namespace WeddingBookingApplication.Database.AppDbContextModels;

public partial class TblBooking
{
    public int BookingId { get; set; }

    public string CustomerName { get; set; } = null!;

    public string CustomerPhone { get; set; } = null!;

    public string? CustomerEmail { get; set; }

    public int VendorId { get; set; }

    public int VenueId { get; set; }

    public DateOnly BookingDate { get; set; }

    public int GuestCount { get; set; }

    public decimal TotalAmount { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<TblBookingDecoration> TblBookingDecorations { get; set; } = new List<TblBookingDecoration>();

    public virtual ICollection<TblBookingService> TblBookingServices { get; set; } = new List<TblBookingService>();

    public virtual TblVendor Vendor { get; set; } = null!;

    public virtual TblVenue Venue { get; set; } = null!;
}
