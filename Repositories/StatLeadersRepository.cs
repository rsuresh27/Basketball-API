using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Basketball_API.Repositories
{
    public class StatLeadersRepository : BaseFunctions, IStatLeadersRepository
    {

        public StatLeadersRepository(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

        #region Endpoints

        public async Task<Dictionary<string, string>> GetTop5PlayersPPG(string year)
        {
            return await GetTop5PPGPlayers(year);
        }

        public async Task<Dictionary<string, string>> GetTopPlayersRPG(string year)
        {
            return await getTop5RBGPlayers(year); 
        }

        public async Task<Dictionary<string, string>> GetTopPlayersAPG(string year)
        {
            return await getTop5APGPlayers(year); 
        }

        public async Task<Dictionary<string, string>> GetTopPlayersSPG(string year)
        {
            return await getTop5SPGPlayers(year);
        }

        public async Task<Dictionary<string, string>> GetTopPlayersBPG(string year)
        {
            return await getTop5BPGPlayers(year); 
        }

        #endregion

        #region Stat Functions 

        private async Task<Dictionary<string, string>> GetTop5PPGPlayers(string year)
        {
            try
            {
                year = (year ?? DateTime.UtcNow.Year.ToString()).Trim(); 

                var url = $"https://www.basketball-reference.com/leagues/NBA_{year}_leaders.html";

                HtmlDocument htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                var allStatsLeaders = GetChildNodes(htmlDocument.GetElementbyId("content").ChildNodes, "all_leaders");

                var statsLeadersDiv = GetChildNodes(allStatsLeaders, "div_leaders");

                var ppgLeadersDiv = GetChildNodes(statsLeadersDiv, "leaders_pts_per_g");

                var top5PPGtr = ppgLeadersDiv.Descendants("tr").Take(5);

                var top5PPGstats = top5PPGtr.Select(node => node.ChildNodes);

                var stats = top5PPGstats.Select(playerStat => playerStat.Where(node => node.HasClass("who") || node.HasClass("value")).ToList());

                Dictionary<string, string> top5PlayersPPG = stats.AsEnumerable().ToDictionary(players => players.FirstOrDefault(stat => stat.HasClass("who")).InnerText.Split("&").ElementAtOrDefault(0).Trim(), players => players.FirstOrDefault(stat => stat.HasClass("value")).InnerText.Trim());

                //only get the week if it is the current nba season
                if((DateTime.UtcNow.Month > 9 ? DateTime.Now.AddYears(1).Year.ToString() : DateTime.Now.Year.ToString()) == year)
                {
                    top5PlayersPPG.Add("Week", await GetWeekNBA());
                }         

                return top5PlayersPPG;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private async Task<Dictionary<string,string>> getTop5RBGPlayers(string year)
        {
            try
            {
                year = (year ?? DateTime.UtcNow.Year.ToString()).Trim();

                var url = $"https://www.basketball-reference.com/leagues/NBA_{year}_leaders.html";

                HtmlDocument htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                var allStatsLeaders = GetChildNodes(htmlDocument.GetElementbyId("content").ChildNodes, "all_leaders");

                var statsLeadersDiv = GetChildNodes(allStatsLeaders, "div_leaders");

                var rpgLeadersDiv = GetChildNodes(statsLeadersDiv, "leaders_trb_per_g");

                var top5RPGtr = rpgLeadersDiv.Descendants("tr").Take(5);

                var top5RPGstats = top5RPGtr.Select(node => node.ChildNodes);

                var stats = top5RPGstats.Select(playerStat => playerStat.Where(node => node.HasClass("who") || node.HasClass("value")).ToList());

                Dictionary<string, string> top5PlayersRPG = stats.AsEnumerable().ToDictionary(players => players.FirstOrDefault(stat => stat.HasClass("who")).InnerText.Split("&").ElementAtOrDefault(0).Trim(), players => players.FirstOrDefault(stat => stat.HasClass("value")).InnerText.Trim());

                //only get the week if it is the current nba season
                if ((DateTime.UtcNow.Month > 9 ? DateTime.Now.AddYears(1).Year.ToString() : DateTime.Now.Year.ToString()) == year)
                {
                    top5PlayersRPG.Add("Week", await GetWeekNBA());
                }


                return top5PlayersRPG;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private async Task<Dictionary<string, string>> getTop5APGPlayers(string year)
        {
            try
            {
                year = (year ?? DateTime.UtcNow.Year.ToString()).Trim();

                var url = $"https://www.basketball-reference.com/leagues/NBA_{year}_leaders.html";

                HtmlDocument htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                var allStatsLeaders = GetChildNodes(htmlDocument.GetElementbyId("content").ChildNodes, "all_leaders");

                var statsLeadersDiv = GetChildNodes(allStatsLeaders, "div_leaders");

                var apgLeadersDiv = GetChildNodes(statsLeadersDiv, "leaders_ast_per_g");

                var top5APGtr = apgLeadersDiv.Descendants("tr").Take(5);

                var top5APGstats = top5APGtr.Select(node => node.ChildNodes);

                var stats = top5APGstats.Select(playerStat => playerStat.Where(node => node.HasClass("who") || node.HasClass("value")).ToList());
                          
                Dictionary<string, string> top5PlayersAPG = stats.AsEnumerable().ToDictionary(players => players.FirstOrDefault(stat => stat.HasClass("who")).InnerText.Split("&").ElementAtOrDefault(0).Trim(), players => players.FirstOrDefault(stat => stat.HasClass("value")).InnerText.Trim());

                //only get the week if it is the current nba season
                if ((DateTime.UtcNow.Month > 9 ? DateTime.Now.AddYears(1).Year.ToString() : DateTime.Now.Year.ToString()) == year)
                {
                    top5PlayersAPG.Add("Week", await GetWeekNBA());
                }


                return top5PlayersAPG;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private async Task<Dictionary<string, string>> getTop5SPGPlayers(string year)
        {
            try
            {
                year = (year ?? DateTime.UtcNow.Year.ToString()).Trim();

                var url = $"https://www.basketball-reference.com/leagues/NBA_{year}_leaders.html";

                HtmlDocument htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                var allStatsLeaders = GetChildNodes(htmlDocument.GetElementbyId("content").ChildNodes, "all_leaders");

                var statsLeadersDiv = GetChildNodes(allStatsLeaders, "div_leaders");

                var spgLeadersDiv = GetChildNodes(statsLeadersDiv, "leaders_stl_per_g");

                var top5SPGtr = spgLeadersDiv.Descendants("tr").Take(5);

                var top5SPGstats = top5SPGtr.Select(node => node.ChildNodes);

                var stats = top5SPGstats.Select(playerStat => playerStat.Where(node => node.HasClass("who") || node.HasClass("value")).ToList());

                Dictionary<string, string> top5PlayersSPG = stats.AsEnumerable().ToDictionary(players => players.FirstOrDefault(stat => stat.HasClass("who")).InnerText.Split("&").ElementAtOrDefault(0).Trim(), players => players.FirstOrDefault(stat => stat.HasClass("value")).InnerText.Trim());

                //only get the week if it is the current nba season
                if ((DateTime.UtcNow.Month > 9 ? DateTime.Now.AddYears(1).Year.ToString() : DateTime.Now.Year.ToString()) == year)
                {
                    top5PlayersSPG.Add("Week", await GetWeekNBA());
                }


                return top5PlayersSPG;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private async Task<Dictionary<string, string>> getTop5BPGPlayers(string year)
        {
            try
            {
                year = (year ?? DateTime.UtcNow.Year.ToString()).Trim();

                var url = $"https://www.basketball-reference.com/leagues/NBA_{year}_leaders.html";

                HtmlDocument htmlDocument = new HtmlDocument();

                htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                var allStatsLeaders = GetChildNodes(htmlDocument.GetElementbyId("content").ChildNodes, "all_leaders");

                var statsLeadersDiv = GetChildNodes(allStatsLeaders, "div_leaders");

                var spgLeadersDiv = GetChildNodes(statsLeadersDiv, "leaders_blk_per_g");

                var top5BPGtr = spgLeadersDiv.Descendants("tr").Take(5);

                var top5BPGstats = top5BPGtr.Select(node => node.ChildNodes);

                var stats = top5BPGstats.Select(playerStat => playerStat.Where(node => node.HasClass("who") || node.HasClass("value")).ToList());

                Dictionary<string, string> top5PlayersBPG = stats.AsEnumerable().ToDictionary(players => players.FirstOrDefault(stat => stat.HasClass("who")).InnerText.Split("&").ElementAtOrDefault(0).Trim(), players => players.FirstOrDefault(stat => stat.HasClass("value")).InnerText.Trim());

                //only get the week if it is the current nba season
                if ((DateTime.UtcNow.Month > 9 ? DateTime.Now.AddYears(1).Year.ToString() : DateTime.Now.Year.ToString()) == year)
                {
                    top5PlayersBPG.Add("Week", await GetWeekNBA());
                }


                return top5PlayersBPG;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        #endregion

    }
}
