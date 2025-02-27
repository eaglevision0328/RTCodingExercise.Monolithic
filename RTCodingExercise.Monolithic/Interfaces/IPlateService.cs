using RTCodingExercise.Monolithic.Models;

namespace RTCodingExercise.Monolithic.Interfaces;

public interface IPlateService
{
    Task<IEnumerable<Plate>> GetAllPlatesAsync();
    Task<Plate> AddPlateAsync(Plate plate);
    Task UpdateReservationStatusAsync(Guid plateId, string isReserved);
    Task<IEnumerable<Plate>> GetAvailablePlates();
    Task<IQueryable<Plate>> GetFilteredAndSortedPlates(string sortOrder, string letterFilter, string numberFilter);
    PromoDetails GetPromoDetails(string promoCode);
}
