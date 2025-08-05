using System;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities
{
    public class UserAnswerOption : BaseEntity
    {
        [Required]
        public Guid AnswerId { get; set; }
        
        [Required]
        public Guid OptionId { get; set; }
        
        // Navigation Properties
        public virtual UserAnswer Answer { get; set; } = null!;
        public virtual QuestionOption Option { get; set; } = null!;
    }
} 