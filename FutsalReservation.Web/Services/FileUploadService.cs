namespace FutsalReservation.Web.Services;

// Menyimpan file gambar yang diunggah ke wwwroot/uploads/<subfolder>
// dan mengembalikan path relatif (mis. /uploads/lapangan/abc.jpg).
public class FileUploadService
{
    private static readonly string[] EkstensiDiizinkan = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
    private const long UkuranMaks = 5 * 1024 * 1024; // 5 MB

    private readonly IWebHostEnvironment _env;

    public FileUploadService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string?> SimpanGambarAsync(IFormFile? file, string subfolder)
    {
        if (file == null || file.Length == 0)
            return null;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!EkstensiDiizinkan.Contains(ext))
            throw new InvalidOperationException("Format file tidak didukung. Gunakan gambar JPG, PNG, WEBP, atau GIF.");

        if (file.Length > UkuranMaks)
            throw new InvalidOperationException("Ukuran file maksimal 5 MB.");

        var folder = Path.Combine(_env.WebRootPath, "uploads", subfolder);
        Directory.CreateDirectory(folder);

        var namaFile = Guid.NewGuid().ToString("N") + ext;
        var fullPath = Path.Combine(folder, namaFile);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/{subfolder}/{namaFile}";
    }
}
