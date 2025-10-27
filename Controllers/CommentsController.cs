using Microsoft.AspNetCore.Mvc;
using YouTube.Models;
using YouTube.Repositories;

namespace WebYouTube.Controllers;


[ApiController]
[Route("api/v1/comments")]
public class CommentsController : ControllerBase
{
    private readonly ILogger<CommentsController> _logger;
    private readonly ICommentRepository _commentRepo;
    private readonly IVideoRepository _videoRepo;
    private readonly IAccountRepository _accountRepo;

    public CommentsController(
        ILogger<CommentsController> logger,
        ICommentRepository commentRepo,
        IVideoRepository videoRepo,
        IAccountRepository accountRepo)
    {
        _logger = logger;
        _commentRepo = commentRepo;
        _videoRepo = videoRepo;
        _accountRepo = accountRepo;
    }


    [HttpPost(Name = "CreateComment")]
    public IActionResult CreateComment([FromBody] NewCommentDTO newComment)
    {
        var video = _videoRepo.GetById(newComment.VideoId);
        if (video == null)
        {
            return BadRequest($"Vídeo com ID {newComment.VideoId} não encontrado");
        }

        var author = _accountRepo.GetById(newComment.AuthorId);
        if (author == null)
        {
            return BadRequest($"Conta com ID {newComment.AuthorId} não encontrada");
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Text = newComment.Text,
            Video = video,
            Author = author,
            CreatedAt = DateTime.UtcNow
        };

        _commentRepo.Save(comment);
        
        video.Comments.Add(comment);
        author.Comments.Add(comment);
        
        _videoRepo.Update(video);
        _accountRepo.Update(author);

        _logger.LogInformation("Novo comentário criado no vídeo {VideoTitle} por {Username}", 
            video.Title, author.Username);

        return CreatedAtRoute("GetCommentById", new { id = comment.Id }, comment);
    }


    [HttpGet(Name = "GetAllComments")]
    public ActionResult<IEnumerable<Comment>> GetAll()
    {
        var comments = _commentRepo.GetAll();
        return Ok(comments);
    }


    [HttpGet("{id}", Name = "GetCommentById")]
    public ActionResult<Comment> GetById(Guid id)
    {
        var comment = _commentRepo.GetById(id);
        
        if (comment == null)
        {
            return NotFound($"Comentário com ID {id} não encontrado");
        }

        return Ok(comment);
    }


    [HttpGet("video/{videoId}", Name = "GetCommentsByVideo")]
    public ActionResult<IEnumerable<Comment>> GetByVideo(Guid videoId)
    {
        var comments = _commentRepo.GetByVideoId(videoId);
        return Ok(comments);
    }


    [HttpGet("author/{authorId}", Name = "GetCommentsByAuthor")]
    public ActionResult<IEnumerable<Comment>> GetByAuthor(Guid authorId)
    {
        var comments = _commentRepo.GetByAuthorId(authorId);
        return Ok(comments);
    }


    [HttpPut("{id}", Name = "EditComment")]
    public IActionResult EditComment(Guid id, [FromBody] EditCommentDTO editComment)
    {
        var comment = _commentRepo.GetById(id);
        
        if (comment == null)
        {
            return NotFound($"Comentário com ID {id} não encontrado");
        }

        // Editar comentário
        comment.Edit(editComment.Text);
        _commentRepo.Update(comment);

        return Ok(comment);
    }


    [HttpPost("{id}/like", Name = "LikeComment")]
    public IActionResult LikeComment(Guid id)
    {
        var comment = _commentRepo.GetById(id);
        
        if (comment == null)
        {
            return NotFound($"Comentário com ID {id} não encontrado");
        }

        comment.AddLike();
        _commentRepo.Update(comment);

        return Ok(new { Likes = comment.Likes });
    }


    [HttpPost("{id}/dislike", Name = "DislikeComment")]
    public IActionResult DislikeComment(Guid id)
    {
        var comment = _commentRepo.GetById(id);
        
        if (comment == null)
        {
            return NotFound($"Comentário com ID {id} não encontrado");
        }

        comment.AddDislike();
        _commentRepo.Update(comment);

        return Ok(new { Dislikes = comment.Dislikes });
    }


    [HttpDelete("{id}", Name = "DeleteComment")]
    public IActionResult DeleteComment(Guid id)
    {
        var comment = _commentRepo.GetById(id);
        
        if (comment == null)
        {
            return NotFound($"Comentário com ID {id} não encontrado");
        }

        _commentRepo.Delete(id);

        return NoContent();
    }
}


public record class NewCommentDTO(string Text, Guid VideoId, Guid AuthorId);


public record class EditCommentDTO(string Text);