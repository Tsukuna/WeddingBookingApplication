using WeddingBookingApplication.Domain.Models.Venue;
using WeddingBookingApplication.Database.AppDbContextModels;


namespace WeddingBookingApplication.Domain.Features.Venue;

public class VenueService
{
    private readonly AppDbContext _db;

    public VenueService(AppDbContext db)
    {
        _db = db;
    }

    // GET ALL (active venues only)
    public List<VenueResponseModel> GetVenues()
    {
        var venues = _db.TblVenues
            .Where(v => v.IsActive)
            .Select(v => MapToResponse(v))
            .ToList();

        return venues;
    }

    // GET BY ID
    public VenueResponseModel? GetVenue(int id)
    {
        var venue = _db.TblVenues.FirstOrDefault(v => v.VenueId == id);

        return venue is null ? null : MapToResponse(venue);
    }

    // CREATE
    public VenueCreateResponseModel CreateVenue(VenueCreateRequestModel request)
    {
        try
        {
            var venue = new TblVenue
            {
                VendorId    = request.VendorId,
                VenueName   = request.VenueName,
                Location    = request.Location,
                Capacity    = request.Capacity,
                Price       = request.Price,
                Description = request.Description,
                IsActive    = request.IsActive,
                CreatedDate = DateTime.Now
            };

            _db.TblVenues.Add(venue);
            int result = _db.SaveChanges();

            return new VenueCreateResponseModel
            {
                IsSuccess = result > 0,
                Message = result > 0 ? "Create Success." : "Create Fail.",
                VenueId = venue.VenueId
            };
        }
        catch (Exception ex)
        {
            return new VenueCreateResponseModel { IsSuccess = false, Message = ex.Message };
        }
    }

    // UPDATE
    public VenueUpdateResponseModel UpdateVenue(int id, VenueUpdateRequestModel request)
    {
        try
        {
            var venue = _db.TblVenues.FirstOrDefault(v => v.VenueId == id);
            if (venue is null)
                return new VenueUpdateResponseModel { IsSuccess = false, Message = "Venue not found." };

            venue.VenueName   = request.VenueName;
            venue.Location    = request.Location;
            venue.Capacity    = request.Capacity;
            venue.Price       = request.Price;
            venue.Description = request.Description;
            venue.IsActive    = request.IsActive;

            int result = _db.SaveChanges();

            return new VenueUpdateResponseModel
            {
                IsSuccess = result > 0,
                Message = result > 0 ? "Update Success." : "Update Fail."
            };
        }
        catch (Exception ex)
        {
            return new VenueUpdateResponseModel { IsSuccess = false, Message = ex.Message };
        }
    }

    // DELETE (soft delete — set IsActive = false)
    public VenueDeleteResponseModel DeleteVenue(int id)
    {
        try
        {
            var venue = _db.TblVenues.FirstOrDefault(v => v.VenueId == id);
            if (venue is null)
                return new VenueDeleteResponseModel { IsSuccess = false, Message = "Venue not found." };

            venue.IsActive = false;
            int result = _db.SaveChanges();

            return new VenueDeleteResponseModel
            {
                IsSuccess = result > 0,
                Message = result > 0 ? "Delete Success." : "Delete Fail."
            };
        }
        catch (Exception ex)
        {
            return new VenueDeleteResponseModel { IsSuccess = false, Message = ex.Message };
        }
    }

    // ── Mapping helper ──────────────────────────────────────────────────────────
    private static VenueResponseModel MapToResponse(TblVenue v)
    {
        return new VenueResponseModel
        {
            VenueId = v.VenueId,
            VendorId = v.VendorId,
            VenueName = v.VenueName,
            Location = v.Location,
            Capacity = v.Capacity,
            Price = v.Price,
            Description = v.Description,
            IsActive = v.IsActive,
            CreatedDate = v.CreatedDate
        };
    }
}
