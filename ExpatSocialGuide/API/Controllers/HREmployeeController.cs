using API.Response;
using Application.Interface;
using Infratructure.Models.HR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class HREmployeeController : ControllerBase
    {
        private readonly IHREmployeeService _hrService;

        public HREmployeeController(IHREmployeeService hrService)
        {
            _hrService = hrService;
        }

        // GET: api/hremployee
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? status = null,
            [FromQuery] string? department = null)
        {
            var result = await _hrService.GetEmployeesAsync(page, pageSize, search, status, department);
            return Ok(ApiResponse<object>.SuccessResult(result));
        }

        // GET: api/hremployee/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployee(Guid id)
        {
            var result = await _hrService.GetEmployeeByIdAsync(id);
            if (result == null)
                return NotFound(ApiResponse<object>.FailResult("Employee not found"));

            return Ok(ApiResponse<object>.SuccessResult(result));
        }

        // GET: api/hremployee/code/{employeeCode}
        [HttpGet("code/{employeeCode}")]
        public async Task<IActionResult> GetEmployeeByCode(string employeeCode)
        {
            var result = await _hrService.GetEmployeeByCodeAsync(employeeCode);
            if (result == null)
                return NotFound(ApiResponse<object>.FailResult("Employee not found"));

            return Ok(ApiResponse<object>.SuccessResult(result));
        }

        // POST: api/hremployee
        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateHREmployeeModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailResult("Invalid model", ModelState));

            var result = await _hrService.CreateEmployeeAsync(model);
            if (result.Success)
                return Ok(ApiResponse<object>.SuccessResult(result.Data, result.Message));

            return BadRequest(ApiResponse<object>.FailResult(result.Message));
        }

        // PUT: api/hremployee/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] UpdateHREmployeeModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.FailResult("Invalid model", ModelState));

            var result = await _hrService.UpdateEmployeeAsync(id, model);
            if (result.Success)
                return Ok(ApiResponse<object>.SuccessResult(result.Data, result.Message));

            return BadRequest(ApiResponse<object>.FailResult(result.Message));
        }

        // DELETE: api/hremployee/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            var result = await _hrService.DeleteEmployeeAsync(id);
            if (result.Success)
                return Ok(ApiResponse<object>.SuccessResult(null, result.Message));

            return BadRequest(ApiResponse<object>.FailResult(result.Message));
        }

        // POST: api/hremployee/import
        [HttpPost("import")]
        public async Task<IActionResult> ImportEmployees([FromForm] ImportHREmployeeModel model)
        {
            if (model.File == null || model.File.Length == 0)
                return BadRequest(ApiResponse<object>.FailResult("File is required"));

            var result = await _hrService.ImportEmployeesFromExcelAsync(model.File);
            if (result.Success)
                return Ok(ApiResponse<object>.SuccessResult(result.Data, result.Message));

            return BadRequest(ApiResponse<object>.FailResult(result.Message, result.Errors));
        }

        // GET: api/hremployee/export
        [HttpGet("export")]
        public async Task<IActionResult> ExportEmployees()
        {
            var fileBytes = await _hrService.ExportEmployeesToExcelAsync();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"HREmployees_{DateTime.Now:yyyyMMdd}.xlsx");
        }

        // GET: api/hremployee/statistics
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var stats = await _hrService.GetStatisticsAsync();
            return Ok(ApiResponse<object>.SuccessResult(stats));
        }

        // POST: api/hremployee/{id}/activate
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateEmployee(Guid id)
        {
            var result = await _hrService.ChangeEmployeeStatusAsync(id, "Active");
            if (result.Success)
                return Ok(ApiResponse<object>.SuccessResult(null, result.Message));

            return BadRequest(ApiResponse<object>.FailResult(result.Message));
        }

        // POST: api/hremployee/{id}/deactivate
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> DeactivateEmployee(Guid id)
        {
            var result = await _hrService.ChangeEmployeeStatusAsync(id, "Inactive");
            if (result.Success)
                return Ok(ApiResponse<object>.SuccessResult(null, result.Message));

            return BadRequest(ApiResponse<object>.FailResult(result.Message));
        }

        // GET: api/hremployee/unregistered
        [HttpGet("unregistered")]
        public async Task<IActionResult> GetUnregisteredEmployees()
        {
            var result = await _hrService.GetUnregisteredEmployeesAsync();
            return Ok(ApiResponse<object>.SuccessResult(result));
        }
    }
}