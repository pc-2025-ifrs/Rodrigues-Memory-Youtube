using Microsoft.AspNetCore.Mvc;
using YouTube.Models;
using YouTube.Repositories;

namespace WebYouTube.Controllers;

/// <summary>
/// Controller para gerenciar contas do YouTube
/// Rota base: /api/v1/accounts
/// </summary>
[ApiController]
[Route("api/v1/accounts")]
public class AccountsController : ControllerBase
{
    private readonly ILogger<AccountsController> _logger;
    private readonly IAccountRepository _repo;

    public AccountsController(ILogger<AccountsController> logger, IAccountRepository repo)
    {
        _logger = logger;
        _repo = repo;
    }

    /// <summary>
    /// POST /api/v1/accounts - Criar nova conta
    /// </summary>
    [HttpPost(Name = "CreateAccount")]
    public IActionResult CreateAccount([FromBody] NewAccountDTO newAccount)
    {
        // Verificar se username já existe
        var existingAccount = _repo.GetByUsername(newAccount.Username);
        if (existingAccount != null)
        {
            return BadRequest($"Username '{newAccount.Username}' já está em uso");
        }

        // Converter DTO para Model
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Username = newAccount.Username,
            Email = newAccount.Email,
            CreatedAt = DateTime.UtcNow
        };

        _repo.Save(account);

        _logger.LogInformation("Nova conta criada: {Username}", account.Username);

        return CreatedAtRoute("GetAccountById", new { id = account.Id }, account);
    }

    /// <summary>
    /// GET /api/v1/accounts - Listar todas as contas
    /// </summary>
    [HttpGet(Name = "GetAllAccounts")]
    public ActionResult<IEnumerable<Account>> GetAll()
    {
        var accounts = _repo.GetAll();
        return Ok(accounts);
    }

    /// <summary>
    /// GET /api/v1/accounts/{id} - Buscar conta por ID
    /// </summary>
    [HttpGet("{id}", Name = "GetAccountById")]
    public ActionResult<Account> GetById(Guid id)
    {
        var account = _repo.GetById(id);
        
        if (account == null)
        {
            return NotFound($"Conta com ID {id} não encontrada");
        }

        return Ok(account);
    }

    /// <summary>
    /// GET /api/v1/accounts/username/{username} - Buscar conta por username
    /// </summary>
    [HttpGet("username/{username}", Name = "GetAccountByUsername")]
    public ActionResult<Account> GetByUsername(string username)
    {
        var account = _repo.GetByUsername(username);
        
        if (account == null)
        {
            return NotFound($"Conta com username '{username}' não encontrada");
        }

        return Ok(account);
    }

    /// <summary>
    /// PUT /api/v1/accounts/{id} - Atualizar conta
    /// </summary>
    [HttpPut("{id}", Name = "UpdateAccount")]
    public IActionResult UpdateAccount(Guid id, [FromBody] UpdateAccountDTO updateAccount)
    {
        var account = _repo.GetById(id);
        
        if (account == null)
        {
            return NotFound($"Conta com ID {id} não encontrada");
        }

        // Atualizar campos
        account.Username = updateAccount.Username ?? account.Username;
        account.Email = updateAccount.Email ?? account.Email;

        _repo.Update(account);

        return Ok(account);
    }

    /// <summary>
    /// DELETE /api/v1/accounts/{id} - Deletar conta
    /// </summary>
    [HttpDelete("{id}", Name = "DeleteAccount")]
    public IActionResult DeleteAccount(Guid id)
    {
        var account = _repo.GetById(id);
        
        if (account == null)
        {
            return NotFound($"Conta com ID {id} não encontrada");
        }

        _repo.Delete(id);

        return NoContent();
    }
}

// DTOs (Data Transfer Objects) para as requisições

/// <summary>
/// DTO para criação de nova conta
/// </summary>
public record class NewAccountDTO(string Username, string Email);

/// <summary>
/// DTO para atualização de conta
/// </summary>
public record class UpdateAccountDTO(string? Username, string? Email);