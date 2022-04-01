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
    public class TeamsRepository : BaseFunctions, ITeamsRepository
    {
        public TeamsRepository(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

        #region Endpoints

        public async Task<string> GetDepthChart(string team, string year)
        {
            return await GetDepthChartNBA(team, year);
        }

        public async Task<string> GetGameResults(string team, string year)
        {
            return await GetGameResultsNBA(team, year);
        }

        public async Task<string> GetTransactions(string team, string year)
        {
            return await GetTransactionsNBA(team, year);
        }

        #endregion

        #region Team Functions

        private async Task<string> GetDepthChartNBA(string team, string year)
        {
            try
            {
                var url = $"https://www.basketball-reference.com";

                HtmlDocument htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                var content = htmlDocument.GetElementbyId("content");

                var teamsList = content.Descendants("select").FirstOrDefault(node => node.Id == "select_team").Descendants("option");

                var teamName = teamsList.FirstOrDefault(node => node.InnerText.ToLower() == team.ToLower()).GetAttributeValue("value", "");

                var depthYear = string.IsNullOrEmpty(year) ? (DateTime.UtcNow.Month > 9 ? DateTime.UtcNow.AddYears(1).Year.ToString() : DateTime.UtcNow.Year.ToString()) : year;

                var depthChartUrl = $"https://www.basketball-reference.com{teamName}/{depthYear}_depth.html";

                htmlDocument.LoadHtml(await LoadWebPageAsString(depthChartUrl));

                var depthChartContent = htmlDocument.GetElementbyId("content");

                var depthChartDiv = depthChartContent.Descendants("div").FirstOrDefault(node => node.Id == "div_depth_chart");

                dynamic depthChart = new ExpandoObject();

                if (depthChartDiv != null)
                {
                    depthChart.DepthChart = new List<Dictionary<string, List<string>>>();

                    var positions = depthChartDiv.Descendants("div").Where(node => node.Id.Contains("depth_chart"));

                    foreach (var position in positions)
                    {
                        var positionDepthTable = position.Descendants("table").FirstOrDefault();

                        var positionName = positionDepthTable.Descendants("caption").FirstOrDefault().InnerText;

                        var playerNames = positionDepthTable.Descendants("a").Select(node => node.InnerText).ToList();

                        var positionDepthChart = new Dictionary<string, List<string>>();

                        positionDepthChart.Add(positionName, playerNames);

                        depthChart.DepthChart.Add(positionDepthChart);
                    }
                }

                return JsonSerializer.Serialize(depthChart);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private async Task<string> GetGameResultsNBA(string team, string year)
        {
            try
            {
                var url = $"https://www.basketball-reference.com";

                HtmlDocument htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                var content = htmlDocument.GetElementbyId("content");

                var teamsList = content.Descendants("select").FirstOrDefault(node => node.Id == "select_team").Descendants("option");

                var teamSelect = teamsList.FirstOrDefault(node => node.InnerText.ToLower() == team.ToLower());

                var teamUrlEndpoint = teamSelect.GetAttributeValue("value", "");

                var teamName = teamSelect.InnerText;

                var gamesYear = string.IsNullOrEmpty(year) ? (DateTime.UtcNow.Month > 9 ? DateTime.UtcNow.AddYears(1).Year.ToString() : DateTime.UtcNow.Year.ToString()) : year;

                var gamesUrl = $"https://www.basketball-reference.com{teamUrlEndpoint}/{gamesYear}_games.html";

                htmlDocument.LoadHtml(await LoadWebPageAsString(gamesUrl));

                var gamesContent = htmlDocument.GetElementbyId("content");

                var gamesTableDiv = gamesContent.Descendants("div").FirstOrDefault(node => node.Id == "div_games");

                var gamesTable = gamesTableDiv.Descendants("table").FirstOrDefault(node => node.Id == "games");

                dynamic gamesPlayed = new ExpandoObject();

                gamesPlayed.Games = new List<Dictionary<string, object>>();

                if (gamesTable != null)
                {
                    var games = gamesTable.Descendants("tr").Where(node => node.XPath.Contains("tbody"));

                    foreach (var game in games)
                    {

                        var gameNumber = game.ChildNodes.FirstOrDefault(node => node.GetAttributeValue("data-stat", "") == "g").InnerText;

                        var gameDate = game.ChildNodes.FirstOrDefault(node => node.GetAttributeValue("data-stat", "") == "date_game").InnerText;

                        var opponent = game.ChildNodes.FirstOrDefault(node => node.GetAttributeValue("data-stat", "") == "opp_name").InnerText;

                        var gameResult = game.ChildNodes.FirstOrDefault(node => node.GetAttributeValue("data-stat", "") == "game_result").InnerText;

                        var overtime = game.ChildNodes.FirstOrDefault(node => node.GetAttributeValue("data-stat", "") == "overtimes").InnerText;

                        var gamePoints = game.ChildNodes.FirstOrDefault(node => node.GetAttributeValue("data-stat", "") == "pts").InnerText;

                        var gameOpponentPoints = game.ChildNodes.FirstOrDefault(node => node.GetAttributeValue("data-stat", "") == "opp_pts").InnerText;

                        var teamWins = game.ChildNodes.FirstOrDefault(node => node.GetAttributeValue("data-stat", "") == "wins").InnerText;

                        var teamLosses = game.ChildNodes.FirstOrDefault(node => node.GetAttributeValue("data-stat", "") == "losses").InnerText;

                        var teamStreak = game.ChildNodes.FirstOrDefault(node => node.GetAttributeValue("data-stat", "") == "game_streak").InnerText;

                        var gameDictJSON = new Dictionary<string, object>();

                        var gameResultJSON = new Dictionary<string, string>()
                        {
                            {"Result", gameResult + (!string.IsNullOrEmpty(overtime) ? $"/{overtime}" : "")  },
                            { teamName, gamePoints },
                            { opponent, gameOpponentPoints},
                        };

                        var teamRecordJSON = new Dictionary<string, string>()
                        {
                             {"Wins", teamWins},
                             {"Losses", teamLosses }
                        };

                        gameDictJSON.Add("GameNumber", gameNumber);
                        gameDictJSON.Add("Date", gameDate);
                        gameDictJSON.Add("Score", gameResultJSON);
                        gameDictJSON.Add("Record", teamRecordJSON);
                        gameDictJSON.Add("Streak", teamStreak);

                        gamesPlayed.Games.Add(gameDictJSON);
                    }
                }

                return JsonSerializer.Serialize(gamesPlayed);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private async Task<string> GetTransactionsNBA(string team, string year)
        {
            try
            {
                var url = $"https://www.basketball-reference.com";

                HtmlDocument htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                var content = htmlDocument.GetElementbyId("content");

                var teamsList = content.Descendants("select").FirstOrDefault(node => node.Id == "select_team").Descendants("option");

                var teamName = teamsList.FirstOrDefault(node => node.InnerText.ToLower() == team.ToLower()).GetAttributeValue("value", "");

                var transactionYear = string.IsNullOrEmpty(year) ? (DateTime.UtcNow.Month > 9 ? DateTime.UtcNow.AddYears(1).Year.ToString() : DateTime.UtcNow.Year.ToString()) : year;

                var transactionsUrl = $"https://www.basketball-reference.com{teamName}/{transactionYear}_transactions.html";

                htmlDocument.LoadHtml(await LoadWebPageAsString(transactionsUrl));

                var transactionsContent = htmlDocument.GetElementbyId("content");

                var transactionsList = transactionsContent.Descendants("ul").FirstOrDefault(node => node.GetAttributeValue("class", "") == "page_index").Descendants("li");

                dynamic transactions = new ExpandoObject();

                if (transactionsList != null)
                {
                    transactions.Transactions = new List<Dictionary<string, List<string>>>();

                    foreach (var transactionGrouping in transactionsList)
                    {
                        var transactionDate = transactionGrouping.Descendants("span").FirstOrDefault().InnerText;

                        var transactionsOnDate = new Dictionary<string, List<string>>();

                        var transactionsDetails = transactionGrouping.Descendants("p").Where(node => node.GetAttributeValue("class", "").Contains("transaction")).Select(node => node.InnerText).ToList();

                        transactionsOnDate.Add(transactionDate, transactionsDetails);

                        transactions.Transactions.Add(transactionsOnDate);

                    }
                }

                return JsonSerializer.Serialize(transactions);
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        #endregion

    }
}
