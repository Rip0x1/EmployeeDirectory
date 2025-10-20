using Microsoft.AspNetCore.Mvc;
using EmployeeDirectory.Services;
using EmployeeDirectory.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace EmployeeDirectory.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;

        public SearchController(IEmployeeService employeeService, IDepartmentService departmentService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<IActionResult> SearchEmployees([FromQuery] string? search, [FromQuery] int? departmentId, [FromQuery] string? positionSearch, [FromQuery] string? phoneSearch, [FromQuery] bool? isHeadOnly, [FromQuery] string? sortBy)
        {
            try
            {
                IEnumerable<Employee> employees;
                
                if (!string.IsNullOrEmpty(search))
                {
                    employees = await _employeeService.SearchEmployeesAsync(search);
                }
                else
                {
                    employees = await _employeeService.GetAllEmployeesAsync();
                }

                if (departmentId.HasValue)
                {
                    employees = employees.Where(e => e.DepartmentId == departmentId.Value);
                }

                if (!string.IsNullOrEmpty(positionSearch))
                {
                    employees = employees.Where(e => 
                        !string.IsNullOrEmpty(e.PositionDescription) && 
                        e.PositionDescription.Contains(positionSearch, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(phoneSearch))
                {
                    employees = employees.Where(e => 
                        (e.CityPhone != null && e.CityPhone.Contains(phoneSearch)) ||
                        (e.LocalPhone != null && e.LocalPhone.Contains(phoneSearch)));
                }
                
                if (isHeadOnly.HasValue)
                {
                    employees = employees.Where(e => e.IsHeadOfDepartment == isHeadOnly.Value);
                }

                if (!string.IsNullOrEmpty(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "name":
                            employees = employees
                                .OrderBy(e => !string.IsNullOrEmpty(e.DepartmentName) ? e.DepartmentName : e.Department.Name)
                                .ThenBy(e => e.IsHeadOfDepartment ? 0 : 1)
                                .ThenBy(e => e.FullName ?? string.Empty);
                            break;
                        case "department":
                            employees = employees
                                .OrderBy(e => !string.IsNullOrEmpty(e.DepartmentName) ? e.DepartmentName : e.Department.Name)
                                .ThenBy(e => e.IsHeadOfDepartment ? 0 : 1)
                                .ThenBy(e => e.FullName ?? string.Empty);
                            break;
                        case "position":
                            employees = employees
                                .OrderBy(e => !string.IsNullOrEmpty(e.DepartmentName) ? e.DepartmentName : e.Department.Name)
                                .ThenBy(e => e.IsHeadOfDepartment ? 0 : 1)
                                .ThenBy(e => e.PositionDescription ?? string.Empty)
                                .ThenBy(e => e.FullName ?? string.Empty);
                            break;
                        default:
                            employees = employees
                                .OrderBy(e => !string.IsNullOrEmpty(e.DepartmentName) ? e.DepartmentName : e.Department.Name)
                                .ThenBy(e => e.IsHeadOfDepartment ? 0 : 1)
                                .ThenBy(e => e.FullName ?? string.Empty);
                            break;
                    }
                }
                else
                {
                    employees = employees
                        .OrderBy(e => !string.IsNullOrEmpty(e.DepartmentName) ? e.DepartmentName : e.Department.Name)
                        .ThenBy(e => e.IsHeadOfDepartment ? 0 : 1)
                        .ThenBy(e => e.FullName ?? string.Empty);
                }
                var departments = await _departmentService.GetAllDepartmentsAsync();

                var result = employees.Select(e => new
                {
                    id = e.Id,
                    fullName = e.FullName ?? "-",
                    departmentName = !string.IsNullOrEmpty(e.DepartmentName) ? e.DepartmentName : e.Department.Name,
                    cityPhone = e.CityPhone ?? "Не указано",
                    localPhone = e.LocalPhone ?? "-",
                    isHeadOfDepartment = e.IsHeadOfDepartment,
                    positionDescription = e.PositionDescription
                }).ToList();

                return Ok(new { employees = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
