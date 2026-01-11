using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetTYB.Api.Models;
using NetTYB.Api.Services;

namespace NetTYB.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/todos")]
public class TodosController : ControllerBase
{
    private readonly TodoService _todoService;

    public TodosController(TodoService todoService)
    {
        _todoService = todoService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<TodoItem>> GetTodos()
    {
        return Ok(_todoService.GetTodos(GetUserId()));
    }

    [HttpGet("{id:guid}")]
    public ActionResult<TodoItem> GetTodo(Guid id)
    {
        var todo = _todoService.GetTodo(GetUserId(), id);
        return todo is null ? NotFound() : Ok(todo);
    }

    [HttpPost]
    public ActionResult<TodoItem> CreateTodo(CreateTodoRequest request)
    {
        var todo = _todoService.CreateTodo(GetUserId(), request);
        return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
    }

    [HttpPut("{id:guid}")]
    public ActionResult<TodoItem> UpdateTodo(Guid id, UpdateTodoRequest request)
    {
        var todo = _todoService.UpdateTodo(GetUserId(), id, request);
        return todo is null ? NotFound() : Ok(todo);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult DeleteTodo(Guid id)
    {
        var removed = _todoService.DeleteTodo(GetUserId(), id);
        return removed ? NoContent() : NotFound();
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(ClaimTypes.Name);
        if (claim is null)
        {
            var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            return sub is null ? Guid.Empty : Guid.Parse(sub);
        }
        return Guid.Parse(claim);
    }
}
