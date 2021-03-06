using System.Collections.Generic;
using System.Threading.Tasks;

namespace Basketball_API.Repositories
{
    public interface IStatsRepository
    {
        public Task<double> GetStat(string player, string stat, string year = null);

        public Task<Dictionary<string, string>> GetSeasonStats(string player, string year = null);

        public Task<Dictionary<string, string>> GetTeamSeasonStats(string team, string year = null);

        public Task<List<Dictionary<string, string>>> GetDayTopPlayers(string daysAgo);
    }
}
