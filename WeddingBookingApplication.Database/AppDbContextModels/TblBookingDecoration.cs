using System;
using System.Collections.Generic;

namespace WeddingBookingApplication.Database.AppDbContextModels;

public partial class TblBookingDecoration
{
    public int BookingDecorationId { get; set; }

    public int BookingId { get; set; }

    public int DecorationPackageId { get; set; }

    public virtual TblBooking Booking { get; set; } = null!;

    public virtual TblDecorationPackage DecorationPackage { get; set; } = null!;
}
