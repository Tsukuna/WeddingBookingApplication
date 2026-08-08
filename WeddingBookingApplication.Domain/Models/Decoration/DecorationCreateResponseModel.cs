namespace WeddingBookingApplication.Domain.Models.Decoration;

public class DecorationCreateResponseModel
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int DecorationPackageId { get; set; }
}
