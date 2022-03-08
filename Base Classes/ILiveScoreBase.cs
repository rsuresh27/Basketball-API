using HtmlAgilityPack;
using System.Threading.Tasks;


namespace Basketball_API.Base_Classes
{
    public interface ILiveScoreBase 
    {
        public Task<string> GameTime(string gameID);   

        public Task<ValidatedScore> ValidateScore(string gameID, string today);

        public Task<string> GameTimeNCAA(string gameID);
    }
}
