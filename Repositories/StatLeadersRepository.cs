using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Utiliy;  

namespace Basketball_API.Repositories
{
    public class StatLeadersRepository : IStatLeadersRepository
    {
        #region Endpoints

        public async Task<Dictionary<string, string>> GetTop5PlayersPPG(string year)
        {
            return await GetTop5PPGPlayers(year);
        }

        #endregion

        #region Stat Functions 

        private async Task<Dictionary<string, string>> GetTop5PPGPlayers(string year)
        {
            try
            {
                year = year.Trim();

                var url = $"https://www.basketball-reference.com/leagues/NBA_{year}_leaders.html";

                HtmlDocument htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(await HttpExtensions.LoadWebPageAsString(url));

                var allStatsLeaders = HtmlExtensions.GetChildNodes(htmlDocument.GetElementbyId("content").ChildNodes, "all_leaders");

                var statsLeadersDiv = HtmlExtensions.GetChildNodes(allStatsLeaders, "div_leaders");

                var ppgLeadersDiv = HtmlExtensions.GetChildNodes(statsLeadersDiv, "leaders_pts_per_g");

                var top5PPGtr = ppgLeadersDiv.Descendants("tr").Take(5);

                var top5PPGstats = top5PPGtr.Select(node => node.ChildNodes);

                var stats = top5PPGstats.Select(playerStat => playerStat.Where(node => node.HasClass("who") || node.HasClass("value")).ToList());

                var playerNames = stats.ToList().Select(node => node.Where(node1 => node1.HasClass("who")).FirstOrDefault()).ToList();

                var playerPPG = stats.ToList().Select(node => node.Where(node1 => node1.HasClass("value")).FirstOrDefault()).ToList();

                Dictionary<string, string> top5PlayersPPG = stats.AsEnumerable().ToDictionary(players => players.FirstOrDefault(stat => stat.HasClass("who")).InnerText.Split("&").ElementAtOrDefault(0).Trim(), players => players.FirstOrDefault(stat => stat.HasClass("value")).InnerText.Trim());

                return top5PlayersPPG;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        #endregion

    }
}
