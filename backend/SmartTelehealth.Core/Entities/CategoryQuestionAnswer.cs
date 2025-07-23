using System;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities
{
    public class CategoryQuestionAnswer : BaseEntity
    {
        public Guid CategoryQuestionId { get; set; }
        public virtual CategoryQuestion CategoryQuestion { get; set; } = null!;

        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        [MaxLength(1000)]
        public string Answer { get; set; } = string.Empty;
        public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
    }
} 