using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


//1. Add Database Context to the DI container
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite("Data Source=todo.db"));

// Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

//2. Map Endpoints
app.MapGet("", () => "Hello World!");

// Create Todo
app.MapPost("/todos", async (TodoDbContext db, Todo todo) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todos/{todo.Id}", todo);
});

// Read all
app.MapGet("/todos", async (TodoDbContext db) =>
    await db.Todos.ToListAsync());

// Read single 
app.MapGet("/todos/{id}", async (int id, TodoDbContext db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound());
// Update
app.MapPut("/todos/{id}", async (int id, TodoDbContext db, Todo inputTodo) =>
{
    var todo = await db.Todos.FindAsync(id);
    if (todo is null) return Results.NotFound();
    todo.Name = inputTodo.Name;
    todo.IsCompleted = inputTodo.IsCompleted;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Delete
app.MapDelete("/todos/{id}", async (int id, TodoDbContext db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(todo);
    }
    return Results.NotFound();
});

app.Run();

