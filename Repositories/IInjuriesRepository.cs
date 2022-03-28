using System.Threading.Tasks; 

namespace Basketball_API.Repositories
{
    public interface IInjuriesRepository
    {
        public Task<string> GetInjuries(); 
    }
}
