using Microsoft.AspNetCore.Mvc;
using EmployeeDirectory.Services;
using EmployeeDirectory.Models;
using EmployeeDirectory.Data;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDirectory.Controllers
{
    [Route("Pdf")]
    public class PdfController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly QuestPdfService _questPdfService;

        public PdfController(ApplicationDbContext context, QuestPdfService questPdfService)
        {
            _context = context;
            _questPdfService = questPdfService;
        }

        [HttpGet("EmployeeDirectory")]
        public async Task<IActionResult> EmployeeDirectory(string orientation = "portrait", string? departments = null)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .AsQueryable();

            if (!string.IsNullOrEmpty(departments))
            {
                var departmentIds = departments.Split(',').Select(int.Parse).ToList();
                query = query.Where(e => departmentIds.Contains(e.DepartmentId));
            }

            var employees = await query
                .OrderBy(e => e.DepartmentName)
                .ThenBy(e => e.FullName)
                .ToListAsync();


            var pdfBytes = _questPdfService.GenerateEmployeeDirectoryPdf(employees, orientation);

            return File(pdfBytes, "application/pdf", $"Справочник_сотрудников_{DateTime.Now:yyyy-MM-dd}.pdf");
        }

        [HttpGet("MyDepartment")]
        public async Task<IActionResult> MyDepartment(string orientation = "portrait")
        {
            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.UserName == currentUser);

            if (user?.Department == null)
            {
                return NotFound("Пользователь не привязан к отделу");
            }

            var employees = await _context.Employees
                .Where(e => e.DepartmentId == user.Department.Id)
                .OrderBy(e => e.FullName)
                .ToListAsync();


            var pdfBytes = _questPdfService.GenerateEmployeeDirectoryPdf(employees, orientation);

            return File(pdfBytes, "application/pdf", $"Мой_отдел_{user.Department.Name}_{DateTime.Now:yyyy-MM-dd}.pdf");
        }

        [HttpGet("ManageEmployees")]
        public async Task<IActionResult> ManageEmployees(string orientation = "portrait", string? departments = null)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .AsQueryable();

            if (!string.IsNullOrEmpty(departments))
            {
                var departmentIds = departments.Split(',').Select(int.Parse).ToList();
                query = query.Where(e => departmentIds.Contains(e.DepartmentId));
            }

            var employees = await query
                .OrderBy(e => e.DepartmentName)
                .ThenBy(e => e.FullName)
                .ToListAsync();

            var pdfBytes = _questPdfService.GenerateEmployeeDirectoryPdf(employees, orientation);

            return File(pdfBytes, "application/pdf", $"Управление_сотрудниками_{DateTime.Now:yyyy-MM-dd}.pdf");
        }

        [HttpGet("Logs")]
        public async Task<IActionResult> Logs(string orientation = "portrait", string? search = null, string? startDate = null, string? endDate = null)
        {
            var query = _context.Logs.AsQueryable();

            query = query.Where(l => l.Action != "SYSTEM_INIT");

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(l => l.Action.Contains(search) || l.UserName.Contains(search));
            }

            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
            {
                query = query.Where(l => l.TimestampUtc >= start);
            }

            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
            {
                query = query.Where(l => l.TimestampUtc <= end);
            }

            var logs = await query
                .OrderByDescending(l => l.TimestampUtc)
                .ToListAsync();

            var pdfBytes = _questPdfService.GenerateLogsPdf(logs, orientation);

            return File(pdfBytes, "application/pdf", $"Логи_системы_{DateTime.Now:yyyy-MM-dd}.pdf");
        }
    }
}
