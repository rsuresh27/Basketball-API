using Basketball_API.Base_Classes;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Basketball_API.Repositories
{
    public class LiveScoresRepository : LiveScoreBase, ILiveScoresRepository
    {
        public LiveScoresRepository(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

        #region Endpoints

        public async Task<string> GetGameScore(string gameID, DateTime? date = null)
        {
            return await GetScoreGame(gameID, date);
        }

        public async Task<List<string>> GetGames(DateTime? date = null)
        {
            return await GetGamesPlayed(date);
        }

        public async Task<string> GetNCAAGameScore(string gameID, DateTime? date = null)
        {
            return await GetScoreGameNCAA(gameID, date);
        }

        public async Task<List<string>> GetNCAAGames(DateTime? date = null)
        {
            return await GetGamesPlayedNCAA(date);
        }

        #endregion

        #region Live Score Functions

        private async Task<string> GetScoreGame(string gameID, DateTime? date = null)
        {
            try
            {
                var formattedDate = Regex.Replace(date.GetValueOrDefault(DateTime.UtcNow.AddHours(-6).Date).ToString("yyyy/MM/dd"), "/", string.Empty);

                var url = $"https://www.espn.com/nba/scoreboard/_/date/{formattedDate}";

                HtmlDocument htmlDocument = new HtmlDocument();

                var validated = ValidatedScore.NotValidated;

                while (validated == ValidatedScore.NotValidated)
                {
                    var validatedScore = await ValidateScore(gameID, formattedDate);
                    validated = validatedScore.Item1;
                    htmlDocument = validatedScore.Item2;
                }

                //htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                var page = GetChildNodes(htmlDocument.GetElementbyId("espnfitt").ChildNodes, "DataWrapper");

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

                var playerTeams = topPerformers.Descendants("span").Where(node => node.GetAttributeValue("class", "") == "Athlete__NameDetails ml2 clr-gray-04 di ns9");

                var topPerformersStats = topPerformers.Descendants("div").Where(node => node.GetAttributeValue("class", "") == "Athlete__Stats mt2 clr-gray-04 ns9");

                //use expandoobject to create json 
                dynamic final = new ExpandoObject();

                final.Time = await GameTime(gameID);

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

                    foreach (var team in playerTeams)
                    {
                        if (team.ParentNode == player.ParentNode)
                        {
                            final.TopPerformers[player.InnerText].Add("TEAM", team.InnerText.Split('-').ElementAtOrDefault(1).Trim());
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

        private async Task<List<string>> GetGamesPlayed(DateTime? date = null)
        {
            try
            {
                var formattedDate = Regex.Replace(date.GetValueOrDefault(DateTime.UtcNow.AddHours(-6).Date).ToString("yyyy/MM/dd"), "/", string.Empty);

                var url = $"https://www.espn.com/nba/scoreboard/_/date/{formattedDate}";

                HtmlDocument htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                var page = GetChildNodes(htmlDocument.GetElementbyId("espnfitt").ChildNodes, "DataWrapper");

                return page.Descendants("section").Where(node => node.GetAttributeValue("class", "") == "Scoreboard bg-clr-white flex flex-auto justify-between").Select(node => node.Id).ToList();
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private async Task<string> GetScoreGameNCAA(string gameID, DateTime? date = null)
        {
            try
            {
                var formattedDate = Regex.Replace(date.GetValueOrDefault(DateTime.UtcNow.AddHours(-6).Date).ToString("yyyy/MM/dd"), "/", string.Empty);

                var url = $"https://www.espn.com/mens-college-basketball/scoreboard/_/date/{formattedDate}";

                HtmlDocument htmlDocument = new HtmlDocument();

                var validated = ValidatedScore.NotValidated;

                while (validated == ValidatedScore.NotValidated)
                {
                    var validatedScore = await ValidateScoreNCAA(gameID, formattedDate);
                    validated = validatedScore.Item1;
                    htmlDocument = validatedScore.Item2;
                }

                //htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                var page = GetChildNodes(htmlDocument.GetElementbyId("espnfitt").ChildNodes, "DataWrapper");

                var games = page.Descendants("section").Where(node => node.GetAttributeValue("class", "") == "Scoreboard bg-clr-white flex flex-auto justify-between");

                var gameIDs = games.Select(game => game.Id).ToList();

                //get the scores for the game
                var gameScoreContainer = page.Descendants("div").Where(node => node.GetAttributeValue("class", "") == "Scoreboard__RowContainer flex flex-column flex-auto" && node.ParentNode.Id == gameID).FirstOrDefault();

                var teamScores = gameScoreContainer.Descendants("li").Where(node => node.GetAttributeValue("class", "").Contains("ScoreboardScoreCell__Item flex items-center relative pb2 ScoreboardScoreCell__Item--"));

                List<HtmlNode> teamNames = new List<HtmlNode>(), finalScore = new List<HtmlNode>();

                Dictionary<string, string> scores = new Dictionary<string, string>();

                //check and see if the game has started. if not then return else get the current score
                foreach (var team in teamScores)
                {
                    if (team.Descendants("div").Where(node => node.GetAttributeValue("class", "").Contains("ScoreCell__Score h4")).FirstOrDefault() == null)
                    {
                        var names = teamScores.Select(node => node.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "") == "ScoreCell__TeamName ScoreCell__TeamName--shortDisplayName truncate db")).ToList();
                        return ($"Game has not started between {names.ElementAtOrDefault(0).InnerText} and {names.ElementAtOrDefault(1).InnerText}");

                    }
                    else
                    {
                        var teamRank = team.Descendants("div").Where(node => node.GetAttributeValue("class", "") == "ScoreCell__Rank ttn ScoreCell__Rank--scoreboard").FirstOrDefault()?.InnerText;
                        var teamName = team.Descendants("div").Where(node => node.GetAttributeValue("class", "") == "ScoreCell__TeamName ScoreCell__TeamName--shortDisplayName truncate db").FirstOrDefault().InnerText;

                        scores.Add(string.IsNullOrEmpty(teamRank) ? teamName : string.Format("{0}-{1}", teamRank, teamName),
                        team.Descendants("div").Where(node => node.GetAttributeValue("class", "").Contains("ScoreCell__Score h4")).FirstOrDefault().InnerText);
                    }
                }

                var topPerformers = gameScoreContainer.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "") == "Scoreboard__Performers");

                var playerNames = topPerformers?.Descendants("span").Where(node => node.GetAttributeValue("class", "") == "Athlete__PlayerName");

                var playerTeams = topPerformers?.Descendants("span").Where(node => node.GetAttributeValue("class", "") == "Athlete__NameDetails ml2 clr-gray-04 di ns9");

                var topPerformersStats = topPerformers?.Descendants("div").Where(node => node.GetAttributeValue("class", "") == "Athlete__Stats mt2 clr-gray-04 ns9");

                //use expandoobject to create json 
                dynamic final = new ExpandoObject();

                var gameDescription = gameScoreContainer.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "") == "ScoreboardScoreCell__Note clr-gray-05 n9 w-auto")?.InnerText;

                final.Time = (await GameTimeNCAA(gameID)) + (!string.IsNullOrEmpty(gameDescription) ? ", " + gameDescription : "");

                final.Scores = scores;

                final.TopPerformers = new Dictionary<string, Dictionary<string, string>>();

                //assign score to correct player if they exist on ESPN website 
                if (topPerformers != null)
                {
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

                        foreach (var team in playerTeams)
                        {
                            if (team.ParentNode == player.ParentNode)
                            {
                                final.TopPerformers[player.InnerText].Add("TEAM", team.InnerText.Split('-').ElementAtOrDefault(1).Trim());
                            }
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

        private async Task<List<string>> GetGamesPlayedNCAA(DateTime? date = null)
        {
            try
            {
                var formattedDate = Regex.Replace(date.GetValueOrDefault(DateTime.UtcNow.AddHours(-6).Date).ToString("yyyy/MM/dd"), "/", string.Empty);

                var url = $"https://www.espn.com/mens-college-basketball/scoreboard/_/date/{formattedDate}/group/50";

                HtmlDocument htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                var page = GetChildNodes(htmlDocument.GetElementbyId("espnfitt").ChildNodes, "DataWrapper");

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
