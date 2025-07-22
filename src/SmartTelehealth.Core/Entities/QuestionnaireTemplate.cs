using System;
using System.Collections.Generic;

namespace SmartTelehealth.Core.Entities
{
    public class QuestionnaireTemplate : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public bool IsActive { get; set; } = true;
        public int Version { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }
} 