using YouTube.Models;

namespace YouTube.Repositories;

public interface IAccountRepository
{
    void Save(Account account);

    IEnumerable<Account> GetAll();

    Account? GetById(Guid id);

    Account? GetByUsername(string username);

    void Update(Account account);

    void Delete(Guid id);
}