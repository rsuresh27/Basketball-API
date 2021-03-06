using System.Collections.Generic;

namespace Basketball_API.Models
{
    public struct Stats
    {
        public Dictionary<string, string> StatNameToStatId { get; set; }
        public List<SingleYearStats> PlayerStats { get; set; }

        public Stats(Dictionary<string, string> statNameToStatId, List<SingleYearStats> playerStats)
        {
            StatNameToStatId = statNameToStatId;
            PlayerStats = playerStats;
        }
    }
}
