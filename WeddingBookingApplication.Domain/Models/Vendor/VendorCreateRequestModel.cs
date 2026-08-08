namespace WeddingBookingApplication.Domain.Models.Vendor;

public class VendorCreateRequestModel
{
    public string VendorName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Address { get; set; }
    public string? Description { get; set; }
    public byte Status { get; set; }
}
