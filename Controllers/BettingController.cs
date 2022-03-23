using Basketball_API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Basketball_API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class BettingController : ControllerBase
    {
        private readonly IBettingRepository _bettingRepository;
        private readonly ILogger<BettingController> _logger;

        public BettingController(IBettingRepository bettingRepository, ILogger<BettingController> logger)
        {
            _bettingRepository = bettingRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetGameOdds(string gameID)
        {
            try
            {
                if (!string.IsNullOrEmpty(gameID))
                {
                    var odds = await _bettingRepository.GetGameOdds(gameID);

                    odds = Regex.Unescape(odds);

                    odds = Regex.Replace(odds, "\n", "");
                    odds = Regex.Replace(odds, "\r", "");
                    odds = Regex.Replace(odds, "\t", "");

                    return Ok(odds);
                }
                else
                {
                    return BadRequest();
                }


            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in getting odds: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNCAAGameOdds(string gameID)
        {
            try
            {
                if (!string.IsNullOrEmpty(gameID))
                {
                    var odds = await _bettingRepository.GetNCAAGameOdds(gameID);

                    odds = Regex.Unescape(odds);

                    odds = Regex.Replace(odds, "\n", "");
                    odds = Regex.Replace(odds, "\r", "");
                    odds = Regex.Replace(odds, "\t", "");

                    return Ok(odds);
                }
                else
                {
                    return BadRequest();
                }


            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in getting odds: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

    }
}
