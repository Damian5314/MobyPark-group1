using Microsoft.EntityFrameworkCore;
using v2.Data;
using v2.Services;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------
// DATABASE
// ---------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql("Host=localhost;Port=5432;Database=mobypark;Username=postgres;Password=postgres"));

// ---------------------------------------------------------
// SERVICES
// ---------------------------------------------------------
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();

// ---------------------------------------------------------
// AUTHENTICATION (Custom Token Authentication)
// ---------------------------------------------------------
builder.Services.AddAuthentication("TokenAuth")
    .AddScheme<AuthenticationSchemeOptions, TokenAuthHandler>("TokenAuth", null);

builder.Services.AddAuthorization();

// ---------------------------------------------------------
// CONTROLLERS
// ---------------------------------------------------------
builder.Services.AddControllers();

// ---------------------------------------------------------
// CORS
// ---------------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.Urls.Add("http://localhost:5000");

// ---------------------------------------------------------
// CHECK DATABASE CONNECTION
// ---------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        db.Database.OpenConnection();
        db.Database.CloseConnection();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Database connection successful!");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Database connection failed:");
        Console.WriteLine(ex.Message);
    }
    finally
    {
        Console.ResetColor();
    }
}

// ---------------------------------------------------------
// OPTIONAL: DATA SEEDING
// ---------------------------------------------------------
if (args.Length > 0 && args[0].ToLower() == "seed")
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DataLoader.ImportData(context);
    Console.WriteLine("Data imported successfully!");
}
else
{
    Console.WriteLine("ℹ Run `dotnet run seed` to import data.");
}

// ---------------------------------------------------------
// MIDDLEWARE PIPELINE
// ---------------------------------------------------------
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();   // IMPORTANT - before Authorization
app.UseAuthorization();

app.MapControllers();

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("Server running at: http://localhost:5000");
Console.ResetColor();

app.Run();

public partial class Program { }
