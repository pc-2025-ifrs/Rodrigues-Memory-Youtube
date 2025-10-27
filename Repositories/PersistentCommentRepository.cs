using Microsoft.EntityFrameworkCore;
using YouTube.Data;
using YouTube.Models;

namespace YouTube.Repositories;


public class PersistentCommentRepository : ICommentRepository
{
    private readonly YouTubeDbContext _context;
    private readonly ILogger<PersistentCommentRepository> _logger;

    public PersistentCommentRepository(
        YouTubeDbContext context,
        ILogger<PersistentCommentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public void Save(Comment comment)
    {
        try
        {
            if (comment.Id == Guid.Empty)
            {
                comment.Id = Guid.NewGuid();
            }

            if (comment.Video != null && comment.Video.Id != Guid.Empty)
            {
                var existingVideo = _context.Videos.Find(comment.Video.Id);
                if (existingVideo != null)
                {
                    _context.Entry(comment.Video).State = EntityState.Unchanged;
                }
            }

            if (comment.Author != null && comment.Author.Id != Guid.Empty)
            {
                var existingAuthor = _context.Accounts.Find(comment.Author.Id);
                if (existingAuthor != null)
                {
                    _context.Entry(comment.Author).State = EntityState.Unchanged;
                }
            }

            _context.Comments.Add(comment);
            _context.SaveChanges();

            _logger.LogInformation("Comentário criado com sucesso. ID: {Id}", comment.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao salvar comentário");
            throw new InvalidOperationException("Erro ao salvar comentário no banco de dados", ex);
        }
    }

    public IEnumerable<Comment> GetAll()
    {
        return _context.Comments
            .Include(c => c.Video)
                .ThenInclude(v => v.Account)
            .Include(c => c.Author)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();
    }

    public Comment? GetById(Guid id)
    {
        return _context.Comments
            .Include(c => c.Video)
                .ThenInclude(v => v.Account)
            .Include(c => c.Author)
            .FirstOrDefault(c => c.Id == id);
    }

    public IEnumerable<Comment> GetByVideoId(Guid videoId)
    {
        return _context.Comments
            .Include(c => c.Video)
            .Include(c => c.Author)
            .Where(c => c.Video.Id == videoId)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();
    }

    public IEnumerable<Comment> GetByAuthorId(Guid authorId)
    {
        return _context.Comments
            .Include(c => c.Video)
                .ThenInclude(v => v.Account)
            .Include(c => c.Author)
            .Where(c => c.Author.Id == authorId)
            .OrderByDescending(c => c.CreatedAt)
            .ToList();
    }

    public void Update(Comment comment)
    {
        try
        {
            var existingComment = _context.Comments.Find(comment.Id);
            if (existingComment == null)
            {
                throw new KeyNotFoundException($"Comentário com ID {comment.Id} não encontrado");
            }

            existingComment.Text = comment.Text;
            existingComment.Likes = comment.Likes;
            existingComment.Dislikes = comment.Dislikes;
            existingComment.IsEdited = comment.IsEdited;

            _context.SaveChanges();

            _logger.LogInformation("Comentário {Id} atualizado com sucesso", comment.Id);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Erro de concorrência ao atualizar comentário {Id}", comment.Id);
            throw new InvalidOperationException("Erro de concorrência ao atualizar comentário", ex);
        }
    }

    public void Delete(Guid id)
    {
        try
        {
            var comment = _context.Comments.Find(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                _context.SaveChanges();

                _logger.LogInformation("Comentário {Id} deletado com sucesso", id);
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao deletar comentário {Id}", id);
            throw new InvalidOperationException("Erro ao deletar comentário", ex);
        }
    }
}