using EmployeeDirectory.Models;

namespace EmployeeDirectory.Services
{
    public interface IPositionService
    {
        Task<IEnumerable<Position>> GetAllPositionsAsync();
        Task<Position?> GetPositionByIdAsync(int id);
        Task<Position> CreatePositionAsync(Position position);
        Task<Position> UpdatePositionAsync(Position position);
        Task DeletePositionAsync(int id);
    }
}
