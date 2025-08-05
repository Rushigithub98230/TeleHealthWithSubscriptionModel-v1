using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities
{
    public class UserAnswer : BaseEntity
    {
        [Required]
        public Guid ResponseId { get; set; }
        
        [Required]
        public Guid QuestionId { get; set; }
        
        [MaxLength(4000)] // For text/textarea answers
        public string? AnswerText { get; set; }
        
        public decimal? NumericValue { get; set; } // For range answers
        
        public DateTime? DateTimeValue { get; set; } // For date, datetime, time answers
        
        // Navigation Properties
        public virtual UserResponse Response { get; set; } = null!;
        public virtual Question Question { get; set; } = null!;
        public virtual ICollection<UserAnswerOption> SelectedOptions { get; set; } = new List<UserAnswerOption>();
        
        // Helper methods
        public bool HasTextAnswer => !string.IsNullOrEmpty(AnswerText);
        public bool HasNumericAnswer => NumericValue.HasValue;
        public bool HasDateTimeAnswer => DateTimeValue.HasValue;
        public bool HasSelectedOptions => SelectedOptions.Count > 0;
        public bool IsAnswered => HasTextAnswer || HasNumericAnswer || HasDateTimeAnswer || HasSelectedOptions;
    }
} 