using System.Threading.Tasks; 

namespace Basketball_API.Repositories
{
    public interface ITeamsRepository
    {
        public Task<string> GetDepthChart(string team, string year);

        public Task<string> GetGameResults(string team, string year); 

        public Task<string> GetTransactions(string team, string year); 
    }
}
