using Basketball_API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Basketball_API.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class NewsController : ControllerBase
    {
        private readonly ILogger<NewsController> _logger;
        private readonly INewsRepository _newsRepository;

        public NewsController(ILogger<NewsController> logger, INewsRepository newsRepository)
        {
            _logger = logger;
            _newsRepository = newsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetNews()
        {
            try
            {
                return Ok(await _newsRepository.GetNews());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in getting news: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetNCAANews()
        {
            try
            {
                return Ok(await _newsRepository.GetNCAANews());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in getting news: {ex.Message}");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
