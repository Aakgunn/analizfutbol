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

        // Hakem ekleme
        [HttpPost("referee")]
        public async Task<IActionResult> AddReferee([FromBody] Referee referee)
        {
            await _mongoDBService.AddReferee(referee);
            return Ok(referee);
        }

        // Takım ekleme
        [HttpPost("team")]
        public async Task<IActionResult> AddTeam([FromBody] Team team)
        {
            await _mongoDBService.AddTeam(team);
            return Ok(team);
        }

        // Maç ekleme
        [HttpPost("match")]
        public async Task<IActionResult> AddMatch([FromBody] Match match)
        {
            await _mongoDBService.AddMatch(match);
            return Ok(match);
        }

        // Hakem istatistikleri
        [HttpGet("referee/{refereeId}/stats")]
        public async Task<IActionResult> GetRefereeStats(string refereeId)
        {
            var stats = await _mongoDBService.GetRefereeCardStatistics(refereeId);
            return Ok(stats);
        }

        // Takım istatistikleri
        [HttpGet("team/{teamId}/stats")]
        public async Task<IActionResult> GetTeamStats(string teamId)
        {
            var stats = await _mongoDBService.GetTeamCardStatistics(teamId);
            return Ok(stats);
        }

        // Hakem-Takım kazanma oranı
        [HttpGet("referee/{refereeId}/team/{teamId}/winrate")]
        public async Task<IActionResult> GetWinRate(string refereeId, string teamId)
        {
            var stats = await _mongoDBService.GetTeamWinRateWithReferee(refereeId, teamId);
            return Ok(stats);
        }
    }
} 