using YouTube.Models;

namespace YouTube.Repositories;


public class InMemoryVideoRepository : IVideoRepository
{
    private readonly Dictionary<Guid, Video> _memory = [];

    public void Save(Video video)
    {
        if (video.Id == Guid.Empty)
        {
            video.Id = Guid.NewGuid();
        }

        _memory[video.Id] = video;
    }

    public IEnumerable<Video> GetAll()
    {
        return _memory.Values;
    }

    public Video? GetById(Guid id)
    {
        return _memory.TryGetValue(id, out var video) ? video : null;
    }

    public IEnumerable<Video> GetByAccountId(Guid accountId)
    {
        return _memory.Values
            .Where(v => v.Account.Id == accountId)
            .OrderByDescending(v => v.PublishedAt);
    }

    public IEnumerable<Video> GetMostViewed(int limit = 10)
    {
        return _memory.Values
            .OrderByDescending(v => v.Views)
            .Take(limit);
    }

    public void Update(Video video)
    {
        if (_memory.ContainsKey(video.Id))
        {
            _memory[video.Id] = video;
        }
        else
        {
            throw new KeyNotFoundException($"Vídeo com ID {video.Id} não encontrado");
        }
    }

    public void Delete(Guid id)
    {
        _memory.Remove(id);
    }
}