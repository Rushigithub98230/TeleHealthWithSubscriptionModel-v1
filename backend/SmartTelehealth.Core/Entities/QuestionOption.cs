using System;

namespace SmartTelehealth.Core.Entities
{
    public class QuestionOption : BaseEntity
    {
        public Guid QuestionId { get; set; }
        public virtual Question Question { get; set; } = null!;
        public string Text { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int Order { get; set; }
        public string? MediaUrl { get; set; }
    }
} 