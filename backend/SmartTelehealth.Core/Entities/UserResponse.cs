using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities
{
    public class UserResponse : BaseEntity
    {
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        public Guid CategoryId { get; set; }
        
        [Required]
        public Guid TemplateId { get; set; }
        
        public ResponseStatus Status { get; set; } = ResponseStatus.Draft;
        
        // Navigation Properties
        public virtual User User { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
        public virtual QuestionnaireTemplate Template { get; set; } = null!;
        public virtual ICollection<UserAnswer> Answers { get; set; } = new List<UserAnswer>();
        
        // Helper methods
        public bool IsCompleted => Status == ResponseStatus.Completed || Status == ResponseStatus.Submitted;
        public bool IsDraft => Status == ResponseStatus.Draft;
    }
} 