using System.Collections.Generic;
using System.Threading.Tasks;
using System; 

namespace Basketball_API.Repositories
{
    public interface ILiveScoresRepository
    {
        public Task<string> GetGameScore(string gameID, DateTime? date = null);

        public Task<List<string>> GetGames(DateTime? date = null);

        public Task<string> GetNCAAGameScore(string gameID, DateTime? date = null);

        public Task<List<string>> GetNCAAGames(DateTime? date = null);
    }
}
