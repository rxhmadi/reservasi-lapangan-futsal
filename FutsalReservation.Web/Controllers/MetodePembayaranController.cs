using FutsalReservation.Web.Filters;
using FutsalReservation.Web.Models;
using FutsalReservation.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FutsalReservation.Web.Controllers;

[ButuhAdmin]
public class MetodePembayaranController : Controller
{
    private readonly ApiClient _api;

    public MetodePembayaranController(ApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var hasil = await _api.GetAsync<List<MetodePembayaranVm>>("api/metodepembayaran");
        if (!hasil.Sukses)
        {
            TempData["Error"] = hasil.Pesan;
            return View(new List<MetodePembayaranVm>());
        }

        return View(hasil.Data);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new MetodePembayaranVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MetodePembayaranVm model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var hasil = await _api.PostAsync<MetodePembayaranVm>("api/metodepembayaran", ToRequest(model));
        if (!hasil.Sukses)
        {
            ModelState.AddModelError(string.Empty, hasil.Pesan);
            return View(model);
        }

        TempData["Sukses"] = "Metode pembayaran berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var hasil = await _api.GetAsync<MetodePembayaranVm>($"api/metodepembayaran/{id}");
        if (!hasil.Sukses || hasil.Data == null)
        {
            TempData["Error"] = hasil.Pesan;
            return RedirectToAction(nameof(Index));
        }

        return View(hasil.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MetodePembayaranVm model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var hasil = await _api.PutAsync<MetodePembayaranVm>($"api/metodepembayaran/{model.Id}", ToRequest(model));
        if (!hasil.Sukses)
        {
            ModelState.AddModelError(string.Empty, hasil.Pesan);
            return View(model);
        }

        TempData["Sukses"] = "Metode pembayaran berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var hasil = await _api.DeleteAsync($"api/metodepembayaran/{id}");
        if (!hasil.Sukses)
            TempData["Error"] = hasil.Pesan;
        else
            TempData["Sukses"] = "Metode pembayaran berhasil dihapus.";

        return RedirectToAction(nameof(Index));
    }

    private static object ToRequest(MetodePembayaranVm m) => new
    {
        tipe = m.Tipe,
        nama = m.Nama,
        nomorAkun = m.NomorAkun,
        atasNama = m.AtasNama,
        instruksi = m.Instruksi,
        aktif = m.Aktif
    };
}
