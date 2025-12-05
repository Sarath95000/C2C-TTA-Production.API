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
CREATE TABLE [Holidays] (
    [HolidayDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Holidays] PRIMARY KEY ([HolidayDate])
);

CREATE TABLE [SystemSettings] (
    [SettingsId] int NOT NULL IDENTITY,
    [DepartureLabel] nvarchar(max) NOT NULL,
    [ArrivalLabel] nvarchar(max) NOT NULL,
    [TripPrice] decimal(10,2) NOT NULL,
    [AllocateForCurrentMonth] bit NOT NULL,
    [UserListViewEnabled] bit NOT NULL,
    CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([SettingsId])
);

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Email] nvarchar(255) NOT NULL,
    [Role] int NOT NULL,
    [SendEmail] bit NOT NULL,
    [Pin] nvarchar(4) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [Allocations] (
    [Id] int NOT NULL IDENTITY,
    [AllocationDate] datetime2 NOT NULL,
    [BookerUserId] int NOT NULL,
    [TripType] nvarchar(max) NULL,
    CONSTRAINT [PK_Allocations] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Allocations_Users_BookerUserId] FOREIGN KEY ([BookerUserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [Plans] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [Year] int NOT NULL,
    [Month] int NOT NULL,
    CONSTRAINT [PK_Plans] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Plans_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AllocationTravelers] (
    [AllocationId] int NOT NULL,
    [UserId] int NOT NULL,
    CONSTRAINT [PK_AllocationTravelers] PRIMARY KEY ([AllocationId], [UserId]),
    CONSTRAINT [FK_AllocationTravelers_Allocations_AllocationId] FOREIGN KEY ([AllocationId]) REFERENCES [Allocations] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AllocationTravelers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [PlanSelectedDays] (
    [PlanId] int NOT NULL,
    [Day] int NOT NULL,
    CONSTRAINT [PK_PlanSelectedDays] PRIMARY KEY ([PlanId], [Day]),
    CONSTRAINT [FK_PlanSelectedDays_Plans_PlanId] FOREIGN KEY ([PlanId]) REFERENCES [Plans] ([Id]) ON DELETE CASCADE
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SettingsId', N'AllocateForCurrentMonth', N'ArrivalLabel', N'DepartureLabel', N'TripPrice', N'UserListViewEnabled') AND [object_id] = OBJECT_ID(N'[SystemSettings]'))
    SET IDENTITY_INSERT [SystemSettings] ON;
INSERT INTO [SystemSettings] ([SettingsId], [AllocateForCurrentMonth], [ArrivalLabel], [DepartureLabel], [TripPrice], [UserListViewEnabled])
VALUES (1, CAST(1 AS bit), N'Arrival', N'Departure', 240.0, CAST(1 AS bit));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SettingsId', N'AllocateForCurrentMonth', N'ArrivalLabel', N'DepartureLabel', N'TripPrice', N'UserListViewEnabled') AND [object_id] = OBJECT_ID(N'[SystemSettings]'))
    SET IDENTITY_INSERT [SystemSettings] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Email', N'Name', N'Pin', N'Role', N'SendEmail') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([Id], [Email], [Name], [Pin], [Role], [SendEmail])
VALUES (1, N'alloc-admin@trip.com', N'Allocation Admin', N'0000', 0, CAST(1 AS bit)),
(2, N'sys-admin@trip.com', N'System Admin', N'0000', 2, CAST(1 AS bit)),
(3, N'gayathri@trip.com', N'Gayathri', N'1001', 1, CAST(1 AS bit)),
(4, N'gokul@trip.com', N'Gokul', N'1001', 1, CAST(1 AS bit)),
(5, N'kiruthika@trip.com', N'Kiruthika', N'1001', 1, CAST(1 AS bit)),
(6, N'narendran@trip.com', N'Narendran', N'1001', 1, CAST(1 AS bit)),
(7, N'navin@trip.com', N'Navin', N'1001', 1, CAST(1 AS bit)),
(8, N'sangeetha@trip.com', N'Sangeetha', N'1001', 1, CAST(1 AS bit)),
(9, N'sarath@trip.com', N'Sarath', N'1001', 1, CAST(1 AS bit)),
(10, N'shalini@trip.com', N'Shalini', N'1001', 1, CAST(1 AS bit));
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Email', N'Name', N'Pin', N'Role', N'SendEmail') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;

CREATE INDEX [IX_Allocations_BookerUserId] ON [Allocations] ([BookerUserId]);

CREATE INDEX [IX_AllocationTravelers_UserId] ON [AllocationTravelers] ([UserId]);

CREATE INDEX [IX_Plans_UserId] ON [Plans] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251016175620_InitialCreate', N'9.0.10');

ALTER TABLE [Users] ADD [CreatedBy] int NULL;

ALTER TABLE [Users] ADD [CreatedTime] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [Users] ADD [UpdatedBy] int NULL;

ALTER TABLE [Users] ADD [UpdatedTime] datetime2 NULL;

ALTER TABLE [SystemSettings] ADD [UpdatedBy] int NULL;

ALTER TABLE [SystemSettings] ADD [UpdatedTime] datetime2 NULL;

ALTER TABLE [Plans] ADD [CreatedTime] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [Plans] ADD [HasUpdatedSinceAllocation] bit NOT NULL DEFAULT CAST(0 AS bit);

ALTER TABLE [Plans] ADD [UpdatedTime] datetime2 NULL;

ALTER TABLE [Holidays] ADD [CreatedBy] int NOT NULL DEFAULT 0;

ALTER TABLE [Holidays] ADD [CreatedTime] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [Allocations] ADD [CreatedBy] int NOT NULL DEFAULT 0;

ALTER TABLE [Allocations] ADD [CreatedTime] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';

ALTER TABLE [Allocations] ADD [UpdatedBy] int NULL;

ALTER TABLE [Allocations] ADD [UpdatedTime] datetime2 NULL;

UPDATE [SystemSettings] SET [UpdatedBy] = NULL, [UpdatedTime] = NULL
WHERE [SettingsId] = 1;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [CreatedBy] = NULL, [CreatedTime] = '0001-01-01T00:00:00.0000000', [UpdatedBy] = NULL, [UpdatedTime] = NULL
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [CreatedBy] = NULL, [CreatedTime] = '0001-01-01T00:00:00.0000000', [UpdatedBy] = NULL, [UpdatedTime] = NULL
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [CreatedBy] = NULL, [CreatedTime] = '0001-01-01T00:00:00.0000000', [UpdatedBy] = NULL, [UpdatedTime] = NULL
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [CreatedBy] = NULL, [CreatedTime] = '0001-01-01T00:00:00.0000000', [UpdatedBy] = NULL, [UpdatedTime] = NULL
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [CreatedBy] = NULL, [CreatedTime] = '0001-01-01T00:00:00.0000000', [UpdatedBy] = NULL, [UpdatedTime] = NULL
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [CreatedBy] = NULL, [CreatedTime] = '0001-01-01T00:00:00.0000000', [UpdatedBy] = NULL, [UpdatedTime] = NULL
WHERE [Id] = 6;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [CreatedBy] = NULL, [CreatedTime] = '0001-01-01T00:00:00.0000000', [UpdatedBy] = NULL, [UpdatedTime] = NULL
WHERE [Id] = 7;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [CreatedBy] = NULL, [CreatedTime] = '0001-01-01T00:00:00.0000000', [UpdatedBy] = NULL, [UpdatedTime] = NULL
WHERE [Id] = 8;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [CreatedBy] = NULL, [CreatedTime] = '0001-01-01T00:00:00.0000000', [UpdatedBy] = NULL, [UpdatedTime] = NULL
WHERE [Id] = 9;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [CreatedBy] = NULL, [CreatedTime] = '0001-01-01T00:00:00.0000000', [UpdatedBy] = NULL, [UpdatedTime] = NULL
WHERE [Id] = 10;
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251016211034_InitialCreate1', N'9.0.10');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251017052744_InitialCreate2', N'9.0.10');

UPDATE [Users] SET [SendEmail] = CAST(0 AS bit)
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [SendEmail] = CAST(0 AS bit)
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251021195656_InitialCreate3', N'9.0.10');

DELETE FROM [Users]
WHERE [Id] = 3;
SELECT @@ROWCOUNT;


DELETE FROM [Users]
WHERE [Id] = 4;
SELECT @@ROWCOUNT;


DELETE FROM [Users]
WHERE [Id] = 5;
SELECT @@ROWCOUNT;


DELETE FROM [Users]
WHERE [Id] = 6;
SELECT @@ROWCOUNT;


DELETE FROM [Users]
WHERE [Id] = 7;
SELECT @@ROWCOUNT;


DELETE FROM [Users]
WHERE [Id] = 8;
SELECT @@ROWCOUNT;


DELETE FROM [Users]
WHERE [Id] = 9;
SELECT @@ROWCOUNT;


DELETE FROM [Users]
WHERE [Id] = 10;
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251022081952_InitialCreate4', N'9.0.10');

CREATE TABLE [LoginEvents] (
    [Id] int NOT NULL IDENTITY,
    [UserName] nvarchar(100) NOT NULL,
    [PinUsedForLogin] nvarchar(max) NOT NULL,
    [LogInTime] datetime2 NOT NULL,
    CONSTRAINT [PK_LoginEvents] PRIMARY KEY ([Id])
);

UPDATE [Users] SET [Pin] = N'5361'
WHERE [Id] = 1;
SELECT @@ROWCOUNT;


UPDATE [Users] SET [Pin] = N'5361'
WHERE [Id] = 2;
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251113101129_InitialCreate5', N'9.0.10');

ALTER TABLE [Allocations] ADD [SentEmailForCurrentAllocation] bit NOT NULL DEFAULT CAST(0 AS bit);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251113190356_InitialCreate6', N'9.0.10');

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Allocations]') AND [c].[name] = N'SentEmailForCurrentAllocation');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Allocations] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Allocations] DROP COLUMN [SentEmailForCurrentAllocation];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251205145839_InitialCreate7', N'9.0.10');

COMMIT;
GO

