using Microsoft.EntityFrameworkCore;
using FutsalReservation.Api.Models;

namespace FutsalReservation.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Lapangan> Lapangan => Set<Lapangan>();
    public DbSet<Reservasi> Reservasi => Set<Reservasi>();
    public DbSet<Pembayaran> Pembayaran => Set<Pembayaran>();
    public DbSet<MetodePembayaran> MetodePembayaran => Set<MetodePembayaran>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.Nama).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(150).IsRequired();
            e.Property(u => u.Role).HasMaxLength(20);
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Lapangan>(e =>
        {
            e.Property(l => l.Nama).HasMaxLength(100).IsRequired();
            e.Property(l => l.Jenis).HasMaxLength(50);
            e.Property(l => l.HargaPerJam).HasColumnType("decimal(12,2)");
            e.Property(l => l.Deskripsi).HasMaxLength(500);
            e.Property(l => l.GambarUtama).HasMaxLength(400);
            e.Property(l => l.Galeri).HasMaxLength(2000);
        });

        modelBuilder.Entity<Reservasi>(e =>
        {
            e.Property(r => r.TotalHarga).HasColumnType("decimal(12,2)");
            e.Property(r => r.Status).HasMaxLength(20);
            e.Property(r => r.Catatan).HasMaxLength(300);

            e.HasOne(r => r.User)
                .WithMany(u => u.DaftarReservasi)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(r => r.Lapangan)
                .WithMany(l => l.DaftarReservasi)
                .HasForeignKey(r => r.LapanganId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Pembayaran>(e =>
        {
            e.Property(p => p.Jumlah).HasColumnType("decimal(12,2)");
            e.Property(p => p.Metode).HasMaxLength(150);
            e.Property(p => p.Status).HasMaxLength(30);
            e.Property(p => p.KodeBooking).HasMaxLength(30);
            e.Property(p => p.BuktiTransfer).HasMaxLength(300);

            e.HasOne(p => p.Reservasi)
                .WithOne(r => r.Pembayaran)
                .HasForeignKey<Pembayaran>(p => p.ReservasiId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<MetodePembayaran>(e =>
        {
            e.Property(m => m.Tipe).HasMaxLength(20).IsRequired();
            e.Property(m => m.Nama).HasMaxLength(80).IsRequired();
            e.Property(m => m.NomorAkun).HasMaxLength(60);
            e.Property(m => m.AtasNama).HasMaxLength(100);
            e.Property(m => m.Instruksi).HasMaxLength(300);
        });
    }
}
