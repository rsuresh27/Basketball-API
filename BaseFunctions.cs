using HtmlAgilityPack;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Basketball_API
{
    public abstract class BaseFunctions : IBaseFunctions
    { 
        private readonly IHttpClientFactory _httpClientFactory;

        protected BaseFunctions(IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
        }

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

        public async Task<string> GetWeekNBA()
        {
            var url = "https://www.nba.com/key-dates";

            HtmlDocument htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(await LoadWebPageAsString(url));

            var dates = htmlDocument.GetElementbyId("__next").Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "") == "Block_blockContainer__2tJ58");

            var startDate = dates.Descendants("li").FirstOrDefault(node => node.InnerText.Contains("NBA Regular Season") && node.InnerText.Contains("Start")).InnerText.Split(':').ElementAtOrDefault(0).Trim();

            startDate = (startDate + " " + DateTime.UtcNow.AddYears(-1).Year);

            DateTime startDateConverted = DateTime.Parse(startDate).Date;

            var daysTillNextMondayStartDate = ((int)DayOfWeek.Monday - (int)startDateConverted.DayOfWeek + 7) % 7;

            var nextMondayStartDate = startDateConverted.AddDays(daysTillNextMondayStartDate);

            return Convert.ToString(Math.Ceiling((DateTime.UtcNow.Date.AddDays(5) - startDateConverted).TotalDays / 7));
        }

        public async Task<ValidatedScore> ValidateScore(string gameID, string today)
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

            var gamecastPage = gamecast.GetElementbyId("global-viewport");

            var gamecastScoreContainer = gamecastPage.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "").Contains("competitors"));

            var gamecastScores = gamecastScoreContainer.Descendants("div").Where(node => node.GetAttributeValue("class", "").Contains("score icon-font")).Select(node => node.InnerText).OrderBy(score => score);

            var gamecastWinner = gamecastScoreContainer.ParentNode.GetAttributeValue("class", ""); 

            if (scoreboardScores.SequenceEqual(gamecastScores))
            {
                if(scoreboardWinner != null || gamecastWinner.Contains("winner"))
                {
                    if(scoreboardWinner != null && gamecastWinner.Contains("winner"))
                    {
                        return ValidatedScore.Validated;
                    }

                    else
                    {
                        return ValidatedScore.NotValidated; 
                    }
                }

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

        #endregion

        #region HTTP

        public async Task<string> LoadWebPageAsString(string url)
        {
            try
            {
                using (HttpClient client = _httpClientFactory.CreateClient())
                {
                    using (HttpResponseMessage httpResponse = await client.GetAsync(url))
                    {
                        httpResponse.Headers.CacheControl = new CacheControlHeaderValue
                        {
                            NoCache = true,
                            NoStore = true,
                            MustRevalidate = true
                        };

                        httpResponse.Headers.Pragma.ParseAdd("no-cache");
                        httpResponse.Content?.Headers.TryAddWithoutValidation("Expires", "0");

                        if (httpResponse.IsSuccessStatusCode)
                        {
                            using (HttpContent httpContent = httpResponse.Content)
                            {
                                var webPage =  await httpContent.ReadAsStringAsync();
                                return webPage; 
                            }
                        }
                        else
                        {
                            throw new Exception("An error occurred getting the data, please check you typed the input parameters correctly or try again later");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        #endregion

        #region HTML

        public HtmlNodeCollection GetChildNodes(HtmlNodeCollection htmlNodes, string divID)
        {
            return htmlNodes.Where(node => node?.Id == divID).Select(selectedNode => selectedNode?.ChildNodes).FirstOrDefault();
        }

        #endregion

    }
}
