using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using EmployeeDirectory.Services;
using EmployeeDirectory.Models;
using System.Text;
using System.Text.Json;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace EmployeeDirectory.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class ImportExportModel : PageModel
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IPositionService _positionService;
        private readonly ILogService _logService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public ImportExportModel(
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            IPositionService positionService,
            ILogService logService,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _positionService = positionService;
            _logService = logService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public class ImportResult
        {
            public int SuccessCount { get; set; }
            public int ErrorCount { get; set; }
            public List<string> Errors { get; set; } = new();
        }

        public async Task<IActionResult> OnGetExportJsonAsync()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            var json = JsonSerializer.Serialize(employees, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            });

            await _logService.WriteAsync(new LogEntry
            {
                Action = "Export",
                EntityType = nameof(Employee),
                Details = $"Экспорт данных сотрудников в JSON ({employees.Count()} записей)"
            });

            return File(Encoding.UTF8.GetBytes(json), "application/json", $"employees_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        }

        public async Task<IActionResult> OnGetExportCsvAsync()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            csv.WriteField("ФИО");
            csv.WriteField("Городской номер");
            csv.WriteField("Внутренний номер");
            csv.WriteField("Мобильный номер");
            csv.WriteField("Email");
            csv.WriteField("Отдел");
            csv.WriteField("Полное название отдела");
            csv.WriteField("Сокращенное название отдела");
            csv.WriteField("Должность");
            csv.WriteField("Начальник отдела");
            csv.WriteField("Заместитель");
            csv.WriteField("Описание должности");
            csv.NextRecord();

            foreach (var emp in employees)
            {
                csv.WriteField(emp.FullName ?? "");
                csv.WriteField(emp.CityPhone ?? "");
                csv.WriteField(emp.LocalPhone ?? "");
                csv.WriteField(emp.MobilePhone ?? "");
                csv.WriteField(emp.Email ?? "");
                csv.WriteField(emp.Department?.Name ?? "");
                csv.WriteField(emp.Department?.FullName ?? "");
                csv.WriteField(emp.Department?.ShortName ?? "");
                csv.WriteField(emp.Position?.Name ?? "");
                csv.WriteField(emp.IsHeadOfDepartment ? "Да" : "Нет");
                csv.WriteField(emp.IsDeputy ? "Да" : "Нет");
                csv.WriteField(emp.PositionDescription ?? "");
                csv.NextRecord();
            }

            writer.Flush();
            memoryStream.Position = 0;

            await _logService.WriteAsync(new LogEntry
            {
                Action = "Export",
                EntityType = nameof(Employee),
                Details = $"Экспорт данных сотрудников в CSV ({employees.Count()} записей)"
            });

            return File(memoryStream.ToArray(), "text/csv", $"employees_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        public async Task<IActionResult> OnPostImportJsonAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Файл не выбран или пуст";
                return Page();
            }

            try
            {
                using var stream = new StreamReader(file.OpenReadStream());
                var json = await stream.ReadToEndAsync();
                var employees = JsonSerializer.Deserialize<List<Employee>>(json);

                if (employees == null || !employees.Any())
                {
                    TempData["Error"] = "Файл не содержит данных";
                    return Page();
                }

                var departments = await _departmentService.GetAllDepartmentsAsync();
                
                foreach (var emp in employees.Where(e => e.Department != null))
                {
                    var existingDept = departments.FirstOrDefault(d => d.Name == emp.Department.Name);
                    if (existingDept == null)
                    {
                        var newDept = new Department
                        {
                            Name = emp.Department.Name,
                            FullName = emp.Department.FullName,
                            ShortName = emp.Department.ShortName,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _departmentService.CreateDepartmentAsync(newDept);
                        departments = await _departmentService.GetAllDepartmentsAsync();
                    }
                    else
                    {
                        if ((!string.IsNullOrWhiteSpace(emp.Department.FullName) && string.IsNullOrWhiteSpace(existingDept.FullName)) ||
                            (!string.IsNullOrWhiteSpace(emp.Department.ShortName) && string.IsNullOrWhiteSpace(existingDept.ShortName)))
                        {
                            var updatedDept = new Department
                            {
                                Id = existingDept.Id,
                                Name = existingDept.Name,
                                FullName = !string.IsNullOrWhiteSpace(emp.Department.FullName) ? emp.Department.FullName : existingDept.FullName,
                                ShortName = !string.IsNullOrWhiteSpace(emp.Department.ShortName) ? emp.Department.ShortName : existingDept.ShortName,
                                CreatedAt = existingDept.CreatedAt,
                                HeadId = existingDept.HeadId
                            };
                            await _departmentService.UpdateDepartmentAsync(updatedDept);
                        }
                    }
                    
                    var dept = departments.FirstOrDefault(d => d.Name == emp.Department.Name);
                    if (dept != null)
                    {
                        emp.DepartmentId = dept.Id;
                    }
                }

                var result = await ImportEmployeesAsync(employees);
                TempData["Success"] = $"Импортировано: {result.SuccessCount}, Ошибок: {result.ErrorCount}";
                
                if (result.Errors.Any())
                {
                    TempData["Errors"] = string.Join("; ", result.Errors);
                }

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при импорте JSON: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostImportCsvAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Файл не выбран или пуст";
                return Page();
            }

            try
            {
                using var stream = new StreamReader(file.OpenReadStream());
                using var csv = new CsvReader(stream, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    TrimOptions = TrimOptions.Trim,
                    MissingFieldFound = null
                });

                var employees = new List<Employee>();
                var departments = await _departmentService.GetAllDepartmentsAsync();
                var positions = await _positionService.GetAllPositionsAsync();

                await csv.ReadAsync();
                csv.ReadHeader();

                while (await csv.ReadAsync())
                {
                    var fullName = csv.GetField<string>("ФИО")?.Trim();
                    var departmentName = csv.GetField<string>("Отдел")?.Trim();

                    if (string.IsNullOrWhiteSpace(departmentName))
                    {
                        continue;
                    }

                    var positionName = csv.GetField<string>("Должность")?.Trim();
                    var isHead = csv.GetField<string>("Начальник отдела")?.Trim().ToLower() == "да";
                    var isDeputy = csv.GetField<string>("Заместитель")?.Trim().ToLower() == "да";

                    var departmentFullName = csv.GetField<string>("Полное название отдела")?.Trim();
                    var departmentShortName = csv.GetField<string>("Сокращенное название отдела")?.Trim();
                    
                    var existingDepartment = departments.FirstOrDefault(d => d.Name == departmentName);
                    int departmentId;
                    
                    if (existingDepartment == null)
                    {
                        var newDepartment = new Department
                        {
                            Name = departmentName,
                            FullName = string.IsNullOrWhiteSpace(departmentFullName) ? null : departmentFullName,
                            ShortName = string.IsNullOrWhiteSpace(departmentShortName) ? null : departmentShortName,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _departmentService.CreateDepartmentAsync(newDepartment);
                        departmentId = newDepartment.Id;
                        departments = await _departmentService.GetAllDepartmentsAsync();
                    }
                    else
                    {
                        departmentId = existingDepartment.Id;
                        
                        if ((!string.IsNullOrWhiteSpace(departmentFullName) && string.IsNullOrWhiteSpace(existingDepartment.FullName)) ||
                            (!string.IsNullOrWhiteSpace(departmentShortName) && string.IsNullOrWhiteSpace(existingDepartment.ShortName)))
                        {
                            var updatedDepartment = new Department
                            {
                                Id = existingDepartment.Id,
                                Name = existingDepartment.Name,
                                FullName = !string.IsNullOrWhiteSpace(departmentFullName) ? departmentFullName : existingDepartment.FullName,
                                ShortName = !string.IsNullOrWhiteSpace(departmentShortName) ? departmentShortName : existingDepartment.ShortName,
                                CreatedAt = existingDepartment.CreatedAt,
                                HeadId = existingDepartment.HeadId
                            };
                            await _departmentService.UpdateDepartmentAsync(updatedDepartment);
                        }
                    }

                    var position = string.IsNullOrEmpty(positionName) 
                        ? null 
                        : positions.FirstOrDefault(p => p.Name == positionName);

                    var employee = new Employee
                    {
                        FullName = fullName,
                        CityPhone = csv.GetField<string>("Городской номер")?.Trim(),
                        LocalPhone = csv.GetField<string>("Внутренний номер")?.Trim(),
                        MobilePhone = csv.GetField<string>("Мобильный номер")?.Trim(),
                        Email = csv.GetField<string>("Email")?.Trim(),
                        DepartmentId = departmentId,
                        PositionId = position?.Id,
                        IsHeadOfDepartment = isHead,
                        IsDeputy = isDeputy,
                        PositionDescription = csv.GetField<string>("Описание должности")?.Trim()
                    };

                    employees.Add(employee);
                }

                if (!employees.Any())
                {
                    TempData["Error"] = "Файл не содержит данных или все строки пустые";
                    return Page();
                }

                var result = await ImportEmployeesAsync(employees);
                TempData["Success"] = $"Импортировано: {result.SuccessCount}, Ошибок: {result.ErrorCount}";
                
                if (result.Errors.Any())
                {
                    TempData["Errors"] = string.Join("; ", result.Errors);
                }

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при импорте CSV: {ex.Message}";
                return Page();
            }
        }

        private async Task<ImportResult> ImportEmployeesAsync(List<Employee> employees)
        {
            var result = new ImportResult();
            var departments = await _departmentService.GetAllDepartmentsAsync();
            var existingEmployees = await _employeeService.GetAllEmployeesAsync();

            foreach (var emp in employees)
            {
                try
                {
                    if (emp.DepartmentId <= 0)
                    {
                        var empId = !string.IsNullOrWhiteSpace(emp.FullName) ? emp.FullName : $"ID: {emp.Id}";
                        result.Errors.Add($"Не найден отдел для сотрудника: {empId}");
                        result.ErrorCount++;
                        continue;
                    }

                    var departmentExists = departments.Any(d => d.Id == emp.DepartmentId);
                    if (!departmentExists)
                    {
                        var empId = !string.IsNullOrWhiteSpace(emp.FullName) ? emp.FullName : $"ID: {emp.Id}";
                        result.Errors.Add($"Отдел не найден для сотрудника: {empId}");
                        result.ErrorCount++;
                        continue;
                    }

                    var normalizedEmpName = emp.FullName?.Trim() ?? "";
                    var normalizedEmpPhone = emp.CityPhone?.Trim() ?? "";
                    
                    var departmentName = departments.FirstOrDefault(d => d.Id == emp.DepartmentId)?.Name ?? "Неизвестный отдел";
                    
                    var isDuplicate = existingEmployees.Any(e => 
                        {
                            var existingName = e.FullName?.Trim() ?? "";
                            var existingPhone = e.CityPhone?.Trim() ?? "";
                            
                            return existingName == normalizedEmpName && 
                                   e.DepartmentId == emp.DepartmentId &&
                                   existingPhone == normalizedEmpPhone;
                        });
                    
                    if (isDuplicate)
                    {
                        var empId = !string.IsNullOrWhiteSpace(emp.FullName) ? emp.FullName : "Без ФИО";
                        result.Errors.Add($"Сотрудник уже существует: {empId} в отделе {departmentName}");
                        result.ErrorCount++;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(emp.CityPhone))
                    {
                        emp.CityPhone = null;
                    }
                    if (string.IsNullOrWhiteSpace(emp.LocalPhone))
                    {
                        emp.LocalPhone = null;
                    }
                    if (string.IsNullOrWhiteSpace(emp.Email))
                    {
                        emp.Email = null;
                    }
                    if (string.IsNullOrWhiteSpace(emp.MobilePhone))
                    {
                        emp.MobilePhone = null;
                    }
                    if (string.IsNullOrWhiteSpace(emp.PositionDescription))
                    {
                        emp.PositionDescription = null;
                    }

                    var cleanEmployee = new Employee
                    {
                        FullName = emp.FullName,
                        CityPhone = emp.CityPhone,
                        LocalPhone = emp.LocalPhone,
                        MobilePhone = emp.MobilePhone,
                        Email = emp.Email,
                        DepartmentId = emp.DepartmentId,
                        PositionId = emp.PositionId,
                        IsHeadOfDepartment = emp.IsHeadOfDepartment,
                        IsDeputy = emp.IsDeputy,
                        PositionDescription = emp.PositionDescription
                    };
                    
                    await _employeeService.AddEmployeeAsync(cleanEmployee);
                    result.SuccessCount++;
                    
                    existingEmployees = await _employeeService.GetAllEmployeesAsync();
                }
                catch (Exception ex)
                {
                    var empId = !string.IsNullOrWhiteSpace(emp.FullName) ? emp.FullName : "Без ФИО";
                    result.Errors.Add($"Ошибка импорта {empId}: {ex.Message}");
                    result.ErrorCount++;
                }
            }

            return result;
        }

        public async Task<IActionResult> OnGetExportUsersJsonAsync()
        {
            var users = _userManager.Users.ToList();
            var departments = await _departmentService.GetAllDepartmentsAsync();
            var usersWithRoles = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var department = user.DepartmentId.HasValue 
                    ? departments.FirstOrDefault(d => d.Id == user.DepartmentId.Value) 
                    : null;
                    
                usersWithRoles.Add(new
                {
                    Username = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    DepartmentId = user.DepartmentId,
                    DepartmentName = department?.Name,
                    Roles = string.Join(", ", roles),
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                });
            }

            var json = JsonSerializer.Serialize(usersWithRoles, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            await _logService.WriteAsync(new LogEntry
            {
                Action = "Export",
                EntityType = nameof(ApplicationUser),
                Details = $"Экспорт пользователей в JSON ({users.Count} записей)"
            });

            return File(Encoding.UTF8.GetBytes(json), "application/json", $"users_{DateTime.Now:yyyyMMdd_HHmmss}.json");
        }

        public async Task<IActionResult> OnGetExportUsersCsvAsync()
        {
            var users = _userManager.Users.ToList();
            var departments = await _departmentService.GetAllDepartmentsAsync();
            
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream, Encoding.UTF8);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            csv.WriteField("Логин");
            csv.WriteField("Email");
            csv.WriteField("Полное имя");
            csv.WriteField("Отдел");
            csv.WriteField("Роли");
            csv.WriteField("Активен");
            csv.WriteField("Дата создания");
            csv.WriteField("Дата последнего входа");
            csv.NextRecord();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var department = user.DepartmentId.HasValue 
                    ? departments.FirstOrDefault(d => d.Id == user.DepartmentId.Value) 
                    : null;
                    
                csv.WriteField(user.UserName ?? "");
                csv.WriteField(user.Email ?? "");
                csv.WriteField(user.FullName ?? "");
                csv.WriteField(department?.Name ?? "");
                csv.WriteField(string.Join(", ", roles));
                csv.WriteField(user.IsActive ? "Да" : "Нет");
                csv.WriteField(user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                csv.WriteField(user.LastLoginAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "");
                csv.NextRecord();
            }

            writer.Flush();
            memoryStream.Position = 0;

            await _logService.WriteAsync(new LogEntry
            {
                Action = "Export",
                EntityType = nameof(ApplicationUser),
                Details = $"Экспорт пользователей в CSV ({users.Count} записей)"
            });

            return File(memoryStream.ToArray(), "text/csv", $"users_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        public async Task<IActionResult> OnPostImportUsersJsonAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Файл не выбран или пуст";
                return Page();
            }

            try
            {
                using var stream = new StreamReader(file.OpenReadStream());
                var json = await stream.ReadToEndAsync();
                var usersData = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(json);

                if (usersData == null || !usersData.Any())
                {
                    TempData["Error"] = "Файл не содержит данных";
                    return Page();
                }

                var result = await ImportUsersAsync(usersData);
                TempData["Success"] = $"Импортировано пользователей: {result.SuccessCount}, Ошибок: {result.ErrorCount}";
                
                if (result.Errors.Any())
                {
                    TempData["Errors"] = string.Join("; ", result.Errors);
                }

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при импорте JSON: {ex.Message}";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostImportUsersCsvAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Файл не выбран или пуст";
                return Page();
            }

            try
            {
                using var stream = new StreamReader(file.OpenReadStream());
                using var csv = new CsvReader(stream, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    TrimOptions = TrimOptions.Trim,
                    MissingFieldFound = null
                });

                var usersData = new List<Dictionary<string, string>>();
                await csv.ReadAsync();
                csv.ReadHeader();

                while (await csv.ReadAsync())
                {
                    var userData = new Dictionary<string, string>
                    {
                        ["Username"] = csv.GetField<string>("Логин")?.Trim() ?? "",
                        ["Email"] = csv.GetField<string>("Email")?.Trim() ?? "",
                        ["FullName"] = csv.GetField<string>("Полное имя")?.Trim() ?? "",
                        ["DepartmentName"] = csv.GetField<string>("Отдел")?.Trim() ?? "",
                        ["Roles"] = csv.GetField<string>("Роли")?.Trim() ?? "",
                        ["IsActive"] = csv.GetField<string>("Активен")?.Trim() ?? ""
                    };
                    usersData.Add(userData);
                }

                if (!usersData.Any())
                {
                    TempData["Error"] = "Файл не содержит данных";
                    return Page();
                }

                var result = await ImportUsersCsvAsync(usersData);
                TempData["Success"] = $"Импортировано пользователей: {result.SuccessCount}, Ошибок: {result.ErrorCount}";
                
                if (result.Errors.Any())
                {
                    TempData["Errors"] = string.Join("; ", result.Errors);
                }

                return Page();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Ошибка при импорте CSV: {ex.Message}";
                return Page();
            }
        }

        private async Task<ImportResult> ImportUsersAsync(List<Dictionary<string, JsonElement>> usersData)
        {
            var result = new ImportResult();
            var departments = await _departmentService.GetAllDepartmentsAsync();

            foreach (var userData in usersData)
            {
                try
                {
                    var username = userData.ContainsKey("Username") ? userData["Username"].GetString() : null;
                    var email = userData.ContainsKey("Email") ? userData["Email"].GetString() : null;
                    var fullName = userData.ContainsKey("FullName") ? userData["FullName"].GetString() : null;
                    var departmentName = userData.ContainsKey("DepartmentName") ? userData["DepartmentName"].GetString() : null;
                    var rolesStr = userData.ContainsKey("Roles") ? userData["Roles"].GetString() : null;
                    var isActive = userData.ContainsKey("IsActive") ? userData["IsActive"].GetBoolean() : true;

                    if (string.IsNullOrWhiteSpace(username))
                    {
                        result.Errors.Add("Пропущена запись без логина");
                        result.ErrorCount++;
                        continue;
                    }

                    var existingUser = await _userManager.FindByNameAsync(username);
                    if (existingUser != null)
                    {
                        result.Errors.Add($"Пользователь уже существует: {username}");
                        result.ErrorCount++;
                        continue;
                    }

                    int? departmentId = null;
                    if (!string.IsNullOrWhiteSpace(departmentName))
                    {
                        var department = departments.FirstOrDefault(d => d.Name == departmentName);
                        if (department != null)
                        {
                            departmentId = department.Id;
                        }
                        else
                        {
                            result.Errors.Add($"Отдел '{departmentName}' не найден для пользователя {username}");
                        }
                    }

                    var user = new ApplicationUser
                    {
                        UserName = username,
                        Email = email,
                        FullName = fullName ?? "",
                        DepartmentId = departmentId,
                        IsActive = isActive,
                        CreatedAt = DateTime.UtcNow,
                        PasswordHash = null
                    };

                    var createResult = await _userManager.CreateAsync(user);

                    if (createResult.Succeeded)
                    {
                        if (!string.IsNullOrWhiteSpace(rolesStr))
                        {
                            var roles = rolesStr.Split(',').Select(r => r.Trim()).Where(r => !string.IsNullOrEmpty(r));
                            foreach (var role in roles)
                            {
                                if (await _roleManager.RoleExistsAsync(role))
                                {
                                    await _userManager.AddToRoleAsync(user, role);
                                }
                            }
                        }
                        var userRoles = await _userManager.GetRolesAsync(user);
                        if (user.DepartmentId.HasValue && userRoles.Contains("Administrator"))
                        {
                            var allEmployees = await _employeeService.GetAllEmployeesAsync();
                            var matches = allEmployees.Where(e => e.DepartmentId == user.DepartmentId && (e.FullName ?? "").Trim().Equals((user.FullName ?? "").Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                            foreach (var emp in matches)
                            {
                                await _employeeService.DeleteEmployeeAsync(emp.Id);
                            }
                        }
                        result.SuccessCount++;
                    }
                    else
                    {
                        result.Errors.Add($"Ошибка создания пользователя {username}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                        result.ErrorCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Ошибка импорта пользователя: {ex.Message}");
                    result.ErrorCount++;
                }
            }

            return result;
        }

        private async Task<ImportResult> ImportUsersCsvAsync(List<Dictionary<string, string>> usersData)
        {
            var result = new ImportResult();
            var departments = await _departmentService.GetAllDepartmentsAsync();

            foreach (var userData in usersData)
            {
                try
                {
                    var username = userData.ContainsKey("Username") ? userData["Username"] : null;
                    var email = userData.ContainsKey("Email") ? userData["Email"] : null;
                    var fullName = userData.ContainsKey("FullName") ? userData["FullName"] : null;
                    var departmentName = userData.ContainsKey("DepartmentName") ? userData["DepartmentName"] : null;
                    var rolesStr = userData.ContainsKey("Roles") ? userData["Roles"] : null;
                    var isActiveStr = userData.ContainsKey("IsActive") ? userData["IsActive"] : "Да";
                    var isActive = isActiveStr.ToLower() == "да" || isActiveStr.ToLower() == "yes" || isActiveStr == "1";

                    if (string.IsNullOrWhiteSpace(username))
                    {
                        result.Errors.Add("Пропущена запись без логина");
                        result.ErrorCount++;
                        continue;
                    }

                    var existingUser = await _userManager.FindByNameAsync(username);
                    if (existingUser != null)
                    {
                        result.Errors.Add($"Пользователь уже существует: {username}");
                        result.ErrorCount++;
                        continue;
                    }

                    int? departmentId = null;
                    if (!string.IsNullOrWhiteSpace(departmentName))
                    {
                        var department = departments.FirstOrDefault(d => d.Name == departmentName);
                        if (department != null)
                        {
                            departmentId = department.Id;
                        }
                        else
                        {
                            result.Errors.Add($"Отдел '{departmentName}' не найден для пользователя {username}");
                        }
                    }

                    var user = new ApplicationUser
                    {
                        UserName = username,
                        Email = email,
                        FullName = fullName ?? "",
                        DepartmentId = departmentId,
                        IsActive = isActive,
                        CreatedAt = DateTime.UtcNow,
                        PasswordHash = null
                    };

                    var createResult = await _userManager.CreateAsync(user);

                    if (createResult.Succeeded)
                    {
                        if (!string.IsNullOrWhiteSpace(rolesStr))
                        {
                            var roles = rolesStr.Split(',').Select(r => r.Trim()).Where(r => !string.IsNullOrEmpty(r));
                            foreach (var role in roles)
                            {
                                if (await _roleManager.RoleExistsAsync(role))
                                {
                                    await _userManager.AddToRoleAsync(user, role);
                                }
                            }
                        }
                        var userRoles = await _userManager.GetRolesAsync(user);
                        if (user.DepartmentId.HasValue && userRoles.Contains("Administrator"))
                        {
                            var allEmployees = await _employeeService.GetAllEmployeesAsync();
                            var matches = allEmployees.Where(e => e.DepartmentId == user.DepartmentId && (e.FullName ?? "").Trim().Equals((user.FullName ?? "").Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
                            foreach (var emp in matches)
                            {
                                await _employeeService.DeleteEmployeeAsync(emp.Id);
                            }
                        }
                        result.SuccessCount++;
                    }
                    else
                    {
                        result.Errors.Add($"Ошибка создания пользователя {username}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                        result.ErrorCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Ошибка импорта пользователя: {ex.Message}");
                    result.ErrorCount++;
                }
            }

            return result;
        }
    }
}

