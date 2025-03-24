using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using analizfutbol.Services;
using analizfutbol.Models;

namespace analizfutbol.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IMongoDBService _mongoDBService;

        public StatisticsController(IMongoDBService mongoDBService)
        {
            _mongoDBService = mongoDBService;
        }

        [HttpGet("referee/{refereeId}/cards")]
        public async Task<IActionResult> GetRefereeCardStatistics(string refereeId)
        {
            var stats = await _mongoDBService.GetRefereeCardStatistics(refereeId);
            return Ok(stats);
        }

        [HttpGet("team/{teamId}/cards")]
        public async Task<IActionResult> GetTeamCardStatistics(string teamId)
        {
            var stats = await _mongoDBService.GetTeamCardStatistics(teamId);
            return Ok(stats);
        }

        [HttpGet("referee/{refereeId}/team/{teamId}/winrate")]
        public async Task<IActionResult> GetTeamWinRateWithReferee(string refereeId, string teamId)
        {
            var stats = await _mongoDBService.GetTeamWinRateWithReferee(refereeId, teamId);
            return Ok(stats);
        }
    }
} 