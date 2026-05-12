using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Configuration;
using ResumeAI.Auth.Services;
using ResumeAI.Auth.Entities;
using ResumeAI.Auth.Enums;

namespace ResumeAI.Auth.API.Tests;

[TestFixture]
public class JwtServiceTests
{
    private Mock<IConfiguration> _configMock;
    private JwtService _jwtService;

    [SetUp]
    public void Setup()
    {
        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(c => c["Jwt:Secret"]).Returns("super_secret_key_that_is_long_enough_for_hmac256");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        _configMock.Setup(c => c["Jwt:ExpiryHours"]).Returns("1");

        _jwtService = new JwtService(_configMock.Object);
    }

    [Test]
    public void GenerateAccessToken_ReturnsValidToken()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Email = "test@example.com",
            Role = Role.USER,
            SubscriptionPlan = SubscriptionPlan.FREE,
            FullName = "Test User"
        };

        // Act
        var token = _jwtService.GenerateAccessToken(user);

        // Assert
        Assert.That(token, Is.Not.Null);
        Assert.That(token, Is.Not.Empty);
    }

    [Test]
    public void GenerateRefreshToken_ReturnsRandomString()
    {
        // Act
        var token1 = _jwtService.GenerateRefreshToken();
        var token2 = _jwtService.GenerateRefreshToken();

        // Assert
        Assert.That(token1, Is.Not.Null);
        Assert.That(token2, Is.Not.Null);
        Assert.That(token1, Is.Not.EqualTo(token2));
    }
}
