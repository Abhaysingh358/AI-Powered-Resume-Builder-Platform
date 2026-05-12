using NUnit.Framework;
using ResumeAI.Export.Services;
using ResumeAI.Export.Models;
using System.Collections.Generic;

namespace ResumeAI.Export.API.Tests;

[TestFixture]
public class QuestPdfRendererTests
{
    private QuestPdfRenderer _renderer;

    [SetUp]
    public void Setup()
    {
        _renderer = new QuestPdfRenderer();
    }

    [Test]
    public void Render_ReturnsNonEmptyByteArray()
    {
        // Arrange
        var resume = new ResumeData
        {
            FullName = "John Doe",
            Email = "john@example.com",
            Sections = new List<SectionData>
            {
                new SectionData { Title = "Experience", Content = "Worked somewhere", IsVisible = true, DisplayOrder = 1 }
            }
        };

        // Act
        var result = _renderer.Render(resume);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.GreaterThan(0));
    }
}
