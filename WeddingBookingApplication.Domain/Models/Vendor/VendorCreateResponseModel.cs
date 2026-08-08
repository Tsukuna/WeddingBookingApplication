namespace WeddingBookingApplication.Domain.Models.Vendor;

public class VendorCreateResponseModel
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int VendorId { get; set; }
}
