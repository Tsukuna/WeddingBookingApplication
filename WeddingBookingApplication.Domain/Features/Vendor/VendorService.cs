using WeddingBookingApplication.Domain.Models.Vendor;
using WeddingBookingApplication.Database.AppDbContextModels;
namespace WeddingBookingApplication.Domain.Features.Vendor;

public class VendorService
{
    private readonly AppDbContext _db;

    public VendorService(AppDbContext db)
    {
        _db = db;
    }

    // GET ALL (active vendors only)
    public List<VendorResponseModel> GetVendors()
    {
        var vendors = _db.TblVendors
            .Where(v => v.Status != 0)
            .Select(v => MapToResponse(v))
            .ToList();

        return vendors;
    }

    // GET BY ID
    public VendorResponseModel? GetVendor(int id)
    {
        var vendor = _db.TblVendors.FirstOrDefault(v => v.VendorId == id);

        return vendor is null ? null : MapToResponse(vendor);
    }

    // CREATE
    public VendorCreateResponseModel CreateVendor(VendorCreateRequestModel request)
    {
        try
        {
            var vendor = new TblVendor
            {
                VendorName  = request.VendorName,
                Email       = request.Email,
                Phone       = request.Phone,
                Address     = request.Address,
                Description = request.Description,
                Status      = request.Status,
                CreatedDate = DateTime.Now
            };

            _db.TblVendors.Add(vendor);
            int result = _db.SaveChanges();

            return new VendorCreateResponseModel
            {
                IsSuccess = result > 0,
                Message = result > 0 ? "Create Success." : "Create Fail.",
                VendorId = vendor.VendorId
            };
        }
        catch (Exception ex)
        {
            return new VendorCreateResponseModel { IsSuccess = false, Message = ex.Message };
        }
    }

    // UPDATE
    public VendorUpdateResponseModel UpdateVendor(int id, VendorUpdateRequestModel request)
    {
        try
        {
            var vendor = _db.TblVendors.FirstOrDefault(v => v.VendorId == id);
            if (vendor is null)
                return new VendorUpdateResponseModel { IsSuccess = false, Message = "Vendor not found." };

            vendor.VendorName  = request.VendorName;
            vendor.Email       = request.Email;
            vendor.Phone       = request.Phone;
            vendor.Address     = request.Address;
            vendor.Description = request.Description;
            vendor.Status      = request.Status;

            int result = _db.SaveChanges();

            return new VendorUpdateResponseModel
            {
                IsSuccess = result > 0,
                Message = result > 0 ? "Update Success." : "Update Fail."
            };
        }
        catch (Exception ex)
        {
            return new VendorUpdateResponseModel { IsSuccess = false, Message = ex.Message };
        }
    }

    // DELETE (soft delete — set Status = 0)
    public VendorDeleteResponseModel DeleteVendor(int id)
    {
        try
        {
            var vendor = _db.TblVendors.FirstOrDefault(v => v.VendorId == id);
            if (vendor is null)
                return new VendorDeleteResponseModel { IsSuccess = false, Message = "Vendor not found." };

            vendor.Status = 0;
            int result = _db.SaveChanges();

            return new VendorDeleteResponseModel
            {
                IsSuccess = result > 0,
                Message = result > 0 ? "Delete Success." : "Delete Fail."
            };
        }
        catch (Exception ex)
        {
            return new VendorDeleteResponseModel { IsSuccess = false, Message = ex.Message };
        }
    }

    // ── Mapping helper ──────────────────────────────────────────────────────────
    private static VendorResponseModel MapToResponse(TblVendor v)
    {
        return new VendorResponseModel
        {
            VendorId = v.VendorId,
            VendorName = v.VendorName,
            Email = v.Email,
            Phone = v.Phone,
            Address = v.Address,
            Description = v.Description,
            Status = v.Status,
            CreatedDate = v.CreatedDate
        };
    }
}
