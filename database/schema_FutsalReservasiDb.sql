IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615073626_InitialCreate'
)
BEGIN
    CREATE TABLE [Lapangan] (
        [Id] int NOT NULL IDENTITY,
        [Nama] nvarchar(100) NOT NULL,
        [Jenis] nvarchar(50) NOT NULL,
        [HargaPerJam] decimal(12,2) NOT NULL,
        [Deskripsi] nvarchar(500) NULL,
        [Aktif] bit NOT NULL,
        [DibuatPada] datetime2 NOT NULL,
        CONSTRAINT [PK_Lapangan] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615073626_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [Nama] nvarchar(100) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [Role] nvarchar(20) NOT NULL,
        [DibuatPada] datetime2 NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615073626_InitialCreate'
)
BEGIN
    CREATE TABLE [Reservasi] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [LapanganId] int NOT NULL,
        [Tanggal] datetime2 NOT NULL,
        [JamMulai] int NOT NULL,
        [JamSelesai] int NOT NULL,
        [TotalHarga] decimal(12,2) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [Catatan] nvarchar(300) NULL,
        [DibuatPada] datetime2 NOT NULL,
        CONSTRAINT [PK_Reservasi] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Reservasi_Lapangan_LapanganId] FOREIGN KEY ([LapanganId]) REFERENCES [Lapangan] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Reservasi_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615073626_InitialCreate'
)
BEGIN
    CREATE TABLE [Pembayaran] (
        [Id] int NOT NULL IDENTITY,
        [ReservasiId] int NOT NULL,
        [Jumlah] decimal(12,2) NOT NULL,
        [Metode] nvarchar(50) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [TanggalBayar] datetime2 NULL,
        [DibuatPada] datetime2 NOT NULL,
        CONSTRAINT [PK_Pembayaran] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Pembayaran_Reservasi_ReservasiId] FOREIGN KEY ([ReservasiId]) REFERENCES [Reservasi] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615073626_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Pembayaran_ReservasiId] ON [Pembayaran] ([ReservasiId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615073626_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reservasi_LapanganId] ON [Reservasi] ([LapanganId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615073626_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Reservasi_UserId] ON [Reservasi] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615073626_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615073626_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260615073626_InitialCreate', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615085247_TambahGambarLapangan'
)
BEGIN
    ALTER TABLE [Lapangan] ADD [Galeri] nvarchar(2000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615085247_TambahGambarLapangan'
)
BEGIN
    ALTER TABLE [Lapangan] ADD [GambarUtama] nvarchar(400) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615085247_TambahGambarLapangan'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260615085247_TambahGambarLapangan', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615091246_MetodePembayaranDanKodeBooking'
)
BEGIN
    ALTER TABLE [Pembayaran] ADD [KodeBooking] nvarchar(30) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615091246_MetodePembayaranDanKodeBooking'
)
BEGIN
    CREATE TABLE [MetodePembayaran] (
        [Id] int NOT NULL IDENTITY,
        [Tipe] nvarchar(20) NOT NULL,
        [Nama] nvarchar(80) NOT NULL,
        [NomorAkun] nvarchar(60) NULL,
        [AtasNama] nvarchar(100) NULL,
        [Instruksi] nvarchar(300) NULL,
        [Aktif] bit NOT NULL,
        [DibuatPada] datetime2 NOT NULL,
        CONSTRAINT [PK_MetodePembayaran] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260615091246_MetodePembayaranDanKodeBooking'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260615091246_MetodePembayaranDanKodeBooking', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617220226_PerpanjangKolomMetode'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Pembayaran]') AND [c].[name] = N'Metode');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Pembayaran] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Pembayaran] ALTER COLUMN [Metode] nvarchar(150) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617220226_PerpanjangKolomMetode'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260617220226_PerpanjangKolomMetode', N'8.0.28');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617222420_TambahBuktiTransfer'
)
BEGIN
    ALTER TABLE [Pembayaran] ADD [BuktiTransfer] nvarchar(300) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260617222420_TambahBuktiTransfer'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260617222420_TambahBuktiTransfer', N'8.0.28');
END;
GO

COMMIT;
GO

