CREATE TABLE [CommunicationTypes] (
    [TypeCode] nvarchar(450) NOT NULL,
    [DisplayName] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_CommunicationTypes] PRIMARY KEY ([TypeCode])
);
GO


CREATE TABLE [GlobalStatuses] (
    [StatusCode] nvarchar(450) NOT NULL,
    [DisplayName] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Phase] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_GlobalStatuses] PRIMARY KEY ([StatusCode])
);
GO


CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Email] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(max) NOT NULL,
    [LastName] nvarchar(max) NOT NULL,
    [Role] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedUtc] datetime2 NOT NULL,
    [LastLoginUtc] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [CommunicationTypeStatuses] (
    [TypeCode] nvarchar(450) NOT NULL,
    [StatusCode] nvarchar(450) NOT NULL,
    [Description] nvarchar(max) NULL,
    CONSTRAINT [PK_CommunicationTypeStatuses] PRIMARY KEY ([TypeCode], [StatusCode]),
    CONSTRAINT [FK_CommunicationTypeStatuses_CommunicationTypes_TypeCode] FOREIGN KEY ([TypeCode]) REFERENCES [CommunicationTypes] ([TypeCode]) ON DELETE CASCADE,
    CONSTRAINT [FK_CommunicationTypeStatuses_GlobalStatuses_StatusCode] FOREIGN KEY ([StatusCode]) REFERENCES [GlobalStatuses] ([StatusCode]) ON DELETE CASCADE
);
GO


CREATE TABLE [Communications] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(max) NOT NULL,
    [TypeCode] nvarchar(450) NOT NULL,
    [CurrentStatus] nvarchar(450) NOT NULL,
    [CreatedUtc] datetime2 NOT NULL,
    [LastUpdatedUtc] datetime2 NOT NULL,
    [SourceFileUrl] nvarchar(max) NULL,
    [MemberInfo] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    [CreatedByUserId] int NULL,
    [LastUpdatedByUserId] int NULL,
    CONSTRAINT [PK_Communications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Communications_CommunicationTypes_TypeCode] FOREIGN KEY ([TypeCode]) REFERENCES [CommunicationTypes] ([TypeCode]) ON DELETE CASCADE,
    CONSTRAINT [FK_Communications_GlobalStatuses_CurrentStatus] FOREIGN KEY ([CurrentStatus]) REFERENCES [GlobalStatuses] ([StatusCode]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Communications_Users_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users] ([Id]),
    CONSTRAINT [FK_Communications_Users_LastUpdatedByUserId] FOREIGN KEY ([LastUpdatedByUserId]) REFERENCES [Users] ([Id])
);
GO


CREATE TABLE [CommunicationStatusHistories] (
    [Id] int NOT NULL IDENTITY,
    [CommunicationId] int NOT NULL,
    [StatusCode] nvarchar(450) NOT NULL,
    [OccurredUtc] datetime2 NOT NULL,
    [Notes] nvarchar(max) NULL,
    [EventSource] nvarchar(max) NULL,
    [UpdatedByUserId] int NULL,
    CONSTRAINT [PK_CommunicationStatusHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CommunicationStatusHistories_Communications_CommunicationId] FOREIGN KEY ([CommunicationId]) REFERENCES [Communications] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CommunicationStatusHistories_GlobalStatuses_StatusCode] FOREIGN KEY ([StatusCode]) REFERENCES [GlobalStatuses] ([StatusCode]) ON DELETE NO ACTION,
    CONSTRAINT [FK_CommunicationStatusHistories_Users_UpdatedByUserId] FOREIGN KEY ([UpdatedByUserId]) REFERENCES [Users] ([Id])
);
GO


CREATE INDEX [IX_Communications_CreatedByUserId] ON [Communications] ([CreatedByUserId]);
GO


CREATE INDEX [IX_Communications_CurrentStatus] ON [Communications] ([CurrentStatus]);
GO


CREATE INDEX [IX_Communications_LastUpdatedByUserId] ON [Communications] ([LastUpdatedByUserId]);
GO


CREATE INDEX [IX_Communications_TypeCode] ON [Communications] ([TypeCode]);
GO


CREATE INDEX [IX_CommunicationStatusHistories_CommunicationId] ON [CommunicationStatusHistories] ([CommunicationId]);
GO


CREATE INDEX [IX_CommunicationStatusHistories_StatusCode] ON [CommunicationStatusHistories] ([StatusCode]);
GO


CREATE INDEX [IX_CommunicationStatusHistories_UpdatedByUserId] ON [CommunicationStatusHistories] ([UpdatedByUserId]);
GO


CREATE INDEX [IX_CommunicationTypeStatuses_StatusCode] ON [CommunicationTypeStatuses] ([StatusCode]);
GO


CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
GO


