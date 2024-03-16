using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace API.Data;

public class Seed
{
    public static async Task ClearConnections(DataContext context)
    {
        context.Connections.RemoveRange(context.Connections);
        await context.SaveChangesAsync();
    }
    public static async Task SeedProjects(DataContext context)
    {
        if (await context.Project.AnyAsync())
            return;
        var projectData = await File.ReadAllTextAsync("Data/ProjectSeed.json");
        var projects = JsonSerializer.Deserialize<List<Project>>(projectData);
        foreach (var project in projects)
        {
            await context.Project.AddAsync(project);
        }
        await context.SaveChangesAsync();

    }
    public static async Task SeedCompanies(DataContext context)
    {
        if (await context.Companies.AnyAsync())
            return;

        var companyData = await File.ReadAllTextAsync("Data/CompanySeedData.json");
        var companies = JsonSerializer.Deserialize<List<Company>>(companyData);
        foreach (var company in companies)
        {
            await context.Companies.AddAsync(company);
        }
        await context.SaveChangesAsync();
    }
    public static async Task SeedUsers(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        if (await userManager.Users.AnyAsync())
            return;

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var userData = await File.ReadAllTextAsync("Data/UserSeedData.json");

        var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

        var roles = new List<AppRole>
        {
            new AppRole{ Name = "Member"},
            new AppRole{ Name = "Admin"},
            new AppRole{ Name = "Moderator"},
        };

        foreach (var role in roles)
        {
            await roleManager.CreateAsync(role);
        }

        foreach (var user in users)
        {
            user.UserName = user.UserName.ToLower();
            user.Created = DateTime.SpecifyKind(user.Created, DateTimeKind.Utc);
            user.LastActive = DateTime.SpecifyKind(user.LastActive, DateTimeKind.Utc);
            await userManager.CreateAsync(user, "Pa$$w0rd");
            await userManager.AddToRoleAsync(user, "Member");
        }

        var admin = new AppUser
        {
            UserName = "admin",
            CompanyId = 1
        };

        await userManager.CreateAsync(admin, "Pa$$w0rd");
        await userManager.AddToRolesAsync(admin, new[] { "Admin", "Moderator" });

    }
    public static void BuildConnectionString()
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = "postgres-sync.internal",
            Port = 5432,
            Username = "postgres",
            Password = "ToNCylSqpjss7qQ",
            Database = "synchub", // Replace with your actual database name
            SslMode = SslMode.Disable, // You can adjust this based on your security requirements
        };

        var connectionString = connectionStringBuilder.ToString();
    }
    public static string ConvertToNpgsqlConnectionString(string inputConnectionString, string databaseName)
    {
        var builder = new NpgsqlConnectionStringBuilder();

        try
        {
            var uri = new Uri(inputConnectionString);

            builder.Host = uri.Host.Replace("flycast", "internal");
            builder.Port = uri.Port > 0 ? (ushort)uri.Port : builder.Port; // Use the port from URI if available
            builder.Username = uri.UserInfo.Split(':')[0];
            builder.Password = uri.UserInfo.Split(':')[1];
            builder.Database = databaseName;
            builder.SslMode = SslMode.Disable; // You may need to adjust this based on your security requirements
        }
        catch (UriFormatException)
        {
            throw new ArgumentException("Invalid PostgreSQL connection string format.");
        }

        string conn =  builder.ToString();
        return conn;
    }
    
    public class AppConfig
    {
        public string Name { get; set; }
        public string FolderId { get; set; }
        public string CredFile { get; set; }
    }
}
