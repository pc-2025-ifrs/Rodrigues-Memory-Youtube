using Microsoft.EntityFrameworkCore;
using YouTube.Data;
using YouTube.Models;

namespace YouTube.Repositories;


public class PersistentVideoRepository : IVideoRepository
{
    private readonly YouTubeDbContext _context;
    private readonly ILogger<PersistentVideoRepository> _logger;

    public PersistentVideoRepository(
        YouTubeDbContext context,
        ILogger<PersistentVideoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public void Save(Video video)
    {
        try
        {
            if (video.Id == Guid.Empty)
            {
                video.Id = Guid.NewGuid();
            }

            if (video.Account != null && video.Account.Id != Guid.Empty)
            {
                var existingAccount = _context.Accounts.Find(video.Account.Id);
                if (existingAccount != null)
                {
                    _context.Entry(video.Account).State = EntityState.Unchanged;
                }
            }

            _context.Videos.Add(video);
            _context.SaveChanges();

            _logger.LogInformation("Vídeo {Title} salvo com sucesso. ID: {Id}", 
                video.Title, video.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao salvar vídeo {Title}", video.Title);
            throw new InvalidOperationException("Erro ao salvar vídeo no banco de dados", ex);
        }
    }

    public IEnumerable<Video> GetAll()
    {
        return _context.Videos
            .Include(v => v.Account)
            .Include(v => v.Comments)
                .ThenInclude(c => c.Author)
            .OrderByDescending(v => v.PublishedAt)
            .ToList();
    }

    public Video? GetById(Guid id)
    {
        return _context.Videos
            .Include(v => v.Account)
            .Include(v => v.Comments)
                .ThenInclude(c => c.Author)
            .FirstOrDefault(v => v.Id == id);
    }

    public IEnumerable<Video> GetByAccountId(Guid accountId)
    {
        return _context.Videos
            .Include(v => v.Account)
            .Include(v => v.Comments)
            .Where(v => v.Account.Id == accountId)
            .OrderByDescending(v => v.PublishedAt)
            .ToList();
    }

    public IEnumerable<Video> GetMostViewed(int limit = 10)
    {
        return _context.Videos
            .Include(v => v.Account)
            .Include(v => v.Comments)
            .OrderByDescending(v => v.Views)
            .ThenByDescending(v => v.PublishedAt)
            .Take(limit)
            .ToList();
    }

    public void Update(Video video)
    {
        try
        {
            var existingVideo = _context.Videos.Find(video.Id);
            if (existingVideo == null)
            {
                throw new KeyNotFoundException($"Vídeo com ID {video.Id} não encontrado");
            }

            existingVideo.Title = video.Title;
            existingVideo.Description = video.Description;
            existingVideo.DurationInSeconds = video.DurationInSeconds;
            existingVideo.Views = video.Views;
            existingVideo.PublishedAt = video.PublishedAt;

            _context.SaveChanges();

            _logger.LogInformation("Vídeo {Id} atualizado com sucesso", video.Id);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Erro de concorrência ao atualizar vídeo {Id}", video.Id);
            throw new InvalidOperationException("Erro de concorrência ao atualizar vídeo", ex);
        }
    }

    public void Delete(Guid id)
    {
        try
        {
            var video = _context.Videos.Find(id);
            if (video != null)
            {
                _context.Videos.Remove(video);
                _context.SaveChanges();

                _logger.LogInformation("Vídeo {Id} deletado com sucesso", id);
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao deletar vídeo {Id}", id);
            throw new InvalidOperationException("Erro ao deletar vídeo", ex);
        }
    }
}