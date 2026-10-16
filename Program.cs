using AcxiomCRM.Data; using AcxiomCRM.Models; using AcxiomCRM.Services;
using Microsoft.AspNetCore.Identity; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddIdentity<AppUser, IdentityRole>(o => {
  o.Password.RequiredLength = 8; o.Password.RequireDigit = o.Password.RequireUppercase = o.Password.RequireLowercase = o.Password.RequireNonAlphanumeric = true;
  o.Lockout.MaxFailedAccessAttempts = 5; o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); o.Lockout.AllowedForNewUsers = true;
  o.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(o => {
  o.LoginPath = "/Account/Login"; o.AccessDeniedPath = "/Account/Denied"; o.Cookie.HttpOnly = true; o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
  o.Events.OnRedirectToLogin = c => { if (c.Request.Path.StartsWithSegments("/api")) c.Response.StatusCode = 401; else c.Response.Redirect(c.RedirectUri); return Task.CompletedTask; };
  o.Events.OnRedirectToAccessDenied = c => { if (c.Request.Path.StartsWithSegments("/api")) c.Response.StatusCode = 403; else c.Response.Redirect(c.RedirectUri); return Task.CompletedTask; };
});
builder.Services.AddHttpContextAccessor(); builder.Services.AddScoped<AuditService>();
builder.Services.AddControllersWithViews(o => o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
var app = builder.Build();
app.UseExceptionHandler("/Home/Error"); app.UseHsts(); app.UseHttpsRedirection(); app.UseStaticFiles();
app.UseAuthentication(); app.UseAuthorization();
app.MapDefaultControllerRoute();
using (var s = app.Services.CreateScope()) {   // migrate + seed roles and an admin
  var db = s.ServiceProvider.GetRequiredService<AppDbContext>(); db.Database.EnsureCreated();
  var rm = s.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(); var um = s.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
  foreach (var r in new[] { "Admin", "Manager", "SalesExecutive" }) if (!await rm.RoleExistsAsync(r)) await rm.CreateAsync(new IdentityRole(r));
  if (await um.FindByEmailAsync("admin@acxiom.com") == null) {
    var a = new AppUser { UserName = "admin@acxiom.com", Email = "admin@acxiom.com", FullName = "Administrator", EmailConfirmed = true };
    if ((await um.CreateAsync(a, "Admin@12345")).Succeeded) await um.AddToRoleAsync(a, "Admin");
  }
}
app.Run();
