using System.Collections.Generic;
using System.Threading.Tasks;
using Utiliy;
using HtmlAgilityPack;
using System.Dynamic;
using System.Text.Json;
using System.Linq;
using System;
using System.Text.RegularExpressions;


namespace Basketball_API.Repositories
{
    public class LiveScoresRepository : ILiveScoresRepository
    {
        #region Endpoint

        public async Task<string> GetGameScore(string gameID)
        {
            return await GetScoreGame(gameID); 
        }

        public async Task<List<string>> GetGamesToday(DateTime date)
        {
            return await GetGamesPlayedToday(date); 
        }

        #endregion

        #region Live Score Functions

        private async Task<string> GetScoreGame(string gameID)
        {
            try
            {
                var today = Regex.Replace(DateTime.Now.ToString("yyyy/MM/dd"), "/", string.Empty);

                var url = $"https://www.espn.com/nba/scoreboard/_/date/{today}";

                HtmlDocument htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(await HttpExtensions.LoadWebPageAsString(url));

                var page = HtmlExtensions.GetChildNodes(htmlDocument.GetElementbyId("espnfitt").ChildNodes, "DataWrapper");

                var games = page.Descendants("section").Where(node => node.GetAttributeValue("class", "") == "Scoreboard bg-clr-white flex flex-auto justify-between");

                var game = games.FirstOrDefault(node => node.Id == gameID);

                var gameIDs = games.Select(game => game.Id).ToList();

                //get the scores for the game
                var gameScore = page.Descendants("div").Where(node => node.GetAttributeValue("class", "") == "Scoreboard__RowContainer flex flex-column flex-auto" && node.ParentNode.Id == gameID).FirstOrDefault();

                var teamScores = gameScore.Descendants("li").Where(node => node.GetAttributeValue("class", "").Contains("ScoreboardScoreCell__Item flex items-center relative pb2 ScoreboardScoreCell__Item--"));

                List<HtmlNode> teamNames = new List<HtmlNode>(), finalScore = new List<HtmlNode>();

                Dictionary<string, string> scores = new Dictionary<string, string>();

                //check and see if the game has started. if not then return else get the current score
                foreach (var team in teamScores)
                {
                    if (team.Descendants("div").Where(node => node.GetAttributeValue("class", "").Contains("ScoreCell__Score h4")).FirstOrDefault() == null)
                    {
                        var names = teamScores.Select(node => node.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "") == "ScoreCell__TeamName ScoreCell__TeamName--shortDisplayName truncate db")).ToList();
                        return ($"Game has not started between the {names.ElementAtOrDefault(0).InnerText} and {names.ElementAtOrDefault(1).InnerText}");

                    }
                    else
                    {

                        scores.Add(team.Descendants("div").Where(node => node.GetAttributeValue("class", "") == "ScoreCell__TeamName ScoreCell__TeamName--shortDisplayName truncate db").FirstOrDefault().InnerText,
                        team.Descendants("div").Where(node => node.GetAttributeValue("class", "").Contains("ScoreCell__Score h4")).FirstOrDefault().InnerText);
                    }
                }

                //get the top performers from the game 
                var topPerformers = gameScore.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "") == "Scoreboard__Performers");

                var playerNames = topPerformers.Descendants("span").Where(node => node.GetAttributeValue("class", "") == "Athlete__PlayerName");

                var topPerformersStats = topPerformers.Descendants("div").Where(node => node.GetAttributeValue("class", "") == "Athlete__Stats mt2 clr-gray-04 ns9");

                //use expandoobject to create json 
                dynamic final = new ExpandoObject();

                final.Time = await LiveScoresExtensions.GameTime(gameID);

                final.Scores = scores;

                final.TopPerformers = new Dictionary<string, Dictionary<string, string>>();

                //assign score to correct player
                foreach (var player in playerNames)
                {
                    final.TopPerformers.Add(player.InnerText, new Dictionary<string, string>());

                    foreach (var stats in topPerformersStats)
                    {
                        if (stats.ParentNode == player.ParentNode.ParentNode)
                        {
                            final.TopPerformers[player.InnerText] = stats.Descendants("div").Where(node => node.ParentNode.ParentNode == player.ParentNode.ParentNode).AsEnumerable().ToDictionary(node => node.ChildNodes.FirstOrDefault(node => node.GetAttributeValue("class", "") == "ml2").InnerText,
                                                                                   node => node.ChildNodes.FirstOrDefault(node => node.GetAttributeValue("class", "") == "clr-gray-01 hs9").InnerText);
                        }
                    }
                }

                return JsonSerializer.Serialize(final);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private async Task<List<string>> GetGamesPlayedToday(DateTime date)
        {
            try
            {
                var today = Regex.Replace(DateTime.Now.ToString("yyyy/MM/dd"), "/", string.Empty);

                var url = $"https://www.espn.com/nba/scoreboard/_/date/{today}";

                HtmlDocument htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(await HttpExtensions.LoadWebPageAsString(url));

                var page = HtmlExtensions.GetChildNodes(htmlDocument.GetElementbyId("espnfitt").ChildNodes, "DataWrapper");

                return page.Descendants("section").Where(node => node.GetAttributeValue("class", "") == "Scoreboard bg-clr-white flex flex-auto justify-between").Select(node => node.Id).ToList();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        #endregion


    }
}
