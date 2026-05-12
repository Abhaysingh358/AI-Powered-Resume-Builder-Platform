using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using ResumeAI.AI.Services;
using ResumeAI.AI.Enums;
using ResumeAI.AI.Configuration;

namespace ResumeAI.AI.API.Tests;

[TestFixture]
public class QuotaServiceTests
{
    private Mock<IDistributedCache> _cacheMock;
    private Mock<IOptions<AiSettings>> _settingsMock;
    private Mock<ILogger<QuotaService>> _loggerMock;
    private QuotaService _quotaService;

    [SetUp]
    public void Setup()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _settingsMock = new Mock<IOptions<AiSettings>>();
        _loggerMock = new Mock<ILogger<QuotaService>>();

        _settingsMock.Setup(s => s.Value).Returns(new AiSettings());

        _quotaService = new QuotaService(_cacheMock.Object, _settingsMock.Object, _loggerMock.Object);
    }

    [Test]
    public void IsAtsRequest_WhenTypeIsAts_ReturnsTrue()
    {
        // Arrange
        var type = RequestType.ATS;

        // Act
        var result = _quotaService.IsAtsRequest(type);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsAtsRequest_WhenTypeIsNotAts_ReturnsFalse()
    {
        // Arrange
        var type = RequestType.SUMMARY;

        // Act
        var result = _quotaService.IsAtsRequest(type);

        // Assert
        Assert.That(result, Is.False);
    }
}
