-- Migration: Add Subject Inheritance Fields
-- Description: Adds fields to support automatic subject assignment from grades to students
-- Date: 2025-01-XX

-- Step 1: Add new columns to GradeSubject table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[GradeSubjects]') AND name = 'AutoAssignToStudents')
BEGIN
    ALTER TABLE [dbo].[GradeSubjects]
    ADD [AutoAssignToStudents] BIT NOT NULL DEFAULT 0;
    PRINT 'Added AutoAssignToStudents column to GradeSubjects';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[GradeSubjects]') AND name = 'AcademicYearId')
BEGIN
    ALTER TABLE [dbo].[GradeSubjects]
    ADD [AcademicYearId] INT NULL;
    PRINT 'Added AcademicYearId column to GradeSubjects';
END
GO

-- Step 2: Add foreign key for AcademicYearId in GradeSubjects
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_GradeSubjects_AcademicYears_AcademicYearId')
BEGIN
    ALTER TABLE [dbo].[GradeSubjects]
    ADD CONSTRAINT [FK_GradeSubjects_AcademicYears_AcademicYearId]
    FOREIGN KEY ([AcademicYearId]) REFERENCES [dbo].[AcademicYears] ([Id]) ON DELETE SET NULL;
    PRINT 'Added foreign key for AcademicYearId in GradeSubjects';
END
GO

-- Step 3: Add new columns to StudentSubject table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[StudentSubjects]') AND name = 'SourceType')
BEGIN
    ALTER TABLE [dbo].[StudentSubjects]
    ADD [SourceType] INT NOT NULL DEFAULT 0;
    PRINT 'Added SourceType column to StudentSubjects';
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[StudentSubjects]') AND name = 'InheritedFromGradeId')
BEGIN
    ALTER TABLE [dbo].[StudentSubjects]
    ADD [InheritedFromGradeId] INT NULL;
    PRINT 'Added InheritedFromGradeId column to StudentSubjects';
END
GO

-- Step 4: Add foreign key for InheritedFromGradeId in StudentSubjects
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_StudentSubjects_Grades_InheritedFromGradeId')
BEGIN
    ALTER TABLE [dbo].[StudentSubjects]
    ADD CONSTRAINT [FK_StudentSubjects_Grades_InheritedFromGradeId]
    FOREIGN KEY ([InheritedFromGradeId]) REFERENCES [dbo].[Grades] ([Id]) ON DELETE SET NULL;
    PRINT 'Added foreign key for InheritedFromGradeId in StudentSubjects';
END
GO

-- Step 5: Create index on SourceType for better query performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StudentSubjects_SourceType')
BEGIN
    CREATE INDEX [IX_StudentSubjects_SourceType] ON [dbo].[StudentSubjects] ([SourceType]);
    PRINT 'Created index on SourceType';
END
GO

-- Step 6: Create index on InheritedFromGradeId for better query performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_StudentSubjects_InheritedFromGradeId')
BEGIN
    CREATE INDEX [IX_StudentSubjects_InheritedFromGradeId] ON [dbo].[StudentSubjects] ([InheritedFromGradeId]);
    PRINT 'Created index on InheritedFromGradeId';
END
GO

-- Step 7: Optional: Migrate existing data
-- Mark existing student subjects as Manual (SourceType = 0) if they're not already set
UPDATE [dbo].[StudentSubjects]
SET [SourceType] = 0
WHERE [SourceType] IS NULL OR [SourceType] = 0;
PRINT 'Updated existing StudentSubjects to have SourceType = 0 (Manual)';
GO

PRINT 'Migration completed successfully!';
GO

