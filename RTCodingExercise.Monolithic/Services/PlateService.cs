using AutoMapper;
using RTCodingExercise.Monolithic.Interfaces;
using RTCodingExercise.Monolithic.Models;

namespace RTCodingExercise.Monolithic.Services;

public class PlateService : IPlateService
{
    private readonly IPlateRepository _repository;
    private readonly ILogger<PlateService> _logger;
    private readonly decimal markupAmount = 1.2m;
    public PlateService(
        IPlateRepository repository,
        ILogger<PlateService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Plate>> GetAllPlatesAsync()
    {
        var plates = await _repository.GetAllAsync();
        return plates;
    }

    public async Task<IEnumerable<Plate>> GetAvailablePlates()
    {
        return await _repository.GetAvailablePlatesAsync();
    }

    public async Task<Plate> AddPlateAsync(Plate plate)
    {
        if(await _repository.ExistsAsync(plate.Registration))
        {
            throw new InvalidOperationException($"Plate with registration {plate.Registration} already exists");
        }

        plate.SalePrice = CalculateMarkup(plate.SalePrice, markupAmount);

        await _repository.AddAsync(plate);
        await _repository.SaveChangesAsync();

        return plate;
    }

    public decimal CalculateMarkup(decimal salePrice, decimal markupAmount)
    {
        return salePrice * markupAmount;
    }
}