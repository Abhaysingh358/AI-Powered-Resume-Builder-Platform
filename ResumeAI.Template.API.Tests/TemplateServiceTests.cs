using NUnit.Framework;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using ResumeAI.Template.Services;
using ResumeAI.Template.Repositories.Interfaces;
using ResumeAI.Template.DTOs.Request;
using ResumeAI.Template.DTOs.Response;
using ResumeAI.Template.Entities;
using ResumeAI.Template.Enums;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace ResumeAI.Template.API.Tests;

[TestFixture]
public class TemplateServiceTests
{
    private Mock<ITemplateRepository> _templateRepoMock;
    private Mock<IMapper> _mapperMock;
    private Mock<ILogger<TemplateService>> _loggerMock;
    private TemplateService _templateService;

    [SetUp]
    public void Setup()
    {
        _templateRepoMock = new Mock<ITemplateRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<TemplateService>>();

        _templateService = new TemplateService(_templateRepoMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task CreateTemplateAsync_WhenValidRequest_CreatesTemplate()
    {
        // Arrange
        var request = new CreateTemplateRequest 
        { 
            Name = "My Template",
            Category = "PROFESSIONAL"
        };
        
        _templateRepoMock.Setup(r => r.CreateAsync(It.IsAny<ResumeTemplate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResumeTemplate { TemplateId = 1, Name = "My Template" });

        _mapperMock.Setup(m => m.Map<TemplateResponse>(It.IsAny<ResumeTemplate>()))
            .Returns(new TemplateResponse { TemplateId = 1, Name = "My Template" });

        // Act
        var result = await _templateService.CreateTemplateAsync(request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.TemplateId, Is.EqualTo(1));
        _templateRepoMock.Verify(r => r.CreateAsync(It.IsAny<ResumeTemplate>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void CreateTemplateAsync_WhenInvalidCategory_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateTemplateRequest 
        { 
            Name = "My Template",
            Category = "INVALID_CATEGORY"
        };

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(async () => 
            await _templateService.CreateTemplateAsync(request));
    }
}
