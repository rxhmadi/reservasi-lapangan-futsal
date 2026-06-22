using FutsalReservation.Web.Filters;
using FutsalReservation.Web.Models;
using FutsalReservation.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FutsalReservation.Web.Controllers;

[ButuhAdmin]
public class UserController : Controller
{
    private readonly ApiClient _api;

    public UserController(ApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var hasil = await _api.GetAsync<List<UserVm>>("api/users");
        if (!hasil.Sukses)
        {
            TempData["Error"] = hasil.Pesan;
            return View(new List<UserVm>());
        }

        return View(hasil.Data);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var hasil = await _api.GetAsync<UserVm>($"api/users/{id}");
        if (!hasil.Sukses || hasil.Data == null)
        {
            TempData["Error"] = hasil.Pesan;
            return RedirectToAction(nameof(Index));
        }

        return View(hasil.Data);
    }
}
