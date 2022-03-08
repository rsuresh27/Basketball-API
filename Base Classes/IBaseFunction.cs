using HtmlAgilityPack;
using System.Threading.Tasks;

namespace Basketball_API.Base_Classes
{
    public interface IBaseFunctions
    {
        public Task<string> LoadWebPageAsString(string url);

        public HtmlNodeCollection GetChildNodes(HtmlNodeCollection htmlNodes, string divID);
    }
}