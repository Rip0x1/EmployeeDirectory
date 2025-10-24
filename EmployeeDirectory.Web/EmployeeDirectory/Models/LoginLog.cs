using System.ComponentModel.DataAnnotations;

namespace EmployeeDirectory.Models
{
    public class LoginLog
    {
        public int Id { get; set; }

        [Display(Name = "Дата/время (UTC)")]
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

        [MaxLength(256)]
        public string? UserName { get; set; }

        [MaxLength(64)]
        public string Action { get; set; } = string.Empty;

        [MaxLength(64)]
        public string? IpAddress { get; set; }

        [MaxLength(256)]
        public string? UserAgent { get; set; }

        public bool Success { get; set; }

        [MaxLength(512)]
        public string? FailureReason { get; set; }
    }
}

