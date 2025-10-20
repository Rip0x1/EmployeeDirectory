using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EmployeeDirectory.Models
{
    public class ApplicationRole : IdentityRole
    {
        [Display(Name = "Описание")]
        public string? Description { get; set; }
        
        [Display(Name = "Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
