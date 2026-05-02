using Avemepls.Auth.Abstractions.Models;
using Avemepls.Auth.Bearer;
using Avemepls.Auth.Bearer.Abstractions;
using Avemepls.Auth.Domain.ViaGoogle;
using Avemepls.Identity.DataAccess;
using Avemepls.Identity.DataAccess.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Avemepls.Auth.Domain.Tests.ViaGoogle;

public class LoginViaGoogleHandlerTests : IDisposable
{
    private readonly IdentityDbContext _dbContext;
    private readonly ITokenGenerator _tokenGenerator = Substitute.For<ITokenGenerator>();
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();
    private readonly LoginViaGoogle.Handler _handler;

    public LoginViaGoogleHandlerTests()
    {
        var options = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new IdentityDbContext(options);

        _tokenGenerator
            .Create(Arg.Any<UserData<int>>())
            .Returns(new TokenInformation { AccessToken = "access", RefreshToken = "refresh" });

        _handler = new LoginViaGoogle.Handler(_dbContext, _tokenGenerator, _publisher);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Handle_Should_ReturnTokenForExistingUser_When_FoundByGoogleId()
    {
        // Arrange
        var existingUser = new User
        {
            Username = "existing@example.com",
            Email = "existing@example.com",
            GoogleId = "google-123",
            FirstName = "Existing",
            LastName = "User",
            IsActive = true,
            EmailConfirmed = true,
        };

        await _dbContext.Users.AddAsync(existingUser);
        await _dbContext.SaveChangesAsync();

        var command = new LoginViaGoogle.Command
        {
            GoogleId = "google-123",
            Email = "different@example.com",
            FirstName = "Ignored",
            LastName = "Ignored",
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("access");

        var users = await _dbContext.Users.ToListAsync();
        users.Should().ContainSingle();
        users[0].Email.Should().Be("existing@example.com");
        users[0].GoogleId.Should().Be("google-123");

        await _publisher.Received(1).Publish<UserLoginNotification>(
            Arg.Is<UserLoginNotification>(n => n.Id == existingUser.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_LinkGoogleIdToExistingUser_When_FoundByEmail()
    {
        // Arrange
        var existingUser = new User
        {
            Username = "user@example.com",
            Email = "user@example.com",
            GoogleId = null,
            PasswordHash = "old-hash",
            IsActive = true,
            EmailConfirmed = true,
        };

        await _dbContext.Users.AddAsync(existingUser);
        await _dbContext.SaveChangesAsync();

        var command = new LoginViaGoogle.Command
        {
            GoogleId = "google-456",
            Email = "user@example.com",
            FirstName = "Some",
            LastName = "Name",
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("access");

        var user = await _dbContext.Users.SingleAsync();
        user.GoogleId.Should().Be("google-456");
        user.PasswordHash.Should().Be("old-hash");
    }

    [Fact]
    public async Task Handle_Should_CreateNewUser_When_NoMatchByGoogleIdOrEmail()
    {
        // Arrange
        var command = new LoginViaGoogle.Command
        {
            GoogleId = "google-789",
            Email = "new@example.com",
            FirstName = "New",
            LastName = "User",
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("access");

        var user = await _dbContext.Users.SingleAsync();
        user.Email.Should().Be("new@example.com");
        user.Username.Should().Be("new@example.com");
        user.GoogleId.Should().Be("google-789");
        user.FirstName.Should().Be("New");
        user.LastName.Should().Be("User");
        user.EmailConfirmed.Should().BeTrue();
        user.IsActive.Should().BeTrue();

        await _publisher.Received(1).Publish(
            Arg.Is<UserLoginNotification>(n => n.Id == user.Id),
            Arg.Any<CancellationToken>());
    }
}
