using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using v2.Data;

namespace v2
{
    class Program
    {
        static void Main(string[] args)
        {
            // Setup DI and DbContext
            var serviceProvider = new ServiceCollection()
                .AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql("Host=localhost;Port=5432;Database=mobypark;Username=postgres;Password=postgres"))
                .BuildServiceProvider();

            if (args.Length > 0 && args[0].ToLower() == "seed")
            {
                using (var context = serviceProvider.GetRequiredService<AppDbContext>())
                {
                    DataLoader.ImportData(context);
                }
            }
            else
            {
                Console.WriteLine("Run `dotnet run seed` to import data into the database.");
            }
        }
    }
}
