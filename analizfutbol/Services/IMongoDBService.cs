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
    }
} 