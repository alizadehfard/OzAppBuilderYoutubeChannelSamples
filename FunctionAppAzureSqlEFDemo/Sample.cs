// Program.cs
using FunctionAppAzureSqlEFDemo.DatabaseModels;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var connectionString = Environment.GetEnvironmentVariable("SqlDb");
        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

        // Ensure database migrations are applied at startup
        using (var scope = services.BuildServiceProvider().CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();  // Apply pending migrations
        }

    })
    .Build();

host.Run();

// GetStudents
using FunctionAppAzureSqlEFDemo.DatabaseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class GetStudents
{
    private readonly ILogger<GetStudents> _logger;
    private readonly AppDbContext _dbContext;

    public GetStudents(
        ILogger<GetStudents> logger,
        AppDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }


    [Function("GetStudents")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function is called...");
        try
        {

            var students = await _dbContext.Students.ToListAsync();


            return new OkObjectResult(students);

        }
        catch (Exception ex)
        {
            return new BadRequestObjectResult(ex);
        }
    }
}

//Student
public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string Lastname { get; set; }
    public int Age { get; set; }
}

// AppDbContext
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Student> Students { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var connectionString = Environment.GetEnvironmentVariable("SqlDb");
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed data
        modelBuilder.Entity<Student>().HasData(
            new Student { Id = 1, FirstName = "Tom", Lastname = "Doe", Age = 38 }
        );
    }
}
