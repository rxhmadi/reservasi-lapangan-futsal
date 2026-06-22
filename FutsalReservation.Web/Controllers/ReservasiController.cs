using FutsalReservation.Web.Filters;
using FutsalReservation.Web.Models;
using FutsalReservation.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FutsalReservation.Web.Controllers;

[ButuhLogin]
public class ReservasiController : Controller
{
    private readonly ApiClient _api;

    public ReservasiController(ApiClient api)
    {
        _api = api;
    }

    private bool IsAdmin => HttpContext.Session.GetString(SesiKeys.Role) == "Admin";

    public async Task<IActionResult> Index()
    {
        var hasil = await _api.GetAsync<List<ReservasiVm>>("api/reservasi");
        if (!hasil.Sukses)
        {
            TempData["Error"] = hasil.Pesan;
            return View(new List<ReservasiVm>());
        }

        ViewBag.IsAdmin = IsAdmin;
        return View(hasil.Data);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? lapanganId = null)
    {
        var vm = new ReservasiFormVm
        {
            JamMulai = 0,
            JamSelesai = 0
        };
        if (lapanganId.HasValue)
            vm.LapanganId = lapanganId.Value;
        await IsiDaftarLapangan(vm);
        return View(vm);
    }

    // dipakai halaman Buat Reservasi (AJAX) untuk menampilkan jam yang sudah terisi
    [HttpGet]
    public async Task<IActionResult> Ketersediaan(int lapanganId, string tanggal)
    {
        if (lapanganId <= 0 || string.IsNullOrWhiteSpace(tanggal))
            return Json(new { slotTerisi = Array.Empty<int>() });

        var hasil = await _api.GetAsync<KetersediaanVm>($"api/reservasi/ketersediaan?lapanganId={lapanganId}&tanggal={tanggal}");
        var slot = hasil.Sukses && hasil.Data != null ? hasil.Data.SlotTerisi : new List<int>();
        return Json(new { slotTerisi = slot });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservasiFormVm model)
    {
        if (model.JamSelesai <= model.JamMulai)
            ModelState.AddModelError(nameof(model.JamSelesai), "Jam selesai harus lebih besar dari jam mulai.");

        if (!ModelState.IsValid)
        {
            await IsiDaftarLapangan(model);
            return View(model);
        }

        var hasil = await _api.PostAsync<ReservasiVm>("api/reservasi", new
        {
            lapanganId = model.LapanganId,
            tanggal = model.Tanggal.ToString("yyyy-MM-dd"),
            jamMulai = model.JamMulai,
            jamSelesai = model.JamSelesai,
            catatan = model.Catatan
        });

        if (!hasil.Sukses)
        {
            ModelState.AddModelError(string.Empty, hasil.Pesan);
            await IsiDaftarLapangan(model);
            return View(model);
        }

        TempData["Sukses"] = "Reservasi berhasil dibuat. Silakan lanjutkan pembayaran.";
        return RedirectToAction(nameof(Detail), new { id = hasil.Data!.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var hasil = await _api.GetAsync<ReservasiVm>($"api/reservasi/{id}");
        if (!hasil.Sukses || hasil.Data == null)
        {
            TempData["Error"] = hasil.Pesan;
            return RedirectToAction(nameof(Index));
        }

        // daftar metode pembayaran aktif untuk form bayar
        var metode = await _api.GetAsync<List<MetodePembayaranVm>>("api/metodepembayaran?hanyaAktif=true");
        ViewBag.MetodePembayaran = metode.Sukses && metode.Data != null ? metode.Data : new List<MetodePembayaranVm>();

        ViewBag.IsAdmin = IsAdmin;
        return View(hasil.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Batal(int id)
    {
        var hasil = await _api.PutAsync<ReservasiVm>($"api/reservasi/{id}/status", new { status = "Dibatalkan" });
        if (!hasil.Sukses)
            TempData["Error"] = hasil.Pesan;
        else
            TempData["Sukses"] = "Reservasi telah dibatalkan.";

        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UbahStatus(int id, string status)
    {
        var hasil = await _api.PutAsync<ReservasiVm>($"api/reservasi/{id}/status", new { status });
        if (!hasil.Sukses)
            TempData["Error"] = hasil.Pesan;
        else
            TempData["Sukses"] = "Status reservasi diperbarui menjadi " + status + ".";

        return RedirectToAction(nameof(Detail), new { id });
    }

    private async Task IsiDaftarLapangan(ReservasiFormVm vm)
    {
        var hasil = await _api.GetAsync<List<LapanganVm>>("api/lapangan?hanyaAktif=true");
        vm.DaftarLapangan = hasil.Sukses && hasil.Data != null ? hasil.Data : new List<LapanganVm>();
    }
}
