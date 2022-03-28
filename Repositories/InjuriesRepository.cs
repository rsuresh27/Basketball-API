using Basketball_API.Base_Classes;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading.Tasks;

namespace Basketball_API.Repositories
{
    public class InjuriesRepository : BaseFunctions, IInjuriesRepository
    {
        public InjuriesRepository(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

        #region Endpoints

        public async Task<string> GetAllInjuries()
        {
            return await GetInjuriesAll();  
        }

        #endregion

        #region Injuries Functions

        private async Task<string> GetInjuriesAll()
        {
            try
            {
                var url = "https://www.espn.com/nba/injuries";

                HtmlDocument htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                var page = GetChildNodes(htmlDocument.GetElementbyId("espnfitt").ChildNodes, "DataWrapper");

                var tableContainer = page.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "") == "Wrapper Card__Content"); 

                tableContainer.ChildNodes.ToList().ForEach(node => Console.WriteLine(node.InnerText));

                if (tableContainer != null)
                {
                    dynamic injuries = new ExpandoObject();

                    //use text info and culture info to format json key values to now be all uppercase
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                    var teams = tableContainer.Descendants("div").Where(node => node.GetAttributeValue("class", "") == "ResponsiveTable Table__league-injuries").ToList();

                    Dictionary<string, List<Dictionary<string, string>>> allTeamInjuries = new Dictionary<string, List<Dictionary<string, string>>>();

                    foreach (var team in teams)
                    {
                        var teamName = team.Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "") == "Table__Title").InnerText;

                        var teamInjuriestTable = team.Descendants("table").FirstOrDefault();

                        var tableHeader = teamInjuriestTable.Descendants("th");

                        //we can break the enumerable into chunks of 5 to represent the 5 columsn per row that represent a players injury status and details
                        var players = new List<List<Tuple<string, string>>>();
                        var teamPlayers = teamInjuriestTable.Descendants("td").Chunk(5);

                        //use ZIP to loop through two enumerables at once and match the th and td for each row in the table and get a key value pair 
                        foreach (var player in teamPlayers)
                        {                           
                            var headerAndPlayers = player.Zip(tableHeader, (w, h) => new Tuple<string, string>(h.InnerText, w.InnerText)).ToList();
                            players.Add(headerAndPlayers);
                        }

                        allTeamInjuries.Add(teamName, new List<Dictionary<string, string>>());

                        foreach (var kvp in players)
                        {
                            var player = kvp.AsEnumerable().ToDictionary(kvp => textInfo.ToTitleCase(kvp.Item1.ToLower()), kvp => kvp.Item2);

                            allTeamInjuries[teamName].Add(player);
                        }
                    }

                    return JsonSerializer.Serialize(allTeamInjuries);
                }

                else
                {
                    return "";
                }

            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex); 
            }
        }


        #endregion
    }
}
