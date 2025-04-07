using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using analizfutbol.Models;

namespace analizfutbol.Services
{
    public interface IMongoDBService
    {
        // Ekleme işlemleri
        Task AddReferee(Referee referee);
        Task AddTeam(Team team);
        Task AddMatch(Match match);

        // İstatistik işlemleri
        Task<RefereeCardStatistics> GetRefereeCardStatistics(string refereeId);
        Task<TeamCardStatistics> GetTeamCardStatistics(string teamId);
        Task<WinRateStatistics> GetTeamWinRateWithReferee(string refereeId, string teamId);

        // Yeni metodlar
        Task<List<Referee>> SearchReferees(string name);
        Task<List<Match>> FilterMatches(DateTime? startDate, DateTime? endDate, string? refereeId, string? teamId);
        Task<RefereeDetailedAnalysis> GetRefereeDetailedAnalysis(string refereeId);
        Task<TeamRefereeHistory> GetTeamRefereeHistory(string teamId);
        Task<ComparisonResult> CompareReferees(List<string> refereeIds);
        Task<ComparisonResult> CompareTeamCards(List<string> teamIds);

        // Tahmin metodları
        Task<MatchPrediction> PredictMatch(string homeTeamId, string awayTeamId, string refereeId);
        Task<List<MatchPrediction>> GetPastPredictionAccuracy(DateTime startDate, DateTime endDate);
        Task SavePrediction(MatchPrediction prediction);
    }
} 