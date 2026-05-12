using NUnit.Framework;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ResumeAI.Resume.Services;
using ResumeAI.Resume.Repositories.Interfaces;
using ResumeAI.Resume.DTOs.Request;
using ResumeAI.Resume.DTOs.Response;
using ResumeAI.Resume.Entities;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace ResumeAI.Resume.API.Tests;

[TestFixture]
public class ResumeServiceTests
{
    private Mock<IResumeRepository> _resumeRepoMock;
    private Mock<IMapper> _mapperMock;
    private Mock<ILogger<ResumeService>> _loggerMock;
    private ResumeService _resumeService;

    [SetUp]
    public void Setup()
    {
        _resumeRepoMock = new Mock<IResumeRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ResumeService>>();

        _resumeService = new ResumeService(_resumeRepoMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task CreateResumeAsync_WhenLimitNotExceeded_CreatesResume()
    {
        // Arrange
        var userId = 1;
        var subscriptionPlan = "FREE";
        var request = new CreateResumeRequest { Title = "My Resume" };
        
        _resumeRepoMock.Setup(r => r.CountByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
            
        _resumeRepoMock.Setup(r => r.CreateAsync(It.IsAny<ResumeEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResumeEntity { ResumeId = 1, Title = "My Resume" });

        _mapperMock.Setup(m => m.Map<ResumeResponse>(It.IsAny<ResumeEntity>()))
            .Returns(new ResumeResponse { ResumeId = 1, Title = "My Resume" });

        // Act
        var result = await _resumeService.CreateResumeAsync(userId, subscriptionPlan, request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ResumeId, Is.EqualTo(1));
        _resumeRepoMock.Verify(r => r.CreateAsync(It.IsAny<ResumeEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void CreateResumeAsync_WhenLimitExceeded_ThrowsException()
    {
        // Arrange
        var userId = 1;
        var subscriptionPlan = "FREE";
        var request = new CreateResumeRequest { Title = "My Resume" };
        
        _resumeRepoMock.Setup(r => r.CountByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3); // Limit is 3

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await _resumeService.CreateResumeAsync(userId, subscriptionPlan, request));
    }
}
