using Domain.Entities;
using Infratructure.Models.HR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infratructure.Models.Common;
namespace Infratructure.Interface
{
    public interface IHREmployeeRepository
    {
        // Basic CRUD
        Task<HREmployee> GetByIdAsync(Guid id);
        Task<HREmployee> GetByEmployeeCodeAsync(string employeeCode);
        Task<HREmployee> GetByEmailAsync(string email);
        Task<List<HREmployee>> GetAllAsync();
        Task<HREmployee> CreateAsync(HREmployee employee);
        Task UpdateAsync(HREmployee employee);
        Task DeleteAsync(Guid id);
        Task<bool> SaveChangesAsync();

        // Pagination & Search
        Task<PagedResult<HREmployeeDto>> GetPagedEmployeesAsync(
            int page,
            int pageSize,
            string search = null,
            string status = null,
            string department = null);

        // Validation
        Task<bool> IsEmployeeCodeExistsAsync(string employeeCode);
        Task<bool> IsEmailExistsAsync(string email);

        // Specific Queries
        Task<List<HREmployee>> GetUnregisteredEmployeesAsync();
        Task<List<HREmployee>> GetEmployeesByDepartmentAsync(string department);
        Task<List<HREmployee>> GetEmployeesByStatusAsync(string status);
        Task<List<HREmployee>> GetEmployeesWithExpiringContractsAsync(int daysAhead);

        // Statistics
        Task<int> CountTotalEmployeesAsync();
        Task<int> CountActiveEmployeesAsync();
        Task<int> CountRegisteredEmployeesAsync();
        Task<Dictionary<string, int>> GetEmployeeCountByDepartmentAsync();
        Task<Dictionary<string, int>> GetEmployeeCountByCampusAsync();

        // Batch Operations
        Task<List<HREmployee>> CreateBatchAsync(List<HREmployee> employees);
        Task UpdateBatchAsync(List<HREmployee> employees);
    }
}
