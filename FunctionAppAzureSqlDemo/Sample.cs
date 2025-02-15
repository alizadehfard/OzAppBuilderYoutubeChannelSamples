using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FunctionAppAzureSqlDemo
{

  public class Student
  {
      public int Id { get; set; }
      public string FirstName { get; set; }
      public string Lastname { get; set; }
      public int Age { get; set; }
  }
  public static class GetStudents
  {
      [FunctionName("GetStudents")]
      public static async Task<IActionResult> Run(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
          ILogger log)
      {
          log.LogInformation("C# HTTP trigger function is called...");

          var students = new List<Student>();

          try
          {
              var connStr = "Server=tcp:{YOUR_DATABASE_SERVER}.database.windows.net,1433;Initial Catalog={YOUR_DATABASE_NAME};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";";

              using (SqlConnection conn = new SqlConnection(connStr))
              {
                  await conn.OpenAsync();

                  var cmdText = "Select ID, FirstName, Lastname, Age from [dbo].[students];";

                  using (SqlCommand cmd = new SqlCommand(cmdText, conn))
                  {
                      using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                      {
                          while (await reader.ReadAsync())
                          {
                              students.Add(new Student
                              {
                                  Id = reader.GetInt32(0),
                                  FirstName = reader.GetString(1),
                                  Lastname = reader.GetString(2),
                                  Age = reader.GetInt32(3)
                              });
                          }
                      }
                  }
              }

              return new OkObjectResult(students);

          }
          catch (Exception ex)
          {
              return new BadRequestObjectResult(ex);
          }
      }
  }
  
}
