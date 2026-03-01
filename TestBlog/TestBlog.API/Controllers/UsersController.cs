using Microsoft.AspNetCore.Mvc;
using TestBlog.Models;
using TestBlog.Services.Interfaces;

namespace TestBlog.API.Controllers;

/// <summary>
/// Управление пользователями
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Получить всех пользователей
    /// </summary>
    /// <returns>Список пользователей</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    /// <summary>
    /// Получить пользователя по идентификатору
    /// </summary>
    /// <param name="id">Идентификатор пользователя</param>
    /// <returns>Пользователь</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    /// <summary>
    /// Создать нового пользователя
    /// </summary>
    /// <param name="user">Данные пользователя</param>
    /// <param name="password">Пароль</param>
    /// <returns>Созданный пользователь</returns>
    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] User user, [FromQuery] string password)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _userService.CreateUserAsync(user, password);
        if (!result)
            return BadRequest("Failed to create user.");

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }

    /// <summary>
    /// Обновить пользователя
    /// </summary>
    /// <param name="id">Идентификатор пользователя</param>
    /// <param name="user">Обновлённые данные пользователя</param>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] User user)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        user.Id = id;
        var result = await _userService.UpdateUserAsync(user);
        if (!result)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Удалить пользователя
    /// </summary>
    /// <param name="id">Идентификатор пользователя</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
