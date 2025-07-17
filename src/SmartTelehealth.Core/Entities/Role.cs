using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SmartTelehealth.Core.Entities;

public class Role : IdentityRole<Guid>
{
    public Role() : base()
    {
    }
    
    public Role(string roleName) : base(roleName)
    {
    }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
} 