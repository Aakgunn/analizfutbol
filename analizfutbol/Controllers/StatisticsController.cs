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

        [HttpPost("test/create")]
        public async Task<IActionResult> CreateTestData()
        {
            try
            {
                Console.WriteLine("Test verisi oluşturma başladı...");
                
                var referee = new Referee
                {
                    Name = "Cüneyt Çakır",
                    TotalMatches = 0,
                    TotalYellowCards = 0,
                    TotalRedCards = 0
                };

                var team = new Team
                {
                    Name = "Galatasaray",
                    TotalYellowCards = 0,
                    TotalRedCards = 0
                };

                await _mongoDBService.AddReferee(referee);
                await _mongoDBService.AddTeam(team);

                Console.WriteLine("Test verisi başarıyla oluşturuldu.");
                
                return Ok(new { message = "Test verisi oluşturuldu", referee, team });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Test verisi oluşturma hatası: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("referee/search")]
        public async Task<IActionResult> SearchReferees([FromQuery] string name)
        {
            var referees = await _mongoDBService.SearchReferees(name);
            return Ok(referees);
        }

        [HttpGet("matches/filter")]
        public async Task<IActionResult> FilterMatches(
            [FromQuery] DateTime? startDate, 
            [FromQuery] DateTime? endDate,
            [FromQuery] string? refereeId,
            [FromQuery] string? teamId)
        {
            var matches = await _mongoDBService.FilterMatches(startDate, endDate, refereeId, teamId);
            return Ok(matches);
        }

        [HttpGet("referee/{refereeId}/analysis")]
        public async Task<IActionResult> GetRefereeAnalysis(string refereeId)
        {
            var analysis = await _mongoDBService.GetRefereeDetailedAnalysis(refereeId);
            return Ok(analysis);
        }

        [HttpGet("team/{teamId}/referee-history")]
        public async Task<IActionResult> GetTeamRefereeHistory(string teamId)
        {
            var history = await _mongoDBService.GetTeamRefereeHistory(teamId);
            return Ok(history);
        }

        [HttpGet("referees/compare")]
        public async Task<IActionResult> CompareReferees([FromQuery] List<string> refereeIds)
        {
            var comparison = await _mongoDBService.CompareReferees(refereeIds);
            return Ok(comparison);
        }

        [HttpGet("teams/card-comparison")]
        public async Task<IActionResult> CompareTeamCards([FromQuery] List<string> teamIds)
        {
            var comparison = await _mongoDBService.CompareTeamCards(teamIds);
            return Ok(comparison);
        }

        [HttpGet("predict/match")]
        public async Task<IActionResult> PredictMatch(
            [FromQuery] string homeTeamId,
            [FromQuery] string awayTeamId,
            [FromQuery] string refereeId)
        {
            try
            {
                var prediction = await _mongoDBService.PredictMatch(homeTeamId, awayTeamId, refereeId);
                return Ok(prediction);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("predictions/accuracy")]
        public async Task<IActionResult> GetPredictionAccuracy(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var accuracy = await _mongoDBService.GetPastPredictionAccuracy(startDate, endDate);
                return Ok(accuracy);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
} 