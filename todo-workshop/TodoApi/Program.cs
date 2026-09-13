using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Dtos;
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();
#region In-memory endpoints
var todos = new List<TodoGetDto>
{
  new TodoGetDto(1, "Learn C#", true),
  new TodoGetDto(2, "Learn ASP.NET Core", false),
  new TodoGetDto(3, "Build a web API", false)
};

var todoGroup = app.MapGroup("/api/todos");

todoGroup.MapGet("/", () => Results.Ok(todos));

todoGroup.MapGet("/{id}", (int id) =>
{
  var todo = todos.FirstOrDefault(x => x.Id == id);
  return todo is null ? Results.NotFound() : Results.Ok(todo);
});

todoGroup.MapPost("/", (TodoUpdateDto dto) =>
{
  var nextId = todos.Count == 0 ? 1 : todos.Max(x => x.Id) + 1;
  var todo = new TodoGetDto(nextId, dto.Title, false);
  todos.Add(todo);
  return Results.Created($"/api/todos/{todo.Id}", todo);
});

todoGroup.MapPut("/{id}", (int id, TodoPutDto dto) =>
{
  var index = todos.FindIndex(x => x.Id == id);
  if (index == -1) return Results.NotFound();

  var updatedTodo = new TodoGetDto(id, dto.Title, dto.IsCompleted);
  todos[index] = updatedTodo;
  return Results.NoContent();
});

todoGroup.MapDelete("/{id}", (int id) =>
{
  var index = todos.FindIndex(x => x.Id == id);
  if (index == -1) return Results.NotFound();

  todos.RemoveAt(index);
  return Results.NoContent();
});

#endregion

#region Database endpoints
var dbGroup = app.MapGroup("/api/db/todos");

dbGroup.MapGet("/", async (AppDbContext db) =>
{
  var todos = await db.Todoitems.ToListAsync();
  var resultDtos = todos.Select(t => new TodoGetDto(t.Id, t.Title ?? "", t.IsCompleted)).ToList();
  return Results.Ok(resultDtos);
});

dbGroup.MapGet("/{id}", async (int id, AppDbContext db) =>
{
  var todo = await db.Todoitems.FindAsync(id);
  if (todo is null) return Results.NotFound();

  var resultDto = new TodoGetDto(todo.Id, todo.Title ?? "", todo.IsCompleted);
  return Results.Ok(resultDto);
});

dbGroup.MapPost("/", async (TodoUpdateDto dto, AppDbContext db) =>
{
  try
  {
    var todo = new Todoitem
    {
      Title = dto.Title,
      IsCompleted = false,
      CreatedDate = DateTime.Now
    };

    db.Todoitems.Add(todo);
    await db.SaveChangesAsync();

    var resultDto = new TodoGetDto(todo.Id, todo.Title ?? "", todo.IsCompleted);
    return Results.Created($"/api/db/todos/{todo.Id}", resultDto);
  }
  catch (Exception ex)
  {
    return Results.Problem($"Error: {ex.Message}", statusCode: 500);
  }
});
#endregion

app.Run();

