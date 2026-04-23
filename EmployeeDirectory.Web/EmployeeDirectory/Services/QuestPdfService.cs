using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EmployeeDirectory.Services
{
    public class QuestPdfService
    {
        public QuestPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerateEmployeeDirectoryPdf(List<EmployeeDirectory.Models.Employee> employees, string orientation = "portrait")
        {
            var groupedEmployees = employees
                .Where(e => e != null)
                .GroupBy(e => e.Department?.GetDisplayName() ?? "Без отдела")
                .OrderBy(g => g.Key)
                .ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(orientation == "landscape" ? PageSizes.A4.Landscape() : PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Content()
                        .Element(container =>
                        {
                            if (orientation == "landscape")
                            {
                                var departmentsList = groupedEmployees.ToList();
                                var leftDepartments = new List<IGrouping<string, EmployeeDirectory.Models.Employee>>();
                                var rightDepartments = new List<IGrouping<string, EmployeeDirectory.Models.Employee>>();

                                for (int i = 0; i < departmentsList.Count; i++)
                                {
                                    if (i % 2 == 0)
                                        leftDepartments.Add(departmentsList[i]);
                                    else
                                        rightDepartments.Add(departmentsList[i]);
                                }

                                container.Row(row =>
                                {
                                    row.RelativeItem().Column(leftColumn =>
                                    {
                                        foreach (var departmentGroup in leftDepartments)
                                        {
                                            leftColumn.Item().Element(depContainer => CreateDepartmentTable(depContainer, departmentGroup));
                                            leftColumn.Item().PaddingBottom(5);
                                        }
                                    });

                                    row.RelativeItem().Column(rightColumn =>
                                    {
                                        foreach (var departmentGroup in rightDepartments)
                                        {
                                            rightColumn.Item().Element(depContainer => CreateDepartmentTable(depContainer, departmentGroup));
                                            rightColumn.Item().PaddingBottom(5);
                                        }
                                    });
                                });
                            }
                            else
                            {
                                container.Column(column =>
                                {
                                    foreach (var departmentGroup in groupedEmployees)
                                    {
                                        column.Item().Element(depContainer => CreateDepartmentTable(depContainer, departmentGroup));
                                        column.Item().PaddingBottom(10);
                                    }
                                });
                            }
                        });
                });
            });

            return document.GeneratePdf();
        }

        private void CreateDepartmentTable(IContainer container, IGrouping<string, EmployeeDirectory.Models.Employee> departmentGroup)
        {
            var departmentName = departmentGroup.Key;
            var employees = departmentGroup
                .OrderByDescending(e => e.IsHeadOfDepartment ? 1 : 0)
                .ThenByDescending(e => e.IsDeputy ? 1 : 0)
                .ThenBy(e => e.FullName ?? "")
                .ToList();

            container.Border(1).BorderColor(Colors.Black).Padding(3).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1.5f);
                });

                table.Header(header =>
                {
                    header.Cell().ColumnSpan(5)
                        .Border(1).BorderColor(Colors.Black)
                        .Background(Colors.Grey.Lighten3)
                        .Padding(4)
                        .Text(departmentName)
                        .FontSize(12)
                        .Bold()
                        .FontColor(Colors.Black)
                        .AlignCenter();

                    header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten2).Padding(2)
                        .Text("Должность/ФИО").FontSize(9).Bold().FontColor(Colors.Black).AlignCenter();
                    header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten2).Padding(2)
                        .Text("Городской").FontSize(9).Bold().FontColor(Colors.Black).AlignCenter();
                    header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten2).Padding(2)
                        .Text("Внутренний").FontSize(9).Bold().FontColor(Colors.Black).AlignCenter();
                    header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten2).Padding(2)
                        .Text("Мобильный").FontSize(9).Bold().FontColor(Colors.Black).AlignCenter();
                    header.Cell().Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten2).Padding(2)
                        .Text("Email").FontSize(9).Bold().FontColor(Colors.Black).AlignCenter();
                });

                foreach (var employee in employees)
                {
                    var positionText = !string.IsNullOrEmpty(employee.PositionDescription) ? employee.PositionDescription : "";
                    var nameText = !string.IsNullOrEmpty(employee.FullName) ? employee.FullName : "";
                    
                    string combinedText;
                    if (!string.IsNullOrEmpty(positionText) && !string.IsNullOrEmpty(nameText))
                    {
                        combinedText = $"{positionText}\n{nameText}";
                    }
                    else if (!string.IsNullOrEmpty(positionText))
                    {
                        combinedText = positionText;
                    }
                    else if (!string.IsNullOrEmpty(nameText))
                    {
                        combinedText = nameText;
                    }
                    else
                    {
                        combinedText = "Без данных";
                    }
                    
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2)
                        .Text(combinedText).FontSize(8).FontColor(Colors.Black).AlignCenter();

                    var cityPhone = !string.IsNullOrEmpty(employee.CityPhone) ? employee.CityPhone : "Не указано";
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2)
                        .Text(cityPhone).FontSize(8).FontColor(Colors.Black).AlignCenter();

                    var localPhone = !string.IsNullOrEmpty(employee.LocalPhone) ? employee.LocalPhone : "Не указано";
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2)
                        .Text(localPhone).FontSize(8).FontColor(Colors.Black).AlignCenter();

                    var mobile = !string.IsNullOrEmpty(employee.MobilePhone) ? employee.MobilePhone : "Не указано";
                    var email = !string.IsNullOrEmpty(employee.Email) ? employee.Email : "Не указано";
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2)
                        .Text(mobile).FontSize(8).FontColor(Colors.Black).AlignCenter();
                    table.Cell().Border(1).BorderColor(Colors.Black).Padding(2)
                        .Text(email).FontSize(8).FontColor(Colors.Black).AlignCenter();
                }
            });
        }

        public byte[] GenerateLogsPdf(List<EmployeeDirectory.Models.LogEntry> logs, string orientation = "portrait")
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(orientation == "landscape" ? PageSizes.A4.Landscape() : PageSizes.A4);
                    page.Margin(20);

                    page.Header()
                        .Text("Логи системы")
                        .FontSize(16)
                        .FontColor(Colors.Black)
                        .Bold()
                        .AlignCenter();

                    page.Content()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(2f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text("Дата/Время").FontSize(9).Bold().FontColor(Colors.Black).AlignCenter();
                                header.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text("Пользователь").FontSize(9).Bold().FontColor(Colors.Black).AlignCenter();
                                header.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text("Действие").FontSize(9).Bold().FontColor(Colors.Black).AlignCenter();
                                header.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text("IP-адрес").FontSize(9).Bold().FontColor(Colors.Black).AlignCenter();
                                header.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text("Сущность").FontSize(9).Bold().FontColor(Colors.Black).AlignCenter();
                                header.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text("Детали").FontSize(9).Bold().FontColor(Colors.Black).AlignCenter();
                            });

                            foreach (var log in logs)
                            {
                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text(TimeZoneInfo.ConvertTimeFromUtc(log.TimestampUtc, TimeZoneInfo.Local).ToString("dd.MM.yyyy HH:mm:ss")).FontSize(7).FontColor(Colors.Black).AlignCenter();

                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text(log.UserName ?? "Система").FontSize(7).FontColor(Colors.Black).AlignCenter();

                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text(log.Action ?? "-").FontSize(7).FontColor(Colors.Black).AlignCenter();

                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text(log.IpAddress ?? "-").FontSize(7).FontColor(Colors.Black).AlignCenter();

                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text($"{log.EntityType} ({log.EntityId})").FontSize(7).FontColor(Colors.Black).AlignCenter();

                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text(log.Details ?? "-").FontSize(7).FontColor(Colors.Black).AlignCenter();
                            }
                        });
                });
            });

            return document.GeneratePdf();
        }

        public byte[] GenerateLoginLogsPdf(List<EmployeeDirectory.Models.LoginLog> logs, string orientation = "portrait")
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(orientation == "landscape" ? PageSizes.A4.Landscape() : PageSizes.A4);
                    page.Margin(20);

                    page.Header()
                        .Text("Логи входа")
                        .FontSize(16)
                        .FontColor(Colors.Black)
                        .Bold()
                        .AlignCenter();

                    page.Content()
                        .Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text("Дата/Время").FontSize(10).Bold().FontColor(Colors.Black).AlignCenter();
                                header.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text("Пользователь").FontSize(10).Bold().FontColor(Colors.Black).AlignCenter();
                                header.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text("Действие").FontSize(10).Bold().FontColor(Colors.Black).AlignCenter();
                                header.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text("IP-адрес").FontSize(10).Bold().FontColor(Colors.Black).AlignCenter();
                                header.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text("Статус").FontSize(10).Bold().FontColor(Colors.Black).AlignCenter();
                            });

                            foreach (var log in logs)
                            {
                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text(TimeZoneInfo.ConvertTimeFromUtc(log.TimestampUtc, TimeZoneInfo.Local).ToString("dd.MM.yyyy HH:mm:ss")).FontSize(8).FontColor(Colors.Black).AlignCenter();

                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text(log.UserName ?? "-").FontSize(8).FontColor(Colors.Black).AlignCenter();

                                var actionText = log.Action switch
                                {
                                    "Login" => "Вход",
                                    "Logout" => "Выход",
                                    "FailedLogin" => "Неудачный вход",
                                    _ => log.Action ?? "-"
                                };
                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text(actionText).FontSize(8).FontColor(Colors.Black).AlignCenter();

                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text(log.IpAddress ?? "-").FontSize(8).FontColor(Colors.Black).AlignCenter();

                                var statusText = log.Success ? "Успешно" : "Ошибка";
                                table.Cell().Border(1).BorderColor(Colors.Black).Padding(3)
                                    .Text(statusText).FontSize(8).FontColor(Colors.Black).AlignCenter();
                            }
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}
