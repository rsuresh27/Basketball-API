using System.Net.Http;
using System.Threading.Tasks;
using System;

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
                            throw new Exception("An error occurred getting the stats, please check you typed the input parameters correctly or try again later");
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
