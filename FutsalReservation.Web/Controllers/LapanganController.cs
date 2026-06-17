using FutsalReservation.Web.Filters;
using FutsalReservation.Web.Models;
using FutsalReservation.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FutsalReservation.Web.Controllers;

[ButuhLogin]
public class LapanganController : Controller
{
    private readonly ApiClient _api;
    private readonly FileUploadService _upload;

    public LapanganController(ApiClient api, FileUploadService upload)
    {
        _api = api;
        _upload = upload;
    }

    private bool IsAdmin => HttpContext.Session.GetString(SesiKeys.Role) == "Admin";

    // katalog lapangan, bisa dilihat semua pengguna
    public async Task<IActionResult> Index()
    {
        var hasil = await _api.GetAsync<List<LapanganVm>>("api/lapangan");
        if (!hasil.Sukses)
        {
            TempData["Error"] = hasil.Pesan;
            return View(new List<LapanganVm>());
        }

        ViewBag.IsAdmin = IsAdmin;
        return View(hasil.Data);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var hasil = await _api.GetAsync<LapanganVm>($"api/lapangan/{id}");
        if (!hasil.Sukses || hasil.Data == null)
        {
            TempData["Error"] = hasil.Pesan;
            return RedirectToAction(nameof(Index));
        }

        ViewBag.IsAdmin = IsAdmin;
        return View(hasil.Data);
    }

    [HttpGet]
    [ButuhAdmin]
    public IActionResult Create()
    {
        return View(new LapanganVm());
    }

    [HttpPost]
    [ButuhAdmin]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LapanganVm model, IFormFile? gambarFile, List<IFormFile>? galeriFiles)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (!await ProsesUpload(model, gambarFile, galeriFiles))
            return View(model);

        var hasil = await _api.PostAsync<LapanganVm>("api/lapangan", ToRequest(model));
        if (!hasil.Sukses)
        {
            ModelState.AddModelError(string.Empty, hasil.Pesan);
            return View(model);
        }

        TempData["Sukses"] = "Lapangan baru berhasil ditambahkan.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [ButuhAdmin]
    public async Task<IActionResult> Edit(int id)
    {
        var hasil = await _api.GetAsync<LapanganVm>($"api/lapangan/{id}");
        if (!hasil.Sukses || hasil.Data == null)
        {
            TempData["Error"] = hasil.Pesan;
            return RedirectToAction(nameof(Index));
        }

        return View(hasil.Data);
    }

    [HttpPost]
    [ButuhAdmin]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LapanganVm model, IFormFile? gambarFile, List<IFormFile>? galeriFiles)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (!await ProsesUpload(model, gambarFile, galeriFiles))
            return View(model);

        var hasil = await _api.PutAsync<LapanganVm>($"api/lapangan/{model.Id}", ToRequest(model));
        if (!hasil.Sukses)
        {
            ModelState.AddModelError(string.Empty, hasil.Pesan);
            return View(model);
        }

        TempData["Sukses"] = "Data lapangan berhasil diperbarui.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ButuhAdmin]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var hasil = await _api.DeleteAsync($"api/lapangan/{id}");
        if (!hasil.Sukses)
            TempData["Error"] = hasil.Pesan;
        else
            TempData["Sukses"] = "Lapangan berhasil dihapus.";

        return RedirectToAction(nameof(Index));
    }

    // simpan gambar yang diunggah lalu perbarui field GambarUtama/Galeri pada model
    private async Task<bool> ProsesUpload(LapanganVm model, IFormFile? gambarFile, List<IFormFile>? galeriFiles)
    {
        try
        {
            var gambarBaru = await _upload.SimpanGambarAsync(gambarFile, "lapangan");
            if (gambarBaru != null)
                model.GambarUtama = gambarBaru;

            if (galeriFiles != null && galeriFiles.Count > 0)
            {
                var daftar = new List<string>();
                if (!string.IsNullOrWhiteSpace(model.Galeri))
                    daftar.AddRange(model.Galeri.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()));

                foreach (var f in galeriFiles)
                {
                    var path = await _upload.SimpanGambarAsync(f, "lapangan");
                    if (path != null)
                        daftar.Add(path);
                }

                model.Galeri = string.Join("\n", daftar);
            }

            return true;
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return false;
        }
    }

    private static object ToRequest(LapanganVm model) => new
    {
        nama = model.Nama,
        jenis = model.Jenis,
        hargaPerJam = model.HargaPerJam,
        deskripsi = model.Deskripsi,
        gambarUtama = model.GambarUtama,
        galeri = model.Galeri,
        aktif = model.Aktif
    };
}
