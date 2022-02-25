using HtmlAgilityPack;
using System.Linq;

namespace Utiliy
{
    public static class HtmlExtensions
    {
        public static HtmlNodeCollection GetChildNodes(this HtmlNodeCollection htmlNodes, string divID)
        {
            return htmlNodes.Where(node => node?.Id == divID).Select(selectedNode => selectedNode?.ChildNodes).FirstOrDefault();
        }
    }
}
