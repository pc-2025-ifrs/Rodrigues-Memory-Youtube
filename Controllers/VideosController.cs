using Microsoft.AspNetCore.Mvc;
using YouTube.Models;
using YouTube.Repositories;

namespace WebYouTube.Controllers;


[ApiController]
[Route("api/v1/videos")]
public class VideosController : ControllerBase
{
    private readonly ILogger<VideosController> _logger;
    private readonly IVideoRepository _videoRepo;
    private readonly IAccountRepository _accountRepo;

    public VideosController(
        ILogger<VideosController> logger,
        IVideoRepository videoRepo,
        IAccountRepository accountRepo)
    {
        _logger = logger;
        _videoRepo = videoRepo;
        _accountRepo = accountRepo;
    }


    [HttpPost(Name = "PublishVideo")]
    public IActionResult PublishVideo([FromBody] NewVideoDTO newVideo)
    {
        var account = _accountRepo.GetById(newVideo.AccountId);
        if (account == null)
        {
            return BadRequest($"Conta com ID {newVideo.AccountId} não encontrada");
        }

        var video = new Video
        {
            Id = Guid.NewGuid(),
            Title = newVideo.Title,
            Description = newVideo.Description,
            DurationInSeconds = newVideo.DurationInSeconds,
            Account = account,
            PublishedAt = DateTime.UtcNow
        };

        _videoRepo.Save(video);
        
        account.Videos.Add(video);
        _accountRepo.Update(account);

        _logger.LogInformation("Novo vídeo publicado: {Title} por {Username}", 
            video.Title, account.Username);

        return CreatedAtRoute("GetVideoById", new { id = video.Id }, video);
    }


    [HttpGet(Name = "GetAllVideos")]
    public ActionResult<IEnumerable<Video>> GetAll()
    {
        var videos = _videoRepo.GetAll();
        return Ok(videos);
    }


    [HttpGet("{id}", Name = "GetVideoById")]
    public ActionResult<Video> GetById(Guid id)
    {
        var video = _videoRepo.GetById(id);
        
        if (video == null)
        {
            return NotFound($"Vídeo com ID {id} não encontrado");
        }

        return Ok(video);
    }


    [HttpGet("account/{accountId}", Name = "GetVideosByAccount")]
    public ActionResult<IEnumerable<Video>> GetByAccount(Guid accountId)
    {
        var videos = _videoRepo.GetByAccountId(accountId);
        return Ok(videos);
    }


    [HttpGet("trending", Name = "GetTrendingVideos")]
    public ActionResult<IEnumerable<Video>> GetTrending([FromQuery] int limit = 10)
    {
        var videos = _videoRepo.GetMostViewed(limit);
        return Ok(videos);
    }


    [HttpPost("{id}/view", Name = "IncrementVideoView")]
    public IActionResult IncrementView(Guid id)
    {
        var video = _videoRepo.GetById(id);
        
        if (video == null)
        {
            return NotFound($"Vídeo com ID {id} não encontrado");
        }

        video.IncrementView();
        _videoRepo.Update(video);

        return Ok(new { Views = video.Views });
    }


    [HttpPut("{id}", Name = "UpdateVideo")]
    public IActionResult UpdateVideo(Guid id, [FromBody] UpdateVideoDTO updateVideo)
    {
        var video = _videoRepo.GetById(id);
        
        if (video == null)
        {
            return NotFound($"Vídeo com ID {id} não encontrado");
        }

        // Atualizar campos
        video.Title = updateVideo.Title ?? video.Title;
        video.Description = updateVideo.Description ?? video.Description;

        _videoRepo.Update(video);

        return Ok(video);
    }


    [HttpDelete("{id}", Name = "DeleteVideo")]
    public IActionResult DeleteVideo(Guid id)
    {
        var video = _videoRepo.GetById(id);
        
        if (video == null)
        {
            return NotFound($"Vídeo com ID {id} não encontrado");
        }

        _videoRepo.Delete(id);

        return NoContent();
    }
}


public record class NewVideoDTO(
    string Title, 
    string? Description, 
    int DurationInSeconds, 
    Guid AccountId
);


public record class UpdateVideoDTO(string? Title, string? Description);