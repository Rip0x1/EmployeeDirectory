using EmployeeDirectory.Data;
using EmployeeDirectory.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDirectory.Services
{
    public class PositionService : IPositionService
    {
        private readonly ApplicationDbContext _context;

        public PositionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Position>> GetAllPositionsAsync()
        {
            return await _context.Positions
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<Position?> GetPositionByIdAsync(int id)
        {
            return await _context.Positions.FindAsync(id);
        }

        public async Task<Position> CreatePositionAsync(Position position)
        {
            position.CreatedAt = DateTime.UtcNow;
            position.UpdatedAt = DateTime.UtcNow;
            _context.Positions.Add(position);
            await _context.SaveChangesAsync();
            return position;
        }

        public async Task<Position> UpdatePositionAsync(Position position)
        {
            position.UpdatedAt = DateTime.UtcNow;
            _context.Positions.Update(position);
            await _context.SaveChangesAsync();
            return position;
        }

        public async Task DeletePositionAsync(int id)
        {
            var position = await _context.Positions.FindAsync(id);
            if (position != null)
            {
                _context.Positions.Remove(position);
                await _context.SaveChangesAsync();
            }
        }
    }
}
