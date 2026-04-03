namespace BookLoans.UnitTests.Services;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BookLoans.Abstractions.Models;
using BookLoans.Abstractions.Interfaces;
using BookLoans.Services;
using Moq;
using Xunit;

public class PublicHomepageQueryServiceTests
{
    [Fact]
    public async Task GetAsync_CallsRepositoryGetAsync()
    {
        var mockRepository = new Mock<IPublicHomepageRepository>();
        var expectedGroups = new List<BorrowerCheckoutGroup>();
        mockRepository
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedGroups);
        var service = new PublicHomepageQueryService(mockRepository.Object);

        var result = await service.GetAsync(CancellationToken.None);

        mockRepository.Verify(r => r.GetAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.Same(expectedGroups, result);
    }

    [Fact]
    public async Task GetAsync_ReturnsRepositoryResult()
    {
        var mockRepository = new Mock<IPublicHomepageRepository>();
        var expectedGroups = new List<BorrowerCheckoutGroup>
        {
            new()
            {
                BorrowerFullName = "John Doe",
                Books = new List<BookLoan>()
            }
        };
        mockRepository
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedGroups);
        var service = new PublicHomepageQueryService(mockRepository.Object);

        var result = await service.GetAsync(CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("John Doe", result[0].BorrowerFullName);
    }

    [Fact]
    public async Task GetAsync_PassesCancellationTokenToRepository()
    {
        var mockRepository = new Mock<IPublicHomepageRepository>();
        mockRepository
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BorrowerCheckoutGroup>());
        var service = new PublicHomepageQueryService(mockRepository.Object);
        var ct = new CancellationToken();

        await service.GetAsync(ct);

        mockRepository.Verify(r => r.GetAsync(ct), Times.Once);
    }

    [Fact]
    public async Task GetAsync_WithEmptyRepository_ReturnsEmptyList()
    {
        var mockRepository = new Mock<IPublicHomepageRepository>();
        mockRepository
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<BorrowerCheckoutGroup>());
        var service = new PublicHomepageQueryService(mockRepository.Object);

        var result = await service.GetAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAsync_WithMultipleGroups_ReturnsAllGroups()
    {
        var mockRepository = new Mock<IPublicHomepageRepository>();
        var expectedGroups = new List<BorrowerCheckoutGroup>
        {
            new() { BorrowerFullName = "Alice" },
            new() { BorrowerFullName = "Bob" },
            new() { BorrowerFullName = "Charlie" }
        };
        mockRepository
            .Setup(r => r.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedGroups);
        var service = new PublicHomepageQueryService(mockRepository.Object);

        var result = await service.GetAsync(CancellationToken.None);

        Assert.Equal(3, result.Count);
    }
}
