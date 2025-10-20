using System.ComponentModel.DataAnnotations;

namespace EmployeeDirectory.Models
{
    public class LogEntry
    {
        public int Id { get; set; }

        [Display(Name = "Дата/время (UTC)")]
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

        public string? UserId { get; set; }
        public string? UserName { get; set; }

        [MaxLength(64)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(64)]
        public string EntityType { get; set; } = string.Empty; 

        public string? EntityId { get; set; }

        [MaxLength(1024)]
        public string? Details { get; set; }

        [MaxLength(64)]
        public string? IpAddress { get; set; }
    }
}



