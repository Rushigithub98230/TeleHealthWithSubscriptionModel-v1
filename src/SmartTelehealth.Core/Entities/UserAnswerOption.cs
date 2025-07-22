using System;

namespace SmartTelehealth.Core.Entities
{
    public class UserAnswerOption : BaseEntity
    {
        public Guid AnswerId { get; set; }
        public virtual UserAnswer Answer { get; set; } = null!;
        public Guid OptionId { get; set; }
        public virtual QuestionOption Option { get; set; } = null!;
    }
} 