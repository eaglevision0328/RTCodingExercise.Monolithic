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

    public async Task UpdateReservationStatusAsync(Guid plateId, string isReserved)
    {
        var plate = await _repository.GetByGuidIDAsync(plateId);
        if (plate == null)
            throw new ArgumentException("Plate not found");

        plate.PlateStatus = isReserved;

        //Logging to make sure info on when a plate is reserved or unreserved is saved
        _logger.LogInformation(
            "Plate {Registration} (ID: {PlateId}) reservation status changed to {Status} at {Timestamp}",
            plate.Registration,
            plateId,
            plate.PlateStatus,
            DateTime.UtcNow);

        await _repository.SaveChangesAsync();
    }

    public async Task<IQueryable<Plate>> GetFilteredAndSortedPlates(string sortOrder, string letterFilter, string numberFilter)
    {
        var plates = (await GetAvailablePlates()).AsQueryable();

        if (!string.IsNullOrEmpty(letterFilter))
        {
            plates = plates.Where(p => p.Letters.Contains(letterFilter));
        }

        if (!string.IsNullOrEmpty(numberFilter))
        {
            plates = plates.Where(p => p.Numbers.ToString().Contains(numberFilter));
        }

        plates = sortOrder switch
        {
            "price_desc" => plates.OrderByDescending(p => p.PurchasePrice),
            "sale_price" => plates.OrderBy(p => p.SalePrice),
            "sale_price_desc" => plates.OrderByDescending(p => p.SalePrice),
            _ => plates.OrderBy(p => p.PurchasePrice), // Default sort
        };

        return plates;
    }

    public PromoDetails GetPromoDetails(string promoCode)
    {
        const decimal MIN_PRICE_PERCENTAGE = 0.90m;
        
        decimal discount = promoCode?.ToUpper() switch
        {
            "DISCOUNT" => 25.0m,
            //"PERCENTOFF" => 0.15m,
            "PERCENTOFF" => Math.Min(0.15m, 1 - MIN_PRICE_PERCENTAGE), // I wasnt sure what this requirement meant so I have added this in to make sure the purchase price never goes below 90%
            _ => 0
        };

        bool isPercentDiscount = promoCode?.ToUpper() == "PERCENTOFF";

        return new PromoDetails
        {
            Discount = discount,
            IsPercentDiscount = isPercentDiscount
        };
    }
}