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
        public async Task<IActionResult> SearchEmployees(
            [FromQuery] string? search,
            [FromQuery] string[]? selectedEmployees,
            [FromQuery] int[]? departments,
            [FromQuery] string[]? positions,
            [FromQuery] string? phoneSearch,
            [FromQuery] string? positionSearch,
            [FromQuery] string? emailSearch,
            [FromQuery] string? departmentSearch,
            [FromQuery] string? sortBy)
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

                if (selectedEmployees != null && selectedEmployees.Length > 0)
                {
                    employees = employees.Where(e => 
                        !string.IsNullOrEmpty(e.FullName) && 
                        selectedEmployees.Contains(e.FullName));
                }

                if (departments != null && departments.Length > 0)
                {
                    employees = employees.Where(e => departments.Contains(e.DepartmentId));
                }

                if (positions != null && positions.Length > 0)
                {
                    employees = employees.Where(e => 
                        !string.IsNullOrEmpty(e.PositionDescription) && 
                        positions.Contains(e.PositionDescription));
                }

                if (!string.IsNullOrEmpty(phoneSearch))
                {
                    employees = employees.Where(e => 
                        (e.CityPhone != null && e.CityPhone.Contains(phoneSearch)) ||
                        (e.LocalPhone != null && e.LocalPhone.Contains(phoneSearch)));
                }
                
                if (!string.IsNullOrEmpty(positionSearch))
                {
                    employees = employees.Where(e => 
                        (e.PositionDescription != null && e.PositionDescription.Contains(positionSearch, StringComparison.OrdinalIgnoreCase)) ||
                        (e.Position != null && e.Position.Name.Contains(positionSearch, StringComparison.OrdinalIgnoreCase)));
                }

                if (!string.IsNullOrEmpty(departmentSearch))
                {
                    employees = employees.Where(e =>
                        (!string.IsNullOrEmpty(e.DepartmentName) && e.DepartmentName.Contains(departmentSearch, StringComparison.OrdinalIgnoreCase)) ||
                        (e.Department != null && e.Department.Name.Contains(departmentSearch, StringComparison.OrdinalIgnoreCase)));
                }

                if (!string.IsNullOrEmpty(emailSearch))
                {
                    employees = employees.Where(e =>
                        !string.IsNullOrEmpty(e.Email) && e.Email.Contains(emailSearch, StringComparison.OrdinalIgnoreCase));
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
                var allDepartments = await _departmentService.GetAllDepartmentsAsync();

                var result = employees.Select(e => new
                {
                    id = e.Id,
                    fullName = e.FullName ?? "-",
                    departmentName = !string.IsNullOrEmpty(e.DepartmentName) ? e.DepartmentName : e.Department.Name,
                    cityPhone = e.CityPhone ?? "Не указано",
                    localPhone = e.LocalPhone ?? "Не указано",
                    email = e.Email ?? "Не указано",
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
