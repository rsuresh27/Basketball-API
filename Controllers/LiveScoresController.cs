using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Basketball_API.Repositories;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Basketball_API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class LiveScoresController : ControllerBase
    {
        private readonly ILiveScoresRepository _liveScoresRepository;
        private readonly ILogger<LiveScoresController> _logger;

        public LiveScoresController(ILiveScoresRepository liveScoresRepository, ILogger<LiveScoresController> logger)
        {
            _liveScoresRepository = liveScoresRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetGameScore(string gameID, DateTime? date = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(gameID))
                {
                    var score = await _liveScoresRepository.GetGameScore(gameID, date);
                    return Ok(score);
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
        public async Task<IActionResult> GetGames(DateTime? date = null)
        {
            try
            {
                var games = await _liveScoresRepository.GetGames(date);
                return Ok(games);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in getting stats: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }
    
        [HttpGet] 
        public async Task<IActionResult> GetNCAAGameScore(string gameID, DateTime? date = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(gameID))
                {
                    var score = await _liveScoresRepository.GetNCAAGameScore(gameID, date);
                    return Ok(score);
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
        public async Task<IActionResult> GetNCAAGames(DateTime? date)
        {
            try
            {
                var games = await _liveScoresRepository.GetNCAAGames(date);
                return Ok(games);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in getting stats: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }
        
    }
}
