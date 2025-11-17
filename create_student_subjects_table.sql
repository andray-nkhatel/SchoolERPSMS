-- Create StudentSubjects table for secondary subject assignment
CREATE TABLE [StudentSubjects] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [StudentId] int NOT NULL,
    [SubjectId] int NOT NULL,
    [EnrolledDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [CompletedDate] datetime2 NULL,
    [DroppedDate] datetime2 NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [Notes] nvarchar(max) NULL,
    [AssignedBy] nvarchar(max) NULL,
    [AssignedDate] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [PK_StudentSubjects] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_StudentSubjects_Students_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Students] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_StudentSubjects_Subjects_SubjectId] FOREIGN KEY ([SubjectId]) REFERENCES [Subjects] ([Id]) ON DELETE CASCADE
);

-- Create unique index to prevent duplicate assignments
CREATE UNIQUE INDEX [IX_StudentSubjects_StudentId_SubjectId] ON [StudentSubjects] ([StudentId], [SubjectId]);

-- Create index for performance
CREATE INDEX [IX_StudentSubjects_SubjectId] ON [StudentSubjects] ([SubjectId]);
