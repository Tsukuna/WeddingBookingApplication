using System;
using System.Collections.Generic;

namespace WeddingBookingApplication.Database.AppDbContextModels;

public partial class TblVendor
{
    public int VendorId { get; set; }

    public string VendorName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Address { get; set; }

    public string? Description { get; set; }

    public byte Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<TblBooking> TblBookings { get; set; } = new List<TblBooking>();

    public virtual ICollection<TblDecorationPackage> TblDecorationPackages { get; set; } = new List<TblDecorationPackage>();

    public virtual ICollection<TblServicePackage> TblServicePackages { get; set; } = new List<TblServicePackage>();

    public virtual ICollection<TblVenue> TblVenues { get; set; } = new List<TblVenue>();
}
