using System;
using System.Collections.Generic;

namespace WeddingBookingApplication.Database.AppDbContextModels;

public partial class TblVenue
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

    public virtual ICollection<TblBooking> TblBookings { get; set; } = new List<TblBooking>();

    public virtual TblVendor Vendor { get; set; } = null!;
}
