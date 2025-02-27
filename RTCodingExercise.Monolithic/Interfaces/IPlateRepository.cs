using RTCodingExercise.Monolithic.Models;

namespace RTCodingExercise.Monolithic.Interfaces
{
    public interface IPlateRepository
    {
        Task<IEnumerable<Plate>> GetAllAsync();
        Task<Plate> GetByIdAsync(int id);
        Task<Plate> GetByGuidIDAsync(Guid id);
        Task<IEnumerable<Plate>> GetAvailablePlates();
        Task AddAsync(Plate plate);
        Task UpdateAsync(Plate plate);
        Task<bool> ExistsAsync(string registration);
        Task SaveChangesAsync();
        Task<IQueryable<Plate>> GetAvailablePlatesAsync();
    }
}
