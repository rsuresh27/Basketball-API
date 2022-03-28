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
    public class NewsRepository : NewsBase, INewsRepository
    {
        public NewsRepository(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

        #region Endpoints

        public async Task<string> GetNews()
        {
            return await GetNewsNBA();
        }

        public async Task<string> GetNCAANews()
        {
            return await GetNewsNCAA();
        }

        #endregion

        #region News Functions

        private async Task<string> GetNewsNBA()
        {
            try
            {
                var url = "https://www.espn.com/core/nba/?device=featurephone";

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                return JsonSerializer.Serialize(ExtractNewsArticles(htmlDocument));
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        private async Task<string> GetNewsNCAA()
        {
            try
            {
                var url = "https://www.espn.com/core/mens-college-basketball/?device=featurephone";

                HtmlDocument htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(await LoadWebPageAsString(url));

                return JsonSerializer.Serialize(ExtractNewsArticles(htmlDocument));
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        #endregion

    }
}
