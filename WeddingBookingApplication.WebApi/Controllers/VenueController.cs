using Microsoft.AspNetCore.Mvc;
using WeddingBookingApplication.Domain.Features.Venue;
using WeddingBookingApplication.Domain.Models.Venue;

namespace WeddingBookingApplication.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VenueController : ControllerBase
{
    private readonly VenueService _venueService;

    public VenueController(VenueService venueService)
    {
        _venueService = venueService;
    }

    // GET api/venue
    [HttpGet]
    public IActionResult GetVenues()
    {
        var result = _venueService.GetVenues();
        return Ok(result);
    }

    // GET api/venue/{id}
    [HttpGet("{id}")]
    public IActionResult GetVenue(int id)
    {
        var result = _venueService.GetVenue(id);
        if (result is null) return NotFound();
        return Ok(result);
    }

    // POST api/venue
    [HttpPost]
    public IActionResult CreateVenue([FromBody] VenueCreateRequestModel request)
    {
        var result = _venueService.CreateVenue(request);
        return result.IsSuccess
            ? Ok(result)
            : BadRequest(result);
    }

    // PUT api/venue/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateVenue(int id, [FromBody] VenueUpdateRequestModel request)
    {
        var result = _venueService.UpdateVenue(id, request);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    // DELETE api/venue/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteVenue(int id)
    {
        var result = _venueService.DeleteVenue(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }
}
