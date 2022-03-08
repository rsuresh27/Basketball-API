using System.Threading.Tasks;

namespace Basketball_API.Base_Classes
{
    public interface IStatLeadersBase
    {
        public Task<string> GetWeekNBA();
    }
}
