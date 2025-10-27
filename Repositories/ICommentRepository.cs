using YouTube.Models;

namespace YouTube.Repositories;


public interface ICommentRepository
{
    void Save(Comment comment);

    IEnumerable<Comment> GetAll();

    Comment? GetById(Guid id);

    IEnumerable<Comment> GetByVideoId(Guid videoId);

    IEnumerable<Comment> GetByAuthorId(Guid authorId);

    void Update(Comment comment);

    void Delete(Guid id);
}