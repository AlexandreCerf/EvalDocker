using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Configuration.AddEnvironmentVariables();

var app = builder.Build();

app.UseCors("AllowAll");

app.MapGet("/test", async () =>
{
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                              ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

    try
    {
        using (var connection = new MySqlConnection(connectionString))
        {
            await connection.OpenAsync();

            var createTableCmd = new MySqlCommand(@"
                CREATE TABLE IF NOT EXISTS TestTable (
                    Id INT AUTO_INCREMENT PRIMARY KEY,
                    Message VARCHAR(255) NOT NULL
                );", connection);
            await createTableCmd.ExecuteNonQueryAsync();

            var insertCmd = new MySqlCommand("INSERT INTO TestTable (Message) VALUES ('Hello from EvalDocker!');", connection);
            await insertCmd.ExecuteNonQueryAsync();

            var selectCmd = new MySqlCommand("SELECT COUNT(*) FROM TestTable;", connection);
            int count = Convert.ToInt32(await selectCmd.ExecuteScalarAsync());

            return Results.Ok($"API mise en relation avec MySQL ! Nombre d'enregistrements dans TestTable : {count}");
        }
    }
    catch (System.Exception ex)
    {
        return Results.Problem($"La connexion a échoué : {ex.ToString()}");
    }
});

app.Run();
