namespace WeddingBookingApplication.Domain.Models.Service;

public class ServiceCreateResponseModel
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ServicePackageId { get; set; }
}
