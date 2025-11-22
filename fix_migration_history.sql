-- Script to mark all previous migrations as applied
-- Run this script on your database before applying the new migration

-- Ensure the __EFMigrationsHistory table exists
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[__EFMigrationsHistory]') AND type in (N'U'))
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END

-- Insert all previous migrations (only if they don't already exist)
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250720113537_SeedExamTypes')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20250720113537_SeedExamTypes', '9.0.5');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20250808213738_AddIsAbsentToExamScore')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20250808213738_AddIsAbsentToExamScore', '9.0.5');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251001045550_AddBabyClassSkillAssessment')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20251001045550_AddBabyClassSkillAssessment', '9.0.5');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251001051130_AddBabyClassSkillTables')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20251001051130_AddBabyClassSkillTables', '9.0.5');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251002001015_AddMissingBabyClassColumns')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20251002001015_AddMissingBabyClassColumns', '9.0.5');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251002191447_FixBabyClassSkillAssessmentEntity')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20251002191447_FixBabyClassSkillAssessmentEntity', '9.0.5');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251006195836_SeedBabyClassSkills')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20251006195836_SeedBabyClassSkills', '9.0.5');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251007012028_FixPendingModelChanges')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20251007012028_FixPendingModelChanges', '9.0.5');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251007213005_RemoveBabyClassSkillIdColumn')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20251007213005_RemoveBabyClassSkillIdColumn', '9.0.5');

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251020235639_AddGeneralCommentFieldsOnly')
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES ('20251020235639_AddGeneralCommentFieldsOnly', '9.0.5');

PRINT 'Migration history has been updated. You can now apply the new migration.';

