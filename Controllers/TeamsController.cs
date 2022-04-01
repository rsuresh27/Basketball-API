using Basketball_API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Basketball_API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class TeamsController : ControllerBase
    {
        private readonly ILogger<TeamsController> _logger;
        private readonly ITeamsRepository _teamsRepository;

        public TeamsController(ILogger<TeamsController> logger, ITeamsRepository teamsRepository)
        {
            _logger = logger;
            _teamsRepository = teamsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetDepthChart(string team, string year = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(team))
                {

                    var depthChart = await _teamsRepository.GetDepthChart(team, year);
                    return Ok(depthChart);
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
        public async Task<IActionResult> GetGameResults(string team, string year = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(team))
                {
                    var gamesPlayed = await _teamsRepository.GetGameResults(team, year);
                    return Ok(gamesPlayed); 
                }
                else
                {
                    return BadRequest(); 
                }
            }

            catch(Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetTransactions(string team, string year)
        {
            try
            {
                if (!string.IsNullOrEmpty(team))
                {
                    var transactions = await _teamsRepository.GetTransactions(team, year);
                    return Ok(transactions);
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

    }
}
