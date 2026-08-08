using Microsoft.AspNetCore.Mvc;
using WeddingBookingApplication.Domain.Features.Vendor;
using WeddingBookingApplication.Domain.Models.Vendor;

namespace WeddingBookingApplication.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VendorController : ControllerBase
{
    private readonly VendorService _vendorService;

    public VendorController(VendorService vendorService)
    {
        _vendorService = vendorService;
    }

    // GET api/vendor
    [HttpGet]
    public IActionResult GetVendors()
    {
        var result = _vendorService.GetVendors();
        return Ok(result);
    }

    // GET api/vendor/{id}
    [HttpGet("{id}")]
    public IActionResult GetVendor(int id)
    {
        var result = _vendorService.GetVendor(id);
        if (result is null) return NotFound();
        return Ok(result);
    }

    // POST api/vendor
    [HttpPost]
    public IActionResult CreateVendor([FromBody] VendorCreateRequestModel request)
    {
        var result = _vendorService.CreateVendor(request);
        return result.IsSuccess
            ? Ok(result)
            : BadRequest(result);
    }

    // PUT api/vendor/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateVendor(int id, [FromBody] VendorUpdateRequestModel request)
    {
        var result = _vendorService.UpdateVendor(id, request);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    // DELETE api/vendor/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteVendor(int id)
    {
        var result = _vendorService.DeleteVendor(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }
}
