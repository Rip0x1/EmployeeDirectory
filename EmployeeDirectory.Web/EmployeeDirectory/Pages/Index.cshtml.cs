using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Services;
using EmployeeDirectory.Models;

namespace EmployeeDirectory.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IPositionService _positionService;

        public IndexModel(IEmployeeService employeeService, IDepartmentService departmentService, IPositionService positionService)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _positionService = positionService;
        }

        public IEnumerable<Employee> Employees { get; set; } = new List<Employee>();
        public IEnumerable<Department> Departments { get; set; } = new List<Department>();
        public IEnumerable<Position> Positions { get; set; } = new List<Position>();
        public string SearchTerm { get; set; } = string.Empty;
        public int? SelectedDepartmentId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 3;
        public int TotalPages { get; set; }
        public int TotalEmployees { get; set; }

        public string? SearchQuery => SearchTerm;
        public int? DepartmentId => SelectedDepartmentId;
        public string? PositionSearch { get; set; }
        public string? PhoneSearch { get; set; }
        public bool? IsHeadOnly { get; set; }
        public string? SortBy { get; set; }
        public int? DepartmentsPerPage { get; set; }

        public async Task OnGetAsync(string search, int? departmentId, int pageNumber = 1, int? departmentsPerPage = null)
        {
            SearchTerm = search ?? string.Empty;
            SelectedDepartmentId = departmentId;
            PageNumber = pageNumber;
            DepartmentsPerPage = departmentsPerPage;

            Departments = await _departmentService.GetAllDepartmentsAsync();
            Positions = await _positionService.GetAllPositionsAsync();

            IEnumerable<Employee> allEmployees;
            if (!string.IsNullOrEmpty(search))
            {
                allEmployees = await _employeeService.SearchEmployeesAsync(search);
            }
            else if (departmentId.HasValue)
            {
                allEmployees = await _employeeService.FilterEmployeesAsync(departmentId, null);
            }
            else
            {
                allEmployees = await _employeeService.GetAllEmployeesAsync();
            }

            TotalEmployees = allEmployees.Count();
            
            var groupedEmployees = allEmployees.GroupBy(e => e.Department.Name).OrderBy(g => g.Key).ToList();
            var totalDepartments = groupedEmployees.Count;
            
            // Если указан параметр departmentsPerPage, используем его для пагинации
            if (departmentsPerPage.HasValue && departmentsPerPage.Value > 0)
            {
                PageSize = departmentsPerPage.Value;
                TotalPages = (int)Math.Ceiling((double)totalDepartments / PageSize);
                
                var departmentsForPage = groupedEmployees
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize);

                Employees = departmentsForPage.SelectMany(g => g);
            }
            // Если нет параметров поиска и фильтрации, показываем все отделы
            else if (string.IsNullOrEmpty(search) && !departmentId.HasValue)
            {
                Employees = allEmployees;
                TotalPages = 1;
            }
            else
            {
                var maxEmployeesPerPage = 50;
                if (TotalEmployees > maxEmployeesPerPage)
                {
                    TotalPages = (int)Math.Ceiling((double)TotalEmployees / maxEmployeesPerPage);
                }
                else
                {
                    TotalPages = (int)Math.Ceiling((double)totalDepartments / PageSize);
                }
                
                var departmentsForPage = groupedEmployees
                    .Skip((PageNumber - 1) * PageSize)
                    .Take(PageSize);

                Employees = departmentsForPage.SelectMany(g => g);
            }
        }
    }
}
