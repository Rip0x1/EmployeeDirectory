using System.ComponentModel.DataAnnotations;

namespace EmployeeDirectory.Models
{
    public class Position
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Введите название должности")]
        [StringLength(100)]
        [Display(Name = "Название должности")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Дата обновления")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
