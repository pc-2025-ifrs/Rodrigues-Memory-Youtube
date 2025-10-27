using YouTube.Models;

namespace YouTube.Repositories;


public interface IVideoRepository
{
    void Save(Video video);

    IEnumerable<Video> GetAll();

    Video? GetById(Guid id);

    IEnumerable<Video> GetByAccountId(Guid accountId);

    IEnumerable<Video> GetMostViewed(int limit = 10);

    void Update(Video video);

    void Delete(Guid id);
}