using BluebirdCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace BluebirdCore.Data
{
    public class SchoolDbContext : DbContext
    {
        public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<GradeSubject> GradeSubjects { get; set; }
        public DbSet<StudentOptionalSubject> StudentOptionalSubjects { get; set; }
        public DbSet<TeacherSubjectAssignment> TeacherSubjectAssignments { get; set; }
        public DbSet<ExamType> ExamTypes { get; set; }
        public DbSet<ExamScore> ExamScores { get; set; }
        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<ReportCard> ReportCards { get; set; }
        public DbSet<BabyClassSkill> BabyClassSkills { get; set; }
        public DbSet<BabyClassSkillItem> BabyClassSkillItems { get; set; }
        public DbSet<BabyClassSkillAssessment> BabyClassSkillAssessments { get; set; }
        public DbSet<StudentTechnologyTrack> StudentTechnologyTracks { get; set; }
        public DbSet<StudentSubject> StudentSubjects { get; set; }
        public DbSet<SmsLog> SmsLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configurations
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasConversion<int>();
            });


            // Grade configurations
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.Property(e => e.Section).HasConversion<int>();
                entity.HasOne(e => e.HomeroomTeacher)
                      .WithMany(e => e.HomeroomGrades)
                      .HasForeignKey(e => e.HomeroomTeacherId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // GradeSubject configurations
            modelBuilder.Entity<GradeSubject>(entity =>
            {
                entity.HasOne(e => e.Grade)
                      .WithMany(e => e.GradeSubjects)
                      .HasForeignKey(e => e.GradeId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Subject)
                      .WithMany(e => e.GradeSubjects)
                      .HasForeignKey(e => e.SubjectId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => new { e.GradeId, e.SubjectId }).IsUnique();
            });

            // StudentOptionalSubject configurations
            modelBuilder.Entity<StudentOptionalSubject>(entity =>
            {
                entity.HasOne(e => e.Student)
                      .WithMany(e => e.OptionalSubjects)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Subject)
                      .WithMany(e => e.StudentOptionalSubjects)
                      .HasForeignKey(e => e.SubjectId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => new { e.StudentId, e.SubjectId }).IsUnique();
            });

            // StudentSubject configurations
            modelBuilder.Entity<StudentSubject>(entity =>
            {
                entity.HasOne(e => e.Student)
                      .WithMany(e => e.StudentSubjects)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Subject)
                      .WithMany(e => e.StudentSubjects)
                      .HasForeignKey(e => e.SubjectId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => new { e.StudentId, e.SubjectId }).IsUnique();
            });

            // TeacherSubjectAssignment configurations
            modelBuilder.Entity<TeacherSubjectAssignment>(entity =>
            {
                entity.HasOne(e => e.Teacher)
                      .WithMany(e => e.TeacherAssignments)
                      .HasForeignKey(e => e.TeacherId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Subject)
                      .WithMany(e => e.TeacherAssignments)
                      .HasForeignKey(e => e.SubjectId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Grade)
                      .WithMany()
                      .HasForeignKey(e => e.GradeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ExamScore configurations
            modelBuilder.Entity<ExamScore>(entity =>
            {
                entity.Property(e => e.Score).HasPrecision(5, 2);
                
                entity.HasOne(e => e.Student)
                      .WithMany(e => e.ExamScores)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Subject)
                      .WithMany(e => e.ExamScores)
                      .HasForeignKey(e => e.SubjectId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.ExamType)
                      .WithMany(e => e.ExamScores)
                      .HasForeignKey(e => e.ExamTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Grade)
                      .WithMany()
                      .HasForeignKey(e => e.GradeId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.RecordedByTeacher)
                      .WithMany()
                      .HasForeignKey(e => e.RecordedBy)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(e => new { e.StudentId, e.SubjectId, e.ExamTypeId, e.AcademicYear, e.Term }).IsUnique();
            });

            // ReportCard configurations
            modelBuilder.Entity<ReportCard>(entity =>
            {
                entity.HasOne(e => e.Student)
                      .WithMany()
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Grade)
                      .WithMany()
                      .HasForeignKey(e => e.GradeId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.GeneratedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.GeneratedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // BabyClassSkillAssessment configurations
            modelBuilder.Entity<BabyClassSkillAssessment>(entity =>
            {
                entity.HasOne(e => e.Student)
                      .WithMany()
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.SkillItem)
                      .WithMany(e => e.SkillAssessments)
                      .HasForeignKey(e => e.SkillItemId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.AssessedByNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.AssessedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // BabyClassSkill configurations
            modelBuilder.Entity<BabyClassSkill>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // SmsLog configurations
            modelBuilder.Entity<SmsLog>(entity =>
            {
                entity.HasOne(e => e.Student)
                      .WithMany()
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(e => e.SentByUser)
                      .WithMany()
                      .HasForeignKey(e => e.SentByUserId)
                      .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasOne(e => e.AcademicYearNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.AcademicYear)
                      .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasIndex(e => e.PhoneNumber);
                entity.HasIndex(e => e.StudentId);
                entity.HasIndex(e => e.SentAt);
                entity.HasIndex(e => e.Status);
            });

            // BabyClassSkillItem configurations
            modelBuilder.Entity<BabyClassSkillItem>(entity =>
            {
                entity.HasOne(e => e.Skill)
                      .WithMany(e => e.SkillItems)
                      .HasForeignKey(e => e.SkillId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed exam types
            modelBuilder.Entity<ExamType>().HasData(
                new ExamType { Id = 1, Name = "Test-One", Description = "First test of the term", Order = 1 },
                new ExamType { Id = 2, Name = "Test-Two", Description = "Second-test examination", Order = 2 },
                new ExamType { Id = 3, Name = "Test-Three", Description = "Third-test examination ( Usually taken by Grade 7)", Order = 3 },
                new ExamType { Id = 4, Name = "End-of-Term", Description = "End of term examination", Order = 4 }
            );

            // Seed academic year
            modelBuilder.Entity<AcademicYear>().HasData(
                new AcademicYear 
                { 
                    Id = 1, 
                    Name = "2025", 
                    StartDate = new DateTime(2025, 1, 1), 
                    EndDate = new DateTime(2025, 12, 1), 
                    IsActive = true 
                }
            );


            // Student configurations
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasIndex(e => e.StudentNumber).IsUnique();
                entity.HasOne(e => e.Grade)
                      .WithMany(e => e.Students)
                      .HasForeignKey(e => e.GradeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed admin user (password: admin123)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "$2a$12$Y5Cr10SW4OuJq6qxj7PXtOhZvb7loVQqIRRwcrH8hsdsoeRCririq",
                    FullName = "System Administrator",
                    Email = "admin@chsschool.com",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt =  new DateTime(2025, 06, 25)
                }
            );

            // Seed subjects by category
            modelBuilder.Entity<Subject>().HasData(
                // PRIMARY & SECONDARY CORE SUBJECTS
                new Subject { Id = 1, Name = "Mathematics", Code = "MATH" },
                new Subject { Id = 2, Name = "English", Code = "ENG" },
                new Subject { Id = 3, Name = "Integrated Science", Code = "SCI" },
                new Subject { Id = 4, Name = "Social Studies", Code = "SS" },
                new Subject { Id = 5, Name = "French", Code = "FR" },
                new Subject { Id = 6, Name = "ICT", Code = "ICT" },
                new Subject { Id = 7, Name = "Physical Education", Code = "PE" },
                new Subject { Id = 8, Name = "Reading", Code = "REA" },
                new Subject { Id = 11, Name = "CTS", Code = "CTS" },
                new Subject { Id = 25, Name = "MDE", Code = "MDE" },
                new Subject { Id = 35, Name = "Cinyanja", Code = "CINY" },
                new Subject { Id = 36, Name = "Science", Code = "SCIE" },
                new Subject { Id = 33, Name = "Computer Studies", Code = "CS" },
                new Subject { Id = 21, Name = "Biology", Code = "BIO" },
                
                // UPPER PRIMARY SPECIAL SUBJECTS
                new Subject { Id = 9, Name = "SP1", Code = "SP1" },
                new Subject { Id = 10, Name = "SP2", Code = "SP2" },
                
                // SECONDARY OPTIONAL SUBJECTS
                new Subject { Id = 12, Name = "Agriculture Science", Code = "AGRI" },
                new Subject { Id = 13, Name = "History", Code = "HIST" },
                new Subject { Id = 14, Name = "Geography", Code = "GEO" },
                new Subject { Id = 15, Name = "Religious Studies", Code = "REL" },
                new Subject { Id = 16, Name = "Business Studies", Code = "BUS" },
                new Subject { Id = 17, Name = "Accounts", Code = "ACC" },
                new Subject { Id = 18, Name = "Economics", Code = "ECO" },
                new Subject { Id = 19, Name = "Physics", Code = "PHY" },
                new Subject { Id = 20, Name = "Chemistry", Code = "CHEM" },
                
                new Subject { Id = 22, Name = "Literature in English", Code = "LIT" },
                new Subject { Id = 23, Name = "Civic Education", Code = "CIV" },
                new Subject { Id = 24, Name = "Music", Code = "MUS" },
                new Subject { Id = 26, Name = "Home Economics", Code = "HE" },
                new Subject { Id = 34, Name = "Computer Science", Code = "COMSCI" },
                new Subject { Id = 37, Name = "Commerce", Code = "COMMER" },
                new Subject { Id = 38, Name = "French", Code = "FRENCH" },
                new Subject { Id = 39, Name = "Art & Design", Code = "ART" },
                new Subject {Id = 40, Name = "Food and Nutrition", Code = "F&N"},
                
                // NEW FORM 1 PATHWAY SUBJECTS
                new Subject { Id = 46, Name = "Design and Technology", Code = "D&T" },
                new Subject { Id = 47, Name = "Fashion and Fabrics", Code = "F&F" },
                new Subject { Id = 48, Name = "Hospitality Management", Code = "HOSP" },
                new Subject { Id = 49, Name = "Travel and Tourism", Code = "T&T" },
                
                // ADDITIONAL PATHWAY SUBJECTS
                new Subject { Id = 50, Name = "Foreign Language", Code = "FL" },
                new Subject { Id = 51, Name = "Zambian Language", Code = "ZL" },
                new Subject { Id = 52, Name = "Principles of Accounts", Code = "POA" },
                new Subject { Id = 53, Name = "Literature in Zambian Language", Code = "LITZL" },
                
                // ELC (EARLY LEARNING CENTER) SUBJECTS
                new Subject { Id = 41, Name = "Language", Code = "LANG" },
                new Subject { Id = 42, Name = "Pre-Math", Code = "PREMATH" },
                new Subject { Id = 43, Name = "Topic", Code = "TOPIC" },
                new Subject { Id = 44, Name = "Creative And Technology Studies (CTS)", Code = "ELCCTS" },
                new Subject { Id = 45, Name = "Creative Activities", Code = "CREAT" },
                
                // ELC DEVELOPMENTAL SKILLS SUBJECTS
                new Subject { Id = 27, Name = "Communication Skills", Code = "COMMSK" },
                new Subject { Id = 28, Name = "Fine-motor Skills", Code = "FM" },
                new Subject { Id = 29, Name = "Gross-motor Skills", Code = "GMS" },
                new Subject { Id = 30, Name = "Social-emotional Skills", Code = "SESK" },
                new Subject { Id = 31, Name = "Colors & Shapes", Code = "COLSS" },
                new Subject { Id = 32, Name = "Numbers", Code = "NUMB" }
            );


            // Seed some grades
            
            modelBuilder.Entity<Grade>().HasData(
                // SECONDARY SECTION 
                // Form 1: W, X, Y (3 streams)
                new Grade { Id = 31, Name = "Form 1", Stream = "W", Level = 11, Section = SchoolSection.SecondaryJunior},
                new Grade { Id = 32, Name = "Form 1", Stream = "X", Level = 11, Section = SchoolSection.SecondaryJunior},
                new Grade { Id = 41, Name = "Form 1", Stream = "Y", Level = 11, Section = SchoolSection.SecondaryJunior},
                // Grade 10: V, W, X, Y (4 streams)
                new Grade { Id = 35, Name = "Grade 10", Stream = "V", Level = 12, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 36, Name = "Grade 10", Stream = "W", Level = 12, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 42, Name = "Grade 10", Stream = "X", Level = 12, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 43, Name = "Grade 10", Stream = "Y", Level = 12, Section = SchoolSection.SecondarySenior},
                // Grade 11: V, W, X, Y (4 streams)
                new Grade { Id = 37, Name = "Grade 11", Stream = "V", Level = 13, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 38, Name = "Grade 11", Stream = "W", Level = 13, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 44, Name = "Grade 11", Stream = "X", Level = 13, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 45, Name = "Grade 11", Stream = "Y", Level = 13, Section = SchoolSection.SecondarySenior},
                // Grade 12: V, W, X, Y (4 streams)
                new Grade { Id = 39, Name = "Grade 12", Stream = "V", Level = 14, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 40, Name = "Grade 12", Stream = "W", Level = 14, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 46, Name = "Grade 12", Stream = "X", Level = 14, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 47, Name = "Grade 12", Stream = "Y", Level = 14, Section = SchoolSection.SecondarySenior}

                
                
               
                
          
            );

            // Seed GradeSubjects for secondary grades only
            modelBuilder.Entity<GradeSubject>().HasData(
                // Secondary grades
                // Core subjects for all secondary grades
                // Form 1 (Grades 31, 32, 41) - Core subjects (W, X, Y)
                new GradeSubject { Id = 193, GradeId = 31, SubjectId = 2, IsActive = true }, // English - Form 1 W
                new GradeSubject { Id = 194, GradeId = 31, SubjectId = 1, IsActive = true }, // Mathematics - Form 1 W
                new GradeSubject { Id = 195, GradeId = 31, SubjectId = 23, IsActive = true }, // Civic Education - Form 1 W
                new GradeSubject { Id = 198, GradeId = 31, SubjectId = 25, IsActive = true }, // MDE - Form 1 W
                new GradeSubject { Id = 199, GradeId = 31, SubjectId = 6, IsActive = true }, // ICT - Form 1 W

                // Form 1 X - Same core subjects
                new GradeSubject { Id = 213, GradeId = 32, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 214, GradeId = 32, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 215, GradeId = 32, SubjectId = 23, IsActive = true }, // Civic Education
                new GradeSubject { Id = 217, GradeId = 32, SubjectId = 25, IsActive = true }, // MDE
                new GradeSubject { Id = 218, GradeId = 32, SubjectId = 6, IsActive = true }, // ICT

                // Form 1 Y - Same core subjects
                new GradeSubject { Id = 219, GradeId = 41, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 220, GradeId = 41, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 221, GradeId = 41, SubjectId = 23, IsActive = true }, // Civic Education
                new GradeSubject { Id = 222, GradeId = 41, SubjectId = 25, IsActive = true }, // MDE
                new GradeSubject { Id = 223, GradeId = 41, SubjectId = 6, IsActive = true }, // ICT

                // Grade 10 (Grades 35, 36, 42, 43) - V, W, X, Y
                // Grade 10 V
                new GradeSubject { Id = 267, GradeId = 35, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 268, GradeId = 35, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 269, GradeId = 35, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 270, GradeId = 35, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 271, GradeId = 35, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 272, GradeId = 35, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 10 V
                new GradeSubject { Id = 273, GradeId = 35, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 274, GradeId = 35, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 275, GradeId = 35, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 276, GradeId = 35, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 277, GradeId = 35, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 278, GradeId = 35, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 279, GradeId = 35, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 280, GradeId = 35, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 281, GradeId = 35, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 282, GradeId = 35, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 283, GradeId = 35, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 284, GradeId = 35, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 10 W
                new GradeSubject { Id = 285, GradeId = 36, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 286, GradeId = 36, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 287, GradeId = 36, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 288, GradeId = 36, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 289, GradeId = 36, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 290, GradeId = 36, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 10 W
                new GradeSubject { Id = 291, GradeId = 36, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 292, GradeId = 36, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 293, GradeId = 36, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 294, GradeId = 36, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 295, GradeId = 36, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 296, GradeId = 36, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 297, GradeId = 36, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 298, GradeId = 36, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 299, GradeId = 36, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 300, GradeId = 36, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 301, GradeId = 36, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 302, GradeId = 36, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 10 X
                new GradeSubject { Id = 500, GradeId = 42, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 501, GradeId = 42, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 502, GradeId = 42, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 503, GradeId = 42, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 504, GradeId = 42, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 505, GradeId = 42, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 10 X
                new GradeSubject { Id = 506, GradeId = 42, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 507, GradeId = 42, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 508, GradeId = 42, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 509, GradeId = 42, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 510, GradeId = 42, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 511, GradeId = 42, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 512, GradeId = 42, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 513, GradeId = 42, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 514, GradeId = 42, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 515, GradeId = 42, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 516, GradeId = 42, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 517, GradeId = 42, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 10 Y
                new GradeSubject { Id = 518, GradeId = 43, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 519, GradeId = 43, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 520, GradeId = 43, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 521, GradeId = 43, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 522, GradeId = 43, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 523, GradeId = 43, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 10 Y
                new GradeSubject { Id = 524, GradeId = 43, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 525, GradeId = 43, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 526, GradeId = 43, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 527, GradeId = 43, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 528, GradeId = 43, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 529, GradeId = 43, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 530, GradeId = 43, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 531, GradeId = 43, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 532, GradeId = 43, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 533, GradeId = 43, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 534, GradeId = 43, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 535, GradeId = 43, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 11 (Grades 37, 38, 44, 45) - V, W, X, Y
                // Grade 11 V
                new GradeSubject { Id = 303, GradeId = 37, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 304, GradeId = 37, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 305, GradeId = 37, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 306, GradeId = 37, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 307, GradeId = 37, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 308, GradeId = 37, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 11 V
                new GradeSubject { Id = 309, GradeId = 37, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 310, GradeId = 37, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 311, GradeId = 37, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 312, GradeId = 37, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 313, GradeId = 37, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 314, GradeId = 37, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 315, GradeId = 37, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 316, GradeId = 37, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 317, GradeId = 37, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 318, GradeId = 37, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 319, GradeId = 37, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 320, GradeId = 37, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 11 W
                new GradeSubject { Id = 321, GradeId = 38, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 322, GradeId = 38, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 323, GradeId = 38, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 324, GradeId = 38, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 325, GradeId = 38, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 326, GradeId = 38, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 11 W
                new GradeSubject { Id = 327, GradeId = 38, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 328, GradeId = 38, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 329, GradeId = 38, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 330, GradeId = 38, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 331, GradeId = 38, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 332, GradeId = 38, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 333, GradeId = 38, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 334, GradeId = 38, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 335, GradeId = 38, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 336, GradeId = 38, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 337, GradeId = 38, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 338, GradeId = 38, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 11 X
                new GradeSubject { Id = 536, GradeId = 44, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 537, GradeId = 44, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 538, GradeId = 44, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 539, GradeId = 44, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 540, GradeId = 44, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 541, GradeId = 44, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 11 X
                new GradeSubject { Id = 542, GradeId = 44, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 543, GradeId = 44, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 544, GradeId = 44, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 545, GradeId = 44, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 546, GradeId = 44, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 547, GradeId = 44, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 548, GradeId = 44, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 549, GradeId = 44, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 550, GradeId = 44, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 551, GradeId = 44, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 552, GradeId = 44, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 553, GradeId = 44, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 11 Y
                new GradeSubject { Id = 554, GradeId = 45, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 555, GradeId = 45, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 556, GradeId = 45, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 557, GradeId = 45, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 558, GradeId = 45, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 559, GradeId = 45, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 11 Y
                new GradeSubject { Id = 560, GradeId = 45, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 561, GradeId = 45, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 562, GradeId = 45, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 563, GradeId = 45, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 564, GradeId = 45, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 565, GradeId = 45, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 566, GradeId = 45, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 567, GradeId = 45, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 568, GradeId = 45, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 569, GradeId = 45, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 570, GradeId = 45, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 571, GradeId = 45, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 12 (Grades 39, 40, 46, 47) - V, W, X, Y
                // Grade 12 V
                new GradeSubject { Id = 339, GradeId = 39, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 340, GradeId = 39, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 341, GradeId = 39, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 342, GradeId = 39, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 343, GradeId = 39, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 344, GradeId = 39, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 12 V
                new GradeSubject { Id = 345, GradeId = 39, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 346, GradeId = 39, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 347, GradeId = 39, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 348, GradeId = 39, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 349, GradeId = 39, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 350, GradeId = 39, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 351, GradeId = 39, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 352, GradeId = 39, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 353, GradeId = 39, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 354, GradeId = 39, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 355, GradeId = 39, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 356, GradeId = 39, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 12 W
                new GradeSubject { Id = 357, GradeId = 40, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 358, GradeId = 40, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 359, GradeId = 40, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 360, GradeId = 40, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 361, GradeId = 40, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 362, GradeId = 40, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 12 W
                new GradeSubject { Id = 363, GradeId = 40, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 364, GradeId = 40, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 365, GradeId = 40, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 366, GradeId = 40, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 367, GradeId = 40, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 368, GradeId = 40, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 369, GradeId = 40, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 370, GradeId = 40, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 371, GradeId = 40, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 372, GradeId = 40, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 373, GradeId = 40, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 374, GradeId = 40, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 12 X
                new GradeSubject { Id = 572, GradeId = 46, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 573, GradeId = 46, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 574, GradeId = 46, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 575, GradeId = 46, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 576, GradeId = 46, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 577, GradeId = 46, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 12 X
                new GradeSubject { Id = 578, GradeId = 46, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 579, GradeId = 46, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 580, GradeId = 46, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 581, GradeId = 46, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 582, GradeId = 46, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 583, GradeId = 46, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 584, GradeId = 46, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 585, GradeId = 46, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 586, GradeId = 46, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 587, GradeId = 46, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 588, GradeId = 46, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 589, GradeId = 46, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 12 Y
                new GradeSubject { Id = 590, GradeId = 47, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 591, GradeId = 47, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 592, GradeId = 47, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 593, GradeId = 47, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 594, GradeId = 47, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 595, GradeId = 47, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 12 Y
                new GradeSubject { Id = 596, GradeId = 47, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 597, GradeId = 47, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 598, GradeId = 47, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 599, GradeId = 47, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 600, GradeId = 47, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 601, GradeId = 47, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 602, GradeId = 47, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 603, GradeId = 47, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 604, GradeId = 47, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 605, GradeId = 47, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 606, GradeId = 47, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 607, GradeId = 47, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Add Reading (SubjectId = 8) as a core subject for secondary grades only
                new GradeSubject { Id = 4021, GradeId = 31, SubjectId = 8, IsActive = true }, // Reading - Form 1 W
                new GradeSubject { Id = 4022, GradeId = 32, SubjectId = 8, IsActive = true }, // Reading - Form 1 X
                new GradeSubject { Id = 4025, GradeId = 35, SubjectId = 8, IsActive = true }, // Reading - Grade 10 V
                new GradeSubject { Id = 4026, GradeId = 36, SubjectId = 8, IsActive = true }, // Reading - Grade 10 W
                new GradeSubject { Id = 4027, GradeId = 37, SubjectId = 8, IsActive = true }, // Reading - Grade 11 V
                new GradeSubject { Id = 4028, GradeId = 38, SubjectId = 8, IsActive = true }, // Reading - Grade 11 W
                new GradeSubject { Id = 4029, GradeId = 39, SubjectId = 8, IsActive = true }, // Reading - Grade 12 V
                new GradeSubject { Id = 4030, GradeId = 40, SubjectId = 8, IsActive = true }, // Reading - Grade 12 W
                new GradeSubject { Id = 4031, GradeId = 41, SubjectId = 8, IsActive = true }, // Reading - Form 1 Y
                new GradeSubject { Id = 4032, GradeId = 42, SubjectId = 8, IsActive = true }, // Reading - Grade 10 X
                new GradeSubject { Id = 4033, GradeId = 43, SubjectId = 8, IsActive = true }, // Reading - Grade 10 Y
                new GradeSubject { Id = 4034, GradeId = 44, SubjectId = 8, IsActive = true }, // Reading - Grade 11 X
                new GradeSubject { Id = 4035, GradeId = 45, SubjectId = 8, IsActive = true }, // Reading - Grade 11 Y
                new GradeSubject { Id = 4036, GradeId = 46, SubjectId = 8, IsActive = true }, // Reading - Grade 12 X
                new GradeSubject { Id = 4037, GradeId = 47, SubjectId = 8, IsActive = true } // Reading - Grade 12 Y
            );
        }
    }
}