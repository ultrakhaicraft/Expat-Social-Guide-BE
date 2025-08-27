using Infratructure.Models.Auth;
using Infratructure.Models.Common;
using Infratructure.Models.HR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface IHREmployeeService
    {
        Task<PagedResult<HREmployeeDto>> GetEmployeesAsync(
            int page, int pageSize, string search, string status, string department);
        Task<HREmployeeDto> GetEmployeeByIdAsync(Guid id);
        Task<HREmployeeDto> GetEmployeeByCodeAsync(string code);
        Task<ServiceResult> CreateEmployeeAsync(CreateHREmployeeModel model);
        Task<ServiceResult> UpdateEmployeeAsync(Guid id, UpdateHREmployeeModel model);
        Task<ServiceResult> DeleteEmployeeAsync(Guid id);
        Task<ServiceResult> ImportEmployeesFromExcelAsync(IFormFile file);
        Task<byte[]> ExportEmployeesToExcelAsync();
        Task<HRStatisticsDto> GetStatisticsAsync();
        Task<ServiceResult> ChangeEmployeeStatusAsync(Guid id, string status);
        Task<List<HREmployeeDto>> GetUnregisteredEmployeesAsync();
    }
}
