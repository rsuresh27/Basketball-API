using HtmlAgilityPack;
using System.Dynamic;
using System.Threading.Tasks;

namespace Basketball_API.Base_Classes
{
    public interface IBettingBase
    {
        public Task<ExpandoObject> ConvertOddsDivToJson(HtmlNode betting); 
    }
}
