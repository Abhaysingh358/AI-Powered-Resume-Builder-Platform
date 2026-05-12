using NUnit.Framework;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ResumeAI.Section.Services;
using ResumeAI.Section.Repositories.Interfaces;
using ResumeAI.Section.DTOs.Request;
using ResumeAI.Section.DTOs.Response;
using ResumeAI.Section.Entities;
using ResumeAI.Section.Enums;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace ResumeAI.Section.API.Tests;

[TestFixture]
public class SectionServiceTests
{
    private Mock<ISectionRepository> _sectionRepoMock;
    private Mock<IMapper> _mapperMock;
    private Mock<ILogger<SectionService>> _loggerMock;
    private SectionService _sectionService;

    [SetUp]
    public void Setup()
    {
        _sectionRepoMock = new Mock<ISectionRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<SectionService>>();

        _sectionService = new SectionService(_sectionRepoMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task AddSectionAsync_WhenValidRequest_CreatesSection()
    {
        // Arrange
        var userId = 1;
        var request = new AddSectionRequest 
        { 
            ResumeId = 1, 
            SectionType = "EXPERIENCE", 
            Title = "My Experience" 
        };
        
        _sectionRepoMock.Setup(r => r.CreateAsync(It.IsAny<ResumeSection>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResumeSection { SectionId = 1, Title = "My Experience" });

        _mapperMock.Setup(m => m.Map<SectionResponse>(It.IsAny<ResumeSection>()))
            .Returns(new SectionResponse { SectionId = 1, Title = "My Experience" });

        // Act
        var result = await _sectionService.AddSectionAsync(userId, request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.SectionId, Is.EqualTo(1));
        _sectionRepoMock.Verify(r => r.CreateAsync(It.IsAny<ResumeSection>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void AddSectionAsync_WhenInvalidSectionType_ThrowsArgumentException()
    {
        // Arrange
        var userId = 1;
        var request = new AddSectionRequest 
        { 
            ResumeId = 1, 
            SectionType = "INVALID_TYPE", 
            Title = "My Experience" 
        };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => 
            await _sectionService.AddSectionAsync(userId, request));
    }
}
