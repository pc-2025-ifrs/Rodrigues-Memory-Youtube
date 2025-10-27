using YouTube.Models;

namespace YouTube.Repositories;

public class InMemoryCommentRepository : ICommentRepository
{
    private readonly Dictionary<Guid, Comment> _memory = [];

    public void Save(Comment comment)
    {
        if (comment.Id == Guid.Empty)
        {
            comment.Id = Guid.NewGuid();
        }

        _memory[comment.Id] = comment;
    }

    public IEnumerable<Comment> GetAll()
    {
        return _memory.Values;
    }

    public Comment? GetById(Guid id)
    {
        return _memory.TryGetValue(id, out var comment) ? comment : null;
    }

    public IEnumerable<Comment> GetByVideoId(Guid videoId)
    {
        return _memory.Values
            .Where(c => c.Video.Id == videoId)
            .OrderByDescending(c => c.CreatedAt);
    }

    public IEnumerable<Comment> GetByAuthorId(Guid authorId)
    {
        return _memory.Values
            .Where(c => c.Author.Id == authorId)
            .OrderByDescending(c => c.CreatedAt);
    }

    public void Update(Comment comment)
    {
        if (_memory.ContainsKey(comment.Id))
        {
            _memory[comment.Id] = comment;
        }
        else
        {
            throw new KeyNotFoundException($"Comentário com ID {comment.Id} não encontrado");
        }
    }

    public void Delete(Guid id)
    {
        _memory.Remove(id);
    }
}