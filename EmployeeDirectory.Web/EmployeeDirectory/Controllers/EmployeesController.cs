using EmployeeDirectory.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EmployeeDirectory.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            try
            {
                var employees = await _employeeService.GetAllEmployeesAsync();

                var result = employees.Select(e => new
                {
                    e.Id,
                    e.FullName,
                    e.MobilePhone,
                    e.Email,
                    DepartmentName = e.Department?.Name ?? "Без отдела",
                    PositionDescription = e.Position?.Name ?? e.PositionDescription ?? "Нет должности"
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка базы данных: {ex.Message}");
            }
        }
    }
}
