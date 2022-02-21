using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Basketball_API.Repositories;
using Microsoft.Extensions.Logging;

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
        public IActionResult GetStat(string player, string stat, string year = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(player) && !string.IsNullOrEmpty(stat))
                {
                    var statValue = _statsRepository.GetStat(player, stat, year);
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
                return StatusCode(500);
            }
        }

        [HttpGet]
        public IActionResult GetPlayerStats(string player, string year = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(player))
                {
                    var statValue = _statsRepository.GetSeasonStats(player, year);
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
                return StatusCode(500);
            }
        }

        [HttpGet]
        public IActionResult GetTeamStats(string team, string year = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(team))
                {
                    var statValue = _statsRepository.GetTeamSeasonStats(team, year);
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
                return StatusCode(500);
            }
        }

        [HttpGet]
        public IActionResult GetDayTop5Players(string year = "1")
        {
            try
            {
                var statValue = _statsRepository.GetDayTopPlayers(year);
                return Ok(statValue);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in getting stats: {ex.Message}");
                return StatusCode(500);
            }
        }
    }
}
