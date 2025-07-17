using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTelehealth.Core.Entities;
using SmartTelehealth.Core.Interfaces;

namespace SmartTelehealth.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PrivilegesController : ControllerBase
{
    private readonly IPrivilegeRepository _privilegeRepo;
    private readonly ISubscriptionPlanPrivilegeRepository _planPrivilegeRepo;
    private readonly ISubscriptionPlanRepository _planRepo;

    public PrivilegesController(
        IPrivilegeRepository privilegeRepo,
        ISubscriptionPlanPrivilegeRepository planPrivilegeRepo,
        ISubscriptionPlanRepository planRepo)
    {
        _privilegeRepo = privilegeRepo;
        _planPrivilegeRepo = planPrivilegeRepo;
        _planRepo = planRepo;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Privilege>>> GetAll()
        => Ok(await _privilegeRepo.GetAllAsync());

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] Privilege privilege)
    {
        await _privilegeRepo.AddAsync(privilege);
        return CreatedAtAction(nameof(GetAll), new { id = privilege.Id }, privilege);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] Privilege privilege)
    {
        if (id != privilege.Id) return BadRequest();
        await _privilegeRepo.UpdateAsync(privilege);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _privilegeRepo.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("/api/plans/{planId}/privileges")]
    public async Task<ActionResult> AssignPrivilegeToPlan(Guid planId, [FromBody] AssignPrivilegeDto dto)
    {
        var plan = await _planRepo.GetByIdAsync(planId);
        if (plan == null) return NotFound("Plan not found");
        var privilege = await _privilegeRepo.GetByIdAsync(dto.PrivilegeId);
        if (privilege == null) return NotFound("Privilege not found");
        var planPrivilege = new SubscriptionPlanPrivilege
        {
            SubscriptionPlanId = planId,
            PrivilegeId = dto.PrivilegeId,
            Value = dto.Value
        };
        await _planPrivilegeRepo.AddAsync(planPrivilege);
        return Ok();
    }

    [HttpDelete("/api/plans/{planId}/privileges/{privilegeId}")]
    public async Task<ActionResult> RemovePrivilegeFromPlan(Guid planId, Guid privilegeId)
    {
        var planPrivileges = await _planPrivilegeRepo.GetByPlanIdAsync(planId);
        var planPrivilege = planPrivileges.FirstOrDefault(pp => pp.PrivilegeId == privilegeId);
        if (planPrivilege == null) return NotFound();
        await _planPrivilegeRepo.DeleteAsync(planPrivilege.Id);
        return NoContent();
    }
}

public class AssignPrivilegeDto
{
    public Guid PrivilegeId { get; set; }
    public string Value { get; set; } = string.Empty;
} 