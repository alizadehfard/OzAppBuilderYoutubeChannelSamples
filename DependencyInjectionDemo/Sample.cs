public class StudentModel
{
    public string StdNum { get; set; }
    public string FirstName { get; set; }
    public string Lastname { get; set; }
    public int Age { get; set; }
}

public interface IDatabaseService
{
    StudentModel GetStudent(string studentNum);
}

public class DatabaseService : IDatabaseService
{
    public StudentModel GetStudent(string studentNum)
    {
        // Fetch student record from database and return it
        //...
        return new StudentModel
        {
            StdNum = studentNum,
            FirstName = "John",
            Lastname = "Doe",
            Age = 14
        };
    }
}

public interface IStudent
{
    int GetAge();
}

public class Student : IStudent
{
    private IDatabaseService _databaseService;

    public Student(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public int GetAge()
    {
        var stdNum = "1234";
        return _databaseService.GetStudent(stdNum).Age;
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var services = new ServiceCollection()
            .AddSingleton<IDatabaseService, DatabaseService>()
            .AddTransient<IStudent, Student>()
            .BuildServiceProvider();

        var std = services.GetService<IStudent>();
        var age = std.GetAge();

        Console.Write($"Student age: {age}");
    }
}
