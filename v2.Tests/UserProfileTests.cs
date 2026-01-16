using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using v2.Data;
using v2.Models;
using v2.Security;
using v2.Services;
using Xunit;

namespace v2.Tests
{
    public class UserProfileServiceTests
    {
        private readonly AppDbContext _context;
        private readonly UserProfileService _service;

        public UserProfileServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            SeedDatabase(_context);

            var loggerMock = new Mock<ILogger<UserProfileService>>();
            _service = new UserProfileService(_context, loggerMock.Object);
        }

        private static void SeedDatabase(AppDbContext context)
        {
            // User 1
            context.Users.Add(new UserProfile
            {
                Id = 1,
                Username = "testuser",
                Password = PasswordHelper.HashPassword("test123"),
                Name = "Test User",
                Email = "testuser@example.com",
                Phone = "123456",
                BirthYear = 1990,
                Role = "USER",
                Active = true
            });

            // User 2 (to test "username taken")
            context.Users.Add(new UserProfile
            {
                Id = 2,
                Username = "takenname",
                Password = PasswordHelper.HashPassword("pw"),
                Name = "Other User",
                Email = "other@example.com",
                Phone = "999",
                BirthYear = 1980,
                Role = "USER",
                Active = true
            });

            context.SaveChanges();
        }

        [Fact]
        public async Task GetByUsername_Should_Return_User()
        {
            var user = await _service.GetByUsernameAsync("testuser");

            user.Should().NotBeNull();
            user!.Username.Should().Be("testuser");
            user.Name.Should().Be("Test User");
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Profile_Fields()
        {
            var dto = new UpdateMyProfileDto
            {
                Username = "testuser", // keep same
                Name = "Updated Name",
                Email = "updated@example.com",
                Phone = "777",
                BirthYear = 1995
            };

            var updated = await _service.UpdateAsync("testuser", dto);

            updated.Should().NotBeNull();
            updated!.Username.Should().Be("testuser");
            updated.Name.Should().Be("Updated Name");
            updated.Email.Should().Be("updated@example.com");
            updated.Phone.Should().Be("777");
            updated.BirthYear.Should().Be(1995);
        }

        [Fact]
        public async Task UpdateAsync_Should_Allow_Username_Change_When_Free()
        {
            var dto = new UpdateMyProfileDto
            {
                Username = "newname",
                Name = "Test User",
                Email = "testuser@example.com",
                Phone = "123456",
                BirthYear = 1990
            };

            var updated = await _service.UpdateAsync("testuser", dto);

            updated.Should().NotBeNull();
            updated!.Username.Should().Be("newname");

            // old username no longer exists
            (await _service.GetByUsernameAsync("testuser")).Should().BeNull();
            (await _service.GetByUsernameAsync("newname")).Should().NotBeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_When_Username_Is_Taken()
        {
            var dto = new UpdateMyProfileDto
            {
                Username = "takenname", // already exists (seeded)
                Name = "X",
                Email = "x@x.com",
                Phone = "1",
                BirthYear = 2000
            };

            Func<Task> act = async () => await _service.UpdateAsync("testuser", dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*taken*");
        }

        [Fact]
        public async Task ChangePasswordAsync_Should_Succeed_With_Correct_CurrentPassword()
        {
            var ok = await _service.ChangePasswordAsync("testuser", "test123", "newpass");
            ok.Should().BeTrue();

            var reloaded = await _service.GetByUsernameAsync("testuser");
            reloaded.Should().NotBeNull();
            PasswordHelper.VerifyPassword("newpass", reloaded!.Password).Should().BeTrue();
        }

        [Fact]
        public async Task ChangePasswordAsync_Should_Fail_With_Wrong_CurrentPassword()
        {
            var ok = await _service.ChangePasswordAsync("testuser", "WRONG", "newpass");
            ok.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_Should_Remove_User()
        {
            var ok = await _service.DeleteAsync("testuser");
            ok.Should().BeTrue();

            var user = await _service.GetByUsernameAsync("testuser");
            user.Should().BeNull();
        }

        [Fact]
        public async Task AdminUpdateAsync_Should_Update_Role_Active_And_Password()
        {
            var dto = new AdminUpdateUserDto
            {
                Role = "ADMIN",
                Active = false,
                NewPassword = "adminpw",
                Name = "Admin Updated"
            };

            var updated = await _service.AdminUpdateAsync("testuser", dto);

            updated.Should().NotBeNull();
            updated!.Role.Should().Be("ADMIN");
            updated.Active.Should().BeFalse();
            updated.Name.Should().Be("Admin Updated");

            PasswordHelper.VerifyPassword("adminpw", updated.Password).Should().BeTrue();
        }
    }
}
