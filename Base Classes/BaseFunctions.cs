using HtmlAgilityPack;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json; 

namespace Basketball_API.Base_Classes
{
    public abstract class BaseFunctions : IBaseFunctions
    {
        private readonly IHttpClientFactory _httpClientFactory;

        protected BaseFunctions(IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
        }

        #region HTTP

        public async Task<string> LoadWebPageAsString(string url)
        {
            try
            {
                using (HttpClient client = _httpClientFactory.CreateClient())
                {
                    using (HttpResponseMessage httpResponse = await client.GetAsync(url))
                    {
                        httpResponse.Headers.CacheControl = new CacheControlHeaderValue
                        {
                            NoCache = true,
                            NoStore = true,
                            MustRevalidate = true
                        };

                        httpResponse.Headers.Pragma.ParseAdd("no-cache");
                        httpResponse.Content?.Headers.TryAddWithoutValidation("Expires", "0");

                        if (httpResponse.IsSuccessStatusCode)
                        {
                            using (HttpContent httpContent = httpResponse.Content)
                            {
                                var webPage = await httpContent.ReadAsStringAsync();
                                return webPage;
                            }
                        }
                        else
                        {
                            throw new Exception("An error occurred getting the data, please check you typed the input parameters correctly or try again later");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<string> PuppeteerServerRequest(string url)
        {
            try
            {
                using (HttpClient client = _httpClientFactory.CreateClient())
                {
                    using (HttpResponseMessage httpResponse = await client.GetAsync(url))
                    {
                        httpResponse.Headers.CacheControl = new CacheControlHeaderValue
                        {
                            NoCache = true,
                            NoStore = true,
                            MustRevalidate = true
                        };

                        httpResponse.Headers.Pragma.ParseAdd("no-cache");
                        httpResponse.Content?.Headers.TryAddWithoutValidation("Expires", "0");

                        if (httpResponse.IsSuccessStatusCode)
                        {
                            using (HttpContent httpContent = httpResponse.Content)
                            {
                                var json = await httpContent.ReadAsStringAsync();
                                return JsonSerializer.Deserialize<string>(json); 
                            }
                        }
                        else
                        {
                            throw new Exception("An error occurred getting the data, please check you typed the input parameters correctly or try again later");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        #endregion

        #region HTML

        public HtmlNodeCollection GetChildNodes(HtmlNodeCollection htmlNodes, string divID)
        {
            return htmlNodes.Where(node => node?.Id == divID).Select(selectedNode => selectedNode?.ChildNodes).FirstOrDefault();
        }

        #endregion

    }
}