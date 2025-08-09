using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


// Define a CORS policy
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

//1. Add Database Context to the DI container
builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlite("Data Source=todo.db"));

// Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          // Replace the port with the one your Blazor app will use!
                          // You can find it in TodoClient/Properties/launchSettings.json
                          policy.WithOrigins("http://localhost:5207")
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors(MyAllowSpecificOrigins);
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

// U: UPDATE a Todo item
app.MapPut("/todos/{id}", async (int id, Todo inputTodo, TodoDbContext db) =>
{
    // This is a more modern and efficient approach using EF Core 7+
    var rowsAffected = await db.Todos.Where(t => t.Id == id)
        .ExecuteUpdateAsync(updates =>
            updates.SetProperty(t => t.Name, inputTodo.Name)
                   .SetProperty(t => t.IsCompleted, inputTodo.IsCompleted));

    // If no rows were affected, it means the ID was not found.
    
    return rowsAffected == 0 ? Results.NotFound() : Results.NoContent();
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

