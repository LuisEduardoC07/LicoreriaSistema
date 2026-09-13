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
CREATE TABLE [Categorias] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(100) NOT NULL,
    [Descripcion] nvarchar(250) NOT NULL,
    [Activa] bit NOT NULL,
    CONSTRAINT [PK_Categorias] PRIMARY KEY ([Id])
);

CREATE TABLE [Sucursales] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(150) NOT NULL,
    [Direccion] nvarchar(250) NOT NULL,
    [Telefono] nvarchar(30) NOT NULL,
    [Activa] bit NOT NULL,
    CONSTRAINT [PK_Sucursales] PRIMARY KEY ([Id])
);

CREATE TABLE [Productos] (
    [Id] int NOT NULL IDENTITY,
    [Nombre] nvarchar(150) NOT NULL,
    [Codigo] nvarchar(50) NOT NULL,
    [Descripcion] nvarchar(max) NULL,
    [PrecioCompra] decimal(18,2) NOT NULL,
    [PrecioVenta] decimal(18,2) NOT NULL,
    [CategoriaId] int NOT NULL,
    [Activo] bit NOT NULL,
    CONSTRAINT [PK_Productos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Productos_Categorias_CategoriaId] FOREIGN KEY ([CategoriaId]) REFERENCES [Categorias] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [InventariosSucursal] (
    [Id] int NOT NULL IDENTITY,
    [ProductoId] int NOT NULL,
    [SucursalId] int NOT NULL,
    [Cantidad] decimal(18,2) NOT NULL,
    [StockMinimo] decimal(18,2) NOT NULL,
    [StockMaximo] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_InventariosSucursal] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InventariosSucursal_Productos_ProductoId] FOREIGN KEY ([ProductoId]) REFERENCES [Productos] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_InventariosSucursal_Sucursales_SucursalId] FOREIGN KEY ([SucursalId]) REFERENCES [Sucursales] ([Id]) ON DELETE NO ACTION
);

CREATE UNIQUE INDEX [IX_InventariosSucursal_ProductoId_SucursalId] ON [InventariosSucursal] ([ProductoId], [SucursalId]);

CREATE INDEX [IX_InventariosSucursal_SucursalId] ON [InventariosSucursal] ([SucursalId]);

CREATE INDEX [IX_Productos_CategoriaId] ON [Productos] ([CategoriaId]);

CREATE UNIQUE INDEX [IX_Productos_Codigo] ON [Productos] ([Codigo]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260912210125_InitialCreate', N'10.0.12');

COMMIT;
GO

