using EmployeeDirectory.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeDirectory.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly IExportService _exportService;

        public ExportController(IExportService exportService)
        {
            _exportService = exportService;
        }

        [HttpPost("excel")]
        public async Task<IActionResult> ExportToExcel([FromBody] ExportRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Запрос не может быть пустым");
                }

                var data = await _exportService.ExportToExcelAsync(request.DepartmentIds);
                var fileName = $"Сотрудники_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx";
                
                return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при экспорте в Excel: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        [HttpPost("word")]
        public async Task<IActionResult> ExportToWord([FromBody] ExportRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Запрос не может быть пустым");
                }

                var data = await _exportService.ExportToWordAsync(request.DepartmentIds);
                var fileName = $"Сотрудники_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.docx";
                
                return File(data, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при экспорте в Word: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }
    }

    public class ExportRequest
    {
        public List<int>? DepartmentIds { get; set; }
    }
}
