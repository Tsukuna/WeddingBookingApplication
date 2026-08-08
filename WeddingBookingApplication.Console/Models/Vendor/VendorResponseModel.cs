namespace WeddingBookingApplication.Domain.Models.Vendor;

public class VendorResponseModel
{
    public int VendorId { get; set; }
    public string VendorName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? Address { get; set; }
    public string? Description { get; set; }
    public byte Status { get; set; }
    public DateTime CreatedDate { get; set; }
}
