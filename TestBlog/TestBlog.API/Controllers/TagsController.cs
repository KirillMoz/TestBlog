using Microsoft.AspNetCore.Mvc;
using TestBlog.Models;
using TestBlog.Services;

namespace TestBlog.API.Controllers;

/// <summary>
/// Управление тегами
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly ILogger<TagsController> _logger;

    public TagsController(ITagService tagService, ILogger<TagsController> logger)
    {
        _tagService = tagService;
        _logger = logger;
    }

    /// <summary>
    /// Получить все теги
    /// </summary>
    /// <returns>Список тегов</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Tag>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var tags = await _tagService.GetAllTagsAsync();
        return Ok(tags);
    }

    /// <summary>
    /// Получить тег по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор тега</param>
    /// <returns>Тег</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Tag), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            return Ok(tag);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Создать новый тег
    /// </summary>
    /// <param name="tag">Данные тега</param>
    /// <returns>Созданный тег</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Tag), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] Tag tag)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _tagService.CreateTagAsync(tag);
        if (!result)
            return BadRequest("Failed to create tag.");

        return CreatedAtAction(nameof(GetById), new { id = tag.Id }, tag);
    }

    /// <summary>
    /// Обновить тег
    /// </summary>
    /// <param name="id">Идентификатор тега</param>
    /// <param name="tag">Обновлённые данные тега</param>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] Tag tag)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        tag.Id = id;
        var result = await _tagService.UpdateTagAsync(tag);
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Удалить тег
    /// </summary>
    /// <param name="id">Идентификатор тега</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _tagService.DeleteTagAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
