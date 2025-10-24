using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeDirectory.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название отдела")]
        [StringLength(100)]
        [Display(Name = "Название отдела")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Полное название отдела")]
        public string? FullName { get; set; }

        [StringLength(50)]
        [Display(Name = "Сокращенное название отдела")]
        public string? ShortName { get; set; }

        [Display(Name = "Глава отдела")]
        public int? HeadId { get; set; }

        [ForeignKey("HeadId")]
        public virtual Employee? Head { get; set; }

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Дата обновления")]
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

        public string GetDisplayName()
        {
            bool hasFullName = !string.IsNullOrWhiteSpace(FullName);
            bool hasShortName = !string.IsNullOrWhiteSpace(ShortName);

            if (hasFullName && hasShortName)
            {
                return $"{FullName} ({ShortName})";
            }
            else if (hasShortName)
            {
                return ShortName;
            }
            else if (hasFullName)
            {
                return FullName;
            }
            else
            {
                return Name;
            }
        }
    }
}
