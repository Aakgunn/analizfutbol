using System.Threading.Tasks;
using analizfutbol.Models;

namespace analizfutbol.Services
{
    public interface IMongoDBService
    {
        Task<RefereeCardStatistics> GetRefereeCardStatistics(string refereeId);
        Task<TeamCardStatistics> GetTeamCardStatistics(string teamId);
        Task<WinRateStatistics> GetTeamWinRateWithReferee(string refereeId, string teamId);
    }
} 