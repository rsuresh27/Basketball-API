using HtmlAgilityPack;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Basketball_API.Base_Classes
{
    public abstract class StatLeadersBase : BaseFunctions, IStatLeadersBase
    {
        protected StatLeadersBase(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

        public async Task<string> GetWeekNBA()
        {
            var url = "https://www.nba.com/key-dates";

            HtmlDocument htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(await LoadWebPageAsString(url));

            var dates = htmlDocument.GetElementbyId("__next").Descendants("div").FirstOrDefault(node => node.GetAttributeValue("class", "") == "Block_blockContainer__2tJ58");

            var startDate = dates.Descendants("li").FirstOrDefault(node => node.InnerText.Contains("NBA Regular Season") && node.InnerText.Contains("Start")).InnerText.Split(':').ElementAtOrDefault(0).Trim();

            if (DateTime.UtcNow.Month < 9)
            {
                startDate = (startDate + " " + DateTime.UtcNow.AddYears(-1).Year);
            }
            else
            {
                startDate = (startDate + " " + DateTime.UtcNow.Year);
            }

            DateTime startDateConverted = DateTime.Parse(startDate).Date;

            var daysTillNextMondayStartDate = (int)DayOfWeek.Monday - (int)startDateConverted.DayOfWeek + 7;

            var nextMondayStartDate = startDateConverted.AddDays(daysTillNextMondayStartDate);

            return Convert.ToString(Math.Ceiling((DateTime.UtcNow.Date - nextMondayStartDate).TotalDays / 7) + 1);
        }
    }
}
