using Domain.Entities;
using Infratructure.Interface;
using Infratructure.Models.HR;
using Infratructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Infratructure.Models.Common;

namespace Infratructure.Repository
{
    public class HREmployeeRepository : IHREmployeeRepository
    {
        private readonly ESGDBContext _context;

        public HREmployeeRepository(ESGDBContext context)
        {
            _context = context;
        }

        // Basic CRUD
        public async Task<HREmployee> GetByIdAsync(Guid id)
        {
            return await _context.HREmployees
                .Include(h => h.User)
                .FirstOrDefaultAsync(h => h.Id == id);
        }

        public async Task<HREmployee> GetByEmployeeCodeAsync(string employeeCode)
        {
            return await _context.HREmployees
                .Include(h => h.User)
                .FirstOrDefaultAsync(h => h.EmployeeCode == employeeCode);
        }

        public async Task<HREmployee> GetByEmailAsync(string email)
        {
            return await _context.HREmployees
                .Include(h => h.User)
                .FirstOrDefaultAsync(h => h.CompanyEmail.ToUpper() == email.ToUpper());
        }

        public async Task<List<HREmployee>> GetAllAsync()
        {
            return await _context.HREmployees
                .Include(h => h.User)
                .OrderBy(h => h.EmployeeCode)
                .ToListAsync();
        }

        public async Task<HREmployee> CreateAsync(HREmployee employee)
        {
            _context.HREmployees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task UpdateAsync(HREmployee employee)
        {
            _context.HREmployees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var employee = await _context.HREmployees.FindAsync(id);
            if (employee != null)
            {
                employee.IsDeleted = true;
                employee.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        // Pagination & Search
        public async Task<Models.Common.PagedResult<HREmployeeDto>> GetPagedEmployeesAsync(
            int page,
            int pageSize,
            string search = null,
            string status = null,
            string department = null)
        {
            var query = _context.HREmployees.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(h =>
                    h.EmployeeCode.ToLower().Contains(search) ||
                    h.FirstName.ToLower().Contains(search) ||
                    h.LastName.ToLower().Contains(search) ||
                    h.CompanyEmail.ToLower().Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(h => h.Status == status);
            }

            if (!string.IsNullOrEmpty(department))
            {
                query = query.Where(h => h.Department == department);
            }

            // Count total
            var totalCount = await query.CountAsync();

            // Paginate and select
            var items = await query
                .OrderBy(h => h.EmployeeCode)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new HREmployeeDto
                {
                    Id = h.Id,
                    EmployeeCode = h.EmployeeCode,
                    FirstName = h.FirstName,
                    LastName = h.LastName,
                    CompanyEmail = h.CompanyEmail,
                    PersonalEmail = h.PersonalEmail,
                    PhoneNumber = h.PhoneNumber,
                    Department = h.Department,
                    Position = h.Position,
                    Campus = h.Campus,
                    Status = h.Status,
                    IsRegistered = h.IsRegistered,
                    RegisteredAt = h.RegisteredAt,
                    IsEmailVerified = h.IsEmailVerified,
                    JoinDate = h.JoinDate,
                    CreatedAt = h.CreatedAt
                })
                .ToListAsync();

            return new Models.Common.PagedResult<HREmployeeDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        // Validation
        public async Task<bool> IsEmployeeCodeExistsAsync(string employeeCode)
        {
            return await _context.HREmployees
                .AnyAsync(h => h.EmployeeCode == employeeCode);
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _context.HREmployees
                .AnyAsync(h => h.CompanyEmail.ToUpper() == email.ToUpper());
        }

        // Specific Queries
        public async Task<List<HREmployee>> GetUnregisteredEmployeesAsync()
        {
            return await _context.HREmployees
                .Where(h => !h.IsRegistered && h.Status == "Active")
                .OrderBy(h => h.EmployeeCode)
                .ToListAsync();
        }

        public async Task<List<HREmployee>> GetEmployeesByDepartmentAsync(string department)
        {
            return await _context.HREmployees
                .Where(h => h.Department == department)
                .OrderBy(h => h.EmployeeCode)
                .ToListAsync();
        }

        public async Task<List<HREmployee>> GetEmployeesByStatusAsync(string status)
        {
            return await _context.HREmployees
                .Where(h => h.Status == status)
                .OrderBy(h => h.EmployeeCode)
                .ToListAsync();
        }

        public async Task<List<HREmployee>> GetEmployeesWithExpiringContractsAsync(int daysAhead)
        {
            var targetDate = DateTime.UtcNow.AddDays(daysAhead);
            return await _context.HREmployees
                .Where(h => h.ContractEndDate != null &&
                           h.ContractEndDate <= targetDate &&
                           h.ContractEndDate >= DateTime.UtcNow)
                .OrderBy(h => h.ContractEndDate)
                .ToListAsync();
        }

        // Statistics
        public async Task<int> CountTotalEmployeesAsync()
        {
            return await _context.HREmployees.CountAsync();
        }

        public async Task<int> CountActiveEmployeesAsync()
        {
            return await _context.HREmployees
                .CountAsync(h => h.Status == "Active");
        }

        public async Task<int> CountRegisteredEmployeesAsync()
        {
            return await _context.HREmployees
                .CountAsync(h => h.IsRegistered);
        }

        public async Task<Dictionary<string, int>> GetEmployeeCountByDepartmentAsync()
        {
            return await _context.HREmployees
                .GroupBy(h => h.Department)
                .Select(g => new { Department = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Department, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetEmployeeCountByCampusAsync()
        {
            return await _context.HREmployees
                .Where(h => h.Campus != null)
                .GroupBy(h => h.Campus)
                .Select(g => new { Campus = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Campus, x => x.Count);
        }

        // Batch Operations
        public async Task<List<HREmployee>> CreateBatchAsync(List<HREmployee> employees)
        {
            _context.HREmployees.AddRange(employees);
            await _context.SaveChangesAsync();
            return employees;
        }

        public async Task UpdateBatchAsync(List<HREmployee> employees)
        {
            _context.HREmployees.UpdateRange(employees);
            await _context.SaveChangesAsync();
        }
    }
}