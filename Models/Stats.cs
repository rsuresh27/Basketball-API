using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basketball_API.Models
{
    public struct Stats
    {
        public Dictionary<string, string> StatNameToStatId { get; set; }
        public Dictionary<string, Dictionary<string, string>> PlayerStats { get; set; }

        public Stats(Dictionary<string, string> statNameToStatId, Dictionary<string, Dictionary<string, string>> playerStats)
        {
            StatNameToStatId = statNameToStatId;
            PlayerStats = playerStats; 
        }
    }
}
