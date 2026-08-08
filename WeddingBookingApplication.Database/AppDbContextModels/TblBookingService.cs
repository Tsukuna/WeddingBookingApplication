using System;
using System.Collections.Generic;

namespace WeddingBookingApplication.Database.AppDbContextModels;

public partial class TblBookingService
{
    public int BookingServiceId { get; set; }

    public int BookingId { get; set; }

    public int ServicePackageId { get; set; }

    public virtual TblBooking Booking { get; set; } = null!;

    public virtual TblServicePackage ServicePackage { get; set; } = null!;
}
