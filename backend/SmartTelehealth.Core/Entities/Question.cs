using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities
{
    public class Question : BaseEntity
    {
        [Required]
        public Guid TemplateId { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Text { get; set; } = string.Empty;
        
        [Required]
        public QuestionType Type { get; set; } = QuestionType.Text;
        
        public bool IsRequired { get; set; } = true;
        
        [Required]
        public int Order { get; set; }
        
        [MaxLength(200)]
        public string? HelpText { get; set; }
        
        [MaxLength(500)]
        public string? MediaUrl { get; set; }
        
        // Range-specific properties
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public decimal? StepValue { get; set; }
        
        // Navigation Properties
        public virtual QuestionnaireTemplate Template { get; set; } = null!;
        public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
        
        // Helper methods for validation
        public bool IsMultipleChoice => Type == QuestionType.Radio || Type == QuestionType.Checkbox || Type == QuestionType.Dropdown;
        public bool IsTextBased => Type == QuestionType.Text || Type == QuestionType.TextArea;
        public bool IsRange => Type == QuestionType.Range;
        public bool IsDateTimeBased => Type == QuestionType.Date || Type == QuestionType.DateTime || Type == QuestionType.Time;
        public bool HasOptions => Options.Count > 0;
    }
} 