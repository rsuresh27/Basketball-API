using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Dynamic; 

namespace Basketball_API.Base_Classes
{
    public interface INewsBase
    {
        public ExpandoObject ExtractNewsArticles(HtmlDocument htmlDocument); 
    }
}
