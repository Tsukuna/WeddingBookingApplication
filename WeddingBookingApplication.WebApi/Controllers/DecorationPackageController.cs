using Microsoft.AspNetCore.Mvc;
using WeddingBookingApplication.Domain.Features.Decoration;
using WeddingBookingApplication.Domain.Models.Decoration;

namespace WeddingBookingApplication.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DecorationPackageController : ControllerBase
{
    private readonly DecorationService _decorationPackageService;

    public DecorationPackageController(DecorationService decorationPackageService)
    {
        _decorationPackageService = decorationPackageService;
    }

    // GET api/decorationpackage
    [HttpGet]
    public IActionResult GetDecorationPackages()
    {
        var result = _decorationPackageService.GetDecorationPackages();
        return Ok(result);
    }

    // GET api/decorationpackage/{id}
    [HttpGet("{id}")]
    public IActionResult GetDecorationPackage(int id)
    {
        var result = _decorationPackageService.GetDecorationPackage(id);
        if (result is null) return NotFound();
        return Ok(result);
    }

    // POST api/decorationpackage
    [HttpPost]
    public IActionResult CreateDecorationPackage([FromBody] DecorationCreateRequestModel request)
    {
        var result = _decorationPackageService.CreateDecorationPackage(request);
        return result.IsSuccess
            ? Ok(result)
            : BadRequest(result);
    }

    // PUT api/decorationpackage/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateDecorationPackage(int id, [FromBody] DecorationUpdateRequestModel request)
    {
        var result = _decorationPackageService.UpdateDecorationPackage(id, request);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    // DELETE api/decorationpackage/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteDecorationPackage(int id)
    {
        var result = _decorationPackageService.DeleteDecorationPackage(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }
}
