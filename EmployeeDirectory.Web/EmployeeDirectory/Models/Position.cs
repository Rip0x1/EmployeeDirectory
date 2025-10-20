using System.ComponentModel.DataAnnotations;

namespace EmployeeDirectory.Models
{
    public class Position
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
