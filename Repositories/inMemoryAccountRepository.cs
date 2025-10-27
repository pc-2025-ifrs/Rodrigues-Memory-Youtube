using YouTube.Models;

namespace YouTube.Repositories;


public class InMemoryAccountRepository : IAccountRepository
{
    private readonly Dictionary<Guid, Account> _memory = [];

    public void Save(Account account)
    {
        if (account.Id == Guid.Empty)
        {
            account.Id = Guid.NewGuid();
        }

        _memory[account.Id] = account;
    }

    public IEnumerable<Account> GetAll()
    {
        return _memory.Values;
    }

    public Account? GetById(Guid id)
    {
        return _memory.TryGetValue(id, out var account) ? account : null;
    }

    public Account? GetByUsername(string username)
    {
        return _memory.Values
            .FirstOrDefault(a => a.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    public void Update(Account account)
    {
        if (_memory.ContainsKey(account.Id))
        {
            _memory[account.Id] = account;
        }
        else
        {
            throw new KeyNotFoundException($"Conta com ID {account.Id} não encontrada");
        }
    }

    public void Delete(Guid id)
    {
        _memory.Remove(id);
    }
}