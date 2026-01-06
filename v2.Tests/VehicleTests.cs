// using FluentAssertions;
// using Microsoft.EntityFrameworkCore;
// using v2.Data;
// using v2.Models;
// using v2.Services;
// using Xunit;

// namespace v2.Tests
// {
//     public class VehicleServiceTests
//     {
//         private VehicleService CreateService(string dbName, out AppDbContext db)
//         {
//             var options = new DbContextOptionsBuilder<AppDbContext>()
//                 .UseInMemoryDatabase(databaseName: dbName)
//                 .Options;

//             db = new AppDbContext(options);
//             return new VehicleService(db);
//         }

//         // GetAllAsync
//         [Fact]
//         public async Task GetAllAsync_Should_Return_All_Vehicles()
//         {
//             var service = CreateService(nameof(GetAllAsync_Should_Return_All_Vehicles), out var db);

//             db.Vehicles.AddRange(
//                 new Vehicle { Id = 1, Make = "A", UserId = 10 },
//                 new Vehicle { Id = 2, Make = "B", UserId = 11 }
//             );
//             await db.SaveChangesAsync();

//             var result = await service.GetAllAsync();

//             result.Should().HaveCount(2);
//         }

//         // GetByIdAsync
//         [Fact]
//         public async Task GetByIdAsync_Should_Return_Vehicle_When_Exists()
//         {
//             var service = CreateService(nameof(GetByIdAsync_Should_Return_Vehicle_When_Exists), out var db);

//             db.Vehicles.Add(new Vehicle { Id = 5, Make = "Tesla", UserId = 1 });
//             await db.SaveChangesAsync();

//             var result = await service.GetByIdAsync(5);

//             result.Should().NotBeNull();
//             result!.Make.Should().Be("Tesla");
//         }

//         [Fact]
//         public async Task GetByIdAsync_Should_Return_Null_When_Not_Found()
//         {
//             var service = CreateService(nameof(GetByIdAsync_Should_Return_Null_When_Not_Found), out _);

//             var result = await service.GetByIdAsync(999);

//             result.Should().BeNull();
//         }

//         // GetByUserIdAsync
//         [Fact]
//         public async Task GetByUserIdAsync_Should_Return_Vehicles_For_User()
//         {
//             var service = CreateService(nameof(GetByUserIdAsync_Should_Return_Vehicles_For_User), out var db);

//             db.Vehicles.AddRange(
//                 new Vehicle { Id = 1, UserId = 10 },
//                 new Vehicle { Id = 2, UserId = 10 },
//                 new Vehicle { Id = 3, UserId = 11 }
//             );
//             await db.SaveChangesAsync();

//             var result = await service.GetByUserIdAsync(10);

//             result.Should().HaveCount(2);
//         }

//         // CreateAsync
//         [Fact]
//         public async Task CreateAsync_Should_Add_Vehicle_With_CreatedAt()
//         {
//             var service = CreateService(nameof(CreateAsync_Should_Add_Vehicle_With_CreatedAt), out var db);

//             var vehicle = new Vehicle
//             {
//                 Make = "Honda",
//                 Model = "Civic",
//                 UserId = 7
//             };

//             var created = await service.CreateAsync(vehicle);

//             created.Should().NotBeNull();
//             created.Id.Should().BeGreaterThan(0);
//             created.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));

//             db.Vehicles.Count().Should().Be(1);
//         }

//         // UpdateAsync
//         [Fact]
//         public async Task UpdateAsync_Should_Update_Fields()
//         {
//             var service = CreateService(nameof(UpdateAsync_Should_Update_Fields), out var db);

//             db.Vehicles.Add(new Vehicle
//             {
//                 Id = 20,
//                 Make = "Ford",
//                 Model = "Focus",
//                 Color = "Blue",
//                 Year = 2010
//             });

//             await db.SaveChangesAsync();

//             var update = new Vehicle
//             {
//                 LicensePlate = "NEW123",
//                 Make = "Toyota",
//                 Model = "Corolla",
//                 Color = "Red",
//                 Year = 2020
//             };

//             var updated = await service.UpdateAsync(20, update);

//             updated.LicensePlate.Should().Be("NEW123");
//             updated.Make.Should().Be("Toyota");
//             updated.Model.Should().Be("Corolla");
//             updated.Color.Should().Be("Red");
//             updated.Year.Should().Be(2020);
//         }

//         [Fact]
//         public async Task UpdateAsync_Should_Throw_When_Not_Found()
//         {
//             var service = CreateService(nameof(UpdateAsync_Should_Throw_When_Not_Found), out _);

//             var update = new Vehicle();

//             var act = async () => await service.UpdateAsync(999, update);

//             await act.Should().ThrowAsync<KeyNotFoundException>()
//                      .WithMessage("Vehicle not found.");
//         }

//         // DeleteAsync
//         [Fact]
//         public async Task DeleteAsync_Should_Remove_Vehicle_When_Exists()
//         {
//             var service = CreateService(nameof(DeleteAsync_Should_Remove_Vehicle_When_Exists), out var db);

//             db.Vehicles.Add(new Vehicle { Id = 50, Make = "BMW" });
//             await db.SaveChangesAsync();

//             var deleted = await service.DeleteAsync(50);

//             deleted.Should().BeTrue();
//             db.Vehicles.Count().Should().Be(0);
//         }

//         [Fact]
//         public async Task DeleteAsync_Should_Return_False_When_Not_Found()
//         {
//             var service = CreateService(nameof(DeleteAsync_Should_Return_False_When_Not_Found), out _);

//             var deleted = await service.DeleteAsync(777);

//             deleted.Should().BeFalse();
//         }
//     }
// }