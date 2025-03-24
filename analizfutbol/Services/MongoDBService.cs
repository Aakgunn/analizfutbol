using Microsoft.Extensions.Options;
using MongoDB.Driver;
using analizfutbol.Models;
using analizfutbol.Settings;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace analizfutbol.Services
{
    public class MongoDBService : IMongoDBService
    {
        private readonly IMongoCollection<Referee> _refereeCollection;
        private readonly IMongoCollection<Team> _teamCollection;
        private readonly IMongoCollection<Match> _matchCollection;

        public MongoDBService(IOptions<MongoDBSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            
            _refereeCollection = database.GetCollection<Referee>("Referees");
            _teamCollection = database.GetCollection<Team>("Teams");
            _matchCollection = database.GetCollection<Match>("Matches");
        }

        public async Task<RefereeCardStatistics> GetRefereeCardStatistics(string refereeId)
        {
            var matches = await _matchCollection
                .Find(m => m.RefereeId == refereeId)
                .ToListAsync();

            var stats = new RefereeCardStatistics
            {
                TotalMatches = matches.Count,
                TotalYellowCards = matches.Sum(m => m.HomeTeamYellowCards + m.AwayTeamYellowCards),
                TotalRedCards = matches.Sum(m => m.HomeTeamRedCards + m.AwayTeamRedCards)
            };

            stats.YellowCardsPerMatch = stats.TotalMatches > 0 ? (double)stats.TotalYellowCards / stats.TotalMatches : 0;
            stats.RedCardsPerMatch = stats.TotalMatches > 0 ? (double)stats.TotalRedCards / stats.TotalMatches : 0;

            return stats;
        }

        public async Task<TeamCardStatistics> GetTeamCardStatistics(string teamId)
        {
            var matches = await _matchCollection
                .Find(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId)
                .ToListAsync();

            var stats = new TeamCardStatistics
            {
                TotalMatches = matches.Count,
                TotalYellowCards = matches.Sum(m => 
                    m.HomeTeamId == teamId ? m.HomeTeamYellowCards : m.AwayTeamYellowCards),
                TotalRedCards = matches.Sum(m => 
                    m.HomeTeamId == teamId ? m.HomeTeamRedCards : m.AwayTeamRedCards)
            };

            stats.YellowCardsPerMatch = stats.TotalMatches > 0 ? (double)stats.TotalYellowCards / stats.TotalMatches : 0;
            stats.RedCardsPerMatch = stats.TotalMatches > 0 ? (double)stats.TotalRedCards / stats.TotalMatches : 0;

            return stats;
        }

        public async Task<WinRateStatistics> GetTeamWinRateWithReferee(string refereeId, string teamId)
        {
            var matches = await _matchCollection
                .Find(m => m.RefereeId == refereeId && (m.HomeTeamId == teamId || m.AwayTeamId == teamId))
                .ToListAsync();

            var stats = new WinRateStatistics
            {
                TotalMatches = matches.Count,
                Wins = matches.Count(m => 
                    (m.HomeTeamId == teamId && m.HomeTeamScore > m.AwayTeamScore) ||
                    (m.AwayTeamId == teamId && m.AwayTeamScore > m.HomeTeamScore)),
                Draws = matches.Count(m => m.HomeTeamScore == m.AwayTeamScore)
            };

            stats.Losses = stats.TotalMatches - stats.Wins - stats.Draws;
            stats.WinRate = stats.TotalMatches > 0 ? (double)stats.Wins / stats.TotalMatches * 100 : 0;

            return stats;
        }
    }
} 