namespace WeddingBookingApplication.Domain.Models.Decoration;

public class DecorationCreateRequestModel
{
    public int VendorId { get; set; }
    public string PackageName { get; set; } = null!;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
