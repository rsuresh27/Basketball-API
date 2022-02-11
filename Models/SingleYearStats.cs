using System.Collections.Generic;

namespace Basketball_API.Models
{
    public class SingleYearStats
    {

        public string Year { get; set; }
        public Dictionary<string, string> Stats { get; set; }

        public SingleYearStats(string year, Dictionary<string, string> stats)
        {
            Year = year;
            Stats = stats;
        }

    }
}
