using RTCodingExercise.Monolithic.Interfaces;
using RTCodingExercise.Monolithic.Models;

namespace RTCodingExercise.Monolithic.Repository;
public class PlateRepository : IPlateRepository
{
    private readonly ApplicationDbContext _context;

    public PlateRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Plate>> GetAllAsync()
    {
        return await _context.Plates.ToListAsync();
    }

    public async Task<Plate> GetByIdAsync(int id)
    {
        return await _context.Plates.FindAsync(id);
    }

    public async Task<IEnumerable<Plate>> GetAvailablePlates() {
        return await _context.Plates.Where(p => p.PlateStatus == "Available").ToListAsync();
    }

    public async Task<Plate> GetByGuidIDAsync(Guid id)
    {
        return await _context.Plates.FindAsync(id);
    }


    public async Task AddAsync(Plate plate)
    {
        await _context.Plates.AddAsync(plate);
    }

    public Task UpdateAsync(Plate plate)
    {
        _context.Entry(plate).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(string registration)
    {
        return await _context.Plates.AnyAsync(p => p.Registration == registration);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<IQueryable<Plate>> GetAvailablePlatesAsync()
    {
        return _context.Plates
            .Where(p => p.PlateStatus == "Available")
            .AsQueryable();
    }
}