using System.Threading.Tasks; 

namespace Basketball_API.Repositories
{
    public interface INewsRepository
    {
        public Task<string> GetNews(); 

        public Task<string> GetNCAANews();
    }
}
