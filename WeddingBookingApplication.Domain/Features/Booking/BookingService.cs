using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WeddingBookingApplication.Database.AppDbContextModels;
using WeddingBookingApplication.Domain.Models.Booking;
using WeddingBookingApplication.Domain.Models.Decoration;
using WeddingBookingApplication.Domain.Models.Service;

namespace WeddingBookingApplication.Domain.Features.Booking;

public class BookingService
{
    private readonly AppDbContext _db;

    public BookingService(AppDbContext db)
    {
        _db = db;
    }

    // CREATE BOOKING
    public BookingCreateResponseModel CreateBooking(BookingCreateRequestModel request)
    {
        try
        {
            // 1. Validate Vendor
            var vendor = _db.TblVendors.FirstOrDefault(v => v.VendorId == request.VendorId && v.Status != 0);
            if (vendor == null)
            {
                return new BookingCreateResponseModel { IsSuccess = false, Message = "Vendor not found or inactive." };
            }

            // 2. Validate Venue
            var venue = _db.TblVenues.FirstOrDefault(v => v.VenueId == request.VenueId && v.IsActive);
            if (venue == null)
            {
                return new BookingCreateResponseModel { IsSuccess = false, Message = "Venue not found or inactive." };
            }
            if (venue.VendorId != request.VendorId)
            {
                return new BookingCreateResponseModel { IsSuccess = false, Message = "The selected venue does not belong to the selected vendor." };
            }

            // Calculate base total amount with Venue price
            decimal totalAmount = venue.Price;

            // 3. Validate and load Decoration Packages
            var decorationPackages = new List<TblDecorationPackage>();
            foreach (var decId in request.DecorationPackageIds)
            {
                var decPkg = _db.TblDecorationPackages.FirstOrDefault(dp => dp.DecorationPackageId == decId && dp.IsActive);
                if (decPkg == null)
                {
                    return new BookingCreateResponseModel { IsSuccess = false, Message = $"Decoration Package ID {decId} not found or inactive." };
                }
                if (decPkg.VendorId != request.VendorId)
                {
                    return new BookingCreateResponseModel { IsSuccess = false, Message = $"Decoration Package ID {decId} does not belong to the selected vendor." };
                }
                decorationPackages.Add(decPkg);
                totalAmount += decPkg.Price;
            }

            // 4. Validate and load Service Packages
            var servicePackages = new List<TblServicePackage>();
            foreach (var srvId in request.ServicePackageIds)
            {
                var srvPkg = _db.TblServicePackages.FirstOrDefault(sp => sp.ServicePackageId == srvId && sp.IsActive);
                if (srvPkg == null)
                {
                    return new BookingCreateResponseModel { IsSuccess = false, Message = $"Service Package ID {srvId} not found or inactive." };
                }
                if (srvPkg.VendorId != request.VendorId)
                {
                    return new BookingCreateResponseModel { IsSuccess = false, Message = $"Service Package ID {srvId} does not belong to the selected vendor." };
                }
                servicePackages.Add(srvPkg);
                totalAmount += srvPkg.Price;
            }

            // 5. Create Booking Entity
            var booking = new TblBooking
            {
                CustomerName = request.CustomerName,
                CustomerPhone = request.CustomerPhone,
                CustomerEmail = request.CustomerEmail,
                VendorId = request.VendorId,
                VenueId = request.VenueId,
                BookingDate = request.BookingDate,
                GuestCount = request.GuestCount,
                TotalAmount = totalAmount,
                Status = (byte)BookingStatus.Pending,
                CreatedDate = DateTime.Now
            };

            _db.TblBookings.Add(booking);
            _db.SaveChanges(); // Save to generate BookingId

            // 6. Save junction table records
            foreach (var decPkg in decorationPackages)
            {
                _db.TblBookingDecorations.Add(new TblBookingDecoration
                {
                    BookingId = booking.BookingId,
                    DecorationPackageId = decPkg.DecorationPackageId
                });
            }

            foreach (var srvPkg in servicePackages)
            {
                _db.TblBookingServices.Add(new TblBookingService
                {
                    BookingId = booking.BookingId,
                    ServicePackageId = srvPkg.ServicePackageId
                });
            }

            _db.SaveChanges();

            return new BookingCreateResponseModel
            {
                IsSuccess = true,
                Message = "Booking created successfully.",
                BookingId = booking.BookingId
            };
        }
        catch (Exception ex)
        {
            return new BookingCreateResponseModel { IsSuccess = false, Message = ex.Message };
        }
    }

    // GET BOOKING HISTORY
    public List<BookingResponseModel> GetBookingHistory(string? customerPhone, string? customerEmail)
    {
        var query = _db.TblBookings
            .Include(b => b.Vendor)
            .Include(b => b.Venue)
            .Include(b => b.TblBookingDecorations).ThenInclude(bd => bd.DecorationPackage)
            .Include(b => b.TblBookingServices).ThenInclude(bs => bs.ServicePackage)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(customerPhone))
        {
            query = query.Where(b => b.CustomerPhone == customerPhone);
        }

        if (!string.IsNullOrWhiteSpace(customerEmail))
        {
            query = query.Where(b => b.CustomerEmail == customerEmail);
        }

        return query.Select(b => MapToResponse(b)).ToList();
    }

    // GET BOOKING DETAIL
    public BookingResponseModel? GetBookingDetail(int bookingId)
    {
        var booking = _db.TblBookings
            .Include(b => b.Vendor)
            .Include(b => b.Venue)
            .Include(b => b.TblBookingDecorations).ThenInclude(bd => bd.DecorationPackage)
            .Include(b => b.TblBookingServices).ThenInclude(bs => bs.ServicePackage)
            .FirstOrDefault(b => b.BookingId == bookingId);

        return booking == null ? null : MapToResponse(booking);
    }

    // CANCEL BOOKING
    public BookingStatusUpdateResponseModel CancelBooking(int bookingId)
    {
        try
        {
            var booking = _db.TblBookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
            {
                return new BookingStatusUpdateResponseModel { IsSuccess = false, Message = "Booking not found." };
            }

            // Validation: Allowed when Pending or Approved
            if (booking.Status != (byte)BookingStatus.Pending && booking.Status != (byte)BookingStatus.Approved)
            {
                return new BookingStatusUpdateResponseModel 
                { 
                    IsSuccess = false, 
                    Message = $"Cannot cancel booking. Current status is {((BookingStatus)booking.Status)}." 
                };
            }

            booking.Status = (byte)BookingStatus.Cancelled;
            int result = _db.SaveChanges();

            return new BookingStatusUpdateResponseModel
            {
                IsSuccess = result > 0,
                Message = result > 0 ? "Booking cancelled successfully." : "Cancel failed."
            };
        }
        catch (Exception ex)
        {
            return new BookingStatusUpdateResponseModel { IsSuccess = false, Message = ex.Message };
        }
    }

    // GET VENDOR BOOKINGS BY STATUS
    public List<BookingResponseModel> GetVendorBookingsByStatus(int vendorId, BookingStatus status)
    {
        var bookings = _db.TblBookings
            .Include(b => b.Vendor)
            .Include(b => b.Venue)
            .Include(b => b.TblBookingDecorations).ThenInclude(bd => bd.DecorationPackage)
            .Include(b => b.TblBookingServices).ThenInclude(bs => bs.ServicePackage)
            .Where(b => b.VendorId == vendorId && b.Status == (byte)status)
            .ToList();

        return bookings.Select(b => MapToResponse(b)).ToList();
    }

    // APPROVE BOOKING
    public BookingStatusUpdateResponseModel ApproveBooking(int bookingId)
    {
        try
        {
            var booking = _db.TblBookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
            {
                return new BookingStatusUpdateResponseModel { IsSuccess = false, Message = "Booking not found." };
            }

            // Validation: Only allowed when Pending
            if (booking.Status != (byte)BookingStatus.Pending)
            {
                return new BookingStatusUpdateResponseModel 
                { 
                    IsSuccess = false, 
                    Message = $"Cannot approve booking. Current status is {((BookingStatus)booking.Status)}." 
                };
            }

            booking.Status = (byte)BookingStatus.Approved;
            int result = _db.SaveChanges();

            return new BookingStatusUpdateResponseModel
            {
                IsSuccess = result > 0,
                Message = result > 0 ? "Booking approved successfully." : "Approval failed."
            };
        }
        catch (Exception ex)
        {
            return new BookingStatusUpdateResponseModel { IsSuccess = false, Message = ex.Message };
        }
    }

    // REJECT BOOKING
    public BookingStatusUpdateResponseModel RejectBooking(int bookingId)
    {
        try
        {
            var booking = _db.TblBookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
            {
                return new BookingStatusUpdateResponseModel { IsSuccess = false, Message = "Booking not found." };
            }

            // Validation: Only allowed when Pending
            if (booking.Status != (byte)BookingStatus.Pending)
            {
                return new BookingStatusUpdateResponseModel 
                { 
                    IsSuccess = false, 
                    Message = $"Cannot reject booking. Current status is {((BookingStatus)booking.Status).ToString()}." 
                };
            }

            booking.Status = (byte)BookingStatus.Rejected;
            int result = _db.SaveChanges();

            return new BookingStatusUpdateResponseModel
            {
                IsSuccess = result > 0,
                Message = result > 0 ? "Booking rejected successfully." : "Rejection failed."
            };
        }
        catch (Exception ex)
        {
            return new BookingStatusUpdateResponseModel { IsSuccess = false, Message = ex.Message };
        }
    }

    // COMPLETE BOOKING
    public BookingStatusUpdateResponseModel CompleteBooking(int bookingId)
    {
        try
        {
            var booking = _db.TblBookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null)
            {
                return new BookingStatusUpdateResponseModel { IsSuccess = false, Message = "Booking not found." };
            }

            // Validation: Only allowed when Approved
            if (booking.Status != (byte)BookingStatus.Approved)
            {
                return new BookingStatusUpdateResponseModel 
                { 
                    IsSuccess = false, 
                    Message = $"Cannot complete booking. Current status is {((BookingStatus)booking.Status)}." 
                };
            }

            booking.Status = (byte)BookingStatus.Completed;
            int result = _db.SaveChanges();

            return new BookingStatusUpdateResponseModel
            {
                IsSuccess = result > 0,
                Message = result > 0 ? "Booking marked as completed." : "Completion failed."
            };
        }
        catch (Exception ex)
        {
            return new BookingStatusUpdateResponseModel { IsSuccess = false, Message = ex.Message };
        }
    }

    // ── Mapping helper ──────────────────────────────────────────────────────────
    private static BookingResponseModel MapToResponse(TblBooking b)
    {
        return new BookingResponseModel
        {
            BookingId = b.BookingId,
            CustomerName = b.CustomerName,
            CustomerPhone = b.CustomerPhone,
            CustomerEmail = b.CustomerEmail,
            VendorId = b.VendorId,
            VendorName = b.Vendor != null ? b.Vendor.VendorName : string.Empty,
            VenueId = b.VenueId,
            VenueName = b.Venue != null ? b.Venue.VenueName : string.Empty,
            BookingDate = b.BookingDate,
            GuestCount = b.GuestCount,
            TotalAmount = b.TotalAmount,
            Status = b.Status,
            StatusName = ((BookingStatus)b.Status).ToString(),
            CreatedDate = b.CreatedDate,
            Decorations = b.TblBookingDecorations
                .Select(bd => bd.DecorationPackage)
                .Where(dp => dp != null)
                .Select(dp => new DecorationResponseModel
                {
                    DecorationPackageId = dp.DecorationPackageId,
                    VendorId = dp.VendorId,
                    PackageName = dp.PackageName,
                    Price = dp.Price,
                    Description = dp.Description,
                    IsActive = dp.IsActive,
                    CreatedDate = dp.CreatedDate
                }).ToList(),
            Services = b.TblBookingServices
                .Select(bs => bs.ServicePackage)
                .Where(sp => sp != null)
                .Select(sp => new ServiceResponseModel
                {
                    ServicePackageId = sp.ServicePackageId,
                    VendorId = sp.VendorId,
                    PackageName = sp.PackageName,
                    Price = sp.Price,
                    Description = sp.Description,
                    IsActive = sp.IsActive,
                    CreatedDate = sp.CreatedDate
                }).ToList()
        };
    }
}
