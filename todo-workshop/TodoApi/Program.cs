using TodoApi.Dtos;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();
var todos = new List<TodoGetDto>
{
  new TodoGetDto(1, "Learn C#", true),
  new TodoGetDto(2, "Learn ASP.NET Core", false),
  new TodoGetDto(3, "Build a web API", false)
};

app.MapGet("/api/todos", () => Results.Ok(todos));

app.MapGet("/api/todos/{id}", (int id) =>
{
  var todo = todos.FirstOrDefault(x => x.Id == id);

  return todo is null
      ? Results.NotFound()
      : Results.Ok(todo);
});



app.MapPost("/api/todos", (TodoUpdateDto dto) =>
{
  var nextId = todos.Count == 0 ? 1 : todos.Max(x => x.Id) + 1;
  var todo = new TodoGetDto(nextId, dto.Title, false);
  todos.Add(todo);

  return Results.Created($"/api/todos/{todo.Id}", todo);
});




app.Run();

