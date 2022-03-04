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

        public async static Task<ValidatedScore> ValidateScore(string gameID, string today)
        {
            var scoreboardURL = $"https://www.espn.com/nba/scoreboard/_/date/{today}";

            var gamecastURL = $"https://www.espn.com/nba/game/_/gameId/{gameID}";

            HtmlDocument scoreboard = new HtmlDocument();
            HtmlDocument gamecast = new HtmlDocument();

            scoreboard.LoadHtml(await HttpExtensions.LoadWebPageAsString(scoreboardURL));

            gamecast.LoadHtml(await HttpExtensions.LoadWebPageAsString(gamecastURL));

            var scoreboardPage = HtmlExtensions.GetChildNodes(scoreboard.GetElementbyId("espnfitt").ChildNodes, "DataWrapper");

            var scoreboardGames = scoreboardPage.Descendants("section").FirstOrDefault(node => node.GetAttributeValue("class", "") == "Scoreboard bg-clr-white flex flex-auto justify-between" && node.Id == gameID);

            var scoreboardScoreContainer = scoreboardPage.Descendants("div").Where(node => node.GetAttributeValue("class", "") == "Scoreboard__RowContainer flex flex-column flex-auto" && node.ParentNode.Id == gameID).FirstOrDefault();

            var scoreboardScores = scoreboardScoreContainer.Descendants("div").Where(node => node.GetAttributeValue("class", "").Contains("ScoreCell__Score h4")).Select(node => node.InnerText).OrderBy(score => score);

            var gamecastPage = gamecast.GetElementbyId("global-viewport");

            var gamecastScoreContainer = gamecastPage.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "").Contains("competitors"));

            var gamecastScores = gamecastScoreContainer.Descendants("div").Where(node => node.GetAttributeValue("class", "").Contains("score icon-font")).Select(node => node.InnerText).OrderBy(score => score);

            if (scoreboardScores.SequenceEqual(gamecastScores))
            {
                return ValidatedScore.Validated;
            }

            else if (!scoreboardScores.Any() || !gamecastScores.Any())
            {
                return ValidatedScore.GameNotStarted;
            }

            else
            {
                return ValidatedScore.NotValidated;
            }
        }

        public async static Task<string> GameTimeNCAA(string gameID)
        {
            var url = $"https://www.espn.com/mens-college-basketball/game/_/gameId/{gameID}";

            HtmlDocument htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(await HttpExtensions.LoadWebPageAsString(url));

            var page = htmlDocument.GetElementbyId("global-viewport").ChildNodes;

            var content = page.FirstOrDefault(node => node.Id == "pane-main");

            var time = content.Descendants().FirstOrDefault(node => node.GetAttributeValue("class", "").Contains("status-detail"));

            return time.InnerText;
        }
    }
}
