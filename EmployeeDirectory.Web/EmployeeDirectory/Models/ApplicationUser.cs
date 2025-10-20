using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeDirectory.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Полное имя")]
        public string FullName { get; set; } = string.Empty;
        
        [Display(Name = "Отдел")]
        public int? DepartmentId { get; set; }
        
        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }
        
        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Дата последнего входа")]
        public DateTime? LastLoginAt { get; set; }
        
        [Display(Name = "Активен")]
        public bool IsActive { get; set; } = true;
    }
}
