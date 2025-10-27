using Microsoft.EntityFrameworkCore;
using YouTube.Data;
using YouTube.Models;

namespace YouTube.Repositories;


public class PersistentAccountRepository : IAccountRepository
{
    private readonly YouTubeDbContext _context;
    private readonly ILogger<PersistentAccountRepository> _logger;

    public PersistentAccountRepository(
        YouTubeDbContext context,
        ILogger<PersistentAccountRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public void Save(Account account)
    {
        try
        {
            if (account.Id == Guid.Empty)
            {
                account.Id = Guid.NewGuid();
            }

            _context.Accounts.Add(account);
            
            _context.SaveChanges();

            _logger.LogInformation("Conta {Username} salva com sucesso. ID: {Id}", 
                account.Username, account.Id);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao salvar conta {Username}", account.Username);
            throw new InvalidOperationException("Erro ao salvar conta no banco de dados", ex);
        }
    }

    public IEnumerable<Account> GetAll()
    {
        return _context.Accounts
            .Include(a => a.Videos)
            .Include(a => a.Comments)
            .ToList();
    }

    public Account? GetById(Guid id)
    {
        return _context.Accounts
            .Include(a => a.Videos)
                .ThenInclude(v => v.Comments) 
            .Include(a => a.Comments)
                .ThenInclude(c => c.Video) 
            .FirstOrDefault(a => a.Id == id);
    }

    public Account? GetByUsername(string username)
    {
        return _context.Accounts
            .Include(a => a.Videos)
            .Include(a => a.Comments)
            .FirstOrDefault(a => a.Username.ToLower() == username.ToLower());
    }

    public void Update(Account account)
    {
        try
        {
            var existingAccount = _context.Accounts.Find(account.Id);
            if (existingAccount == null)
            {
                throw new KeyNotFoundException($"Conta com ID {account.Id} não encontrada");
            }

            _context.Entry(existingAccount).CurrentValues.SetValues(account);
            
            _context.SaveChanges();

            _logger.LogInformation("Conta {Id} atualizada com sucesso", account.Id);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Erro de concorrência ao atualizar conta {Id}", account.Id);
            throw new InvalidOperationException("Erro de concorrência ao atualizar conta", ex);
        }
    }

    public void Delete(Guid id)
    {
        try
        {
            var account = _context.Accounts.Find(id);
            if (account != null)
            {
                _context.Accounts.Remove(account);
                _context.SaveChanges();

                _logger.LogInformation("Conta {Id} deletada com sucesso", id);
            }
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Erro ao deletar conta {Id}", id);
            throw new InvalidOperationException(
                "Erro ao deletar conta. Pode haver comentários associados.", ex);
        }
    }
}