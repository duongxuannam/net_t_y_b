namespace NetTYB.Api.Models;

public record TodoItem(Guid Id, string Title, bool Completed);
public record CreateTodoRequest(string Title);
public record UpdateTodoRequest(string Title, bool Completed);
