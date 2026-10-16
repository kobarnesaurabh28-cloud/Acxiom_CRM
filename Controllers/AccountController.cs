using AcxiomCRM.Models; using AcxiomCRM.Services; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Identity; using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
namespace AcxiomCRM.Controllers;
public class LoginVm { [Required, EmailAddress] public string Email { get; set; } [Required, DataType(DataType.Password)] public string Password { get; set; } }
public class RegisterVm { [Required, StringLength(100)] public string FullName { get; set; } [Required, EmailAddress] public string Email { get; set; }
  [Required, DataType(DataType.Password)] public string Password { get; set; } [Compare("Password"), DataType(DataType.Password)] public string ConfirmPassword { get; set; } }
public class AccountController : Controller {
  private readonly SignInManager<AppUser> _sm; private readonly UserManager<AppUser> _um; private readonly AuditService _audit;
  public AccountController(SignInManager<AppUser> sm, UserManager<AppUser> um, AuditService a) { _sm = sm; _um = um; _audit = a; }
  [AllowAnonymous] public IActionResult Login() => View();
  [AllowAnonymous, HttpPost] public async Task<IActionResult> Login(LoginVm m) {
    if (!ModelState.IsValid) return View(m);
    var u = await _um.FindByEmailAsync(m.Email);
    if (u != null && !u.IsActive) { ModelState.AddModelError("", "Account is deactivated."); return View(m); }
    var r = await _sm.PasswordSignInAsync(m.Email, m.Password, false, lockoutOnFailure: true);
    await _audit.LogAsync(r.Succeeded ? "Login" : r.IsLockedOut ? "Lockout" : "FailedLogin", "User", u?.Id, userId: u?.Id);
    if (r.Succeeded) return RedirectToAction("Index", "Home");
    ModelState.AddModelError("", r.IsLockedOut ? "Account locked. Try again later." : "Invalid credentials."); return View(m);
  }
  [AllowAnonymous] public IActionResult Register() => View();
  [AllowAnonymous, HttpPost] public async Task<IActionResult> Register(RegisterVm m) {
    if (!ModelState.IsValid) return View(m);
    var u = new AppUser { UserName = m.Email, Email = m.Email, FullName = m.FullName };
    var r = await _um.CreateAsync(u, m.Password);
    if (!r.Succeeded) { foreach (var e in r.Errors) ModelState.AddModelError("", e.Description); return View(m); }
    await _um.AddToRoleAsync(u, "SalesExecutive"); await _audit.LogAsync("Register", "User", u.Id, userId: u.Id);
    await _sm.SignInAsync(u, false); return RedirectToAction("Index", "Home");
  }
  [HttpPost, Authorize] public async Task<IActionResult> Logout() { await _audit.LogAsync("Logout", "User"); await _sm.SignOutAsync(); return RedirectToAction(nameof(Login)); }
  public IActionResult Denied() => View();
}
