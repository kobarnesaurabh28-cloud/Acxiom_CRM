using AcxiomCRM.Data; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore; using System.Security.Claims;
namespace AcxiomCRM.Controllers;
[Authorize]
public class HomeController : Controller {
  private readonly AppDbContext _db; public HomeController(AppDbContext db) { _db = db; }
  public async Task<IActionResult> Index() {
    var uid = User.FindFirstValue(ClaimTypes.NameIdentifier); bool all = User.IsInRole("Admin") || User.IsInRole("Manager");
    var cust = _db.Customers.Where(c => all || c.CreatedBy == uid);
    var leads = _db.Leads.Where(l => all || l.AssignedTo == uid);
    var opps = _db.Opportunities.Where(o => all || o.AssignedTo == uid);
    ViewBag.Cards = new Dictionary<string, object> {
      ["Total Customers"] = await cust.CountAsync(), ["Total Leads"] = await leads.CountAsync(),
      ["Open Leads"] = await leads.CountAsync(l => l.Status != "Lost" && l.Status != "Converted"),
      ["Total Opportunities"] = await opps.CountAsync(), ["Open Opportunities"] = await opps.CountAsync(o => o.Status == "Open"),
      ["Won"] = await opps.CountAsync(o => o.Stage == "Won"), ["Lost"] = await opps.CountAsync(o => o.Stage == "Lost"),
      ["Pipeline Value"] = (await opps.Where(o => o.Status == "Open").SumAsync(o => (double?)o.Amount) ?? 0).ToString("N0") };
    ViewBag.LeadChart = await leads.GroupBy(l => l.Status).Select(g => new { k = g.Key, v = g.Count() }).ToListAsync();
    ViewBag.OppChart = await opps.GroupBy(o => o.Stage).Select(g => new { k = g.Key, v = g.Count() }).ToListAsync();
    return View();
  }
  [AllowAnonymous] public IActionResult Error() => Content("Something went wrong.");
}
