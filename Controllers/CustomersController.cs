using AcxiomCRM.Data; using AcxiomCRM.Models; using AcxiomCRM.Services; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore; using System.Security.Claims;
namespace AcxiomCRM.Controllers;
[Authorize]
public class CustomersController : Controller {
  private readonly AppDbContext _db; private readonly AuditService _audit;
  public CustomersController(AppDbContext db, AuditService a) { _db = db; _audit = a; }
  private string Uid => User.FindFirstValue(ClaimTypes.NameIdentifier);
  private IQueryable<Customer> Scoped() => _db.Customers.Where(c => User.IsInRole("Admin") || User.IsInRole("Manager") || c.CreatedBy == Uid);
  public async Task<IActionResult> Index(string q) {
    var s = Scoped(); if (!string.IsNullOrWhiteSpace(q)) s = s.Where(c => c.CustomerName.Contains(q) || c.Email.Contains(q) || c.Phone.Contains(q) || c.CompanyName.Contains(q));
    return View(await s.OrderByDescending(c => c.CreatedDate).Take(200).ToListAsync());
  }
  public IActionResult Create() => View(new Customer());
  [HttpPost] public async Task<IActionResult> Create(Customer m) {
    if (await _db.Customers.AnyAsync(c => c.Email == m.Email)) ModelState.AddModelError(nameof(m.Email), "Email already exists.");
    if (await _db.Customers.AnyAsync(c => c.Phone == m.Phone)) ModelState.AddModelError(nameof(m.Phone), "Phone already exists.");
    if (!ModelState.IsValid) return View(m);
    m.CreatedBy = Uid; m.CustomerCode = "CUST-" + Guid.NewGuid().ToString("N")[..6].ToUpper(); m.CreatedDate = DateTime.UtcNow;
    _db.Customers.Add(m); await _db.SaveChangesAsync(); await _audit.LogAsync("Create", "Customer", m.CustomerId.ToString(), null, m.CustomerName);
    return RedirectToAction(nameof(Index));
  }
  [HttpPost, Authorize(Roles = "Admin,Manager")] public async Task<IActionResult> Deactivate(int id) {
    var c = await Scoped().FirstOrDefaultAsync(x => x.CustomerId == id); if (c == null) return NotFound();
    var old = c.Status; c.Status = "Inactive"; await _db.SaveChangesAsync(); await _audit.LogAsync("Delete", "Customer", id.ToString(), old, c.Status);
    return RedirectToAction(nameof(Index));
  }
}
