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
    WHERE [MigrationId] = N'20260520081902_init-table'
)
BEGIN
    CREATE TABLE [TrainingCourses] (
        [id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NULL,
        [NameAr] nvarchar(200) NULL,
        [Creatd] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [OrganizationId] int NOT NULL,
        [UserId] nvarchar(100) NULL,
        [TargetedAudiance] nvarchar(max) NULL,
        [AttachRecordId] uniqueidentifier NULL,
        [SurveyId] int NULL,
        CONSTRAINT [PK_TrainingCourses] PRIMARY KEY ([id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520081902_init-table'
)
BEGIN
    CREATE TABLE [CourseSections] (
        [id] int NOT NULL IDENTITY,
        [TrainingCourseId] int NOT NULL,
        [NameAr] nvarchar(max) NULL,
        [Name] nvarchar(max) NULL,
        [Creatd] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [OrganizationId] int NOT NULL,
        [UserId] nvarchar(max) NULL,
        [AttachRecordId] uniqueidentifier NULL,
        CONSTRAINT [PK_CourseSections] PRIMARY KEY ([id]),
        CONSTRAINT [FK_CourseSections_TrainingCourses_TrainingCourseId] FOREIGN KEY ([TrainingCourseId]) REFERENCES [TrainingCourses] ([id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520081902_init-table'
)
BEGIN
    CREATE TABLE [Tests] (
        [id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NULL,
        [IsActive] bit NOT NULL,
        [UserId] nvarchar(max) NULL,
        [TrainingCourseId] int NOT NULL,
        [OrganizationId] int NOT NULL,
        [PassScore] int NOT NULL,
        [ExpiryDuration] int NOT NULL,
        CONSTRAINT [PK_Tests] PRIMARY KEY ([id]),
        CONSTRAINT [FK_Tests_TrainingCourses_TrainingCourseId] FOREIGN KEY ([TrainingCourseId]) REFERENCES [TrainingCourses] ([id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520081902_init-table'
)
BEGIN
    CREATE TABLE [Questions] (
        [id] int NOT NULL IDENTITY,
        [Text] nvarchar(max) NULL,
        [TestId] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CourseSectionId] int NULL,
        [sorting] int NULL,
        CONSTRAINT [PK_Questions] PRIMARY KEY ([id]),
        CONSTRAINT [FK_Questions_Tests_TestId] FOREIGN KEY ([TestId]) REFERENCES [Tests] ([id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520081902_init-table'
)
BEGIN
    CREATE TABLE [UserTestResults] (
        [id] int NOT NULL IDENTITY,
        [TestId] int NOT NULL,
        [OrganizationId] int NOT NULL,
        [UserId] nvarchar(max) NULL,
        [ExamDate] datetime2 NOT NULL,
        [Score] float NOT NULL,
        CONSTRAINT [PK_UserTestResults] PRIMARY KEY ([id]),
        CONSTRAINT [FK_UserTestResults_Tests_TestId] FOREIGN KEY ([TestId]) REFERENCES [Tests] ([id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520081902_init-table'
)
BEGIN
    CREATE TABLE [Answers] (
        [id] int NOT NULL IDENTITY,
        [Text] nvarchar(max) NULL,
        [IsCorrect] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [QuestionId] int NOT NULL,
        CONSTRAINT [PK_Answers] PRIMARY KEY ([id]),
        CONSTRAINT [FK_Answers_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Questions] ([id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520081902_init-table'
)
BEGIN
    CREATE TABLE [UserAnswers] (
        [id] int NOT NULL IDENTITY,
        [UserTestResultId] int NOT NULL,
        [QuestionId] int NOT NULL,
        [SelectedAnswerId] int NOT NULL,
        [IsCorrect] bit NOT NULL,
        CONSTRAINT [PK_UserAnswers] PRIMARY KEY ([id]),
        CONSTRAINT [FK_UserAnswers_Answers_SelectedAnswerId] FOREIGN KEY ([SelectedAnswerId]) REFERENCES [Answers] ([id]),
        CONSTRAINT [FK_UserAnswers_Questions_QuestionId] FOREIGN KEY ([QuestionId]) REFERENCES [Questions] ([id]),
        CONSTRAINT [FK_UserAnswers_UserTestResults_UserTestResultId] FOREIGN KEY ([UserTestResultId]) REFERENCES [UserTestResults] ([id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520081902_init-table'
)
BEGIN
    CREATE INDEX [IX_Answers_QuestionId] ON [Answers] ([QuestionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520081902_init-table'
)
BEGIN
    CREATE INDEX [IX_CourseSections_TrainingCourseId] ON [CourseSections] ([TrainingCourseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520081902_init-table'
)
BEGIN
    CREATE INDEX [IX_Questions_TestId] ON [Questions] ([TestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520081902_init-table'
)
BEGIN
    CREATE INDEX [IX_Tests_TrainingCourseId] ON [Tests] ([TrainingCourseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520081902_init-table'
)
BEGIN
    CREATE INDEX [IX_UserAnswers_QuestionId] ON [UserAnswers] ([QuestionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520081902_init-table'
)
BEGIN
    CREATE INDEX [IX_UserAnswers_SelectedAnswerId] ON [UserAnswers] ([SelectedAnswerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520081902_init-table'
)
BEGIN
    CREATE INDEX [IX_UserAnswers_UserTestResultId] ON [UserAnswers] ([UserTestResultId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520081902_init-table'
)
BEGIN
    CREATE INDEX [IX_UserTestResults_TestId] ON [UserTestResults] ([TestId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520081902_init-table'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260520081902_init-table', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520114227_AddFileRecordAndSectionFields'
)
BEGIN
    ALTER TABLE [CourseSections] ADD [Description] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520114227_AddFileRecordAndSectionFields'
)
BEGIN
    ALTER TABLE [CourseSections] ADD [ExternalUrl] nvarchar(max) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520114227_AddFileRecordAndSectionFields'
)
BEGIN
    CREATE TABLE [FileRecords] (
        [Id] uniqueidentifier NOT NULL,
        [OriginalName] nvarchar(500) NOT NULL,
        [ContentType] nvarchar(200) NOT NULL,
        [Size] bigint NOT NULL,
        [StoredPath] nvarchar(1000) NOT NULL,
        [UploadedBy] nvarchar(200) NOT NULL,
        [UploadedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_FileRecords] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260520114227_AddFileRecordAndSectionFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260520114227_AddFileRecordAndSectionFields', N'8.0.0');
END;
GO

COMMIT;
GO

