using System;
using System.Collections.Generic;

namespace SmartTelehealth.Core.Entities
{
    public class UserAnswer : BaseEntity
    {
        public Guid ResponseId { get; set; }
        public virtual UserResponse Response { get; set; } = null!;
        public Guid QuestionId { get; set; }
        public virtual Question Question { get; set; } = null!;
        public string? AnswerText { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public virtual ICollection<UserAnswerOption> SelectedOptions { get; set; } = new List<UserAnswerOption>();
    }
} 