using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using EmployeeDirectory.Data;
using EmployeeDirectory.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EmployeeDirectory.Services
{
    public interface IExportService
    {
        Task<byte[]> ExportToExcelAsync(List<int>? departmentIds = null);
        Task<byte[]> ExportToWordAsync(List<int>? departmentIds = null);
    }

    public class ExportService : IExportService
    {
        private readonly ApplicationDbContext _context;

        public ExportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<byte[]> ExportToExcelAsync(List<int>? departmentIds = null)
        {
            var employees = await GetEmployeesAsync(departmentIds);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Сотрудники");

            var currentRow = 1;

            var groupedEmployees = employees.GroupBy(e => e.Department?.GetDisplayName() ?? e.Department?.Name ?? "Неизвестный отдел")
                                          .OrderBy(g => g.Key);

            foreach (var departmentGroup in groupedEmployees)
            {
                if (currentRow > 1)
                {
                    currentRow += 1;
                }

                var departmentName = departmentGroup.Key;
                var departmentEmployees = departmentGroup.OrderByDescending(e => e.IsHeadOfDepartment)
                                                       .ThenByDescending(e => e.IsDeputy)
                                                       .ThenBy(e => e.FullName ?? "");

                worksheet.Cell(currentRow, 1).Value = departmentName;
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(currentRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Range(currentRow, 1, currentRow, 4).Merge();

                currentRow++;

                worksheet.Cell(currentRow, 1).Value = "Должность / ФИО";
                worksheet.Cell(currentRow, 2).Value = "Городской номер";
                worksheet.Cell(currentRow, 3).Value = "Внутренний номер";
                worksheet.Cell(currentRow, 4).Value = "Email";

                var headerRange = worksheet.Range(currentRow, 1, currentRow, 4);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                currentRow++;

                foreach (var emp in departmentEmployees)
                {
                    var positionText = !string.IsNullOrEmpty(emp.PositionDescription) 
                        ? emp.PositionDescription 
                        : emp.Position?.Name;
                    var fullName = emp.FullName;
                    
                    string positionAndName;
                    if (string.IsNullOrEmpty(fullName))
                    {
                        positionAndName = positionText ?? "Без должности";
                    }
                    else
                    {
                        positionAndName = !string.IsNullOrEmpty(positionText) 
                            ? $"{positionText} / {fullName}"
                            : fullName;
                    }
                    
                    worksheet.Cell(currentRow, 1).Value = positionAndName;
                    worksheet.Cell(currentRow, 2).Value = string.IsNullOrEmpty(emp.CityPhone) ? "Не указано" : emp.CityPhone;
                    worksheet.Cell(currentRow, 3).Value = string.IsNullOrEmpty(emp.LocalPhone) ? "Не указано" : emp.LocalPhone;
                    worksheet.Cell(currentRow, 4).Value = string.IsNullOrEmpty(emp.Email) ? "Не указано" : emp.Email;

                    var rowRange = worksheet.Range(currentRow, 1, currentRow, 4);
                    rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    currentRow++;
                }

                var tableRange = worksheet.Range(currentRow - departmentEmployees.Count(), 1, currentRow - 1, 4);
                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                tableRange.Style.Fill.BackgroundColor = XLColor.White;
            }

            worksheet.Columns().AdjustToContents();
            worksheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportToWordAsync(List<int>? departmentIds = null)
        {
            var employees = await GetEmployeesAsync(departmentIds);

            if (!employees.Any())
            {
                throw new InvalidOperationException("Нет сотрудников для экспорта");
            }


            using var stream = new MemoryStream();
            using var document = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
            
            var mainPart = document.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            var title = new Paragraph();
            var titleRun = new Run();
            titleRun.AppendChild(new Text("Справочник сотрудников"));
            var titleRunProperties = new RunProperties();
            titleRunProperties.AppendChild(new Bold());
            titleRunProperties.AppendChild(new FontSize() { Val = "24" });
            titleRunProperties.AppendChild(new RunFonts() { Ascii = "Calibri", HighAnsi = "Calibri" });
            titleRun.RunProperties = titleRunProperties;
            title.AppendChild(titleRun);
            
            var titleParagraphProperties = new ParagraphProperties();
            titleParagraphProperties.AppendChild(new Justification() { Val = JustificationValues.Center });
            title.ParagraphProperties = titleParagraphProperties;
            
            body.AppendChild(title);

            var groupedEmployees = employees.GroupBy(e => e.Department?.GetDisplayName() ?? e.Department?.Name ?? "Неизвестный отдел")
                                          .OrderBy(g => g.Key);


            if (!groupedEmployees.Any())
            {
                var noDataParagraph = new Paragraph();
                var noDataRun = new Run();
                noDataRun.AppendChild(new Text("Нет данных для экспорта"));
                noDataParagraph.AppendChild(noDataRun);
                body.AppendChild(noDataParagraph);
            }
            else
            {
                foreach (var departmentGroup in groupedEmployees)
                {
                    var departmentName = departmentGroup.Key;
                var departmentEmployees = departmentGroup.OrderByDescending(e => e.IsHeadOfDepartment)
                                                       .ThenByDescending(e => e.IsDeputy)
                                                       .ThenBy(e => e.FullName ?? "");

                var departmentTitle = new Paragraph();
                var departmentTitleRun = new Run();
                departmentTitleRun.AppendChild(new Text(departmentName));
                var departmentTitleRunProperties = new RunProperties();
                departmentTitleRunProperties.AppendChild(new Bold());
                departmentTitleRunProperties.AppendChild(new FontSize() { Val = "16" });
                departmentTitleRunProperties.AppendChild(new RunFonts() { Ascii = "Calibri", HighAnsi = "Calibri" });
                departmentTitleRun.RunProperties = departmentTitleRunProperties;
                departmentTitle.AppendChild(departmentTitleRun);
                
                var departmentTitleParagraphProperties = new ParagraphProperties();
                departmentTitleParagraphProperties.AppendChild(new Justification() { Val = JustificationValues.Center });
                departmentTitle.ParagraphProperties = departmentTitleParagraphProperties;
                
                body.AppendChild(departmentTitle);

                var table = new Table();
                var tableProperties = new TableProperties();
                var tableBorders = new TableBorders();
                tableBorders.AppendChild(new TopBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 });
                tableBorders.AppendChild(new BottomBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 });
                tableBorders.AppendChild(new LeftBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 });
                tableBorders.AppendChild(new RightBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 });
                tableBorders.AppendChild(new InsideHorizontalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 });
                tableBorders.AppendChild(new InsideVerticalBorder() { Val = new EnumValue<BorderValues>(BorderValues.Single), Size = 12 });
                tableProperties.AppendChild(tableBorders);
                table.AppendChild(tableProperties);

                var headerRow = new TableRow();
                string[] headers = { "Должность / ФИО", "Городской номер", "Внутренний номер", "Email" };
                
                foreach (var header in headers)
                {
                    var cell = new TableCell();
                    var paragraph = new Paragraph();
                    var run = new Run();
                    run.AppendChild(new Text(header));
                    var runProperties = new RunProperties();
                    runProperties.AppendChild(new Bold());
                    runProperties.AppendChild(new RunFonts() { Ascii = "Calibri", HighAnsi = "Calibri" });
                    run.RunProperties = runProperties;
                    paragraph.AppendChild(run);
                    cell.AppendChild(paragraph);
                    headerRow.AppendChild(cell);
                }
                table.AppendChild(headerRow);

                foreach (var emp in departmentEmployees)
                {
                    var row = new TableRow();
                    
                    var positionText = !string.IsNullOrEmpty(emp.PositionDescription) 
                        ? emp.PositionDescription 
                        : emp.Position?.Name;
                    var fullName = emp.FullName;
                    
                    string positionAndName;
                    if (string.IsNullOrEmpty(fullName))
                    {
                        positionAndName = positionText ?? "Без должности";
                    }
                    else
                    {
                        positionAndName = !string.IsNullOrEmpty(positionText) 
                            ? $"{positionText} / {fullName}"
                            : fullName;
                    }
                    
                    var cells = new[]
                    {
                        positionAndName,
                        string.IsNullOrEmpty(emp.CityPhone) ? "Не указано" : emp.CityPhone,
                        string.IsNullOrEmpty(emp.LocalPhone) ? "Не указано" : emp.LocalPhone,
                        string.IsNullOrEmpty(emp.Email) ? "Не указано" : emp.Email
                    };

                    foreach (var cellData in cells)
                    {
                        var cell = new TableCell();
                        var paragraph = new Paragraph();
                        var run = new Run();
                        run.AppendChild(new Text(cellData));
                        var runProperties = new RunProperties();
                        runProperties.AppendChild(new RunFonts() { Ascii = "Calibri", HighAnsi = "Calibri" });
                        run.RunProperties = runProperties;
                        paragraph.AppendChild(run);
                        cell.AppendChild(paragraph);
                        row.AppendChild(cell);
                    }
                    
                    table.AppendChild(row);
                }

                body.AppendChild(table);

                var spacing = new Paragraph();
                body.AppendChild(spacing);
                }
            }

            document.Save();
            document.Close();

            return stream.ToArray();
        }

        private async Task<List<Employee>> GetEmployeesAsync(List<int>? departmentIds = null)
        {
            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Position)
                .AsQueryable();

            if (departmentIds != null && departmentIds.Any())
            {
                query = query.Where(e => departmentIds.Contains(e.DepartmentId));
            }

            return await query.OrderBy(e => e.Department!.FullName ?? e.Department!.Name ?? "")
                              .ThenBy(e => e.FullName ?? "")
                              .ToListAsync();
        }
    }
}
