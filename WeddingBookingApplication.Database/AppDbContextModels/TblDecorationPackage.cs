using System;
using System.Collections.Generic;

namespace WeddingBookingApplication.Database.AppDbContextModels;

public partial class TblDecorationPackage
{
    public int DecorationPackageId { get; set; }

    public int VendorId { get; set; }

    public string PackageName { get; set; } = null!;

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<TblBookingDecoration> TblBookingDecorations { get; set; } = new List<TblBookingDecoration>();

    public virtual TblVendor Vendor { get; set; } = null!;
}
