using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace FutsalReservation.Web.Services;

// hasil pemanggilan API dalam bentuk yang gampang dipakai di controller
public class HasilApi<T>
{
    public bool Sukses { get; set; }
    public T? Data { get; set; }
    public string Pesan { get; set; } = string.Empty;
    public HttpStatusCode KodeStatus { get; set; }
}

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _ctx;

    private static readonly JsonSerializerOptions _opsi = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient http, IHttpContextAccessor ctx)
    {
        _http = http;
        _ctx = ctx;
        PasangToken();
    }

    private void PasangToken()
    {
        var token = _ctx.HttpContext?.Session.GetString(SesiKeys.Token);
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<HasilApi<T>> GetAsync<T>(string url)
    {
        try
        {
            var resp = await _http.GetAsync(url);
            return await BacaHasil<T>(resp);
        }
        catch (Exception ex)
        {
            return Gagal<T>(ex);
        }
    }

    public async Task<HasilApi<T>> PostAsync<T>(string url, object? body)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync(url, body);
            return await BacaHasil<T>(resp);
        }
        catch (Exception ex)
        {
            return Gagal<T>(ex);
        }
    }

    public async Task<HasilApi<T>> PutAsync<T>(string url, object? body)
    {
        try
        {
            HttpResponseMessage resp = body == null
                ? await _http.PutAsync(url, null)
                : await _http.PutAsJsonAsync(url, body);
            return await BacaHasil<T>(resp);
        }
        catch (Exception ex)
        {
            return Gagal<T>(ex);
        }
    }

    public async Task<HasilApi<bool>> DeleteAsync(string url)
    {
        try
        {
            var resp = await _http.DeleteAsync(url);
            if (resp.IsSuccessStatusCode)
                return new HasilApi<bool> { Sukses = true, Data = true, KodeStatus = resp.StatusCode };

            return new HasilApi<bool>
            {
                Sukses = false,
                Pesan = await AmbilPesanError(resp),
                KodeStatus = resp.StatusCode
            };
        }
        catch (Exception ex)
        {
            return Gagal<bool>(ex);
        }
    }

    private async Task<HasilApi<T>> BacaHasil<T>(HttpResponseMessage resp)
    {
        if (resp.IsSuccessStatusCode)
        {
            var data = await resp.Content.ReadFromJsonAsync<T>(_opsi);
            return new HasilApi<T> { Sukses = true, Data = data, KodeStatus = resp.StatusCode };
        }

        return new HasilApi<T>
        {
            Sukses = false,
            Pesan = await AmbilPesanError(resp),
            KodeStatus = resp.StatusCode
        };
    }

    // API mengembalikan error dalam format { "message": "..." }
    private static async Task<string> AmbilPesanError(HttpResponseMessage resp)
    {
        try
        {
            var isi = await resp.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(isi))
                return "Terjadi kesalahan pada server.";

            using var doc = JsonDocument.Parse(isi);
            if (doc.RootElement.TryGetProperty("message", out var pesan))
                return pesan.GetString() ?? "Terjadi kesalahan.";

            return "Terjadi kesalahan pada server.";
        }
        catch
        {
            return "Tidak dapat memproses respons dari server.";
        }
    }

    private static HasilApi<T> Gagal<T>(Exception ex) => new()
    {
        Sukses = false,
        Pesan = "Tidak dapat terhubung ke server API. Pastikan layanan API sedang berjalan. (" + ex.Message + ")"
    };
}
