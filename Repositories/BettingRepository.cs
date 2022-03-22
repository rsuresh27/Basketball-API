using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Basketball_API.Base_Classes;

namespace Basketball_API.Repositories
{
    public class BettingRepository : BaseFunctions, IBettingRepository
    {
        #region Endpoints

        public BettingRepository(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

        public async Task<string> GetGameOdds(string gameId)
        {
            return await GetOddsGame(gameId);
        }

        public async Task<string> GetNCAAGameOdds(string gameId)
        {
            return new string("Test");
        }

        #endregion

        #region Betting Functions

        private async Task<string> GetOddsGame(string gameId)
        {
            var url = $"https://www.espn.com/nba/game/_/gameId/{gameId}";

            HtmlDocument htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(await LoadWebPageAsString(url));

            var page = htmlDocument.GetElementbyId("global-viewport");

            var betting = page.Descendants("div").FirstOrDefault(node => node.Id == "gamepackage-pick-center");

            dynamic odds = new ExpandoObject();

            var teamOdds = new Dictionary<string, Dictionary<string, string>>();

            if (betting != null)
            {
                var oddsName = betting.Descendants("th").ToList().GetRange(4, 3).Select(node => node.InnerText).ToList();

                var teams = betting.Descendants("td").Where(node => node.GetAttributeValue("class", "") == "team").ToList();

                odds.Teams = new Dictionary<string, Dictionary<string, string>>();

                //create datarows with odds for each time 
                foreach (var team in teams)
                {
                    var teamName = team.Descendants("a").FirstOrDefault().InnerText;

                    odds.Teams.Add(teamName, new Dictionary<string, string>());

                    var oddsValue = betting.Descendants("td").Where(node => node.ParentNode == team.ParentNode).Select(node => node.InnerText).TakeLast(3).ToList();

                    var singleTeamOddsDictionary = new Dictionary<string, string>();

                    //since first row uses rowspan=2, we need remove this one on the second team pass through and use the O/U for the previous team
                    if (oddsValue.ElementAt(0) == ("--"))
                    {
                        oddsValue.RemoveAt(0);
                    }

                    foreach (var value in oddsValue)
                    {
                        singleTeamOddsDictionary.Add(oddsName[oddsValue.IndexOf(value)], value);

                        //resuse O/U since it is same for both teams and because rowspan=2 causes two rows with different column lengths
                        if (oddsValue.IndexOf(value) == 1 && teams.IndexOf(team) == 1)
                        {
                            var team1Odds = teamOdds.ElementAtOrDefault(0).Value;

                            var team1OU = team1Odds.FirstOrDefault(kvp => kvp.Key == "O/U");

                            singleTeamOddsDictionary.Add(team1OU.Key, team1OU.Value);
                        }
                    }

                    teamOdds.Add(teamName, singleTeamOddsDictionary);
                }
            }

            odds.Teams = teamOdds;

            return JsonSerializer.Serialize(odds); ;
        }



        #endregion


    }
}
