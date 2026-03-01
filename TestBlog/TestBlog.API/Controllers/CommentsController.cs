using Microsoft.AspNetCore.Mvc;
using TestBlog.Models;
using TestBlog.Services;

namespace TestBlog.API.Controllers;

/// <summary>
/// Управление комментариями
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(ICommentService commentService, ILogger<CommentsController> logger)
    {
        _commentService = commentService;
        _logger = logger;
    }

    /// <summary>
    /// Получить комментарии к статье
    /// </summary>
    /// <param name="articleId">Идентификатор статьи</param>
    /// <returns>Список комментариев</returns>
    [HttpGet("article/{articleId}")]
    [ProducesResponseType(typeof(IEnumerable<Comment>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByArticle(int articleId)
    {
        var comments = await _commentService.GetCommentsByArticleAsync(articleId);
        return Ok(comments);
    }

    /// <summary>
    /// Получить комментарий по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор комментария</param>
    /// <returns>Комментарий</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Comment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            return Ok(comment);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Создать новый комментарий
    /// </summary>
    /// <param name="comment">Данные комментария</param>
    /// <returns>Созданный комментарий</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Comment), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Comment comment)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _commentService.CreateCommentAsync(comment);
        if (!result)
            return BadRequest("Failed to create comment.");

        return CreatedAtAction(nameof(GetById), new { id = comment.Id }, comment);
    }

    /// <summary>
    /// Обновить комментарий
    /// </summary>
    /// <param name="id">Идентификатор комментария</param>
    /// <param name="comment">Обновлённые данные комментария</param>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] Comment comment)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        comment.Id = id;
        var result = await _commentService.UpdateCommentAsync(comment);
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Удалить комментарий
    /// </summary>
    /// <param name="id">Идентификатор комментария</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _commentService.DeleteCommentAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
