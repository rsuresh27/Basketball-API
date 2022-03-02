using HtmlAgilityPack;
using System.Linq;
using System.Threading.Tasks;
using System;

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

        public async static Task<string> GetWeekNBA()
        {
            var url = "https://www.nba.com/key-dates";

            HtmlDocument htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(await HttpExtensions.LoadWebPageAsString(url));

            var dates = htmlDocument.GetElementbyId("__next").Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "") == "Block_blockContainer__2tJ58");

            var startDate = dates.Descendants("li").FirstOrDefault(node => node.InnerText.Contains("NBA Regular Season") && node.InnerText.Contains("Start")).InnerText.Split(':').ElementAtOrDefault(0).Trim();

            startDate = (startDate + " " + DateTime.UtcNow.AddYears(-1).Year);

            DateTime startDateConverted = DateTime.Parse(startDate).Date;

            var daysTillNextMondayStartDate = ((int)DayOfWeek.Monday - (int)startDateConverted.DayOfWeek + 7) % 7;

            var nextMondayStartDate = startDateConverted.AddDays(daysTillNextMondayStartDate);

            return Convert.ToString(Math.Ceiling((DateTime.UtcNow.Date.AddDays(5) - startDateConverted).TotalDays / 7));
        }
    }
}
