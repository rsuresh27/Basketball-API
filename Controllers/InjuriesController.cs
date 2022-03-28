using Basketball_API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Basketball_API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class InjuriesController : ControllerBase
    {
        private readonly IInjuriesRepository _injuriesRepository;
        private readonly ILogger<InjuriesController> _logger;

        public InjuriesController(IInjuriesRepository injuriesRepository, ILogger<InjuriesController> logger)
        {
            _injuriesRepository = injuriesRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetInjuries()
        {
            try
            {
                return Ok(await _injuriesRepository.GetInjuries());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in getting injuries: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
