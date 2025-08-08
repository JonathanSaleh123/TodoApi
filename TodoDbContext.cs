using Microsoft.EntityFrameworkCore;

class TodoDbContext : DbContext
{
    // This constructor allows us to pass in the configuration, like the connection string
    public TodoDbContext(DbContextOptions<TodoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Todo> Todos  { get; set; } = null!;
}