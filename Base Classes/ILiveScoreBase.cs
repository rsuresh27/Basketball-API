using HtmlAgilityPack;
using System.Threading.Tasks;
using System;

namespace Basketball_API.Base_Classes
{
    public interface ILiveScoreBase
    {
        public Task<string> GameTime(string gameID);

        public Task<Tuple<ValidatedScore, HtmlDocument>> ValidateScore(string gameID, string today);

        public Task<string> GameTimeNCAA(string gameID);

        public Task<Tuple<ValidatedScore, HtmlDocument>> ValidateScoreNCAA(string gameID, string today);
    }
}
