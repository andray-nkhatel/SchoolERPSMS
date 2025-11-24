using SchoolErpSMS.Entities;
using Microsoft.EntityFrameworkCore;

namespace SchoolErpSMS.Data
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
                new ExamType { Id = 3, Name = "End-of-Term", Description = "End of term examination", Order = 3}
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

            // Seed admin user (password: root@scherp25)
            // Using static hash to avoid migration warnings about dynamic values
            // Hash for "root@scherp25": $2a$11$I0Q7cC9y7.NTwp3hWV3QnOfYsVkRZi1ZsRa1IVGck5VIriBdoip.O
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "$2a$11$I0Q7cC9y7.NTwp3hWV3QnOfYsVkRZi1ZsRa1IVGck5VIriBdoip.O",
                    FullName = "System Admin",
                    Email = "admin@scherp.sch.edu",
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
                new Grade { Id = 1, Name = "Form 1", Stream = "W", Level = 11, Section = SchoolSection.SecondaryJunior},
                new Grade { Id = 2, Name = "Form 1", Stream = "X", Level = 11, Section = SchoolSection.SecondaryJunior},
                new Grade { Id = 3, Name = "Form 1", Stream = "Y", Level = 11, Section = SchoolSection.SecondaryJunior},
                // Grade 10: V, W, X, Y (4 streams)
                new Grade { Id = 4, Name = "Grade 10", Stream = "V", Level = 12, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 5, Name = "Grade 10", Stream = "W", Level = 12, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 6, Name = "Grade 10", Stream = "X", Level = 12, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 7, Name = "Grade 10", Stream = "Y", Level = 12, Section = SchoolSection.SecondarySenior},
                // Grade 11: V, W, X, Y (4 streams)
                new Grade { Id = 8, Name = "Grade 11", Stream = "V", Level = 13, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 9, Name = "Grade 11", Stream = "W", Level = 13, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 10, Name = "Grade 11", Stream = "X", Level = 13, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 11, Name = "Grade 11", Stream = "Y", Level = 13, Section = SchoolSection.SecondarySenior},
                // Grade 12: V, W, X, Y (4 streams)
                new Grade { Id = 12, Name = "Grade 12", Stream = "V", Level = 14, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 13, Name = "Grade 12", Stream = "W", Level = 14, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 14, Name = "Grade 12", Stream = "X", Level = 14, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 15, Name = "Grade 12", Stream = "Y", Level = 14, Section = SchoolSection.SecondarySenior}

                
                
               
                
          
            );
        }
    }
}