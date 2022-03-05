using System;
using System.Net.Http;
using System.Net.Http.Headers; 
using System.Threading.Tasks;

namespace Utiliy
{
    public static class HttpExtensions
    {
        public static Task<string> LoadWebPageAsString(this string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {                    
                    using (HttpResponseMessage httpResponse = client.GetAsync(url).Result)
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
                                var webPage = httpContent.ReadAsStringAsync();
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
    }
}
