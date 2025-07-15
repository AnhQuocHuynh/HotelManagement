using HotelManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HotelManager.Core.Interfaces
{
    public interface IMaintenanceRepository
    {
        Task AddAsync(Maintenance entity);
        Task<IEnumerable<Maintenance>> GetAllAsync();
        Task<Maintenance?> GetByIdAsync(int id);
        Task<bool> DeleteAsync(int id);
        Task UpdateAsync(Maintenance entity);
        Task<bool> AnyAsync(Expression<Func<Maintenance, bool>> predicate);
        Task SaveChangesAsync(); // Thêm để gọi commit dữ liệu
    }
}
