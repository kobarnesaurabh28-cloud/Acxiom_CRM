using AcxiomCRM.Data; using AcxiomCRM.Models; using AcxiomCRM.Services; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore; using System.ComponentModel.DataAnnotations; using System.Security.Claims;
namespace AcxiomCRM.Controllers.Api;
public record CustomerDto(int Id, string Name, string Email, string Phone, string Company, string Status);
public class CustomerInput { [Required, StringLength(100)] public string Name { get; set; } [Required, EmailAddress] public string Email { get; set; }
  [Required, RegularExpression(@"^[6-9]\d{9}$")] public string Phone { get; set; } public string Company { get; set; } }
[ApiController, Route("api/customers"), Authorize, IgnoreAntiforgeryToken]
public class CustomersApiController : ControllerBase {
  private readonly AppDbContext _db; private readonly AuditService _audit;
  public CustomersApiController(AppDbContext db, AuditService a) { _db = db; _audit = a; }
  private string Uid => User.FindFirstValue(ClaimTypes.NameIdentifier);
  private IQueryable<Customer> Scoped() => _db.Customers.Where(c => User.IsInRole("Admin") || User.IsInRole("Manager") || c.CreatedBy == Uid);
  static CustomerDto D(Customer c) => new(c.CustomerId, c.CustomerName, c.Email, c.Phone, c.CompanyName, c.Status);
  [HttpGet] public async Task<IEnumerable<CustomerDto>> Get() => (await Scoped().ToListAsync()).Select(D);
  [HttpGet("{id}")] public async Task<ActionResult<CustomerDto>> Get(int id) { var c = await Scoped().FirstOrDefaultAsync(x => x.CustomerId == id); return c == null ? NotFound() : D(c); }
  [HttpPost] public async Task<ActionResult<CustomerDto>> Post(CustomerInput i) {
    if (await _db.Customers.AnyAsync(c => c.Email == i.Email || c.Phone == i.Phone)) return Conflict(new { error = "Duplicate customer." });
    var c = new Customer { CustomerName = i.Name, Email = i.Email, Phone = i.Phone, CompanyName = i.Company, CreatedBy = Uid, CustomerCode = "CUST-" + Guid.NewGuid().ToString("N")[..6].ToUpper() };
    _db.Customers.Add(c); await _db.SaveChangesAsync(); await _audit.LogAsync("Create", "Customer", c.CustomerId.ToString(), null, c.CustomerName);
    return CreatedAtAction(nameof(Get), new { id = c.CustomerId }, D(c));
  }
  [HttpPut("{id}")] public async Task<IActionResult> Put(int id, CustomerInput i) {
    var c = await Scoped().FirstOrDefaultAsync(x => x.CustomerId == id); if (c == null) return NotFound();
    if (await _db.Customers.AnyAsync(x => x.CustomerId != id && (x.Email == i.Email || x.Phone == i.Phone))) return Conflict(new { error = "Duplicate customer." });
    var old = c.CustomerName; c.CustomerName = i.Name; c.Email = i.Email; c.Phone = i.Phone; c.CompanyName = i.Company;
    await _db.SaveChangesAsync(); await _audit.LogAsync("Update", "Customer", id.ToString(), old, c.CustomerName); return NoContent();
  }
  [HttpDelete("{id}"), Authorize(Roles = "Admin,Manager")] public async Task<IActionResult> Delete(int id) {
    var c = await Scoped().FirstOrDefaultAsync(x => x.CustomerId == id); if (c == null) return NotFound();
    c.Status = "Inactive"; await _db.SaveChangesAsync(); await _audit.LogAsync("Delete", "Customer", id.ToString()); return NoContent();
  }
}
