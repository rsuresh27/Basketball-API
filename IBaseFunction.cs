using HtmlAgilityPack;
using System.Threading.Tasks;

namespace Basketball_API
{
    public interface IBaseFunctions
    {
        public Task<string> GameTime(string gameID);

        public Task<string> GetWeekNBA();

        public Task<ValidatedScore> ValidateScore(string gameID, string today);

        public Task<string> GameTimeNCAA(string gameID);

        public Task<string> LoadWebPageAsString(string url);

        public HtmlNodeCollection GetChildNodes(HtmlNodeCollection htmlNodes, string divID);
    }
}
