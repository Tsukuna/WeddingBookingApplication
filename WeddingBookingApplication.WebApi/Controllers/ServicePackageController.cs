using Microsoft.AspNetCore.Mvc;
using WeddingBookingApplication.Domain.Features.ServicePackage;
using WeddingBookingApplication.Domain.Models.Service;

namespace WeddingBookingApplication.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ServicePackageController : ControllerBase
{
    private readonly ServicePackageService _servicePackageService;

    public ServicePackageController(ServicePackageService servicePackageService)
    {
        _servicePackageService = servicePackageService;
    }

    // GET api/servicepackage
    [HttpGet]
    public IActionResult GetServicePackages()
    {
        var result = _servicePackageService.GetServicePackages();
        return Ok(result);
    }

    // GET api/servicepackage/{id}
    [HttpGet("{id}")]
    public IActionResult GetServicePackage(int id)
    {
        var result = _servicePackageService.GetServicePackage(id);
        if (result is null) return NotFound();
        return Ok(result);
    }

    // POST api/servicepackage
    [HttpPost]
    public IActionResult CreateServicePackage([FromBody] ServiceCreateRequestModel request)
    {
        var result = _servicePackageService.CreateServicePackage(request);
        return result.IsSuccess
            ? Ok(result)
            : BadRequest(result);
    }

    // PUT api/servicepackage/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateServicePackage(int id, [FromBody] ServiceUpdateRequestModel request)
    {
        var result = _servicePackageService.UpdateServicePackage(id, request);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    // DELETE api/servicepackage/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteServicePackage(int id)
    {
        var result = _servicePackageService.DeleteServicePackage(id);
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }
}
