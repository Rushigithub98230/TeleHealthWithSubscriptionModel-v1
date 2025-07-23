using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities
{
    public class CategoryQuestion : BaseEntity
    {
        public Guid CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string QuestionType { get; set; } = "text"; // text, multiple_choice, yes_no, etc.

        public string? OptionsJson { get; set; } // For multiple choice, store options as JSON
        public bool IsRequired { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 