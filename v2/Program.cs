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
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IParkingSessionService, ParkingSessionService>();
builder.Services.AddScoped<IParkingLotService, ParkingLotService>();
builder.Services.AddScoped<IReservationService, ReservationService>();


// ---------------------------------------------------------
// AUTHENTICATION (Custom Token Authentication)
// ---------------------------------------------------------

builder.Services.AddAuthorization();

// ---------------------------------------------------------
// CONTROLLERS
// ---------------------------------------------------------
builder.Services.AddControllers();

// ---------------------------------------------------------
// SWAGGER/OPENAPI
// ---------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "MobyPark API",
        Version = "v1",
        Description = "API voor het MobyPark parking management systeem"
    });

    // Enable XML documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

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

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MobyPark API v1");
    options.RoutePrefix = "swagger"; // Access at http://localhost:5000/swagger
});

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();   // IMPORTANT - before Authorization
app.UseAuthorization();

app.MapControllers();

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("Server running at: http://localhost:5000");
Console.WriteLine("Swagger UI available at: http://localhost:5000/swagger");
Console.ResetColor();

app.Run();

public partial class Program { }
