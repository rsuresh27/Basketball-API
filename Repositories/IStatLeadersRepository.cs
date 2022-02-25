using System.Collections.Generic;
using System.Threading.Tasks;

namespace Basketball_API.Repositories
{
    public interface IStatLeadersRepository
    {
        public Task<Dictionary<string, string>> GetTop5PlayersPPG(string year);

        public Task<Dictionary<string, string>> GetTopPlayersRPG(string year);

        public Task<Dictionary<string, string>> GetTopPlayersAPG(string year);

        public Task<Dictionary<string, string>> GetTopPlayersSPG(string year);

        public Task<Dictionary<string, string>> GetTopPlayersBPG(string year);
    }
}
