using Basketball_API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Basketball_API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class StatsLeaderController : ControllerBase
    {
        private readonly IStatLeadersRepository _statLeadersRepository;
        private readonly ILogger<StatsLeaderController> _logger;

        public StatsLeaderController(IStatLeadersRepository statLeadersRepository, ILogger<StatsLeaderController> logger)
        {
            _statLeadersRepository = statLeadersRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetTop5PlayersPPG(string year = "2022")
        {
            try
            {
                var statValue = await _statLeadersRepository.GetTop5PlayersPPG(year);
                return Ok(statValue);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in getting stats: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

    }
}
