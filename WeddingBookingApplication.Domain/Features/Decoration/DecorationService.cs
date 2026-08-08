using WeddingBookingApplication.Database.AppDbContextModels;
using WeddingBookingApplication.Domain.Models.Decoration;

namespace WeddingBookingApplication.Domain.Features.Decoration;

public class DecorationService
{
    private readonly AppDbContext _db;

    public DecorationService(AppDbContext db)
    {
        _db = db;
    }

    // GET ALL (active packages only)
    public List<DecorationResponseModel> GetDecorationPackages()
    {
        List<DecorationResponseModel> packages = _db.TblDecorationPackages
            .Where(p => p.IsActive)
            .Select(p => MapToResponse(p))
            .ToList();

        return packages;
    }

    // GET BY ID
    public DecorationResponseModel? GetDecorationPackage(int id)
    {
        var package = _db.TblDecorationPackages.FirstOrDefault(p => p.DecorationPackageId == id);

        return package is null ? null : MapToResponse(package);
    }

    // CREATE
    public DecorationCreateResponseModel CreateDecorationPackage(DecorationCreateRequestModel request)
    {
        try
        {
            var package = new TblDecorationPackage
            {
                VendorId    = request.VendorId,
                PackageName = request.PackageName,
                Price       = request.Price,
                Description = request.Description,
                IsActive    = request.IsActive,
                CreatedDate = DateTime.Now
            };

            _db.TblDecorationPackages.Add(package);
            int result = _db.SaveChanges();

            return new DecorationCreateResponseModel
            {
                IsSuccess = result > 0,
                Message = result > 0 ? "Create Success." : "Create Fail.",
                DecorationPackageId = package.DecorationPackageId
            };
        }
        catch (Exception ex)
        {
            return new DecorationCreateResponseModel { IsSuccess = false, Message = ex.Message };
        }
    }

    // UPDATE
    public DecorationUpdateResponseModel UpdateDecorationPackage(int id, DecorationUpdateRequestModel request)
    {
        try
        {
            var package = _db.TblDecorationPackages.FirstOrDefault(p => p.DecorationPackageId == id);

            if (package is null)
                return new DecorationUpdateResponseModel { IsSuccess = false, Message = "Decoration package not found." };

            package.VendorId    = request.VendorId;
            package.PackageName = request.PackageName;
            package.Price       = request.Price;
            package.Description = request.Description;
            package.IsActive    = request.IsActive;

            int result = _db.SaveChanges();

            return new DecorationUpdateResponseModel
            {
                IsSuccess = result > 0,
                Message = result > 0 ? "Update Success." : "Update Fail."
            };
        }
        catch (Exception ex)
        {
            return new DecorationUpdateResponseModel { IsSuccess = false, Message = ex.Message };
        }
    }

    // DELETE (soft delete — set IsActive = false)
    public DecorationDeleteResponseModel DeleteDecorationPackage(int id)
    {
        try
        {
            var package = _db.TblDecorationPackages.FirstOrDefault(p => p.DecorationPackageId == id);

            if (package is null)
                return new DecorationDeleteResponseModel { IsSuccess = false, Message = "Decoration package not found." };

            package.IsActive = false;
            int result = _db.SaveChanges();

            return new DecorationDeleteResponseModel
            {
                IsSuccess = result > 0,
                Message = result > 0 ? "Delete Success." : "Delete Fail."
            };
        }
        catch (Exception ex)
        {
            return new DecorationDeleteResponseModel { IsSuccess = false, Message = ex.Message };
        }
    }

    // ── Mapping helper ──────────────────────────────────────────────────────────
    private static DecorationResponseModel MapToResponse(TblDecorationPackage p)
    {
        return new DecorationResponseModel
        {
            DecorationPackageId = p.DecorationPackageId,
            VendorId = p.VendorId,
            PackageName = p.PackageName,
            Price = p.Price,
            Description = p.Description,
            IsActive = p.IsActive,
            CreatedDate = p.CreatedDate
        };
    }
}
