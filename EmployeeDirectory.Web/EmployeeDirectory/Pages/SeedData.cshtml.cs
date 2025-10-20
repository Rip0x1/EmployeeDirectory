using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using EmployeeDirectory.Data;
using EmployeeDirectory.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeDirectory.Pages
{
    public class SeedDataModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SeedDataModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Проверяем, есть ли уже данные в базе - если есть, не пересоздаем
            if (!_context.Employees.Any() && !_context.Departments.Any() && !_context.Positions.Any())
            {
                if (!_context.Departments.Any())
                {
                    var departments = new List<Department>
                    {
                        new Department { Name = "IT отдел", CreatedAt = DateTime.UtcNow },
                        new Department { Name = "HR отдел", CreatedAt = DateTime.UtcNow },
                        new Department { Name = "Финансовый отдел", CreatedAt = DateTime.UtcNow },
                        new Department { Name = "Отдел продаж", CreatedAt = DateTime.UtcNow },
                        new Department { Name = "Отдел маркетинга", CreatedAt = DateTime.UtcNow }
                    };

                    _context.Departments.AddRange(departments);
                    await _context.SaveChangesAsync();
                }

                if (!_context.Positions.Any())
                {
                    var positions = new List<Position>
                    {
                        new Position { Name = "Разработчик", CreatedAt = DateTime.UtcNow },
                        new Position { Name = "Тестировщик", CreatedAt = DateTime.UtcNow },
                        new Position { Name = "Аналитик", CreatedAt = DateTime.UtcNow },
                        new Position { Name = "HR-менеджер", CreatedAt = DateTime.UtcNow },
                        new Position { Name = "Бухгалтер", CreatedAt = DateTime.UtcNow },
                        new Position { Name = "Менеджер по продажам", CreatedAt = DateTime.UtcNow },
                        new Position { Name = "Маркетолог", CreatedAt = DateTime.UtcNow }
                    };

                    _context.Positions.AddRange(positions);
                    await _context.SaveChangesAsync();
                }

                var deptList = await _context.Departments.ToListAsync();
                var posList = await _context.Positions.ToListAsync();

                var employees = new List<Employee>
                {
                    new Employee { FullName = "Иванов Иван Иванович", CityPhone = "+7 (495) 123-45-67", LocalPhone = "1234", DepartmentId = deptList[0].Id, IsHeadOfDepartment = true, CreatedAt = DateTime.UtcNow },
                    new Employee { FullName = "Петров Петр Петрович", CityPhone = "+7 (495) 234-56-78", LocalPhone = "2345", DepartmentId = deptList[0].Id, IsHeadOfDepartment = false, PositionId = posList[0].Id, CreatedAt = DateTime.UtcNow },
                    new Employee { FullName = "Сидоров Сидор Сидорович", CityPhone = "+7 (495) 345-67-89", LocalPhone = "3456", DepartmentId = deptList[0].Id, IsHeadOfDepartment = false, PositionId = posList[1].Id, CreatedAt = DateTime.UtcNow },
                    new Employee { FullName = "Козлов Алексей Владимирович", CityPhone = "+7 (495) 456-78-90", LocalPhone = "4567", DepartmentId = deptList[0].Id, IsHeadOfDepartment = false, PositionId = posList[2].Id, CreatedAt = DateTime.UtcNow },
                    
                    new Employee { FullName = "Козлова Анна Сергеевна", CityPhone = "+7 (495) 567-89-01", LocalPhone = "5678", DepartmentId = deptList[1].Id, IsHeadOfDepartment = true, CreatedAt = DateTime.UtcNow },
                    new Employee { FullName = "Смирнова Елена Николаевна", CityPhone = "+7 (495) 678-90-12", LocalPhone = "6789", DepartmentId = deptList[1].Id, IsHeadOfDepartment = false, PositionId = posList[3].Id, CreatedAt = DateTime.UtcNow },
                    new Employee { FullName = "Васильева Мария Петровна", CityPhone = "+7 (495) 789-01-23", LocalPhone = "7890", DepartmentId = deptList[1].Id, IsHeadOfDepartment = false, PositionId = posList[3].Id, CreatedAt = DateTime.UtcNow },
                    new Employee { FullName = "Николаев Дмитрий Игоревич", CityPhone = "+7 (495) 890-12-34", LocalPhone = "8901", DepartmentId = deptList[1].Id, IsHeadOfDepartment = false, PositionId = posList[3].Id, CreatedAt = DateTime.UtcNow },
                    
                    new Employee { FullName = "Смирнов Алексей Владимирович", CityPhone = "+7 (495) 901-23-45", LocalPhone = "9012", DepartmentId = deptList[2].Id, IsHeadOfDepartment = true, CreatedAt = DateTime.UtcNow },
                    new Employee { FullName = "Федорова Ольга Сергеевна", CityPhone = "+7 (495) 012-34-56", LocalPhone = "0123", DepartmentId = deptList[2].Id, IsHeadOfDepartment = false, PositionId = posList[4].Id, CreatedAt = DateTime.UtcNow },
                    new Employee { FullName = "Морозов Игорь Александрович", CityPhone = "+7 (495) 123-45-67", LocalPhone = "1234", DepartmentId = deptList[2].Id, IsHeadOfDepartment = false, PositionId = posList[4].Id, CreatedAt = DateTime.UtcNow },
                    new Employee { FullName = "Кузнецова Татьяна Викторовна", CityPhone = "+7 (495) 234-56-78", LocalPhone = "2345", DepartmentId = deptList[2].Id, IsHeadOfDepartment = false, PositionId = posList[4].Id, CreatedAt = DateTime.UtcNow },
                    
                    new Employee { FullName = "Волков Сергей Михайлович", CityPhone = "+7 (495) 345-67-89", LocalPhone = "3456", DepartmentId = deptList[3].Id, IsHeadOfDepartment = true, CreatedAt = DateTime.UtcNow },
                    new Employee { FullName = "Лебедева Наталья Андреевна", CityPhone = "+7 (495) 456-78-90", LocalPhone = "4567", DepartmentId = deptList[3].Id, IsHeadOfDepartment = false, PositionId = posList[5].Id, CreatedAt = DateTime.UtcNow },
                    new Employee { FullName = "Соколов Андрей Дмитриевич", CityPhone = "+7 (495) 567-89-01", LocalPhone = "5678", DepartmentId = deptList[3].Id, IsHeadOfDepartment = false, PositionId = posList[5].Id, CreatedAt = DateTime.UtcNow },
                    new Employee { FullName = "Попова Ирина Владимировна", CityPhone = "+7 (495) 678-90-12", LocalPhone = "6789", DepartmentId = deptList[3].Id, IsHeadOfDepartment = false, PositionId = posList[5].Id, CreatedAt = DateTime.UtcNow },
                    
                    new Employee { FullName = "Новиков Максим Сергеевич", CityPhone = "+7 (495) 789-01-23", LocalPhone = "7890", DepartmentId = deptList[4].Id, IsHeadOfDepartment = true, CreatedAt = DateTime.UtcNow },
                    new Employee { FullName = "Морозова Екатерина Игоревна", CityPhone = "+7 (495) 890-12-34", LocalPhone = "8901", DepartmentId = deptList[4].Id, IsHeadOfDepartment = false, PositionId = posList[6].Id, CreatedAt = DateTime.UtcNow },
                    new Employee { FullName = "Степанов Роман Александрович", CityPhone = "+7 (495) 901-23-45", LocalPhone = "9012", DepartmentId = deptList[4].Id, IsHeadOfDepartment = false, PositionId = posList[6].Id, CreatedAt = DateTime.UtcNow },
                    new Employee { FullName = "Григорьева Анна Дмитриевна", CityPhone = "+7 (495) 012-34-56", LocalPhone = "0123", DepartmentId = deptList[4].Id, IsHeadOfDepartment = false, PositionId = posList[6].Id, CreatedAt = DateTime.UtcNow }
                };

                _context.Employees.AddRange(employees);
                await _context.SaveChangesAsync();

                var employeesList = await _context.Employees.ToListAsync();
                var heads = employeesList.Where(e => e.IsHeadOfDepartment).ToList();
                
                for (int i = 0; i < deptList.Count && i < heads.Count; i++)
                {
                    deptList[i].HeadId = heads[i].Id;
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                // Данные уже существуют, просто перенаправляем
                TempData["Info"] = "Тестовые данные уже существуют в базе данных.";
            }

            return RedirectToPage("/Index");
        }
    }
}
