using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities
{
    public class QuestionnaireTemplate : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public Guid CategoryId { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int Version { get; set; } = 1;
        
        // Navigation Properties
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public virtual ICollection<UserResponse> UserResponses { get; set; } = new List<UserResponse>();
    }
} 