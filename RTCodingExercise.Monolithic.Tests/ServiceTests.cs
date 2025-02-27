using Moq;
using Xunit;
using RTCodingExercise.Monolithic.Services;
using RTCodingExercise.Monolithic.Interfaces;
using RTCodingExercise.Monolithic.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace RTCodingExercise.Tests.Services;

public class PlateServiceTests
{
    private readonly Mock<IPlateRepository> _mockRepository;
    private readonly Mock<ILogger<PlateService>> _mockLogger;
    private readonly PlateService _service;

    public PlateServiceTests()
    {
        _mockRepository = new Mock<IPlateRepository>();
        _mockLogger = new Mock<ILogger<PlateService>>();
        _service = new PlateService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllPlatesAsync_ReturnsAllPlates()
    {
        // Arrange
        var expectedPlates = new List<Plate>
        {
            new() { Id = Guid.NewGuid(), Registration = "ABC123" },
            new() { Id = Guid.NewGuid(), Registration = "XYZ789" }
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedPlates);

        // Act
        var result = await _service.GetAllPlatesAsync();

        // Assert
        Assert.Equal(expectedPlates, result);
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task AddPlateAsync_WhenPlateDoesNotExist_AddsPlate()
    {
        // Arrange
        var plate = new Plate
        {
            Registration = "ABC123",
            SalePrice = 100m
        };
        _mockRepository.Setup(r => r.ExistsAsync(plate.Registration)).ReturnsAsync(false);

        // Act
        var result = await _service.AddPlateAsync(plate);

        // Assert
        Assert.Equal(120m, result.SalePrice); // Checks markup calculation
        _mockRepository.Verify(r => r.AddAsync(plate), Times.Once);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddPlateAsync_WhenPlateExists_ThrowsException()
    {
        // Arrange
        var plate = new Plate { Registration = "ABC123" };
        _mockRepository.Setup(r => r.ExistsAsync(plate.Registration)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AddPlateAsync(plate));
    }

    [Theory]
    [InlineData("DISCOUNT", 25.0, false)]
    [InlineData("PERCENTOFF", 0.10, true)]
    [InlineData("INVALID", 0, false)]
    public void GetPromoDetails_ReturnsCorrectDiscounts(string promoCode, decimal expectedDiscount, bool expectedIsPercent)
    {
        // Act
        var result = _service.GetPromoDetails(promoCode);

        // Assert
        Assert.Equal(expectedDiscount, result.Discount);
        Assert.Equal(expectedIsPercent, result.IsPercentDiscount);
    }

    [Fact]
    public async Task UpdateReservationStatusAsync_WhenPlateExists_UpdatesStatus()
    {
        // Arrange
        var plateId = Guid.NewGuid();
        var plate = new Plate { Id = plateId, Registration = "ABC123" };
        _mockRepository.Setup(r => r.GetByGuidIDAsync(plateId)).ReturnsAsync(plate);

        // Act
        await _service.UpdateReservationStatusAsync(plateId, "Reserved");

        // Assert
        Assert.Equal("Reserved", plate.PlateStatus);
        _mockRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateReservationStatusAsync_WhenPlateDoesNotExist_ThrowsException()
    {
        // Arrange
        var plateId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByGuidIDAsync(plateId)).ReturnsAsync((Plate)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.UpdateReservationStatusAsync(plateId, "Reserved"));
    }

    [Theory]
    [InlineData(100, 1.2, 120)]
    [InlineData(0, 1.2, 0)]
    [InlineData(1000, 1.5, 1500)]
    [InlineData(99.99, 1.2, 119.988)]
    public void CalculateMarkup_ReturnsCorrectAmount(decimal salePrice, decimal markup, decimal expected)
    {
        // Act
        var result = _service.CalculateMarkup(salePrice, markup);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task GetAvailablePlates_ReturnsEmptyList_WhenNoAvailablePlates()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAvailablePlates()).ReturnsAsync(new List<Plate>());

        // Act
        var result = await _service.GetAvailablePlates();

        // Assert
        Assert.Empty(result);
        _mockRepository.Verify(r => r.GetAvailablePlatesAsync(), Times.Once);
    }
}