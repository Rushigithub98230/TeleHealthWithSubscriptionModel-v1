using System;
using System.Collections.Generic;

namespace SmartTelehealth.Core.Entities
{
    public class Question : BaseEntity
    {
        public Guid TemplateId { get; set; }
        public virtual QuestionnaireTemplate Template { get; set; } = null!;
        public string Text { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // text, textarea, radio, checkbox, dropdown, range, etc.
        public bool IsRequired { get; set; } = true;
        public int Order { get; set; }
        public string? HelpText { get; set; }
        public string? MediaUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    }
} 