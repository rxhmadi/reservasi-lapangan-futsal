using FutsalReservation.Web.Filters;
using FutsalReservation.Web.Models;
using FutsalReservation.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FutsalReservation.Web.Controllers;

[ButuhLogin]
public class PembayaranController : Controller
{
    private readonly ApiClient _api;
    private readonly FileUploadService _upload;

    public PembayaranController(ApiClient api, FileUploadService upload)
    {
        _api = api;
        _upload = upload;
    }

    private bool IsAdmin => HttpContext.Session.GetString(SesiKeys.Role) == "Admin";

    public async Task<IActionResult> Index()
    {
        var hasil = await _api.GetAsync<List<PembayaranVm>>("api/pembayaran");
        if (!hasil.Sukses)
        {
            TempData["Error"] = hasil.Pesan;
            return View(new List<PembayaranVm>());
        }

        ViewBag.IsAdmin = IsAdmin;
        return View(hasil.Data);
    }

    // user mengirim pembayaran dari halaman detail reservasi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bayar(int reservasiId, int metodePembayaranId, IFormFile? buktiTransfer)
    {
        if (metodePembayaranId <= 0)
        {
            TempData["Error"] = "Silakan pilih metode pembayaran.";
            return RedirectToAction("Detail", "Reservasi", new { id = reservasiId });
        }

        string? pathBukti = null;
        try
        {
            pathBukti = await _upload.SimpanGambarAsync(buktiTransfer, "bukti");
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Detail", "Reservasi", new { id = reservasiId });
        }

        var hasil = await _api.PostAsync<PembayaranVm>("api/pembayaran", new
        {
            reservasiId,
            metodePembayaranId,
            buktiTransfer = pathBukti
        });

        if (!hasil.Sukses)
            TempData["Error"] = hasil.Pesan;
        else if (!string.IsNullOrEmpty(hasil.Data?.KodeBooking))
            TempData["Sukses"] = "Kode booking dibuat: " + hasil.Data.KodeBooking + ". Tunjukkan saat membayar di lokasi.";
        else
            TempData["Sukses"] = "Pembayaran dikirim. Menunggu verifikasi dari admin.";

        return RedirectToAction("Detail", "Reservasi", new { id = reservasiId });
    }

    [HttpPost]
    [ButuhAdmin]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verifikasi(int id)
    {
        var hasil = await _api.PutAsync<PembayaranVm>($"api/pembayaran/{id}/verifikasi", null);
        if (!hasil.Sukses)
            TempData["Error"] = hasil.Pesan;
        else
            TempData["Sukses"] = "Pembayaran berhasil diverifikasi. Reservasi telah dikonfirmasi.";

        return RedirectToAction(nameof(Index));
    }
}
