using AcxiomCRM.Data; using AcxiomCRM.Models; using System.Security.Claims;
namespace AcxiomCRM.Services;
public class AuditService {
  private readonly AppDbContext _db; private readonly IHttpContextAccessor _http;
  public AuditService(AppDbContext db, IHttpContextAccessor http) { _db = db; _http = http; }
  public async Task LogAsync(string action, string entity, string recordId = null, string oldV = null, string newV = null, string userId = null) {
    var ctx = _http.HttpContext;
    _db.AuditLogs.Add(new AuditLog { UserId = userId ?? ctx?.User.FindFirstValue(ClaimTypes.NameIdentifier), Action = action, EntityName = entity,
      RecordId = recordId, OldValue = oldV, NewValue = newV, IpAddress = ctx?.Connection.RemoteIpAddress?.ToString() });
    await _db.SaveChangesAsync();   // never pass passwords/secrets in oldV/newV
  }
}
