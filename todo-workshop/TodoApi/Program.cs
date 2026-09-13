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

// 1. GET: ดึงรายการ Todo ทั้งหมด
app.MapGet("/api/todos", () => Results.Ok(todos));

// 2. GET by ID: ดึงข้อมูล Todo ตาม ID ที่ระบุ
app.MapGet("/api/todos/{id}", (int id) =>
{
  var todo = todos.FirstOrDefault(x => x.Id == id);

  // ถ้าไม่เจอให้คืนค่า 404 Not Found, ถ้าเจอคืนค่า 200 OK พร้อมข้อมูล
  return todo is null
      ? Results.NotFound()
      : Results.Ok(todo);
});



// 3. POST: สร้าง Todo รายการใหม่
app.MapPost("/api/todos", (TodoUpdateDto dto) =>
{
  // หาค่า ID ถัดไป (Max ID + 1)
  var nextId = todos.Count == 0 ? 1 : todos.Max(x => x.Id) + 1;
  var todo = new TodoGetDto(nextId, dto.Title, false);
  
  // เพิ่มข้อมูลใหม่ลงใน List
  todos.Add(todo);

  // คืนค่า 201 Created พร้อมกับ URL สำหรับดึงข้อมูลที่เพิ่งสร้าง
  return Results.Created($"/api/todos/{todo.Id}", todo);
});


// 4. PUT: อัปเดตข้อมูล Todo ตาม ID (อัปเดตทั้งหมดทั้ง Title และ IsCompleted)
app.MapPut("/api/todos/{id}", (int id, TodoPutDto dto) =>
{
  // ค้นหา Index ของรายการที่ต้องการแก้ไข
  var index = todos.FindIndex(x => x.Id == id);
  if (index == -1) return Results.NotFound(); // ถ้าไม่เจอคืนค่า 404

  // สร้าง Object ใหม่ที่มีการอัปเดตค่าและแทนที่ของเดิมใน List
  var updatedTodo = new TodoGetDto(id, dto.Title, dto.IsCompleted);
  todos[index] = updatedTodo;

  // คืนค่า 204 No Content (สำเร็จแต่ไม่มีข้อมูลส่งกลับ)
  return Results.NoContent();
});

// 5. DELETE: ลบข้อมูล Todo ตาม ID
app.MapDelete("/api/todos/{id}", (int id) =>
{
  // ค้นหา Index ของรายการที่ต้องการลบ
  var index = todos.FindIndex(x => x.Id == id);
  if (index == -1) return Results.NotFound(); // ถ้าไม่เจอคืนค่า 404

  // ลบออกจาก List
  todos.RemoveAt(index);
  
  // คืนค่า 204 No Content (ลบสำเร็จแต่ไม่มีข้อมูลส่งกลับ)
  return Results.NoContent();
});

app.Run();

