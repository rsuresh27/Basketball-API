using Basketball_API.Base_Classes;
using Basketball_API.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Basketball_API.Repositories
{
    public class StatRepository : BaseFunctions, IStatsRepository
    {
        public StatRepository(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

        #region Endpoints

        public async Task<double> GetStat(string player, string stat, string year = null)
        {
            return await GetSingleStat(player, stat, year);
        }

        public async Task<Dictionary<string, string>> GetSeasonStats(string player, string year = null)
        {
            return await GetSeasonPlayerStats(player, year);
        }

        public async Task<Dictionary<string, string>> GetTeamSeasonStats(string team, string year = null)
        {
            return await GetSeasonTeamStats(team, year);
        }

        public async Task<List<Dictionary<string, string>>> GetDayTopPlayers(string daysAgo)
        {
            return await GetDayTop5Players(daysAgo);
        }

        #endregion

        #region Stat Functions

        private async Task<Stats> LoadPlayerStats(string player)
        {
            try
            {
                player = player.Trim();

                //this list will hold player stats for entire career
                List<SingleYearStats> playerStats = new List<SingleYearStats>();

                //this dict will map all common stat abbreviations to stat ids that are used by basketball-reference.com
                Dictionary<string, string> statToStatId = new Dictionary<string, string>();

                //player must have first and last name 
                if (player.Split(' ').Count() != 2)
                {
                    throw new Exception("Invalid name");
                }

                //format name to match basketball-reference query string
                string playerFirstNameSearch = player.Split(' ').ElementAt(0).Substring(0, 2).ToLower();

                string playerLastNameSearch = player.Split(' ').ElementAt(1).Length > 5 ? player.Split(' ').ElementAt(1).Substring(0, 5).ToLower() : player.Split(' ').ElementAt(1).ToLower();

                var playerSearchName = String.Format("{0}{1}", playerLastNameSearch, playerFirstNameSearch);

                string playerLastNameIntial = playerLastNameSearch.Substring(0, 1);

                var url = $"https://www.basketball-reference.com/players/{playerLastNameIntial}/{playerSearchName}01.html";

                HtmlDocument htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                //get div that contains the info we need
                var content = htmlDocument.GetElementbyId("content")?.ChildNodes;


                //navigate down divs to get the div that contains the table 
                var tableContainer = GetChildNodes(content, "all_per_game-playoffs_per_game");


                //if a player hasnt been to the playoffs, we need to go down a different path within the html because the eleme
                //switcher will have two tabs if a player has gone to the playoffs vs one for a person that hasnt gone, so we count the number of tabs in the switcher to know where to go 
                var playoffSwitcher = tableContainer.Where(node => node.GetAttributeValue("class", "").Contains("filter switcher")).Select(node => node).FirstOrDefault();

                bool playoffs = playoffSwitcher?.Descendants("div").Count() > 1;

                HtmlNodeCollection statsTableDiv = null;

                if (playoffs)
                {
                    statsTableDiv = GetChildNodes(tableContainer, "switcher_per_game-playoffs_per_game");
                }
                else
                {
                    statsTableDiv = tableContainer;
                }

                var regularSeasonStatsTableDiv = GetChildNodes(statsTableDiv, "div_per_game");

                //once table is reached, get each trow from the tbody to get the data needed
                var trowsTbody = regularSeasonStatsTableDiv.Descendants("tr").Where(node => node.ParentNode.XPath.Contains("tbody"));

                //loop through each trow in the table and use the trow id (ex. per_game.2022) value as key for the dictionary
                trowsTbody.ToList().ForEach(trow => playerStats.Add(new SingleYearStats(trow.Id, null)));

                //next, get the stat id for each stat from the individual header in the "data-stat" attribute, which is in thead section
                var thead = regularSeasonStatsTableDiv.Descendants("tr").Where(node => node.ParentNode.XPath.Contains("thead"));

                //gets all rows of thead (only one row in table header)
                var thThead = thead.Select(node => node.Descendants("th")).SingleOrDefault();

                //for each key (year) in the playerStats dictionary, we want to access the appropriate value and insert a dictionary that contains the stats for that year 
                //the thead contains a single row of tds. Each td contains an attribute called "data-stat" which can be used to identify each unique stat 
                //create a dictionry whose key is the data-stat value and value is empty (for now). Loop through each td and insert new keyvaluepair into dictionary, then insert dict into the value of the current dict  
                Dictionary<string, string> seasonStatsIds = new Dictionary<string, string>();

                thThead?.ToList().ForEach(th => { seasonStatsIds.Add(th.GetAttributeValue("data-stat", ""), ""); statToStatId.Add(th.InnerText.ToLower(), th.GetAttributeValue("data-stat", "")); });

                //assign new dictionary to each year 
                foreach (SingleYearStats singleYear in playerStats)
                {
                    singleYear.Stats = new Dictionary<string, string>(seasonStatsIds);
                }

                //get individual stat for each seaason, which is in td of each tr and match the stat ids
                var trows = trowsTbody.Select(node => node.ChildNodes.AsEnumerable());

                //loop through all the trows, which contains stats for each year
                //each td within a trow contains an individual stat value, which we can match with a stat id using the "data-stat" attribute
                //the parent node of each td is the trow, which contains the year (trow element id i.e. per_game.2022) that is used to match with the appropriate key and get the appropriate dictionary
                //each td element contains the value for the stat (data-stat attribute value), find the value (dictionary) to write to using the key (year) and once that value is found write to the correct value by using the statID ("data-stat" attribute value) as the key
                foreach (var trow in trows)
                {
                    foreach (var td in trow)
                    {
                        var statYear = td.ParentNode.Id;
                        var statId = td.GetAttributeValue("data-stat", "empty");
                        var statValue = td.InnerText;

                        var year = playerStats.Where(year => year.Year == statYear).Select(selectedYear => selectedYear.Stats).FirstOrDefault();

                        year[statId] = statValue;
                    }
                }

                //use struct to easily pass both dictionaries in one object
                return new Stats(statToStatId, playerStats);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private async Task<double> GetSingleStat(string player, string statToSearch, string year)
        {
            try
            {
                var stats = await LoadPlayerStats(player);

                //basketball-reference uses different terms for rebounds and turnovers, so if the request wants rebounds or turnovers then replace it with the correct term 
                statToSearch = statToSearch.Trim();

                statToSearch = Regex.Replace(statToSearch.ToLower(), "reb", "trb");

                statToSearch = Regex.Replace(statToSearch.ToLower(), "to", "tov");

                var statId = stats.StatNameToStatId.Where(stat => stat.Key.Contains(statToSearch)).Select(selectedStat => selectedStat.Value).FirstOrDefault();

                year ??= Convert.ToString(DateTime.Today.Year);

                if (statId.Count() < 1)
                {
                    throw new Exception("Invalid stat");
                }

                var yearsList = stats.PlayerStats.ToList().Select(year => Convert.ToInt32(year.Year.Split('.').ElementAt(1)));

                if (yearsList.Min() > Convert.ToInt32(year))
                {
                    throw new Exception("Player was not playing in NBA during this season");
                }

                if (yearsList.Max() < Convert.ToInt32(year))
                {
                    throw new Exception("Invalid NBA Season");
                }

                year = year.Trim();

                //only get stats for current season (2022 in this case)
                var yearStats = stats.PlayerStats.Where(statYear => statYear.Year.Contains(year)).Select(selectedYear => selectedYear.Stats).FirstOrDefault();

                return yearStats.Where(stat => stat.Key == statId).Select(statValue => Convert.ToDouble(statValue.Value)).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private async Task<Dictionary<string, string>> GetSeasonPlayerStats(string player, string year)
        {
            try
            {
                var stats = await LoadPlayerStats(player);

                var currentSeasonStats = new Dictionary<string, string>();

                var yearsList = stats.PlayerStats.ToList().Select(year => Convert.ToInt32(year.Year.Split('.').ElementAt(1)));

                year ??= Convert.ToString(DateTime.Today.Year);

                if (yearsList.Min() > Convert.ToInt32(year))
                {
                    throw new Exception("Player was not playing in NBA during this season");
                }

                if (yearsList.Max() < Convert.ToInt32(year))
                {
                    throw new Exception("Invalid NBA Season");
                }

                year = year.Trim();

                var yearStats = stats.PlayerStats.Where(statYear => statYear.Year.Contains(year)).Select(selectedYear => selectedYear.Stats).FirstOrDefault();

                yearStats.ToList().ForEach(stat => currentSeasonStats.Add(stats.StatNameToStatId.Where(statIds => statIds.Value == stat.Key).Select(statId => statId.Key).FirstOrDefault(), stat.Value));

                return currentSeasonStats;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private async Task<Dictionary<string, string>> GetSeasonTeamStats(string team, string year = null)
        {
            try
            {
                team = team.Trim().ToLower();

                year = year?.Trim();

                var url = $"https://www.basketball-reference.com";

                HtmlDocument htmlDocument = new HtmlDocument();

                //load webpage and get html within the webpage to parse for stats
                htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                var content = htmlDocument.GetElementbyId("content").ChildNodes.Where(node => node.GetAttributeValue("class", "") == "flexindex").FirstOrDefault();

                var forms = content.Descendants("form").ToList().Where(node => node.GetAttributeValue("class", "") == "srbasic sr_goto no-deserialize concat");

                var teamSelectForm = forms.ToList().Select(node => node.Descendants("select")).FirstOrDefault();

                var nbaTeams = teamSelectForm.Where(node => node.Id == "select_team").FirstOrDefault().ChildNodes;

                var teamUrl = nbaTeams.ToList().Where(teamName => teamName.InnerText.ToLower().Contains(team)).Select(selectedTeam => selectedTeam.GetAttributeValue("value", "")).FirstOrDefault();

                teamUrl = Regex.Replace(teamUrl, "BRK", "NJN");

                var teamCurrentSeasonStatsUrl = $"{url}{teamUrl}/stats_per_game_totals.html";

                Console.WriteLine(teamCurrentSeasonStatsUrl);

                //team stats 
                htmlDocument.LoadHtml(await LoadWebPageAsString(teamCurrentSeasonStatsUrl));

                var teamContent = htmlDocument.GetElementbyId("content").ChildNodes;

                var allStats = GetChildNodes(teamContent, "all_stats");

                var statsDiv = GetChildNodes(allStats, "div_stats");

                var statsTable = GetChildNodes(statsDiv, "stats");

                var statsTableThead = statsTable.Descendants();

                var statTableTh = statsTableThead.ToList().Where(node => node.XPath.Contains("thead")).GroupBy(node => node.GetAttributeValue("data-stat", "")).Select(node => node.FirstOrDefault()).Where(node => node.GetAttributeValue("data-stat", "") != "foo");

                Dictionary<string, string> statIds = statTableTh.AsEnumerable().Where(node => node.GetAttributeValue("data-stat", "") != "").ToDictionary(node => node.GetAttributeValue("data-stat", ""), node => node.InnerText);

                var statTableTbody = statsTableThead.ToList().Where(node => node.XPath.Contains("tbody"));

                var statTableYearColumn = statTableTbody.ToList().Where(node => node.GetAttributeValue("data-stat", "") == "season" && node.InnerText.ToLower() != "season");

                var seasonStatsRow = statTableYearColumn.ToList().Where(node => node.InnerText.Substring(node.InnerText.Length - 2, 2) == (year ?? Convert.ToString(DateTime.Now.Year)).Substring(2, 2)).Select(node => node.ParentNode).FirstOrDefault();

                Dictionary<string, string> stats = seasonStatsRow.ChildNodes.AsEnumerable().Where(node => node.GetAttributeValue("data-stat", "") != "foo").ToDictionary(node => node.GetAttributeValue("data-stat", ""), node => node.InnerText);

                return statIds.Keys.ToList().AsEnumerable().ToDictionary(statId => statIds[statId], statId => stats[statId]);
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private async Task<List<Dictionary<string, string>>> GetDayTop5Players(string daysAgo)
        {
            try
            {
                daysAgo = daysAgo.Trim();

                var url = $"https://www.basketball-reference.com/friv/last_n_days.fcgi?n={daysAgo}&type=per_game";

                HtmlDocument htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                var allPlayersTableDiv = GetChildNodes(htmlDocument.GetElementbyId("content").ChildNodes, "all_players");

                if (allPlayersTableDiv == null)
                {
                    throw new Exception($"No games played within the past {daysAgo} days");
                }

                var tableDiv = GetChildNodes(allPlayersTableDiv, "div_players");

                var table = GetChildNodes(tableDiv, "players");

                var thead = table.Descendants("th").Where(node => node.ParentNode.XPath.Contains("thead"));

                List<string> statHighlighs = new List<string> { "pts", "trb", "ast", "player" };

                Dictionary<string, string> statIds = thead.ToList().GroupBy(node => node.GetAttributeValue("data-stat", "")).Where(grouping => statHighlighs.Any(stat => grouping.Key.Contains(stat))).Select(node => node.FirstOrDefault()).AsEnumerable().ToDictionary(node => node.GetAttributeValue("data-stat", ""), node => node.InnerText);

                var tbody = table.Descendants("tr").Where(node => node.ParentNode.XPath.Contains("tbody")).Take(5);

                var stats = tbody.Select(node => node.ChildNodes);

                List<Dictionary<string, string>> playerStats = stats.ToList().Select(player => player.Where(playerStat => statHighlighs.Any(statId => playerStat.GetAttributeValue("data-stat", "").Contains(statId))).AsEnumerable().ToDictionary(node => node.GetAttributeValue("data-stat", ""), node => node.InnerText)).ToList();

                return playerStats;
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

        }

        #endregion
    }
}
