using Microsoft.AspNetCore.Mvc;
using WeddingBookingApplication.Domain.Features.Booking;
using WeddingBookingApplication.Domain.Models.Booking;

namespace WeddingBookingApplication.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VendorBookingController : ControllerBase
{
    private readonly BookingService _bookingService;

    public VendorBookingController(BookingService bookingService)
    {
        _bookingService = bookingService;
    }

    // GET api/VendorBooking/{vendorId}/requests
    [HttpGet("{vendorId}/requests")]
    public IActionResult GetBookingRequests(int vendorId)
    {
        var result = _bookingService.GetVendorBookingsByStatus(vendorId, BookingStatus.Pending);
        return Ok(result);
    }

    // GET api/VendorBooking/{vendorId}/pending
    [HttpGet("{vendorId}/pending")]
    public IActionResult GetPendingBookings(int vendorId)
    {
        var result = _bookingService.GetVendorBookingsByStatus(vendorId, BookingStatus.Approved);
        return Ok(result);
    }

    // GET api/VendorBooking/{vendorId}/completed
    [HttpGet("{vendorId}/completed")]
    public IActionResult GetCompletedBookings(int vendorId)
    {
        var result = _bookingService.GetVendorBookingsByStatus(vendorId, BookingStatus.Completed);
        return Ok(result);
    }

    // PUT api/VendorBooking/{bookingId}/approve
    [HttpPut("{bookingId}/approve")]
    public IActionResult ApproveBooking(int bookingId)
    {
        var result = _bookingService.ApproveBooking(bookingId);
        return result.IsSuccess 
            ? Ok(result) 
            : BadRequest(result);
    }

    // PUT api/VendorBooking/{bookingId}/reject
    [HttpPut("{bookingId}/reject")]
    public IActionResult RejectBooking(int bookingId)
    {
        var result = _bookingService.RejectBooking(bookingId);
        return result.IsSuccess 
            ? Ok(result) 
            : BadRequest(result);
    }

    // PUT api/VendorBooking/{bookingId}/complete
    [HttpPut("{bookingId}/complete")]
    public IActionResult CompleteBooking(int bookingId)
    {
        var result = _bookingService.CompleteBooking(bookingId);
        return result.IsSuccess 
            ? Ok(result) 
            : BadRequest(result);
    }
}
