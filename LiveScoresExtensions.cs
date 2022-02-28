using HtmlAgilityPack;
using System.Linq;
using System.Threading.Tasks;

namespace Utiliy
{
    public class LiveScoresExtensions
    {
        public async static Task<string> GameTime(string gameID)
        {
            var url = $"https://www.espn.com/nba/game/_/gameId/{gameID}";

            HtmlDocument htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(await HttpExtensions.LoadWebPageAsString(url));

            var page = htmlDocument.GetElementbyId("global-viewport").ChildNodes;

            var content = page.FirstOrDefault(node => node.Id == "pane-main");

            var time = content.Descendants().FirstOrDefault(node => node.GetAttributeValue("class", "").Contains("status-detail"));

            return time.InnerText;
        }
    }
}
