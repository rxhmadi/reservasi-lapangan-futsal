using FutsalReservation.Web.Filters;
using FutsalReservation.Web.Models;
using FutsalReservation.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace FutsalReservation.Web.Controllers;

[ButuhLogin]
public class HomeController : Controller
{
    private readonly ApiClient _api;

    public HomeController(ApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index(bool tolak = false)
    {
        if (tolak)
            TempData["Error"] = "Halaman tersebut hanya dapat diakses oleh Admin.";

        bool isAdmin = HttpContext.Session.GetString(SesiKeys.Role) == "Admin";

        var vm = new DashboardVm { IsAdmin = isAdmin };

        var ringkasan = await _api.GetAsync<RingkasanDto>("api/dashboard/ringkasan");
        if (ringkasan.Sukses && ringkasan.Data != null)
        {
            var d = ringkasan.Data;
            vm.TotalLapangan = d.TotalLapangan;
            vm.TotalReservasi = d.TotalReservasi;
            vm.MenungguKonfirmasi = d.MenungguKonfirmasi;
            vm.MenungguVerifikasi = d.MenungguVerifikasi;
            vm.TotalPendapatan = d.TotalPendapatan;
            vm.ReservasiAktif = d.ReservasiAktif;
            vm.MenungguBayar = d.MenungguBayar;
        }

        var reservasi = await _api.GetAsync<List<ReservasiVm>>("api/reservasi");
        if (reservasi.Sukses && reservasi.Data != null)
            vm.ReservasiTerbaru = reservasi.Data.Take(5).ToList();

        return View(vm);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }

    // bentuk respons ringkasan dari API (gabungan field admin & user)
    private class RingkasanDto
    {
        public int TotalLapangan { get; set; }
        public int TotalReservasi { get; set; }
        public int MenungguKonfirmasi { get; set; }
        public int MenungguVerifikasi { get; set; }
        public decimal TotalPendapatan { get; set; }
        public int ReservasiAktif { get; set; }
        public int MenungguBayar { get; set; }
    }
}
