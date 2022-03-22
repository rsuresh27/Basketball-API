﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Basketball_API.Repositories;
using Microsoft.Extensions.Logging;
using System.Web;
using System.Text.RegularExpressions; 

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
        public async Task<IActionResult> GetGameOdds(string gameId)
        {
            try
            {
                var odds = await _bettingRepository.GetGameOdds(gameId);

                odds = Regex.Unescape(odds);

                odds = Regex.Replace(odds, "\n", "");
                odds = Regex.Replace(odds, "\r", "");
                odds = Regex.Replace(odds, "\t", "");

                return Ok(odds); 
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in getting odds: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

    }
}
