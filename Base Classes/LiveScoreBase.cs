using HtmlAgilityPack;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions; 

namespace Basketball_API.Base_Classes
{
    public abstract class LiveScoreBase : BaseFunctions, ILiveScoreBase
    {
        protected LiveScoreBase(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

        #region Live Scores

        public async Task<string> GameTime(string gameID)
        {
            var url = $"https://www.espn.com/nba/game/_/gameId/{gameID}";

            HtmlDocument htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(await LoadWebPageAsString(url));

            var page = htmlDocument.GetElementbyId("global-viewport").ChildNodes;

            var content = page.FirstOrDefault(node => node.Id == "pane-main");

            var time = content.Descendants().FirstOrDefault(node => node.GetAttributeValue("class", "").Contains("status-detail"));

            return time.InnerText;
        }

        public async Task<Tuple<ValidatedScore, HtmlDocument>> ValidateScore(string gameID, string today)
        {
            var scoreboardURL = $"https://www.espn.com/nba/scoreboard/_/date/{today}";

            var gamecastURL = $"https://www.espn.com/nba/game/_/gameId/{gameID}";

            HtmlDocument scoreboard = new HtmlDocument();
            HtmlDocument gamecast = new HtmlDocument();

            scoreboard.LoadHtml(await LoadWebPageAsString(scoreboardURL));

            gamecast.LoadHtml(await LoadWebPageAsString(gamecastURL));

            var scoreboardPage = GetChildNodes(scoreboard.GetElementbyId("espnfitt").ChildNodes, "DataWrapper");

            var scoreboardGames = scoreboardPage.Descendants("section").FirstOrDefault(node => node.GetAttributeValue("class", "") == "Scoreboard bg-clr-white flex flex-auto justify-between" && node.Id == gameID);

            var scoreboardScoreContainer = scoreboardPage.Descendants("div").Where(node => node.GetAttributeValue("class", "") == "Scoreboard__RowContainer flex flex-column flex-auto" && node.ParentNode.Id == gameID).FirstOrDefault();

            var scoreboardScores = scoreboardScoreContainer.Descendants("div").Where(node => node.GetAttributeValue("class", "").Contains("ScoreCell__Score h4")).Select(node => node.InnerText).OrderBy(score => score);

            var scoreboardWinner = scoreboardScoreContainer.Descendants("svg").FirstOrDefault(node => node.GetAttributeValue("class", "") == "ScoreboardScoreCell__WinnerIcon absolute icon__svg");

            var scoreboardGameDescription = scoreboardScoreContainer.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "").Contains("ScoreboardScoreCell__Note"))?.InnerText; 

            var gamecastPage = gamecast.GetElementbyId("global-viewport");

            var gamecastScoreContainer = gamecastPage.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "").Contains("competitors"));

            var gamecastScores = gamecastScoreContainer.Descendants("div").Where(node => node.GetAttributeValue("class", "").Contains("score icon-font")).Select(node => node.InnerText).OrderBy(score => score);

            var gamecastWinner = gamecastScoreContainer.ParentNode.GetAttributeValue("class", "");

            if (scoreboardScores.SequenceEqual(gamecastScores))
            {
                if (scoreboardWinner != null || gamecastWinner.Contains("winner"))
                {
                    if (scoreboardWinner != null && gamecastWinner.Contains("winner"))
                    {
                        //verify the series record is updated if it is a playoff/finals game
                        if (!string.IsNullOrEmpty(scoreboardGameDescription) && scoreboardGameDescription.ToLower().Contains("game"))
                        {
                            var numbers = scoreboardGameDescription.Where(ch => char.IsDigit(ch)).Select(number => Convert.ToInt32(number.ToString())); 
                            var currentGame = numbers.ElementAtOrDefault(0);
                            var winningRecord = numbers.ElementAtOrDefault(1);
                            var losingRecord = numbers.ElementAtOrDefault(2);

                            if(currentGame == winningRecord + losingRecord)
                            {
                                return new Tuple<ValidatedScore, HtmlDocument>(ValidatedScore.Validated, scoreboard);
                            }

                            else
                            {
                                return new Tuple<ValidatedScore, HtmlDocument>(ValidatedScore.NotValidated, scoreboard);
                            }

                        }

                        return new Tuple<ValidatedScore, HtmlDocument>(ValidatedScore.Validated, scoreboard);
                    }

                    else
                    {
                        return new Tuple<ValidatedScore, HtmlDocument>(ValidatedScore.NotValidated, scoreboard);
                    }
                }

                return new Tuple<ValidatedScore, HtmlDocument>(ValidatedScore.Validated, scoreboard);
            }

            else if (!scoreboardScores.Any() || !gamecastScores.Any())
            {
                return new Tuple<ValidatedScore, HtmlDocument>(ValidatedScore.GameNotStarted, scoreboard); ;
            }

            else
            {
                return new Tuple<ValidatedScore, HtmlDocument>(ValidatedScore.NotValidated, scoreboard);
            }
        }

        public async Task<string> GameTimeNCAA(string gameID)
        {
            var url = $"https://www.espn.com/mens-college-basketball/game/_/gameId/{gameID}";

            HtmlDocument htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(await LoadWebPageAsString(url));

            var page = htmlDocument.GetElementbyId("global-viewport").ChildNodes;

            var content = page.FirstOrDefault(node => node.Id == "pane-main");

            var time = content.Descendants().FirstOrDefault(node => node.GetAttributeValue("class", "").Contains("status-detail"));

            return time.InnerText;
        }

        public async Task<Tuple<ValidatedScore, HtmlDocument>> ValidateScoreNCAA(string gameID, string today)
        {
            var scoreboardURL = $"https://www.espn.com/mens-college-basketball/scoreboard/_/date/{today}/group/50";

            var gamecastURL = $"https://www.espn.com/mens-college-basketball/game/_/gameId/{gameID}";

            HtmlDocument scoreboard = new HtmlDocument();
            HtmlDocument gamecast = new HtmlDocument();

            scoreboard.LoadHtml(await LoadWebPageAsString(scoreboardURL));

            gamecast.LoadHtml(await LoadWebPageAsString(gamecastURL));

            var scoreboardPage = GetChildNodes(scoreboard.GetElementbyId("espnfitt").ChildNodes, "DataWrapper");

            var scoreboardGames = scoreboardPage.Descendants("section").FirstOrDefault(node => node.GetAttributeValue("class", "") == "Scoreboard bg-clr-white flex flex-auto justify-between" && node.Id == gameID);

            var scoreboardScoreContainer = scoreboardPage.Descendants("div").Where(node => node.GetAttributeValue("class", "") == "Scoreboard__RowContainer flex flex-column flex-auto" && node.ParentNode.Id == gameID).FirstOrDefault();

            var scoreboardScores = scoreboardScoreContainer.Descendants("div").Where(node => node.GetAttributeValue("class", "").Contains("ScoreCell__Score h4")).Select(node => node.InnerText).OrderBy(score => score);

            var scoreboardWinner = scoreboardScoreContainer.Descendants("svg").FirstOrDefault(node => node.GetAttributeValue("class", "") == "ScoreboardScoreCell__WinnerIcon absolute icon__svg");

            var gamecastPage = gamecast.GetElementbyId("global-viewport");

            var gamecastScoreContainer = gamecastPage.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "").Contains("competitors"));

            var gamecastScores = gamecastScoreContainer.Descendants("div").Where(node => node.GetAttributeValue("class", "").Contains("score icon-font")).Select(node => node.InnerText).OrderBy(score => score);

            var gamecastWinner = gamecastScoreContainer.ParentNode.GetAttributeValue("class", "");

            if (scoreboardScores.SequenceEqual(gamecastScores))
            {
                if (scoreboardWinner != null || gamecastWinner.Contains("winner"))
                {
                    if (scoreboardWinner != null && gamecastWinner.Contains("winner"))
                    {
                        return new Tuple<ValidatedScore, HtmlDocument>(ValidatedScore.Validated, scoreboard);
                    }

                    else
                    {
                        return new Tuple<ValidatedScore, HtmlDocument>(ValidatedScore.NotValidated, scoreboard);
                    }
                }

                return new Tuple<ValidatedScore, HtmlDocument>(ValidatedScore.Validated, scoreboard);
            }

            else if (!scoreboardScores.Any() || !gamecastScores.Any())
            {
                return new Tuple<ValidatedScore, HtmlDocument>(ValidatedScore.GameNotStarted, scoreboard); ;
            }

            else
            {
                return new Tuple<ValidatedScore, HtmlDocument>(ValidatedScore.NotValidated, scoreboard);
            }
        }

        #endregion
    }
}
