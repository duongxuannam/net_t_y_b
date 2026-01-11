using NetTYB.Api.Models;

namespace NetTYB.Api.Services;

public class TodoService
{
    private readonly Dictionary<Guid, List<TodoItem>> _todos = new();
    private readonly object _lock = new();

    public IReadOnlyList<TodoItem> GetTodos(Guid userId)
    {
        lock (_lock)
        {
            return _todos.TryGetValue(userId, out var items)
                ? items.Select(todo => todo with { }).ToList()
                : Array.Empty<TodoItem>();
        }
    }

    public TodoItem? GetTodo(Guid userId, Guid todoId)
    {
        lock (_lock)
        {
            return _todos.TryGetValue(userId, out var items)
                ? items.FirstOrDefault(todo => todo.Id == todoId)
                : null;
        }
    }

    public TodoItem CreateTodo(Guid userId, CreateTodoRequest request)
    {
        var todo = new TodoItem(Guid.NewGuid(), request.Title, false);
        lock (_lock)
        {
            if (!_todos.TryGetValue(userId, out var items))
            {
                items = new List<TodoItem>();
                _todos[userId] = items;
            }
            items.Add(todo);
        }
        return todo;
    }

    public TodoItem? UpdateTodo(Guid userId, Guid todoId, UpdateTodoRequest request)
    {
        lock (_lock)
        {
            if (!_todos.TryGetValue(userId, out var items))
            {
                return null;
            }

            var index = items.FindIndex(todo => todo.Id == todoId);
            if (index < 0)
            {
                return null;
            }

            var updated = new TodoItem(todoId, request.Title, request.Completed);
            items[index] = updated;
            return updated;
        }
    }

    public bool DeleteTodo(Guid userId, Guid todoId)
    {
        lock (_lock)
        {
            if (!_todos.TryGetValue(userId, out var items))
            {
                return false;
            }

            var removed = items.RemoveAll(todo => todo.Id == todoId);
            return removed > 0;
        }
    }
}
