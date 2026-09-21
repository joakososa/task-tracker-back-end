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
CREATE TABLE [TaskStates] (
    [Id] int NOT NULL,
    [Name] nvarchar(20) NOT NULL,
    CONSTRAINT [PK_TaskStates] PRIMARY KEY ([Id])
);

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Email] nvarchar(256) NOT NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    [AvatarUrl] nvarchar(2048) NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [CreatedBy] int NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [Projects] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [OwnerId] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [CreatedBy] int NULL,
    CONSTRAINT [PK_Projects] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Projects_Users_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [ProjectMembers] (
    [ProjectId] int NOT NULL,
    [UserId] int NOT NULL,
    [JoinedAt] datetimeoffset NOT NULL,
    CONSTRAINT [PK_ProjectMembers] PRIMARY KEY ([ProjectId], [UserId]),
    CONSTRAINT [FK_ProjectMembers_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProjectMembers_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [TaskItems] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(4000) NOT NULL,
    [Priority] nvarchar(20) NOT NULL,
    [State] int NOT NULL,
    [ProjectId] int NOT NULL,
    [AssigneeId] int NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [CreatedBy] int NULL,
    CONSTRAINT [PK_TaskItems] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_TaskItems_Priority] CHECK ([Priority] IN ('Low', 'Medium', 'High')),
    CONSTRAINT [FK_TaskItems_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TaskItems_TaskStates_State] FOREIGN KEY ([State]) REFERENCES [TaskStates] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_TaskItems_Users_AssigneeId] FOREIGN KEY ([AssigneeId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);

CREATE TABLE [Comments] (
    [Id] int NOT NULL IDENTITY,
    [Content] nvarchar(2000) NOT NULL,
    [TaskItemId] int NOT NULL,
    [AuthorId] int NOT NULL,
    [CreatedAt] datetimeoffset NOT NULL,
    [UpdatedAt] datetimeoffset NOT NULL,
    [CreatedBy] int NULL,
    CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Comments_TaskItems_TaskItemId] FOREIGN KEY ([TaskItemId]) REFERENCES [TaskItems] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Comments_Users_AuthorId] FOREIGN KEY ([AuthorId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[TaskStates]'))
    SET IDENTITY_INSERT [TaskStates] ON;
INSERT INTO [TaskStates] ([Id], [Name])
VALUES (0, N'Todo'),
(1, N'InProgress'),
(2, N'Done');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name') AND [object_id] = OBJECT_ID(N'[TaskStates]'))
    SET IDENTITY_INSERT [TaskStates] OFF;

CREATE INDEX [IX_Comments_AuthorId] ON [Comments] ([AuthorId]);

CREATE INDEX [IX_Comments_TaskItemId_CreatedAt] ON [Comments] ([TaskItemId], [CreatedAt]);

CREATE INDEX [IX_ProjectMembers_UserId] ON [ProjectMembers] ([UserId]);

CREATE INDEX [IX_Projects_OwnerId] ON [Projects] ([OwnerId]);

CREATE INDEX [IX_TaskItems_AssigneeId] ON [TaskItems] ([AssigneeId]);

CREATE INDEX [IX_TaskItems_ProjectId_State] ON [TaskItems] ([ProjectId], [State]);

CREATE INDEX [IX_TaskItems_State] ON [TaskItems] ([State]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260921054459_InitialCreate', N'9.0.20');

COMMIT;
GO

