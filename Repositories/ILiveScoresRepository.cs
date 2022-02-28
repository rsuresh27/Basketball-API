using System.Collections.Generic;
using System.Threading.Tasks;
using System; 

namespace Basketball_API.Repositories
{
    public interface ILiveScoresRepository
    {
        public Task<string> GetGameScore(string gameID);

        public Task<List<string>> GetGamesToday(DateTime date); 
 
    }
}
