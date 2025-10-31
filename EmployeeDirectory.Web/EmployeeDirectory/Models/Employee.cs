using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeDirectory.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100)]
        [Display(Name = "ФИО")]
        public string? FullName { get; set; }

        [StringLength(20)]
        [Display(Name = "Городской номер")]
        public string? CityPhone { get; set; }

        [StringLength(20)]
        [Display(Name = "Внутренний номер")]
        public string? LocalPhone { get; set; }

        [StringLength(20)]
        [Display(Name = "Мобильный номер")]
        public string? MobilePhone { get; set; }

        [StringLength(150)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Отдел обязателен")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; } = null!;

        [Display(Name = "Название отдела")]
        public string DepartmentName { get; set; } = string.Empty;

        [Display(Name = "Является начальником отдела")]
        public bool IsHeadOfDepartment { get; set; } = false;

        [Display(Name = "Является заместителем отдела")]
        public bool IsDeputy { get; set; } = false;

        [Display(Name = "Должность/Описание")]
        public string? PositionDescription { get; set; }

        [Display(Name = "Должность")]
        public int? PositionId { get; set; }

        [ForeignKey("PositionId")]
        public virtual Position? Position { get; set; }

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Дата обновления")]
        public DateTime? UpdatedAt { get; set; }
    }
}
