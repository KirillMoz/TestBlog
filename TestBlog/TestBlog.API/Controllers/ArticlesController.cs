using Microsoft.AspNetCore.Mvc;
using TestBlog.Models;
using TestBlog.Services;

namespace TestBlog.API.Controllers;

/// <summary>
/// Управление статьями блога
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleService _articleService;
    private readonly ILogger<ArticlesController> _logger;

    public ArticlesController(IArticleService articleService, ILogger<ArticlesController> logger)
    {
        _articleService = articleService;
        _logger = logger;
    }

    /// <summary>
    /// Получить все опубликованные статьи
    /// </summary>
    /// <returns>Список статей</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Article>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var articles = await _articleService.GetPublishedArticlesAsync();
        return Ok(articles);
    }

    /// <summary>
    /// Получить статью по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор статьи</param>
    /// <returns>Статья</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Article), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            return Ok(article);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Создать новую статью
    /// </summary>
    /// <param name="article">Данные статьи</param>
    /// <param name="tagIds">Список идентификаторов тегов</param>
    /// <returns>Созданная статья</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Article), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Article article, [FromQuery] List<int>? tagIds)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _articleService.CreateArticleAsync(article, tagIds ?? new List<int>());
        if (!result)
            return BadRequest("Failed to create article.");

        return CreatedAtAction(nameof(GetById), new { id = article.Id }, article);
    }

    /// <summary>
    /// Обновить статью
    /// </summary>
    /// <param name="id">Идентификатор статьи</param>
    /// <param name="article">Обновлённые данные статьи</param>
    /// <param name="tagIds">Список идентификаторов тегов</param>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] Article article, [FromQuery] List<int>? tagIds)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        article.Id = id;
        var result = await _articleService.UpdateArticleAsync(article, tagIds ?? new List<int>());
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Удалить статью
    /// </summary>
    /// <param name="id">Идентификатор статьи</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _articleService.DeleteArticleAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
