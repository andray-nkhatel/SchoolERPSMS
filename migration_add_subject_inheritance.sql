-- Migration: Add Subject Inheritance Fields
-- Description: Adds fields to support automatic subject assignment from grades to students
-- Date: 2025-01-XX

-- Step 1: Add new columns to GradeSubject table
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'GradeSubjects' AND column_name = 'AutoAssignToStudents'
    ) THEN
        ALTER TABLE "GradeSubjects"
        ADD COLUMN "AutoAssignToStudents" BOOLEAN NOT NULL DEFAULT FALSE;
        RAISE NOTICE 'Added AutoAssignToStudents column to GradeSubjects';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'GradeSubjects' AND column_name = 'AcademicYearId'
    ) THEN
        ALTER TABLE "GradeSubjects"
        ADD COLUMN "AcademicYearId" INTEGER NULL;
        RAISE NOTICE 'Added AcademicYearId column to GradeSubjects';
    END IF;
END $$;

-- Step 2: Add foreign key for AcademicYearId in GradeSubjects
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_GradeSubjects_AcademicYears_AcademicYearId'
    ) THEN
        ALTER TABLE "GradeSubjects"
        ADD CONSTRAINT "FK_GradeSubjects_AcademicYears_AcademicYearId"
        FOREIGN KEY ("AcademicYearId") REFERENCES "AcademicYears" ("Id") ON DELETE SET NULL;
        RAISE NOTICE 'Added foreign key for AcademicYearId in GradeSubjects';
    END IF;
END $$;

-- Step 3: Add new columns to StudentSubject table
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'StudentSubjects' AND column_name = 'SourceType'
    ) THEN
        ALTER TABLE "StudentSubjects"
        ADD COLUMN "SourceType" INTEGER NOT NULL DEFAULT 0;
        RAISE NOTICE 'Added SourceType column to StudentSubjects';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_name = 'StudentSubjects' AND column_name = 'InheritedFromGradeId'
    ) THEN
        ALTER TABLE "StudentSubjects"
        ADD COLUMN "InheritedFromGradeId" INTEGER NULL;
        RAISE NOTICE 'Added InheritedFromGradeId column to StudentSubjects';
    END IF;
END $$;

-- Step 4: Add foreign key for InheritedFromGradeId in StudentSubjects
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.table_constraints 
        WHERE constraint_name = 'FK_StudentSubjects_Grades_InheritedFromGradeId'
    ) THEN
        ALTER TABLE "StudentSubjects"
        ADD CONSTRAINT "FK_StudentSubjects_Grades_InheritedFromGradeId"
        FOREIGN KEY ("InheritedFromGradeId") REFERENCES "Grades" ("Id") ON DELETE SET NULL;
        RAISE NOTICE 'Added foreign key for InheritedFromGradeId in StudentSubjects';
    END IF;
END $$;

-- Step 5: Create index on SourceType for better query performance
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes 
        WHERE indexname = 'IX_StudentSubjects_SourceType'
    ) THEN
        CREATE INDEX "IX_StudentSubjects_SourceType" ON "StudentSubjects" ("SourceType");
        RAISE NOTICE 'Created index on SourceType';
    END IF;
END $$;

-- Step 6: Create index on InheritedFromGradeId for better query performance
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_indexes 
        WHERE indexname = 'IX_StudentSubjects_InheritedFromGradeId'
    ) THEN
        CREATE INDEX "IX_StudentSubjects_InheritedFromGradeId" ON "StudentSubjects" ("InheritedFromGradeId");
        RAISE NOTICE 'Created index on InheritedFromGradeId';
    END IF;
END $$;

-- Step 7: Optional: Migrate existing data
-- Mark existing student subjects as Manual (SourceType = 0) if they're not already set
UPDATE "StudentSubjects"
SET "SourceType" = 0
WHERE "SourceType" IS NULL OR "SourceType" = 0;

DO $$
BEGIN
    RAISE NOTICE 'Updated existing StudentSubjects to have SourceType = 0 (Manual)';
    RAISE NOTICE 'Migration completed successfully!';
END $$;
