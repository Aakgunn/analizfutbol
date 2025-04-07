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
        private readonly IMongoCollection<MatchPrediction> _predictionCollection;
        private readonly IMongoDatabase _database;

        public MongoDBService(IOptions<MongoDBSettings> settings)
        {
            try
            {
                Console.WriteLine("MongoDB bağlantısı kuruluyor...");
                var client = new MongoClient(settings.Value.ConnectionString);
                Console.WriteLine("MongoDB client oluşturuldu.");

                _database = client.GetDatabase(settings.Value.DatabaseName);
                Console.WriteLine($"Veritabanı seçildi: {settings.Value.DatabaseName}");

                _refereeCollection = _database.GetCollection<Referee>("Referees");
                _teamCollection = _database.GetCollection<Team>("Teams");
                _matchCollection = _database.GetCollection<Match>("Matches");
                _predictionCollection = _database.GetCollection<MatchPrediction>("Predictions");
                Console.WriteLine("Koleksiyonlar başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MongoDB bağlantı hatası: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task AddReferee(Referee referee)
        {
            try
            {
                Console.WriteLine($"Hakem ekleniyor: {referee.Name}");
                await _refereeCollection.InsertOneAsync(referee);
                Console.WriteLine("Hakem başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hakem ekleme hatası: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task AddTeam(Team team)
        {
            try
            {
                Console.WriteLine($"Takım ekleniyor: {team.Name}");
                await _teamCollection.InsertOneAsync(team);
                Console.WriteLine("Takım başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Takım ekleme hatası: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task AddMatch(Match match)
        {
            await _matchCollection.InsertOneAsync(match);
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

        public async Task<MatchPrediction> PredictMatch(string homeTeamId, string awayTeamId, string refereeId)
        {
            try
            {
                // Son 10 maçı al
                var recentMatches = await _matchCollection
                    .Find(m => (m.HomeTeamId == homeTeamId || m.AwayTeamId == homeTeamId ||
                               m.HomeTeamId == awayTeamId || m.AwayTeamId == awayTeamId))
                    .SortByDescending(m => m.MatchDate)
                    .Limit(10)
                    .ToListAsync();

                // Hakemin son 10 maçını al
                var refereeMatches = await _matchCollection
                    .Find(m => m.RefereeId == refereeId)
                    .SortByDescending(m => m.MatchDate)
                    .Limit(10)
                    .ToListAsync();

                // Takımların karşılıklı maçlarını al
                var headToHead = await _matchCollection
                    .Find(m => (m.HomeTeamId == homeTeamId && m.AwayTeamId == awayTeamId) ||
                              (m.HomeTeamId == awayTeamId && m.AwayTeamId == homeTeamId))
                    .SortByDescending(m => m.MatchDate)
                    .Limit(5)
                    .ToListAsync();

                // Tahmin hesaplamaları
                var prediction = CalculatePrediction(recentMatches, refereeMatches, headToHead, homeTeamId, awayTeamId);
                
                // Tahmini kaydet
                await SavePrediction(prediction);

                return prediction;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tahmin hatası: {ex.Message}");
                throw;
            }
        }

        private MatchPrediction CalculatePrediction(
            List<Match> recentMatches, 
            List<Match> refereeMatches, 
            List<Match> headToHead,
            string homeTeamId,
            string awayTeamId)
        {
            // Form puanları hesapla
            var homeTeamForm = CalculateTeamForm(recentMatches, homeTeamId);
            var awayTeamForm = CalculateTeamForm(recentMatches, awayTeamId);

            // Hakem eğilimlerini hesapla
            var refereeStats = CalculateRefereeStats(refereeMatches);

            // Kart tahminleri
            var predictedHomeCards = (homeTeamForm.AverageCards * 0.4) + 
                                   (refereeStats.AverageCardsPerMatch * 0.4) +
                                   (CalculateHeadToHeadCards(headToHead, homeTeamId) * 0.2);

            var predictedAwayCards = (awayTeamForm.AverageCards * 0.4) +
                                    (refereeStats.AverageCardsPerMatch * 0.4) +
                                    (CalculateHeadToHeadCards(headToHead, awayTeamId) * 0.2);

            // Kazanma olasılıkları
            var winProbabilities = CalculateWinProbabilities(homeTeamForm, awayTeamForm, headToHead);

            return new MatchPrediction
            {
                HomeTeamId = homeTeamId,
                AwayTeamId = awayTeamId,
                PredictionDate = DateTime.UtcNow,
                PredictedHomeTeamCards = Math.Round(predictedHomeCards, 2),
                PredictedAwayTeamCards = Math.Round(predictedAwayCards, 2),
                WinProbabilityHome = Math.Round(winProbabilities.HomeProbability, 2),
                WinProbabilityAway = Math.Round(winProbabilities.AwayProbability, 2),
                DrawProbability = Math.Round(winProbabilities.DrawProbability, 2),
                ConfidenceScore = CalculateConfidenceScore(recentMatches.Count, refereeMatches.Count, headToHead.Count),
                HistoricalMatchesUsed = recentMatches.Count + refereeMatches.Count + headToHead.Count
            };
        }

        public async Task<List<Referee>> SearchReferees(string name)
        {
            var filter = Builders<Referee>.Filter.Regex(r => r.Name, new BsonRegularExpression(name, "i"));
            return await _refereeCollection.Find(filter).ToListAsync();
        }

        public async Task<List<Match>> FilterMatches(DateTime? startDate, DateTime? endDate, string? refereeId, string? teamId)
        {
            var filter = Builders<Match>.Filter.Empty;
            
            if (startDate.HasValue)
                filter &= Builders<Match>.Filter.Gte(m => m.MatchDate, startDate.Value);
            
            if (endDate.HasValue)
                filter &= Builders<Match>.Filter.Lte(m => m.MatchDate, endDate.Value);
            
            if (!string.IsNullOrEmpty(refereeId))
                filter &= Builders<Match>.Filter.Eq(m => m.RefereeId, refereeId);
            
            if (!string.IsNullOrEmpty(teamId))
                filter &= Builders<Match>.Filter.Or(
                    Builders<Match>.Filter.Eq(m => m.HomeTeamId, teamId),
                    Builders<Match>.Filter.Eq(m => m.AwayTeamId, teamId));

            return await _matchCollection.Find(filter).ToListAsync();
        }

        public async Task<RefereeDetailedAnalysis> GetRefereeDetailedAnalysis(string refereeId)
        {
            var matches = await _matchCollection.Find(m => m.RefereeId == refereeId).ToListAsync();
            var referee = await _refereeCollection.Find(r => r.Id == refereeId).FirstOrDefaultAsync();

            return new RefereeDetailedAnalysis
            {
                RefereeId = refereeId,
                RefereeName = referee?.Name ?? "Unknown",
                TotalMatches = matches.Count,
                AverageYellowCardsPerMatch = matches.Count > 0 ? 
                    matches.Average(m => m.HomeTeamYellowCards + m.AwayTeamYellowCards) : 0,
                AverageRedCardsPerMatch = matches.Count > 0 ? 
                    matches.Average(m => m.HomeTeamRedCards + m.AwayTeamRedCards) : 0
            };
        }

        public async Task<TeamRefereeHistory> GetTeamRefereeHistory(string teamId)
        {
            var matches = await _matchCollection
                .Find(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId)
                .ToListAsync();

            var team = await _teamCollection.Find(t => t.Id == teamId).FirstOrDefaultAsync();

            return new TeamRefereeHistory
            {
                TeamId = teamId,
                TeamName = team?.Name ?? "Unknown",
                RefereeHistory = await CreateRefereeHistory(matches, teamId)
            };
        }

        private async Task<List<RefereeMatchHistory>> CreateRefereeHistory(List<Match> matches, string teamId)
        {
            var refereeGroups = matches.GroupBy(m => m.RefereeId);
            var history = new List<RefereeMatchHistory>();

            foreach (var group in refereeGroups)
            {
                var referee = await _refereeCollection.Find(r => r.Id == group.Key).FirstOrDefaultAsync();
                history.Add(new RefereeMatchHistory
                {
                    RefereeId = group.Key,
                    RefereeName = referee?.Name ?? "Unknown",
                    TotalMatches = group.Count(),
                    // ... diğer istatistikler
                });
            }

            return history;
        }

        public async Task<ComparisonResult> CompareReferees(List<string> refereeIds)
        {
            var result = new ComparisonResult();
            result.Items = new List<ComparisonItem>();

            foreach (var refereeId in refereeIds)
            {
                var referee = await _refereeCollection.Find(r => r.Id == refereeId).FirstOrDefaultAsync();
                var matches = await _matchCollection.Find(m => m.RefereeId == refereeId).ToListAsync();

                var item = new ComparisonItem
                {
                    Id = refereeId,
                    Name = referee?.Name ?? "Unknown",
                    Statistics = new Dictionary<string, double>
                    {
                        { "TotalMatches", matches.Count },
                        { "AverageYellowCards", matches.Count > 0 ? 
                            matches.Average(m => m.HomeTeamYellowCards + m.AwayTeamYellowCards) : 0 },
                        { "AverageRedCards", matches.Count > 0 ? 
                            matches.Average(m => m.HomeTeamRedCards + m.AwayTeamRedCards) : 0 }
                    }
                };

                result.Items.Add(item);
            }

            return result;
        }

        public async Task<ComparisonResult> CompareTeamCards(List<string> teamIds)
        {
            var result = new ComparisonResult();
            result.Items = new List<ComparisonItem>();

            foreach (var teamId in teamIds)
            {
                var team = await _teamCollection.Find(t => t.Id == teamId).FirstOrDefaultAsync();
                var matches = await _matchCollection.Find(m => 
                    m.HomeTeamId == teamId || m.AwayTeamId == teamId).ToListAsync();

                var item = new ComparisonItem
                {
                    Id = teamId,
                    Name = team?.Name ?? "Unknown",
                    Statistics = new Dictionary<string, double>
                    {
                        { "TotalMatches", matches.Count },
                        { "TotalYellowCards", matches.Sum(m => 
                            m.HomeTeamId == teamId ? m.HomeTeamYellowCards : m.AwayTeamYellowCards) },
                        { "TotalRedCards", matches.Sum(m => 
                            m.HomeTeamId == teamId ? m.HomeTeamRedCards : m.AwayTeamRedCards) }
                    }
                };

                result.Items.Add(item);
            }

            return result;
        }

        public async Task<List<MatchPrediction>> GetPastPredictionAccuracy(DateTime startDate, DateTime endDate)
        {
            var filter = Builders<MatchPrediction>.Filter.And(
                Builders<MatchPrediction>.Filter.Gte(p => p.PredictionDate, startDate),
                Builders<MatchPrediction>.Filter.Lte(p => p.PredictionDate, endDate)
            );

            return await _predictionCollection.Find(filter).ToListAsync();
        }

        public async Task SavePrediction(MatchPrediction prediction)
        {
            await _predictionCollection.InsertOneAsync(prediction);
        }

        private class RefereeStats
        {
            public double AverageCardsPerMatch { get; set; }
            public double YellowCardRate { get; set; }
            public double RedCardRate { get; set; }
        }

        private class TeamForm
        {
            public double AverageCards { get; set; }
            public double WinRate { get; set; }
        }

        private class WinProbabilities
        {
            public double HomeProbability { get; set; }
            public double AwayProbability { get; set; }
            public double DrawProbability { get; set; }
        }

        private TeamForm CalculateTeamForm(List<Match> matches, string teamId)
        {
            var teamMatches = matches.Where(m => m.HomeTeamId == teamId || m.AwayTeamId == teamId).ToList();
            
            return new TeamForm
            {
                AverageCards = teamMatches.Count > 0 ? 
                    teamMatches.Average(m => 
                        m.HomeTeamId == teamId ? 
                        (m.HomeTeamYellowCards + m.HomeTeamRedCards) : 
                        (m.AwayTeamYellowCards + m.AwayTeamRedCards)) : 0,
                WinRate = CalculateWinRate(teamMatches, teamId)
            };
        }

        private RefereeStats CalculateRefereeStats(List<Match> matches)
        {
            if (!matches.Any()) return new RefereeStats();

            return new RefereeStats
            {
                AverageCardsPerMatch = matches.Average(m => 
                    m.HomeTeamYellowCards + m.HomeTeamRedCards + 
                    m.AwayTeamYellowCards + m.AwayTeamRedCards),
                YellowCardRate = matches.Average(m => 
                    m.HomeTeamYellowCards + m.AwayTeamYellowCards),
                RedCardRate = matches.Average(m => 
                    m.HomeTeamRedCards + m.AwayTeamRedCards)
            };
        }

        private double CalculateHeadToHeadCards(List<Match> matches, string teamId)
        {
            if (!matches.Any()) return 0;

            return matches.Average(m => 
                m.HomeTeamId == teamId ? 
                (m.HomeTeamYellowCards + m.HomeTeamRedCards) : 
                (m.AwayTeamYellowCards + m.AwayTeamRedCards));
        }

        private WinProbabilities CalculateWinProbabilities(TeamForm homeForm, TeamForm awayForm, List<Match> headToHead)
        {
            double homeProbability = 0.4 * homeForm.WinRate + 0.3;  // Ev sahibi avantajı
            double awayProbability = 0.4 * awayForm.WinRate;
            double drawProbability = 1 - (homeProbability + awayProbability);

            // Head to head sonuçlarını da hesaba kat
            if (headToHead.Any())
            {
                var homeWins = headToHead.Count(m => 
                    m.HomeTeamScore > m.AwayTeamScore);
                var awayWins = headToHead.Count(m => 
                    m.AwayTeamScore > m.HomeTeamScore);
                
                homeProbability = (homeProbability + (double)homeWins / headToHead.Count) / 2;
                awayProbability = (awayProbability + (double)awayWins / headToHead.Count) / 2;
                drawProbability = 1 - (homeProbability + awayProbability);
            }

            return new WinProbabilities
            {
                HomeProbability = homeProbability,
                AwayProbability = awayProbability,
                DrawProbability = drawProbability
            };
        }

        private double CalculateWinRate(List<Match> matches, string teamId)
        {
            if (!matches.Any()) return 0;

            var wins = matches.Count(m => 
                (m.HomeTeamId == teamId && m.HomeTeamScore > m.AwayTeamScore) ||
                (m.AwayTeamId == teamId && m.AwayTeamScore > m.HomeTeamScore));

            return (double)wins / matches.Count;
        }

        private double CalculateConfidenceScore(int recentMatchesCount, int refereeMatchesCount, int headToHeadCount)
        {
            // Basit bir güven skoru hesaplama
            double baseScore = 50.0;
            baseScore += Math.Min(recentMatchesCount * 5, 25); // Son maçlar için maksimum 25 puan
            baseScore += Math.Min(refereeMatchesCount * 3, 15); // Hakem maçları için maksimum 15 puan
            baseScore += Math.Min(headToHeadCount * 2, 10); // Karşılıklı maçlar için maksimum 10 puan

            return Math.Min(baseScore, 100); // Maksimum 100 puan
        }
    }
} 