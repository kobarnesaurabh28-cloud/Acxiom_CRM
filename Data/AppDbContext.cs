using AcxiomCRM.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace AcxiomCRM.Data;
public class AppDbContext : IdentityDbContext<AppUser> {
  public AppDbContext(DbContextOptions<AppDbContext> o) : base(o) { }
  public DbSet<Customer> Customers { get; set; }
  public DbSet<Lead> Leads { get; set; }
  public DbSet<Opportunity> Opportunities { get; set; }
  public DbSet<FollowUp> FollowUps { get; set; }
  public DbSet<AuditLog> AuditLogs { get; set; }
  protected override void OnModelCreating(ModelBuilder b) {
    base.OnModelCreating(b);
    b.Entity<Customer>().HasIndex(c => c.Email).IsUnique();
    b.Entity<Customer>().HasIndex(c => c.Phone).IsUnique();
    b.Entity<Opportunity>().Property(o => o.Amount).HasPrecision(18, 2);
    b.Entity<Lead>().Property(l => l.ExpectedValue).HasPrecision(18, 2);
  }
}
