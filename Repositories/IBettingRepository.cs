using System.Threading.Tasks;

namespace Basketball_API.Repositories
{
    public interface IBettingRepository
    {
        public Task<string> GetGameOdds(string gameId);

        public Task<string> GetNCAAGameOdds(string gameId);
    }
}
