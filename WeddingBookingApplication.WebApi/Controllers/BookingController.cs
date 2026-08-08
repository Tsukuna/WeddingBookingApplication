using Microsoft.AspNetCore.Mvc;
using WeddingBookingApplication.Domain.Features.Booking;
using WeddingBookingApplication.Domain.Models.Booking;

namespace WeddingBookingApplication.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BookingController : ControllerBase
{
    private readonly BookingService _bookingService;

    public BookingController(BookingService bookingService)
    {
        _bookingService = bookingService;
    }

    // POST api/Booking
    [HttpPost]
    public IActionResult CreateBooking([FromBody] BookingCreateRequestModel request)
    {
        var result = _bookingService.CreateBooking(request);
        return result.IsSuccess 
            ? Ok(result) 
            : BadRequest(result);
    }

    // GET api/Booking/history
    [HttpGet("history")]
    public IActionResult GetBookingHistory([FromQuery] string? customerPhone, [FromQuery] string? customerEmail)
    {
        var result = _bookingService.GetBookingHistory(customerPhone, customerEmail);
        return Ok(result);
    }

    // GET api/Booking/{id}
    [HttpGet("{id}")]
    public IActionResult GetBookingDetail(int id)
    {
        var result = _bookingService.GetBookingDetail(id);
        if (result == null)
        {
            return NotFound(new { Message = "Booking not found." });
        }
        return Ok(result);
    }

    // PUT api/Booking/{id}/cancel
    [HttpPut("{id}/cancel")]
    public IActionResult CancelBooking(int id)
    {
        var result = _bookingService.CancelBooking(id);
        return result.IsSuccess 
            ? Ok(result) 
            : BadRequest(result);
    }
}
