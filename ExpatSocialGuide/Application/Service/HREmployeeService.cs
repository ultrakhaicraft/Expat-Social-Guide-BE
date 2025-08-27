// Application/Service/HREmployeeService.cs
using Application.Interface;
using AutoMapper;
using Domain.Entities;
using Infratructure.Interface;
using Infratructure.Models.Auth;
using Infratructure.Models.Common;
using Infratructure.Models.HR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Service
{
    public class HREmployeeService : IHREmployeeService
    {
        private readonly IHREmployeeRepository _hrRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<HREmployeeService> _logger;

        public HREmployeeService(
            IHREmployeeRepository hrRepository,
            IMapper mapper,
            ILogger<HREmployeeService> logger)
        {
            _hrRepository = hrRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResult<HREmployeeDto>> GetEmployeesAsync(
            int page, int pageSize, string search, string status, string department)
        {
            return await _hrRepository.GetPagedEmployeesAsync(page, pageSize, search, status, department);
        }

        public async Task<HREmployeeDto> GetEmployeeByIdAsync(Guid id)
        {
            var employee = await _hrRepository.GetByIdAsync(id);
            return _mapper.Map<HREmployeeDto>(employee);
        }

        public async Task<HREmployeeDto> GetEmployeeByCodeAsync(string code)
        {
            var employee = await _hrRepository.GetByEmployeeCodeAsync(code);
            return _mapper.Map<HREmployeeDto>(employee);
        }

        public async Task<ServiceResult> CreateEmployeeAsync(CreateHREmployeeModel model)
        {
            try
            {
                // Validate duplicates
                if (await _hrRepository.IsEmployeeCodeExistsAsync(model.EmployeeCode))
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Mã nhân viên đã tồn tại"
                    };
                }

                if (await _hrRepository.IsEmailExistsAsync(model.CompanyEmail))
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Email đã được sử dụng"
                    };
                }

                var employee = _mapper.Map<HREmployee>(model);
                await _hrRepository.CreateAsync(employee);

                return new ServiceResult
                {
                    Success = true,
                    Message = "Tạo nhân viên thành công",
                    Data = _mapper.Map<HREmployeeDto>(employee)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating employee");
                return new ServiceResult
                {
                    Success = false,
                    Message = "Lỗi khi tạo nhân viên",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ServiceResult> UpdateEmployeeAsync(Guid id, UpdateHREmployeeModel model)
        {
            try
            {
                var employee = await _hrRepository.GetByIdAsync(id);
                if (employee == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Nhân viên không tồn tại"
                    };
                }

                _mapper.Map(model, employee);
                employee.UpdatedAt = DateTime.UtcNow;

                await _hrRepository.UpdateAsync(employee);

                return new ServiceResult
                {
                    Success = true,
                    Message = "Cập nhật thành công",
                    Data = _mapper.Map<HREmployeeDto>(employee)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee {Id}", id);
                return new ServiceResult
                {
                    Success = false,
                    Message = "Lỗi khi cập nhật",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ServiceResult> DeleteEmployeeAsync(Guid id)
        {
            try
            {
                var employee = await _hrRepository.GetByIdAsync(id);
                if (employee == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Nhân viên không tồn tại"
                    };
                }

                if (employee.IsRegistered)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Không thể xóa nhân viên đã có tài khoản"
                    };
                }

                await _hrRepository.DeleteAsync(id);

                return new ServiceResult
                {
                    Success = true,
                    Message = "Xóa nhân viên thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee {Id}", id);
                return new ServiceResult
                {
                    Success = false,
                    Message = "Lỗi khi xóa",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ServiceResult> ImportEmployeesFromExcelAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "File không hợp lệ"
                    };
                }

                var employees = new List<HREmployee>();
                var errors = new List<string>();

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                // EPPlus 8: Không cần LicenseContext, chỉ cần reset stream
                stream.Position = 0;

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets.First();
                var rowCount = worksheet.Dimension?.Rows ?? 0;

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var employeeCode = worksheet.Cells[row, 1].Value?.ToString()?.Trim();
                        var email = worksheet.Cells[row, 4].Value?.ToString()?.Trim();

                        if (string.IsNullOrEmpty(employeeCode))
                        {
                            errors.Add($"Dòng {row}: Thiếu mã nhân viên");
                            continue;
                        }

                        if (await _hrRepository.IsEmployeeCodeExistsAsync(employeeCode))
                        {
                            errors.Add($"Dòng {row}: Mã nhân viên {employeeCode} đã tồn tại");
                            continue;
                        }

                        if (!string.IsNullOrEmpty(email) && await _hrRepository.IsEmailExistsAsync(email))
                        {
                            errors.Add($"Dòng {row}: Email {email} đã tồn tại");
                            continue;
                        }

                        var employee = new HREmployee
                        {
                            EmployeeCode = employeeCode,
                            FirstName = worksheet.Cells[row, 2].Value?.ToString()?.Trim() ?? "",
                            LastName = worksheet.Cells[row, 3].Value?.ToString()?.Trim() ?? "",
                            CompanyEmail = email ?? $"{employeeCode.ToLower()}@fpt.edu.vn",
                            PersonalEmail = worksheet.Cells[row, 5].Value?.ToString()?.Trim(),
                            PhoneNumber = worksheet.Cells[row, 6].Value?.ToString()?.Trim(),
                            Department = worksheet.Cells[row, 7].Value?.ToString()?.Trim() ?? "Unknown",
                            Position = worksheet.Cells[row, 8].Value?.ToString()?.Trim(),
                            Campus = worksheet.Cells[row, 9].Value?.ToString()?.Trim(),
                            Building = worksheet.Cells[row, 10].Value?.ToString()?.Trim(),
                            Status = "Active"
                        };

                        if (DateTime.TryParse(worksheet.Cells[row, 11].Value?.ToString(), out var joinDate))
                            employee.JoinDate = joinDate;

                        if (DateTime.TryParse(worksheet.Cells[row, 12].Value?.ToString(), out var contractEnd))
                            employee.ContractEndDate = contractEnd;

                        employees.Add(employee);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Dòng {row}: {ex.Message}");
                    }
                }

                if (employees.Any())
                {
                    await _hrRepository.CreateBatchAsync(employees);
                }

                return new ServiceResult
                {
                    Success = employees.Any(),
                    Message = $"Import thành công {employees.Count} nhân viên",
                    Data = new { Imported = employees.Count, Errors = errors.Count },
                    Errors = errors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing employees from Excel");
                return new ServiceResult
                {
                    Success = false,
                    Message = "Lỗi khi import Excel",
                    Errors = new List<string> { ex.Message }
                };
            }
        }


        public async Task<byte[]> ExportEmployeesToExcelAsync()
        {
            try
            {
                var employees = await _hrRepository.GetAllAsync();

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Employees");

                // Headers
                worksheet.Cells[1, 1].Value = "Mã NV";
                worksheet.Cells[1, 2].Value = "Họ";
                worksheet.Cells[1, 3].Value = "Tên";
                worksheet.Cells[1, 4].Value = "Email Công Ty";
                worksheet.Cells[1, 5].Value = "Email Cá Nhân";
                worksheet.Cells[1, 6].Value = "Số Điện Thoại";
                worksheet.Cells[1, 7].Value = "Phòng Ban";
                worksheet.Cells[1, 8].Value = "Chức Vụ";
                worksheet.Cells[1, 9].Value = "Campus";
                worksheet.Cells[1, 10].Value = "Tòa Nhà";
                worksheet.Cells[1, 11].Value = "Ngày Vào";
                worksheet.Cells[1, 12].Value = "Ngày Hết HĐ";
                worksheet.Cells[1, 13].Value = "Trạng Thái";
                worksheet.Cells[1, 14].Value = "Đã Đăng Ký";

                using (var range = worksheet.Cells[1, 1, 1, 14])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                var row = 2;
                foreach (var emp in employees)
                {
                    worksheet.Cells[row, 1].Value = emp.EmployeeCode;
                    worksheet.Cells[row, 2].Value = emp.FirstName;
                    worksheet.Cells[row, 3].Value = emp.LastName;
                    worksheet.Cells[row, 4].Value = emp.CompanyEmail;
                    worksheet.Cells[row, 5].Value = emp.PersonalEmail;
                    worksheet.Cells[row, 6].Value = emp.PhoneNumber;
                    worksheet.Cells[row, 7].Value = emp.Department;
                    worksheet.Cells[row, 8].Value = emp.Position;
                    worksheet.Cells[row, 9].Value = emp.Campus;
                    worksheet.Cells[row, 10].Value = emp.Building;
                    worksheet.Cells[row, 11].Value = emp.JoinDate?.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 12].Value = emp.ContractEndDate?.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 13].Value = emp.Status;
                    worksheet.Cells[row, 14].Value = emp.IsRegistered ? "Có" : "Không";
                    row++;
                }

                worksheet.Cells.AutoFitColumns();
                return await package.GetAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting employees to Excel");
                throw;
            }
        }


        public async Task<HRStatisticsDto> GetStatisticsAsync()
        {
            try
            {
                var stats = new HRStatisticsDto
                {
                    TotalEmployees = await _hrRepository.CountTotalEmployeesAsync(),
                    ActiveEmployees = await _hrRepository.CountActiveEmployeesAsync(),
                    RegisteredUsers = await _hrRepository.CountRegisteredEmployeesAsync(),
                    ByDepartment = await _hrRepository.GetEmployeeCountByDepartmentAsync(),
                    ByCampus = await _hrRepository.GetEmployeeCountByCampusAsync()
                };

                stats.InactiveEmployees = stats.TotalEmployees - stats.ActiveEmployees;
                stats.UnregisteredUsers = stats.TotalEmployees - stats.RegisteredUsers;
                stats.VerifiedEmails = stats.RegisteredUsers; // Assuming registered = verified

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics");
                throw;
            }
        }

        public async Task<ServiceResult> ChangeEmployeeStatusAsync(Guid id, string status)
        {
            try
            {
                var employee = await _hrRepository.GetByIdAsync(id);
                if (employee == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Nhân viên không tồn tại"
                    };
                }

                employee.Status = status;
                employee.UpdatedAt = DateTime.UtcNow;
                await _hrRepository.UpdateAsync(employee);

                return new ServiceResult
                {
                    Success = true,
                    Message = $"Đã cập nhật trạng thái thành {status}",
                    Data = _mapper.Map<HREmployeeDto>(employee)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing employee status");
                return new ServiceResult
                {
                    Success = false,
                    Message = "Lỗi khi thay đổi trạng thái",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<List<HREmployeeDto>> GetUnregisteredEmployeesAsync()
        {
            var employees = await _hrRepository.GetUnregisteredEmployeesAsync();
            return _mapper.Map<List<HREmployeeDto>>(employees);
        }
    }
}