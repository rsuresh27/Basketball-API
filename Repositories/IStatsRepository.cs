using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basketball_API.Repositories
{
    public interface IStatsRepository
    {
        public Task<double> GetStat(string player, string stat); 

    }
}
