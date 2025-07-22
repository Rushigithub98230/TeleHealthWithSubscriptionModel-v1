using System;
using System.Collections.Generic;

namespace SmartTelehealth.Core.Entities
{
    public class UserResponse : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid TemplateId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public virtual ICollection<UserAnswer> Answers { get; set; } = new List<UserAnswer>();
    }
} 