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
            [FromQuery] string? departmentFullNameSearch,
            [FromQuery] string? departmentShortNameSearch,
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
                        (e.LocalPhone != null && e.LocalPhone.Contains(phoneSearch)) ||
                        (e.MobilePhone != null && e.MobilePhone.Contains(phoneSearch)));
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
                        (e.Department != null && (
                            e.Department.Name.Contains(departmentSearch, StringComparison.OrdinalIgnoreCase) ||
                            (!string.IsNullOrEmpty(e.Department.FullName) && e.Department.FullName.Contains(departmentSearch, StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrEmpty(e.Department.ShortName) && e.Department.ShortName.Contains(departmentSearch, StringComparison.OrdinalIgnoreCase)) ||
                            e.Department.GetDisplayName().Contains(departmentSearch, StringComparison.OrdinalIgnoreCase)
                        )));
                }

                if (!string.IsNullOrEmpty(emailSearch))
                {
                    employees = employees.Where(e =>
                        !string.IsNullOrEmpty(e.Email) && e.Email.Contains(emailSearch, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(departmentFullNameSearch))
                {
                    employees = employees.Where(e =>
                        e.Department != null && 
                        !string.IsNullOrEmpty(e.Department.FullName) && 
                        e.Department.FullName.Contains(departmentFullNameSearch, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(departmentShortNameSearch))
                {
                    employees = employees.Where(e =>
                        e.Department != null && (
                            (!string.IsNullOrEmpty(e.Department.ShortName) && e.Department.ShortName.Contains(departmentShortNameSearch, StringComparison.OrdinalIgnoreCase)) ||
                            (!string.IsNullOrEmpty(e.Department.Name) && e.Department.Name.Contains(departmentShortNameSearch, StringComparison.OrdinalIgnoreCase))
                        ));
                }

                if (!string.IsNullOrEmpty(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "name":
                            employees = employees
                                .OrderBy(e => e.Department?.GetDisplayName() ?? e.Department?.Name ?? "Неизвестный отдел")
                                .ThenByDescending(e => e.IsHeadOfDepartment)
                                .ThenByDescending(e => e.IsDeputy)
                                .ThenBy(e => e.FullName ?? string.Empty);
                            break;
                        case "department":
                            employees = employees
                                .OrderBy(e => e.Department?.GetDisplayName() ?? e.Department?.Name ?? "Неизвестный отдел")
                                .ThenByDescending(e => e.IsHeadOfDepartment)
                                .ThenByDescending(e => e.IsDeputy)
                                .ThenBy(e => e.FullName ?? string.Empty);
                            break;
                        case "position":
                            employees = employees
                                .OrderBy(e => e.Department?.GetDisplayName() ?? e.Department?.Name ?? "Неизвестный отдел")
                                .ThenByDescending(e => e.IsHeadOfDepartment)
                                .ThenByDescending(e => e.IsDeputy)
                                .ThenBy(e => e.PositionDescription ?? string.Empty)
                                .ThenBy(e => e.FullName ?? string.Empty);
                            break;
                        default:
                            employees = employees
                                .OrderBy(e => e.Department?.GetDisplayName() ?? e.Department?.Name ?? "Неизвестный отдел")
                                .ThenByDescending(e => e.IsHeadOfDepartment)
                                .ThenByDescending(e => e.IsDeputy)
                                .ThenBy(e => e.FullName ?? string.Empty);
                            break;
                    }
                }
                else
                {
                    employees = employees
                        .OrderBy(e => e.Department?.GetDisplayName() ?? e.Department?.Name ?? "Неизвестный отдел")
                        .ThenByDescending(e => e.IsHeadOfDepartment)
                        .ThenByDescending(e => e.IsDeputy)
                        .ThenBy(e => e.FullName ?? string.Empty);
                }
                var allDepartments = await _departmentService.GetAllDepartmentsAsync();

                var result = employees.Select(e => new
                {
                    id = e.Id,
                    fullName = e.FullName ?? "-",
                    departmentName = e.Department?.GetDisplayName() ?? e.Department?.Name ?? "Неизвестный отдел",
                    cityPhone = e.CityPhone ?? "Не указано",
                    localPhone = e.LocalPhone ?? "Не указано",
                    mobilePhone = e.MobilePhone ?? "Не указано",
                    email = e.Email ?? "Не указано",
                    isHeadOfDepartment = e.IsHeadOfDepartment,
                    isDeputy = e.IsDeputy,
                    positionDescription = e.PositionDescription
                }).ToList();

                return Ok(new { employees = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();
                var result = departments.Select(d => new
                {
                    id = d.Id,
                    name = d.Name,
                    fullName = d.FullName,
                    shortName = d.ShortName
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}
