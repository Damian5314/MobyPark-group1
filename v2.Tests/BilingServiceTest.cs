// using FluentAssertions;
// using Microsoft.EntityFrameworkCore;
// using v2.Data;
// using v2.Models;
// using v2.Services;
// using Xunit;

// namespace v2.Tests
// {
//     public class BillingServiceTests
//     {
//         private BillingService CreateService(string dbName, out AppDbContext db)
//         {
//             var options = new DbContextOptionsBuilder<AppDbContext>()
//                 .UseInMemoryDatabase(databaseName: dbName)
//                 .Options;

//             db = new AppDbContext(options);
//             return new BillingService(db);
//         }

//         // GetByUserIdAsync

//         [Fact]
//         public async Task GetByUserIdAsync_Should_Return_Null_When_User_Not_Found()
//         {
//             var service = CreateService(nameof(GetByUserIdAsync_Should_Return_Null_When_User_Not_Found), out _);

//             var result = await service.GetByUserIdAsync(999);

//             result.Should().BeNull();
//         }

//         [Fact]
//         public async Task GetByUserIdAsync_Should_Return_Null_When_User_Has_No_Payments()
//         {
//             var service = CreateService(nameof(GetByUserIdAsync_Should_Return_Null_When_User_Has_No_Payments), out var db);

//             db.Users.Add(new UserProfile { Id = 1, Username = "testuser" });
//             await db.SaveChangesAsync();

//             var result = await service.GetByUserIdAsync(1);

//             result.Should().BeNull();
//         }

//         [Fact]
//         public async Task GetByUserIdAsync_Should_Return_Billing_With_Payments()
//         {
//             var service = CreateService(nameof(GetByUserIdAsync_Should_Return_Billing_With_Payments), out var db);

//             db.Users.Add(new UserProfile { Id = 1, Username = "john" }); db.Payments.Add(new Payment { Id = 10, Amount = 5.00m, Initiator = "john" });
//             db.Payments.Add(new Payment { Id = 11, Amount = 10.00m, Initiator = "john" });

//             await db.SaveChangesAsync();

//             var result = await service.GetByUserIdAsync(1);

//             result.Should().NotBeNull();
//             result!.User.Should().Be("john");
//             result.Payments.Should().HaveCount(2);
//         }

//         // GetAllAsync TESTS

//         [Fact]
//         public async Task GetAllAsync_Should_Return_Empty_When_No_Payments()
//         {
//             var service = CreateService(nameof(GetAllAsync_Should_Return_Empty_When_No_Payments), out _);

//             var result = await service.GetAllAsync();

//             result.Should().BeEmpty();
//         }

//         [Fact]
//         public async Task GetAllAsync_Should_Group_Payments_By_User()
//         {
//             var service = CreateService(nameof(GetAllAsync_Should_Group_Payments_By_User), out var db);

//             db.Payments.AddRange(
//                 new Payment { Id = 1, Amount = 5, Initiator = "alice" },
//                 new Payment { Id = 2, Amount = 6, Initiator = "alice" },
//                 new Payment { Id = 3, Amount = 7, Initiator = "bob" }
//             );

//             await db.SaveChangesAsync();

//             var result = (await service.GetAllAsync()).ToList();

//             result.Should().HaveCount(2);

//             var aliceBilling = result.First(b => b.User == "alice");
//             aliceBilling.Payments.Should().HaveCount(2);

//             var bobBilling = result.First(b => b.User == "bob");
//             bobBilling.Payments.Should().HaveCount(1);
//         }
//     }
// }