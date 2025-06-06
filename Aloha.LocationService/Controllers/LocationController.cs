using Aloha.LocationService.Services;
using Aloha.ServiceDefaults.Meta;
using Microsoft.AspNetCore.Mvc;

namespace Aloha.LocationService.Controllers
{
    [Route("api/location")]
    [ApiController]
    public class LocationController(ILogger<LocationController> logger, ILocationService locationService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetLocationList()
        {
            var provinces = await locationService.GetAllProvincesAsync();
            return Ok(ApiResponseBuilder.BuildResponse("All Provinces Retrieve successfully!", provinces));
        }

        [HttpGet("{code:int}")]
        public async Task<IActionResult> GetProvinceByCode(int code)
        {
            var province = await locationService.GetProvinceByCodeAsync(code);
            return Ok(ApiResponseBuilder.BuildResponse("Province Retrieve successfully!", province));
        }
    }
}
