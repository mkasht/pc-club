using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PcClub.Models;

namespace PcClub.Controllers;

[Route("account")]
public class AccountController(UserManager<IdentityUser> users, SignInManager<IdentityUser> signIn, IWebHostEnvironment env) : Controller
{
    [HttpGet("register")]
    public IActionResult Register() => View(new RegisterForm());
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterForm form)
    {
        if (!ModelState.IsValid) return View(form);
        var user = new IdentityUser { UserName = form.Email, Email = form.Email };
        var result = await users.CreateAsync(user, form.Password);
        if (!result.Succeeded) { AddErrors(result); return View(form); }
        var roleResult = await users.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded) throw new InvalidOperationException("Не удалось назначить роль.");
        await signIn.SignInAsync(user, false);
        TempData["Notice"] = "Аккаунт создан. Добро пожаловать в клуб.";
        return Redirect("/");
    }
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl) => View(new LoginForm { ReturnUrl = returnUrl });
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginForm form)
    {
        if (!ModelState.IsValid) return View(form);
        var result = await signIn.PasswordSignInAsync(form.Email, form.Password, false, true);
        if (result.Succeeded) return !string.IsNullOrEmpty(form.ReturnUrl) && Url.IsLocalUrl(form.ReturnUrl) ? LocalRedirect(form.ReturnUrl) : Redirect("/");
        ModelState.AddModelError("", result.IsLockedOut ? "Слишком много попыток. Повторите вход через 10 минут." : "Не удалось войти. Проверьте email и пароль.");
        return View(form);
    }
    [Authorize, HttpPost("logout")]
    public async Task<IActionResult> Logout() { await signIn.SignOutAsync(); return Redirect("/"); }
    [HttpGet("denied")]
    public IActionResult Denied() { Response.StatusCode = 403; return View("~/Views/Home/Message.cshtml", new MessagePage("Доступ ограничен", "Для этой страницы нужны права администратора.")); }

    // Initial admin setup is available only on this computer in Development.
    private async Task<bool> CanSetupAsync()
    {
        var ip = HttpContext.Connection.RemoteIpAddress;
        if (ip?.IsIPv4MappedToIPv6 == true) ip = ip.MapToIPv4();
        return env.IsDevelopment() && ip is not null && IPAddress.IsLoopback(ip) && (await users.GetUsersInRoleAsync("Admin")).Count == 0;
    }
    [HttpGet("/setup")]
    public async Task<IActionResult> Setup()
    {
        if (!await CanSetupAsync()) return NotFound();
        ViewData["Setup"] = true;
        return View("Register", new RegisterForm());
    }
    [HttpPost("/setup")]
    public async Task<IActionResult> Setup(RegisterForm form)
    {
        if (!await CanSetupAsync()) return NotFound();
        ViewData["Setup"] = true;
        if (!ModelState.IsValid) return View("Register", form);
        var user = new IdentityUser { UserName = form.Email, Email = form.Email };
        var result = await users.CreateAsync(user, form.Password);
        if (!result.Succeeded) { AddErrors(result); return View("Register", form); }
        var roleResult = await users.AddToRoleAsync(user, "Admin");
        if (!roleResult.Succeeded) throw new InvalidOperationException("Не удалось назначить администратора.");
        await signIn.SignInAsync(user, false);
        return Redirect("/admin");
    }
    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors) ModelState.AddModelError("", error.Code switch
        {
            "DuplicateUserName" or "DuplicateEmail" => "Этот email уже используется.",
            "PasswordTooShort" => "Пароль должен содержать минимум 10 символов.",
            _ => "Не удалось сохранить аккаунт. Проверьте email и требования к паролю."
        });
    }
}
