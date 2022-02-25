using System.Collections.Generic;
using System.Threading.Tasks;

namespace Basketball_API.Repositories
{
    public interface IStatLeadersRepository
    {
        public Task<Dictionary<string, string>> GetTop5PlayersPPG(string year);
    }
}
