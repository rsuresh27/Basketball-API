using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Net.Http;
using Basketball_API.Models; 

namespace Basketball_API.Repositories
{
    public class StatRepository : IStatsRepository
    {
        public async Task<double> GetStat(string player, string stat)
        {
            return await GetSingleStat(player, stat); 
        }

        private static async Task<Stats> LoadPlayerStats(string player)
        {
            try
            {
                //this dict will hold player stats for entire career
                Dictionary<string, Dictionary<string, string>> playerStats = new Dictionary<string, Dictionary<string, string>>();

                //this dict will map all common stat abbreviations to stat ids that are used by basketball-reference.com
                Dictionary<string, string> statToStatId = new Dictionary<string, string>();

                //player must have first and last name 
                if(player.Split(' ').Count() != 2)
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

                //load webpage and get html within the webpage to parse for stats
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage httpResponse = client.GetAsync(url).Result)
                    {
                        if(httpResponse.IsSuccessStatusCode)
                        {
                            using (HttpContent httpContent = httpResponse.Content)
                            {
                                var webPage = await httpContent.ReadAsStringAsync();
                                htmlDocument.LoadHtml(webPage);
                            }
                        }

                        //if basketball-reference returns any error code at or above 400, the players name was not typed correctly or something is wrong with basketball-references website
                        else if(Convert.ToInt32(httpResponse.StatusCode) >= 400)
                        {
                            throw new Exception("An error occurred getting the stats, please check you typed the players name correctly or try again later"); 
                        }
                    }
                }

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
                trowsTbody.ToList().ForEach(trow => playerStats.Add(trow.Id, new Dictionary<string, string>()));

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
                foreach (string year in playerStats.Keys)
                {
                    playerStats[year] = new Dictionary<string, string>(seasonStatsIds);
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

                        var year = playerStats.Where(year => year.Key == statYear).Select(selectedYear => selectedYear.Value).FirstOrDefault();

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

        private static async Task<double> GetSingleStat(string player, string statToSearch)
        {
            try
            {
                var stats = await LoadPlayerStats(player);

                //basketball-reference uses different terms for rebounds and turnovers, so if the request wants rebounds or turnovers then replace it with the correct term 
                statToSearch = Regex.Replace(statToSearch.ToLower(), "reb", "trb");

                statToSearch = Regex.Replace(statToSearch.ToLower(), "to", "tov");

                var statId = stats.StatNameToStatId.Where(stat => stat.Key.Contains(statToSearch)).Select(selectedStat => selectedStat.Value).FirstOrDefault();

                if (statId.Count() < 1)
                {
                    throw new Exception("Invalid stat"); 
                }

                //only get stats for current season (2022 in this case)
                var yearStats = stats.PlayerStats.Where(year => year.Key.Contains("2022")).Select(selectedYear => selectedYear.Value).FirstOrDefault();

                return yearStats.Where(stat => stat.Key == statId).Select(statValue => Convert.ToDouble(statValue.Value)).FirstOrDefault();
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message, ex); 
            }
        }

        private static HtmlNodeCollection GetChildNodes(HtmlNodeCollection htmlNodes, string divID)
        {
            return htmlNodes.Where(node => node?.Id == divID).Select(selectedNode => selectedNode?.ChildNodes).FirstOrDefault();
        }
    }
}
