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

namespace Basketball_API.Base_Classes
{
    public class NewsBase : BaseFunctions, INewsBase
    {
        public NewsBase(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

        public ExpandoObject ExtractNewsArticles(HtmlDocument htmlDocument)
        {
            try
            {
                var page = htmlDocument.DocumentNode;

                var newsArticles = page.Descendants("table").Where(node => node.XPath.Contains("td"));

                dynamic news = new ExpandoObject();

                if (newsArticles.Count() > 0)
                {                   
                    news.Articles = new List<List<Dictionary<string, string>>>();

                    foreach (var article in newsArticles)
                    {
                        var articleTitle = article.Descendants("h4").FirstOrDefault()?.InnerText;

                        var articleLink = $"https://www.espn.com{article.Descendants("a").FirstOrDefault().GetAttributeValue("href", "").Split('?').ElementAtOrDefault(0)}";

                        var articleJSON = new List<Dictionary<string, string>>();

                        articleJSON.Add(new Dictionary<string, string>
                        {
                            {"title", articleTitle},
                            {"link", articleLink},
                        });

                        news.Articles.Add(articleJSON);
                    }                   
                }

                return news;
            }

            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }
    }
}
