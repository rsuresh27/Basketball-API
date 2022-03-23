using Basketball_API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Basketball_API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class StatController : ControllerBase
    {
        private readonly IStatsRepository _statsRepository;
        private readonly ILogger<StatController> _logger;

        public StatController(IStatsRepository statsRepository, ILogger<StatController> logger)
        {
            _statsRepository = statsRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetStat(string player, string stat, string year = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(player) && !string.IsNullOrEmpty(stat))
                {
                    var statValue = await _statsRepository.GetStat(player, stat, year);
                    return Ok(statValue);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in getting stats: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPlayerStats(string player, string year = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(player))
                {
                    var statValue = await _statsRepository.GetSeasonStats(player, year);
                    return Ok(statValue);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in getting stats: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTeamStats(string team, string year = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(team))
                {
                    var statValue = await _statsRepository.GetTeamSeasonStats(team, year);
                    return Ok(statValue);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in getting stats: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDayTop5Players(string daysAgo = "1")
        {
            try
            {
                var statValue = await _statsRepository.GetDayTopPlayers(daysAgo);
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
