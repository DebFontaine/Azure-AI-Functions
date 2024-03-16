
using API.Data;
using API.Entities;
using API.Extensions;
using API.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);



var connString = "";

if (builder.Environment.IsDevelopment())
{
    connString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine("connString:" + connString);
}
else
{
    connString = Environment.GetEnvironmentVariable("DATABASE_URL");
} 
builder.Services.AddDbContext<DataContext>(opt =>
{
    opt.UseNpgsql(connString);
});




// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
/* builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
 */
var app = builder.Build();
//app.UseSwagger();
//app.UseSwaggerUI(c => 
//{
    //c.SwaggerEndpoint("/swagger/v1/swagger.json", "SynHub API v1");
    //c.RoutePrefix = string.Empty;
//});

app.UseMiddleware<ExceptionMiddleware>();


app.UseCors(builder => builder
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins("https://localhost:4200",
                 "http://localhost:4200"));


//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapFallbackToController("Index", "Fallback");

using var scope = app.Services.CreateScope();

var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<DataContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    await context.Database.MigrateAsync();
    await Seed.ClearConnections(context);
    await Seed.SeedProjects(context);
    await Seed.SeedCompanies(context);
    await Seed.SeedUsers(userManager, roleManager);
}
catch (Exception ex)
{
    var logger = services.GetService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
}

app.Run();


