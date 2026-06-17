using FutsalReservation.Web.Models;
using FutsalReservation.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FutsalReservation.Web.Controllers;

public class AccountController : Controller
{
    private readonly ApiClient _api;

    public AccountController(ApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SesiKeys.Token)))
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var hasil = await _api.PostAsync<AuthResult>("api/auth/login", new
        {
            email = model.Email,
            password = model.Password
        });

        if (!hasil.Sukses || hasil.Data == null)
        {
            ModelState.AddModelError(string.Empty, hasil.Pesan);
            return View(model);
        }

        SimpanSesi(hasil.Data);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SesiKeys.Token)))
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var hasil = await _api.PostAsync<AuthResult>("api/auth/register", new
        {
            nama = model.Nama,
            email = model.Email,
            password = model.Password
        });

        if (!hasil.Sukses || hasil.Data == null)
        {
            ModelState.AddModelError(string.Empty, hasil.Pesan);
            return View(model);
        }

        SimpanSesi(hasil.Data);
        TempData["Sukses"] = "Pendaftaran berhasil. Selamat datang, " + hasil.Data.Nama + ".";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    private void SimpanSesi(AuthResult data)
    {
        var sesi = HttpContext.Session;
        sesi.SetString(SesiKeys.Token, data.Token);
        sesi.SetString(SesiKeys.UserId, data.UserId.ToString());
        sesi.SetString(SesiKeys.Nama, data.Nama);
        sesi.SetString(SesiKeys.Role, data.Role);
    }
}
