using WeddingBookingApplication.Domain.Models.Service;
using WeddingBookingApplication.Database.AppDbContextModels;


namespace WeddingBookingApplication.Domain.Features.ServicePackage;

public class ServicePackageService
{
    private readonly AppDbContext _db;

    public ServicePackageService(AppDbContext db)
    {
        _db = db;
    }

    // GET ALL (active packages only)
    public List<ServiceResponseModel> GetServicePackages()
    {
        var packages = _db.TblServicePackages
            .Where(p => p.IsActive)
            .Select(p => MapToResponse(p))
            .ToList();

        return packages;
    }

    // GET BY ID
    public ServiceResponseModel? GetServicePackage(int id)
    {
        var package = _db.TblServicePackages.FirstOrDefault(p => p.ServicePackageId == id);

        return package is null ? null : MapToResponse(package);
    }

    // CREATE
    public ServiceCreateResponseModel CreateServicePackage(ServiceCreateRequestModel request)
    {
        try
        {
            var package = new TblServicePackage
            {
                VendorId    = request.VendorId,
                PackageName = request.PackageName,
                Price       = request.Price,
                Description = request.Description,
                IsActive    = request.IsActive,
                CreatedDate = DateTime.Now
            };

            _db.TblServicePackages.Add(package);
            int result = _db.SaveChanges();

            return new ServiceCreateResponseModel
            {
                IsSuccess = result > 0,
                Message = result > 0 ? "Create Success." : "Create Fail.",
                ServicePackageId = package.ServicePackageId
            };
        }
        catch (Exception ex)
        {
            return new ServiceCreateResponseModel { IsSuccess = false, Message = ex.Message };
        }
    }

    // UPDATE
    public ServiceUpdateResponseModel UpdateServicePackage(int id, ServiceUpdateRequestModel request)
    {
        try
        {
            var package = _db.TblServicePackages.FirstOrDefault(p => p.ServicePackageId == id);
            if (package is null)
                return new ServiceUpdateResponseModel { IsSuccess = false, Message = "Service package not found." };

            package.VendorId    = request.VendorId;
            package.PackageName = request.PackageName;
            package.Price       = request.Price;
            package.Description = request.Description;
            package.IsActive    = request.IsActive;

            int result = _db.SaveChanges();

            return new ServiceUpdateResponseModel
            {
                IsSuccess = result > 0,
                Message = result > 0 ? "Update Success." : "Update Fail."
            };
        }
        catch (Exception ex)
        {
            return new ServiceUpdateResponseModel { IsSuccess = false, Message = ex.Message };
        }
    }

    // DELETE (soft delete — set IsActive = false)
    public ServiceDeleteResponseModel DeleteServicePackage(int id)
    {
        try
        {
            var package = _db.TblServicePackages.FirstOrDefault(p => p.ServicePackageId == id);
            if (package is null)
                return new ServiceDeleteResponseModel { IsSuccess = false, Message = "Service package not found." };

            package.IsActive = false;
            int result = _db.SaveChanges();

            return new ServiceDeleteResponseModel
            {
                IsSuccess = result > 0,
                Message = result > 0 ? "Delete Success." : "Delete Fail."
            };
        }
        catch (Exception ex)
        {
            return new ServiceDeleteResponseModel { IsSuccess = false, Message = ex.Message };
        }
    }

    // ── Mapping helper ──────────────────────────────────────────────────────────
    private static ServiceResponseModel MapToResponse(TblServicePackage p)
    {
        return new ServiceResponseModel
        {
            ServicePackageId = p.ServicePackageId,
            VendorId = p.VendorId,
            PackageName = p.PackageName,
            Price = p.Price,
            Description = p.Description,
            IsActive = p.IsActive,
            CreatedDate = p.CreatedDate
        };
    }
}
