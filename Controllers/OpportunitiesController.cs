using AcxiomCRM.Data; using AcxiomCRM.Models; using AcxiomCRM.Services; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.Rendering; using Microsoft.EntityFrameworkCore; using System.Security.Claims;
namespace AcxiomCRM.Controllers;
[Authorize]
public class OpportunitiesController : Controller {
  private readonly AppDbContext _db; private readonly AuditService _audit;
  public OpportunitiesController(AppDbContext db, AuditService a) { _db = db; _audit = a; }
  private string Uid => User.FindFirstValue(ClaimTypes.NameIdentifier);
  public async Task<IActionResult> Index() {
    bool all = User.IsInRole("Admin") || User.IsInRole("Manager");
    return View(await _db.Opportunities.Include(o => o.Customer).Where(o => all || o.AssignedTo == Uid).OrderBy(o => o.ExpectedCloseDate).ToListAsync());
  }
  private async Task Load() => ViewBag.Customers = new SelectList(await _db.Customers.ToListAsync(), "CustomerId", "CustomerName");
  public async Task<IActionResult> Create() { await Load(); return View(new Opportunity()); }
  [HttpPost] public async Task<IActionResult> Create(Opportunity m) {
    ModelState.Remove(nameof(m.Customer));
    if (!await _db.Customers.AnyAsync(c => c.CustomerId == m.CustomerId)) ModelState.AddModelError(nameof(m.CustomerId), "Select a valid customer.");
    if (!new[] { "Qualification", "Proposal", "Negotiation", "Won", "Lost" }.Contains(m.Stage)) ModelState.AddModelError(nameof(m.Stage), "Invalid stage.");
    if (!ModelState.IsValid) { await Load(); return View(m); }
    m.AssignedTo = Uid; m.Status = m.Stage is "Won" or "Lost" ? "Closed" : "Open";
    _db.Opportunities.Add(m); await _db.SaveChangesAsync(); await _audit.LogAsync("Create", "Opportunity", m.OpportunityId.ToString(), null, $"{m.OpportunityName}:{m.Amount}");
    return RedirectToAction(nameof(Index));
  }
}
