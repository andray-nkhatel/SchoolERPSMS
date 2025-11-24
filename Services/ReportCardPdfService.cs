// ===== DYNAMIC 4-PAGE QUESTPDF REPORT CARD SERVICE =====
// Integrated with Vue Score Entry Component Data Structure

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SchoolErpSMS.Entities;
using Microsoft.EntityFrameworkCore;
using SchoolErpSMS.Data;
using QuestPDF.Previewer;
using SchoolErpSMS.DTOs;
using SchoolErpSMS.Models;

namespace SchoolErpSMS.Services
{
    public class ReportCardPdfService
    {
        private readonly SchoolDbContext _context;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReportCardPdfService> _logger;
        private readonly SchoolSettings _schoolSettings;

        public ReportCardPdfService(IConfiguration configuration, SchoolDbContext context, ILogger<ReportCardPdfService> logger, IServiceProvider serviceProvider)
        {
            _context = context;
            _logger = logger;
            _serviceProvider = serviceProvider;
            
            // Bind school settings from configuration
            _schoolSettings = new SchoolSettings();
            configuration.GetSection(SchoolSettings.SectionName).Bind(_schoolSettings);
            
            // Configure QuestPDF license
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateReportCardPdfAsync(int studentId, int academicYearId, int term)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();
                var student = await context.Students
                    .Include(s => s.Grade)
                        .ThenInclude(g => g.HomeroomTeacher)
                    .FirstOrDefaultAsync(s => s.Id == studentId);

                if (student == null)
                    throw new ArgumentException($"Student with ID {studentId} not found");

                var academicYear = await context.AcademicYears
                    .FirstOrDefaultAsync(ay => ay.Id == academicYearId);

                if (academicYear == null)
                    throw new ArgumentException($"Academic year with ID {academicYearId} not found");

                var examScores = await GetStudentExamScoresWithContext(context, studentId, academicYearId, term);

                switch (student.Grade.Section)
                {
                    case SchoolSection.EarlyLearningBeginner:
                        return await GenerateEarlyLearningBeginnerReportCardAsync(student, examScores, academicYear, term);
                    case SchoolSection.EarlyLearningIntermediate:
                        return await GenerateEarlyLearningIntermediateReportCardAsync(student, examScores,academicYear, term);
                    case SchoolSection.PrimaryLower:
                        return await GeneratePrimaryLowerReportCardAsync(student, examScores, academicYear, term);
                    case SchoolSection.PrimaryUpper:
                        return await GeneratePrimaryUpperReportCardAsync(student, examScores, academicYear, term);
                    case SchoolSection.SecondaryJunior:
                        return await GenerateSecondaryJuniorReportCardAsync(student, examScores, academicYear, term);
                    case SchoolSection.SecondarySenior:
                        return await GenerateSecondarySeniorReportCardAsync(student, examScores, academicYear, term);
                    default:
                        throw new InvalidOperationException("Unknown school section");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating report card for student {studentId}");
                throw;
            }
        }

        public async Task<byte[]> GenerateReportCardPdfAsync(int reportCardId, int studentId, int academicYearId, int term)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();
                var student = await context.Students
                    .Include(s => s.Grade)
                        .ThenInclude(g => g.HomeroomTeacher)
                    .FirstOrDefaultAsync(s => s.Id == studentId);

                if (student == null)
                    throw new ArgumentException($"Student with ID {studentId} not found");

                var academicYear = await context.AcademicYears
                    .FirstOrDefaultAsync(ay => ay.Id == academicYearId);

                if (academicYear == null)
                    throw new ArgumentException($"Academic year with ID {academicYearId} not found");

                var examScores = await GetStudentExamScoresWithContext(context, studentId, academicYearId, term);

                switch (student.Grade.Section)
                {
                    case SchoolSection.EarlyLearningBeginner:
                        return await GenerateEarlyLearningBeginnerReportCardAsync(reportCardId, student, examScores, academicYear, term);
                    case SchoolSection.EarlyLearningIntermediate:
                        return await GenerateEarlyLearningIntermediateReportCardAsync(reportCardId, student, examScores, academicYear, term);
                    case SchoolSection.PrimaryLower:
                        return await GeneratePrimaryLowerReportCardAsync(reportCardId, student, examScores, academicYear, term);
                    case SchoolSection.PrimaryUpper:
                        return await GeneratePrimaryUpperReportCardAsync(reportCardId, student, examScores, academicYear, term);
                    case SchoolSection.SecondaryJunior:
                        return await GenerateSecondaryJuniorReportCardAsync(reportCardId, student, examScores, academicYear, term);
                    case SchoolSection.SecondarySenior:
                        return await GenerateSecondarySeniorReportCardAsync(reportCardId, student, examScores, academicYear, term);
                    default:
                        throw new InvalidOperationException("Unknown school section");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating report card for student {studentId}");
                throw;
            }
        }

        public async Task<byte[]> GenerateReportCardPdfAsync(Student student, IEnumerable<ExamScore> scores, int academicYear, int term)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();
            var academicYearEntity = await context.AcademicYears
                .FirstOrDefaultAsync(ay => ay.Name.Contains(academicYear.ToString()));

            if (academicYearEntity == null)
                throw new ArgumentException($"Academic year {academicYear} not found");

            // Defensive: allow null or empty scores
            var examScores = scores?.ToList() ?? new List<ExamScore>();
            // If no scores, pass empty list to downstream logic
            if (!examScores.Any())
            {
                _logger.LogWarning($"No exam scores found for student {student?.Id} in year {academicYear}, term {term}. Generating blank report card.");
            }
            // Use the same logic as the main method, but allow empty scores
            return await GenerateReportCardPdfAsync(student.Id, academicYearEntity.Id, term);
        }

        public async Task<byte[]> GenerateReportCardPdfAsync(int reportCardId, Student student, IEnumerable<ExamScore> scores, int academicYear, int term)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();
            var academicYearEntity = await context.AcademicYears
                .FirstOrDefaultAsync(ay => ay.Name.Contains(academicYear.ToString()));

            if (academicYearEntity == null)
                throw new ArgumentException($"Academic year {academicYear} not found");

            // Defensive: allow null or empty scores
            var examScores = scores?.ToList() ?? new List<ExamScore>();
            // If no scores, pass empty list to downstream logic
            if (!examScores.Any())
            {
                _logger.LogWarning($"No exam scores found for student {student?.Id} in year {academicYear}, term {term}. Generating blank report card.");
            }
            // Use the new method with reportCardId
            return await GenerateReportCardPdfAsync(reportCardId, student.Id, academicYearEntity.Id, term);
        }

        private async Task<List<StudentExamData>> GetStudentExamScores(int studentId, int academicYearId, int term)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();
            return await GetStudentExamScoresWithContext(context, studentId, academicYearId, term);
        }

        private async Task<List<StudentExamData>> GetStudentExamScoresWithContext(SchoolDbContext context, int studentId, int academicYearId, int term)
        {
            // Get student with grade information
            var student = await context.Students
                .Include(s => s.Grade)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null)
                return new List<StudentExamData>();

            // Get all exam scores for the student
            var scores = await context.ExamScores
                .Include(es => es.Subject)
                .Include(es => es.ExamType)
                .Include(es => es.RecordedByTeacher)
                .Include(es => es.CommentsUpdatedByTeacher)
                .Where(es => es.StudentId == studentId && 
                           es.AcademicYear == academicYearId && 
                           es.Term == term)
                .ToListAsync();

            // Determine student subjects based on curriculum type
            List<Subject> studentSubjects;

            // Check if student is in secondary section - use new flexible subject assignment
            if (student.Grade?.Section == SchoolSection.SecondaryJunior || student.Grade?.Section == SchoolSection.SecondarySenior)
            {
                // Use the new ultra-flexible subject assignment system for secondary students
                var studentSubjectAssignments = await context.StudentSubjects
                    .Include(ss => ss.Subject)
                    .Where(ss => ss.StudentId == studentId && ss.IsActive)
                    .ToListAsync();

                studentSubjects = studentSubjectAssignments
                    .Select(ss => ss.Subject)
                    .ToList();
            }
            else
            {
                // Use the traditional grade-based system for primary and preschool students
                var gradeSubjects = await context.GradeSubjects
                    .Include(gs => gs.Subject)
                    .Where(gs => gs.GradeId == student.GradeId && gs.IsActive)
                    .ToListAsync();

                studentSubjects = gradeSubjects
                    .Where(gs => !gs.IsOptional || 
                                context.StudentOptionalSubjects.Any(sos => 
                                    sos.StudentId == studentId && sos.SubjectId == gs.SubjectId))
                    .Select(gs => gs.Subject)
                    .ToList();
            }

            // Filter scores to only include subjects the student actually takes
            var filteredScores = scores.Where(s => studentSubjects.Any(sub => sub.Id == s.Subject.Id)).ToList();

            var subjectGroups = filteredScores.GroupBy(s => s.Subject);
            var examData = new List<StudentExamData>();

            foreach (var subjectGroup in subjectGroups)
            {
                var subject = subjectGroup.Key;
                var subjectScores = subjectGroup.ToList();

                var test1Score = subjectScores.FirstOrDefault(s => s.ExamType.Name == "Test-One");
                var midTermScore = subjectScores.FirstOrDefault(s => s.ExamType.Name == "Test-Two");
                var test3Score = subjectScores.FirstOrDefault(s => s.ExamType.Name == "Test-Three");
                var endTermScore = subjectScores.FirstOrDefault(s => s.ExamType.Name == "End-of-Term");

                var comments = endTermScore?.Comments;

                examData.Add(new StudentExamData
                {
                    SubjectId = subject.Id,
                    SubjectName = subject.Name,
                    Test1Score = test1Score?.Score ?? 0,
                    MidTermScore = midTermScore?.Score ?? 0,
                    Test3Score = test3Score?.Score ?? 0,  
                    EndTermScore = endTermScore?.Score ?? 0,
                    Comments = comments,
                    CommentsUpdatedAt = endTermScore?.CommentsUpdatedAt,
                    CommentsUpdatedBy = endTermScore?.CommentsUpdatedByTeacher?.FullName,
                    LastUpdated = endTermScore?.RecordedAt ?? DateTime.Now,
                    RecordedBy = endTermScore?.RecordedByTeacher?.FullName
                });
            }

            return examData.OrderBy(e => e.SubjectName).ToList();
        }


        // For Primary Upper: Selects SP1, SP2, and the 4 highest scoring other subjects (total 6).
        private static List<StudentExamData> SelectPrimaryUpperSubjects(List<StudentExamData> examData)
        {
            // Always include SP1 and SP2 if present
            var selected = new List<StudentExamData>();
            var sp1 = examData.FirstOrDefault(e => e.SubjectName.Equals("SP1", StringComparison.OrdinalIgnoreCase));
            var sp2 = examData.FirstOrDefault(e => e.SubjectName.Equals("SP2", StringComparison.OrdinalIgnoreCase));
            

            if (sp1 != null) selected.Add(sp1);
            if (sp2 != null) selected.Add(sp2);

            // Exclude SP1 and SP2 from the rest
            var others = examData
                .Where(e => !e.SubjectName.Equals("SP1", StringComparison.OrdinalIgnoreCase) &&
                            !e.SubjectName.Equals("SP2", StringComparison.OrdinalIgnoreCase))
                .Where(e => !e.SubjectName.Equals("French", StringComparison.OrdinalIgnoreCase) &&
                            !e.SubjectName.Equals("MDE", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.EndTermScore)
                .Take(4)
                .ToList();

            selected.AddRange(others);

            // Ensure only 6 subjects are returned
            return selected.Take(6).ToList();
        }
        

         private static List<StudentExamData> SelectPrimaryLowerSubjects(List<StudentExamData> examData)
        {
           
            var selected = new List<StudentExamData>();
            var others = examData
                .Where(e => !e.SubjectName.Equals("French", StringComparison.OrdinalIgnoreCase) &&
                            !e.SubjectName.Equals("MDE", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.EndTermScore)
                .Take(6)
                .ToList();

            selected.AddRange(others);

            // Ensure only 6 subjects are returned
            return selected.Take(6).ToList();
        }
        private static List<StudentExamData> SelectElcSubjects(List<StudentExamData> examData)
        {
           
            var selected = new List<StudentExamData>();
            var others = examData
                .Where(e => !e.SubjectName.Equals("French", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.EndTermScore)
                .ToList();

            selected.AddRange(others);

            // Ensure only 4 subjects are returned
            return selected.Take(4).ToList();
        }

         private static List<StudentExamData> SelectBestSixSubjects(List<StudentExamData> examData)
        {

            var selected = new List<StudentExamData>();
            var others = examData
                .Where(e => !e.SubjectName.Equals("French", StringComparison.OrdinalIgnoreCase) &&
                            !e.SubjectName.Equals("MDE", StringComparison.OrdinalIgnoreCase))
                .Where(e => !e.SubjectName.Equals("Reading", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.EndTermScore)
                .Take(6)
                .ToList();

            selected.AddRange(others);

            // Ensure only 6 subjects are returned
            return selected.Take(6).ToList();
        }

       
        
        


     private static List<StudentExamData> SelectBestSixIncludingEngSubjects(List<StudentExamData> examData)
        {
            // Always include English if present
            var selected = new List<StudentExamData>();
            var eng = examData.FirstOrDefault(e => e.SubjectName.Equals("English", StringComparison.OrdinalIgnoreCase));



            if (eng != null) selected.Add(eng);


            // Exclude English from the rest
            var others = examData
                .Where(e => !e.SubjectName.Equals("English", StringComparison.OrdinalIgnoreCase))
                 .Where(e => !e.SubjectName.Equals("MDE", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e =>  e.EndTermScore)
                .Take(5)
                .ToList();

            selected.AddRange(others);

            // Ensure only 6 subjects are returned
            return selected.Take(6).ToList();
        }

         private static string DetermineCertificateOrGce(List<StudentExamData> examData)
        {
            // Select the best six subjects including English
            var bestSix = SelectBestSixIncludingEngSubjects(examData);

            // Handle case where bestSix is null or empty
            if (bestSix == null || bestSix.Count == 0)
            {
                return "N/A"; // Or "No Certificate", depending on your requirements
            }


             // Award "School Certificate" only when all grades are 7 or below.
            bool allGradesSevenOrBelow = bestSix.All(e => GetSecondarySeniorGrade(e.EndTermScore) <= 7);
            return allGradesSevenOrBelow ? "School Certificate" : "GCE";

        }




        

        // private async Task<byte[]> GenerateEarlyLearningBeginnerReportCardAsync(Student student, List<StudentExamData> examData,
        //     AcademicYear academicYear, int term)
        // {
        //     return await Task.Run(async () =>
        //     {
        //         using var ms = new MemoryStream();

        //         var document = Document.Create(container =>
        //         {
        //             container.Page(page =>
        //                 {
        //                     ElcConfigureBasicPage(page, _schoolSettings.WatermarkLogoPath);

        //                     // Remove page.Header() and put everything in Content
        //                     page.Content()
        //                     .Border(5)
        //                     .BorderColor(Colors.Blue.Darken2)
        //                     .Column(column =>
        //                     {
        //                         // Header section (only on page 1)
        //                         column.Item()
        //                         .PaddingTop(20)
        //                         .PaddingBottom(5)
        //                         .PaddingLeft(20)
        //                         .PaddingRight(20)
        //                         .Column(headerColumn =>
        //                         {
        //                             headerColumn.Item().Text(_schoolSettings.Name.ToUpper()).FontSize(18).Bold().AlignCenter();
        //                             headerColumn.Item().Text("PCELC REPORT CARD").FontSize(14).AlignCenter();
        //                             headerColumn.Item().PaddingTop(10).LineHorizontal(1);
        //                             AddStudentInformation(headerColumn, student, academicYear, term);
        //                             headerColumn.Item().PaddingTop(10).LineHorizontal(1);
        //                             headerColumn.Item().PaddingTop(20).Text("DEVELOPMENTAL PERFORMANCE").FontSize(14).Bold();
        //                         });

        //                         // Content section
        //                         column.Item()
        //                         .PaddingTop(10)
        //                         .PaddingLeft(10)
        //                         .PaddingRight(10)
        //                         .PaddingBottom(10)
        //                         .Column(async contentColumn =>
        //                         {
        //                             // For baby class students, we only need skill assessments, not exam data
        //                             try
        //                             {
        //                                 await AddBabyScoreTableWithCommentsAsync(contentColumn, student.Id, academicYear.Id, term);
        //                             }
        //                             catch (Exception ex)
        //                             {
        //                                 _logger.LogError(ex, $"Error with baby class skill assessments for student {student.Id}. Using fallback.");
        //                                 // Fallback: show a simple message instead of exam data
        //                                 contentColumn.Item().Text("BABY CLASS SKILL ASSESSMENT").FontSize(16).Bold().AlignCenter();
        //                                 contentColumn.Item().PaddingTop(10).Text("Assessment data temporarily unavailable.").FontSize(12).AlignCenter();
        //                             }

        //                             contentColumn.Item().PaddingTop(20).LineHorizontal(1);
        //                             contentColumn.Item().PageBreak();
        //                             contentColumn.Item().PaddingTop(10).Text("Class Teacher's General Comment:").FontSize(14).Bold();

        //                             // For baby class students, generate a general comment based on skill assessments
        //                             var safeFirstName = student?.FirstName ?? "the student";
        //                             var teacherComment = await GenerateBabyClassTeacherCommentAsync(student.Id, academicYear.Id, term, safeFirstName);

        //                             // Use homeroom teacher for general comments
        //                             var generalCommentTeacher = student?.Grade?.HomeroomTeacher?.FullName ?? "Class Teacher";

        //                             // Ensure teacher comment is not null or empty
        //                             var safeTeacherComment = string.IsNullOrWhiteSpace(teacherComment) 
        //                                 ? "Assessment comments are being prepared." 
        //                                 : teacherComment;

        //                             contentColumn.Item()
        //                             .PaddingTop(10)
        //                             .MinHeight(80)
        //                             .Background(Colors.Grey.Lighten3)
        //                             .Padding(10)
        //                             .DefaultTextStyle(x => x.FontSize(11).LineHeight(1.3f))
        //                             .Text($"{generalCommentTeacher}:\n\n{teacherComment}");

        //                             // contentColumn.Item().PaddingTop(10).MinHeight(80)
        //                             //     .Background(Colors.Grey.Lighten3)
        //                             //     .Padding(10)
        //                             //     .Text($"{generalCommentTeacher}:\n\n{safeTeacherComment}")
        //                             //     .FontSize(11)
        //                             //     .LineHeight(1.3f);
        //                         });
        //                     });
        //                 });

        //             // PAGE 2: Administrative Section & Grading Scale (without header)
        //             container.Page(page =>
        //             {
        //                 ElcConfigureBasicPage(page, _schoolSettings.WatermarkLogoPath);

        //                 page.Content().Border(5).BorderColor(Colors.Blue.Darken2).Padding(20).Column(column =>
        //                 {
        //                     column.Item().PaddingTop(5).LineHorizontal(1);

        //                     var overallAverage = CalculateOverallAverage(examData);

        //                     AddPreschoolAdministrativeSection(column, student.Grade);
        //                     //AddPrimaryGradingScale(column);

        //                     column.Item().PaddingTop(170).Column(contact =>
        //                     {
        //                         contact.Item().Text(_schoolSettings.Name).FontSize(14).Bold().AlignCenter();
        //                         contact.Item().Text("Plot 11289, Lusaka, Zambia").FontSize(8).AlignCenter();
        //                         contact.Item().Text("Tel: +260-955-876333  | +260-953-074465").FontSize(12).AlignCenter().FontSize(8);
        //                         contact.Item().Text($"Email: {schoolSettings.Email}").AlignCenter().FontSize(8);
        //                         contact.Item().Text($"Website: {schoolSettings.Website}").FontSize(8).AlignCenter();
        //                     });
        //                 });
        //             });

        //             // PAGE 3: Cover Page (without header)
        //             AddCoverPageELC(container, student, academicYear, term, "PCELC", _schoolSettings);

        //         });
        //         document.GeneratePdf(ms);
        //         return ms.ToArray();
        //     });
        // }



    private async Task<byte[]> GenerateEarlyLearningBeginnerReportCardAsync(Student student, List<StudentExamData> examData,
    AcademicYear academicYear, int term)
    {
        return await GenerateEarlyLearningBeginnerReportCardAsync(0, student, examData, academicYear, term);
    }

    private async Task<byte[]> GenerateEarlyLearningBeginnerReportCardAsync(int reportCardId, Student student, List<StudentExamData> examData,
    AcademicYear academicYear, int term)
{
    // FETCH ALL DATA BEFORE BUILDING THE DOCUMENT
    var safeFirstName = student?.FirstName ?? "the student";
    
    // Use manual comment if available, otherwise fall back to auto-generation
    string teacherComment;
    if (reportCardId > 0)
    {
        teacherComment = await GetGeneralCommentOrGenerateAsync(reportCardId, examData, safeFirstName);
    }
    else
    {
        teacherComment = await GenerateBabyClassTeacherCommentAsync(student.Id, academicYear.Id, term, safeFirstName);
    }
    
    var generalCommentTeacher = student?.Grade?.HomeroomTeacher?.FullName ?? "Class Teacher";
    var safeTeacherComment = string.IsNullOrWhiteSpace(teacherComment) 
        ? "Assessment comments are being prepared." 
        : teacherComment;
    
    // Fetch baby class skill assessments data
    List<BabyClassSkillAssessmentDto> skillAssessments = null;
    try
    {
        skillAssessments = await GetBabyClassSkillAssessmentsAsync(student.Id, academicYear.Id, term);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Error fetching baby class skill assessments for student {student.Id}");
    }

    // NOW BUILD THE DOCUMENT (synchronously)
    return await Task.Run(() =>
    {
        // Capture the teacher comment for use in the lambda
        var capturedTeacherComment = safeTeacherComment;
        using var ms = new MemoryStream();

        var document = Document.Create(container =>
        {
            // Capture the teacher comment for use in the document creation
            var documentTeacherComment = capturedTeacherComment;
            container.Page(page =>
            {
                // Capture the teacher comment for use in the page creation
                var pageTeacherComment = documentTeacherComment;
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10)); // Add default font
                ElcConfigureBasicPage(page, _schoolSettings.WatermarkLogoPath);

                page.Content()
                .Border(5)
                .BorderColor(Colors.Blue.Darken2)
                .Column(column =>
                {
                    // Header section
                    column.Item()
                    .PaddingTop(20)
                    .PaddingBottom(5)
                    .PaddingLeft(20)
                    .PaddingRight(20)
                    .Column(headerColumn =>
                    {
                        headerColumn.Item().Text(_schoolSettings.Name.ToUpper()).FontSize(18).Bold().AlignCenter();
                        headerColumn.Item().Text("PCELC REPORT CARD").FontSize(14).AlignCenter();
                        //headerColumn.Item().PaddingTop(10).LineHorizontal(1);
                        //AddStudentInformation(headerColumn, student, academicYear, term);
                        //headerColumn.Item().PaddingTop(10).LineHorizontal(1);
                        headerColumn.Item().PaddingTop(5).Text("BABY CLASS SKILL ASSESSMENT").FontSize(14).Bold().AlignCenter();
                    });

                    // Content section - NOW SYNCHRONOUS
                    column.Item()
                    .PaddingTop(10)
                    .PaddingLeft(10)
                    .PaddingRight(10)
                    .PaddingBottom(10)
                    .Column(contentColumn =>
                    {
                        // Add skill assessments if available
                        if (skillAssessments != null && skillAssessments.Any())
                        {
                            AddBabyScoreTableWithComments(contentColumn, skillAssessments);
                        }
                        else
                        {
                            //contentColumn.Item().Text("BABY CLASS SKILL ASSESSMENT").FontSize(16).Bold().AlignCenter();
                            contentColumn.Item().PaddingTop(10).Text("Assessment data temporarily unavailable.").FontSize(12).AlignCenter();
                        }

                        contentColumn.Item().PaddingTop(5).LineHorizontal(1);
                        contentColumn.Item().PageBreak();
                        contentColumn.Item().PaddingTop(10).Text("Class Teacher's General Comment:").FontSize(14).Bold();

                        // Teacher comment box
                        contentColumn.Item()
                            .PaddingTop(10)
                            .MinHeight(80)
                            .Background(Colors.Grey.Lighten3)
                            .Padding(10)
                            .DefaultTextStyle(x => x.FontSize(11).LineHeight(1.3f))
                            .Text($"{generalCommentTeacher}:\n\n{safeTeacherComment}");
                    });
                });
            });

            // PAGE 2: Administrative Section
            container.Page(page =>
            {
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(10)); // Add default font
                ElcConfigureBasicPage(page, _schoolSettings.WatermarkLogoPath);

                page.Content().Border(5).BorderColor(Colors.Blue.Darken2).Padding(20).Column(column =>
                {
                    column.Item().PaddingTop(5).LineHorizontal(1);

                    var overallAverage = CalculateOverallAverage(examData);

                    AddPreschoolAdministrativeSection(column, student.Grade);

                    column.Item().PaddingTop(170).Column(contact =>
                    {
                        contact.Item().Text(_schoolSettings.Name).FontSize(14).Bold().AlignCenter();
                        contact.Item().Text("Plot 11289, Lusaka, Zambia").FontSize(8).AlignCenter();
                        contact.Item().Text("Tel: +260-955-876333  | +260-953-074465").FontSize(12).AlignCenter().FontSize(8);
                        contact.Item().Text($"Email: {_schoolSettings.Email}").AlignCenter().FontSize(8);
                        contact.Item().Text($"Website: {_schoolSettings.Website}").FontSize(8).AlignCenter();
                    });
                });
            });

            // PAGE 3: Cover Page
            AddCoverPageELC(container, student, academicYear, term, "PCELC", _schoolSettings);
        });
        
        document.GeneratePdf(ms);
        return ms.ToArray();
    });
}






    private async Task<byte[]> GenerateEarlyLearningIntermediateReportCardAsync(Student student, List<StudentExamData> examData,
    AcademicYear academicYear, int term)
    {
        return await GenerateEarlyLearningIntermediateReportCardAsync(0, student, examData, academicYear, term);
    }

    private async Task<byte[]> GenerateEarlyLearningIntermediateReportCardAsync(int reportCardId, Student student, List<StudentExamData> examData,
    AcademicYear academicYear, int term)
        {
            return await Task.Run(async() =>
            {
                using var ms = new MemoryStream();
                 var allStudentsExamData = await FetchClassExamDataAsync(student.GradeId, academicYear.Id, term);
                
                // FETCH TEACHER COMMENT DATA
                var safeFirstName = student?.FirstName ?? "the student";
                
                // Use manual comment if available, otherwise fall back to auto-generation
                string teacherComment;
                if (reportCardId > 0)
                {
                    teacherComment = await GetGeneralCommentOrGenerateAsync(reportCardId, examData, safeFirstName);
                }
                else
                {
                    teacherComment = await GenerateBabyClassTeacherCommentAsync(student.Id, academicYear.Id, term, safeFirstName);
                }
                
                var generalCommentTeacher = student?.Grade?.HomeroomTeacher?.FullName ?? "Class Teacher";
                var safeTeacherComment = string.IsNullOrWhiteSpace(teacherComment) 
                    ? "Assessment comments are being prepared." 
                    : teacherComment;
                
                // Capture the teacher comment for use in the lambda
                var capturedTeacherComment = safeTeacherComment;
                
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                        {
                            ElcConfigureBasicPage(page, _schoolSettings.WatermarkLogoPath);




                            // Remove page.Header() and put everything in Content
                            page.Content()
                            .Border(5)
                            .BorderColor(Colors.Blue.Darken2)
                            .Column(column =>
                            {
                                // Header section (only on page 1)
                                column.Item()
                                .PaddingTop(20)
                                .PaddingBottom(5)
                                .PaddingLeft(20)
                                .PaddingRight(20)
                                .Column(headerColumn =>
                                {
                                    headerColumn.Item().Text(_schoolSettings.Name.ToUpper()).FontSize(18).Bold().AlignCenter();
                                    headerColumn.Item().Text("PCELC REPORT CARD").FontSize(14).AlignCenter();
                                    headerColumn.Item().PaddingTop(10).LineHorizontal(1);
                                    AddStudentInformation(headerColumn, student, academicYear, term);
                                    headerColumn.Item().PaddingTop(10).LineHorizontal(1);
                                    headerColumn.Item().PaddingTop(20).Text("DEVELOPMENTAL PERFORMANCE").FontSize(14).Bold();
                                });

                                // Content section
                                column.Item()
                                .PaddingTop(10)
                                .PaddingLeft(10)
                                .PaddingRight(10)
                                .PaddingBottom(10)
                                .Column(contentColumn =>
                                {
                                    // Full table - will auto-break across pages
                                    AddElcScoreTable(contentColumn, examData, true);

                                    contentColumn.Item().PaddingTop(20).LineHorizontal(1);
                                    contentColumn.Item().PageBreak();
                                    AddExamTotalsElcTable(contentColumn, examData, allStudentsExamData);
                                    contentColumn.Item().PaddingTop(10).Text("Class Teacher's General Comment:").FontSize(14).Bold();

                                    // Use the teacher comment (manual or auto-generated)
                                    // var teacherComment = GenerateTeacherAssessment(examData, student.FirstName);

                                    // Use homeroom teacher for general comments
                                    var generalCommentTeacher = student.Grade?.HomeroomTeacher?.FullName ?? "Class Teacher";

                                    contentColumn.Item().PaddingTop(10).MinHeight(80)
                                        .Background(Colors.Grey.Lighten3)
                                        .Padding(10)
                                        .Text($"{generalCommentTeacher}:\n\n{capturedTeacherComment}")
                                        .FontSize(11)
                                        .LineHeight(1.3f);
                                });
                            });
                        });

                    // PAGE 2: Administrative Section & Grading Scale (without header)
                    container.Page(page =>
                    {
                        ElcConfigureBasicPage(page, _schoolSettings.WatermarkLogoPath);

                        page.Content().Border(5).BorderColor(Colors.Blue.Darken2).Padding(20).Column(column =>
                        {
                            column.Item().PaddingTop(5).LineHorizontal(1);

                            var overallAverage = CalculateOverallAverage(examData);

                            AddPreschoolAdministrativeSection(column, student.Grade);
                            //AddPrimaryGradingScale(column);

                            column.Item().PaddingTop(170).Column(contact =>
                            {
                                contact.Item().Text(_schoolSettings.Name).FontSize(14).Bold().AlignCenter();
                                contact.Item().Text("Plot 11289, Lusaka, Zambia").FontSize(8).AlignCenter();
                                contact.Item().Text("Tel: +260-955-876333  | +260-953-074465").FontSize(12).AlignCenter().FontSize(8);
                                contact.Item().Text($"Email: {_schoolSettings.Email}").AlignCenter().FontSize(8);
                                contact.Item().Text($"Website: {_schoolSettings.Website}").FontSize(8).AlignCenter();
                            });
                        });
                    });

                    // PAGE 3: Cover Page (without header)
                    AddCoverPageELC(container, student, academicYear, term, "PCELC", _schoolSettings);

                });
                document.GeneratePdf(ms);
                return ms.ToArray();
            });
        }


    private async Task<byte[]> GeneratePrimaryLowerReportCardAsync(Student student, List<StudentExamData> examData,
    AcademicYear academicYear, int term)
    {
        return await GeneratePrimaryLowerReportCardAsync(0, student, examData, academicYear, term);
    }

    private async Task<byte[]> GeneratePrimaryLowerReportCardAsync(int reportCardId, Student student, List<StudentExamData> examData,
    AcademicYear academicYear, int term)
        {
            return await Task.Run(async() =>
            {
                var halfCount = (examData.Count + 1) / 2;
                var allStudentsExamData = await FetchClassExamDataAsync(student.GradeId, academicYear.Id, term);
                
                // FETCH TEACHER COMMENT DATA
                var safeFirstName = student?.FirstName ?? "the student";
                
                // Use manual comment if available, otherwise fall back to auto-generation
                string teacherComment;
                if (reportCardId > 0)
                {
                    teacherComment = await GetGeneralCommentOrGenerateAsync(reportCardId, examData, safeFirstName);
                }
                else
                {
                    teacherComment = await GenerateBabyClassTeacherCommentAsync(student.Id, academicYear.Id, term, safeFirstName);
                }
                
                var generalCommentTeacher = student?.Grade?.HomeroomTeacher?.FullName ?? "Class Teacher";
                var safeTeacherComment = string.IsNullOrWhiteSpace(teacherComment) 
                    ? "Assessment comments are being prepared." 
                    : teacherComment;
                
                // Capture the teacher comment for use in the lambda
                var capturedTeacherComment = safeTeacherComment;
                
                try
                {
                    using var ms = new MemoryStream();
                    Document.Create(container =>
                    {
                        // PAGE 1: Student Information & Scores (Part 1)
                        container.Page(page =>
                        {
                            ConfigureBasicPage(page, _schoolSettings.WatermarkLogoPath);

                            // Remove page.Header() and put everything in Content
                            page.Content()
                            .Border(5)
                            .BorderColor(Colors.Black)
                            .Column(column =>
                            {
                                // Header section (only on page 1)
                                column.Item()
                                .PaddingTop(20)
                                .PaddingBottom(5)
                                .PaddingLeft(20)
                                .PaddingRight(20)
                                .Column(headerColumn =>
                                {
                                    headerColumn.Item().Text(_schoolSettings.Name.ToUpper()).FontSize(18).Bold().AlignCenter();
                                    headerColumn.Item().Text("LOWER-PRIMARY SCHOOL REPORT CARD").FontSize(14).AlignCenter();
                                    headerColumn.Item().PaddingTop(10).LineHorizontal(1);
                                    //AddStudentInformation(headerColumn, student, academicYear, term);
                                    //headerColumn.Item().PaddingTop(10).LineHorizontal(1);
                                    headerColumn.Item().PaddingTop(20).Text("ACADEMIC PERFORMANCE").FontSize(14).Bold();
                                });

                                // Content section
                                column.Item()
                                .PaddingTop(10)
                                .PaddingLeft(10)
                                .PaddingRight(10)
                                .PaddingBottom(10)
                                .Column(contentColumn =>
                                {
                                    // Full table - will auto-break across pages
                                    AddPrimaryScoreTable(contentColumn, examData, true, isLowerPrimary: true);

                                    contentColumn.Item().PaddingTop(20).LineHorizontal(1);
                                    contentColumn.Item().PageBreak();
                                    AddExamTotalsLowerTable(contentColumn, examData, allStudentsExamData);
                                    contentColumn.Item().PaddingTop(10).Text("Class Teacher's General Comment:").FontSize(14).Bold();

                                    // Use the teacher comment (manual or auto-generated)
                                    // var teacherComment = GenerateTeacherAssessment(examData, student.FirstName);

                                    // Use homeroom teacher for general comments
                                    var generalCommentTeacher = student.Grade?.HomeroomTeacher?.FullName ?? "Class Teacher";

                                    contentColumn.Item().PaddingTop(10).MinHeight(80)
                                        .Background(Colors.Grey.Lighten3)
                                        .Padding(10)
                                        .Text($"{generalCommentTeacher}:\n\n{capturedTeacherComment}")
                                        .FontSize(11)
                                        .LineHeight(1.3f);
                                });
                            });
                        });

                        // PAGE 2: Administrative Section & Grading Scale (without header)
                        container.Page(page =>
                        {
                            ConfigureBasicPage(page, _schoolSettings.WatermarkLogoPath);

                            page.Content().Border(5).BorderColor(Colors.Black).Padding(20).Column(column =>
                            {
                                column.Item().PaddingTop(5).LineHorizontal(1);

                                var overallAverage = CalculateOverallAverage(examData);

                                AddPrimaryAdministrativeSection(column, student.Grade);
                                AddPrimaryLowerGradingScale(column);

                                column.Item().PaddingTop(170).Column(contact =>
                                {
                                    contact.Item().Text(_schoolSettings.Name).FontSize(14).Bold().AlignCenter();
                                    contact.Item().Text("Plot 11289, Lusaka, Zambia").FontSize(8).AlignCenter();
                                    contact.Item().Text("Tel: +260-955-876333  | +260-953-074465").FontSize(12).AlignCenter().FontSize(8);
                                    contact.Item().Text($"Email: {_schoolSettings.Email}").AlignCenter().FontSize(8);
                                    contact.Item().Text($"Website: {_schoolSettings.Website}").FontSize(8).AlignCenter();
                                });
                            });
                        });

                        // PAGE 3: Cover Page (without header)
                        AddCoverPage(container, student, academicYear, term, "PRIMARY SCHOOL", _schoolSettings);

                    }).GeneratePdf(ms);
                    _logger.LogInformation("PDF generated successfully");
                    return ms.ToArray();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"PDF generation failed: {ex.Message}");
                    throw;
                }
            });
        }


    private async Task<byte[]> GeneratePrimaryUpperReportCardAsync(Student student, List<StudentExamData> examData,
    AcademicYear academicYear, int term)
    {
        return await GeneratePrimaryUpperReportCardAsync(0, student, examData, academicYear, term);
    }

    private async Task<byte[]> GeneratePrimaryUpperReportCardAsync(int reportCardId, Student student, List<StudentExamData> examData,
    AcademicYear academicYear, int term)
        {
            return await Task.Run(async () =>
            {
                var halfCount = (examData.Count + 1) / 2;
                var allStudentsExamData = await FetchClassExamDataAsync(student.GradeId, academicYear.Id, term);
                
                // FETCH TEACHER COMMENT DATA
                var safeFirstName = student?.FirstName ?? "the student";
                
                // Use manual comment if available, otherwise fall back to auto-generation
                string teacherComment;
                if (reportCardId > 0)
                {
                    teacherComment = await GetGeneralCommentOrGenerateAsync(reportCardId, examData, safeFirstName);
                }
                else
                {
                    teacherComment = await GenerateBabyClassTeacherCommentAsync(student.Id, academicYear.Id, term, safeFirstName);
                }
                
                var generalCommentTeacher = student?.Grade?.HomeroomTeacher?.FullName ?? "Class Teacher";
                var safeTeacherComment = string.IsNullOrWhiteSpace(teacherComment) 
                    ? "Assessment comments are being prepared." 
                    : teacherComment;
                
                // Capture the teacher comment for use in the lambda
                var capturedTeacherComment = safeTeacherComment;
                
                try
                {
                    using var ms = new MemoryStream();
                    Document.Create(container =>
                    {
                        // PAGE 1: Student Information & Scores (Part 1)
                        container.Page(page =>
                        {
                            ConfigureBasicPage(page, _schoolSettings.WatermarkLogoPath);

                            // Remove page.Header() and put everything in Content
                            page.Content()
                            .Border(5)
                            .BorderColor(Colors.Black)
                            .Column(column =>
                            {
                                // Header section (only on page 1)
                                column.Item()
                                .PaddingTop(20)
                                .PaddingBottom(5)
                                .PaddingLeft(20)
                                .PaddingRight(20)
                                .Column(headerColumn =>
                                {
                                    headerColumn.Item().Text(_schoolSettings.Name.ToUpper()).FontSize(18).Bold().AlignCenter();
                                    headerColumn.Item().Text("UPPER-PRIMARY SCHOOL REPORT CARD").FontSize(14).AlignCenter();
                                    headerColumn.Item().PaddingTop(10).LineHorizontal(1);
                                    //AddStudentInformation(headerColumn, student, academicYear, term);
                                    //headerColumn.Item().PaddingTop(10).LineHorizontal(1);
                                    headerColumn.Item().PaddingTop(20).Text("ACADEMIC PERFORMANCE").FontSize(14).Bold();
                                });

                                // Content section
                                column.Item()
                                .PaddingTop(10)
                                .PaddingLeft(10)
                                .PaddingRight(10)
                                .PaddingBottom(10)
                                .Column(contentColumn =>
                                {
                                    // Select SP1, SP2, and 4 highest scoring subjects
                                    var selectedSubjects = SelectPrimaryUpperSubjects(examData);



                                    // Full table - will auto-break across pages
                                    AddPrimaryUpperScoreTable(contentColumn, examData, true, student);

                                    contentColumn.Item().PaddingTop(10).LineHorizontal(1);
                                    contentColumn.Item().PageBreak();

                                    AddExamTotalsTable(contentColumn, selectedSubjects, allStudentsExamData);



                                    contentColumn.Item().PaddingTop(10).Text("Class Teacher's General Comment:").FontSize(14).Bold();

                                    // Use the teacher comment (manual or auto-generated)
                                    // var teacherComment = GenerateTeacherAssessment(examData, student.FirstName);

                                    // Use homeroom teacher for general comments
                                    var generalCommentTeacher = student.Grade?.HomeroomTeacher?.FullName ?? "Class Teacher";

                                    contentColumn.Item().PaddingTop(10).MinHeight(80)
                                        .Background(Colors.Grey.Lighten3)
                                        .Padding(10)
                                        .Text($"{generalCommentTeacher}:\n\n{capturedTeacherComment}")
                                        .FontSize(11)
                                        .LineHeight(1.3f);
                                });
                            });
                        });

                        // PAGE 2: Administrative Section & Grading Scale (without header)
                        container.Page(page =>
                        {
                            ConfigureBasicPage(page, _schoolSettings.WatermarkLogoPath);

                            page.Content().Border(5).BorderColor(Colors.Black).Padding(20).Column(column =>
                            {
                                column.Item().PaddingTop(5).LineHorizontal(1);

                                var overallAverage = CalculateOverallAverage(examData);

                                AddPrimaryAdministrativeSection(column, student.Grade);
                                AddPrimaryUpperGradingScale(column);

                                column.Item().PaddingTop(170).Column(contact =>
                                {
                                    contact.Item().Text(_schoolSettings.Name).FontSize(14).Bold().AlignCenter();
                                    contact.Item().Text("Plot 11289, Lusaka, Zambia").FontSize(8).AlignCenter();
                                    contact.Item().Text("Tel: +260-955-876333  | +260-953-074465").FontSize(12).AlignCenter().FontSize(8);
                                    contact.Item().Text($"Email: {_schoolSettings.Email}").AlignCenter().FontSize(8);
                                    contact.Item().Text($"Website: {_schoolSettings.Website}").FontSize(8).AlignCenter();
                                });
                            });
                        });

                        // PAGE 3: Cover Page (without header)
                        AddCoverPage(container, student, academicYear, term, "UPPER-PRIMARY SCHOOL", _schoolSettings);

                    }).GeneratePdf(ms);
                    _logger.LogInformation("PDF generated successfully");
                    return ms.ToArray();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"PDF generation failed: {ex.Message}");
                    throw;
                }
            });
        }


    private async Task<byte[]> GenerateSecondaryJuniorReportCardAsync(Student student, List<StudentExamData> examData,
    AcademicYear academicYear, int term)
    {
        return await GenerateSecondaryJuniorReportCardAsync(0, student, examData, academicYear, term);
    }

    private async Task<byte[]> GenerateSecondaryJuniorReportCardAsync(int reportCardId, Student student, List<StudentExamData> examData,
    AcademicYear academicYear, int term)
        {
            return await Task.Run(async() =>
            {
                var halfCount = (examData.Count + 1) / 2;
                var allStudentsExamData = await FetchClassExamDataAsync(student.GradeId, academicYear.Id, term);
                
                // FETCH TEACHER COMMENT DATA
                var safeFirstName = student?.FirstName ?? "the student";
                
                // Use manual comment if available, otherwise fall back to auto-generation
                string teacherComment;
                if (reportCardId > 0)
                {
                    teacherComment = await GetGeneralCommentOrGenerateAsync(reportCardId, examData, safeFirstName);
                }
                else
                {
                    teacherComment = await GenerateBabyClassTeacherCommentAsync(student.Id, academicYear.Id, term, safeFirstName);
                }
                
                var generalCommentTeacher = student?.Grade?.HomeroomTeacher?.FullName ?? "Class Teacher";
                var safeTeacherComment = string.IsNullOrWhiteSpace(teacherComment) 
                    ? "Assessment comments are being prepared." 
                    : teacherComment;
                
                // Capture the teacher comment for use in the lambda
                var capturedTeacherComment = safeTeacherComment;
                
                using var ms = new MemoryStream();
                Document.Create(container =>
                {
                    // PAGE 1: Student Information & Scores (Part 1)

                    container.Page(page =>
                        {
                            ConfigureBasicPageSec(page, _schoolSettings.WatermarkLogoPath);

                            // Remove page.Header() and put everything in Content
                            page.Content()
                            .Border(5)
                            .BorderColor(Colors.Black)
                            .Column(column =>
                            {
                                // Header section (only on page 1)
                                column.Item()
                                .PaddingTop(20)
                                .PaddingBottom(5)
                                .PaddingLeft(20)
                                .PaddingRight(20)
                                .Column(headerColumn =>
                                {
                                    headerColumn.Item().Text(_schoolSettings.Name.ToUpper()).FontSize(18).Bold().AlignCenter();
                                    headerColumn.Item().Text("JUNIOR-SECONDARY SCHOOL REPORT CARD").FontSize(14).AlignCenter();
                                    headerColumn.Item().PaddingTop(10).LineHorizontal(1);
                                    // AddStudentInformation(headerColumn, student, academicYear, term);
                                    // headerColumn.Item().PaddingTop(10).LineHorizontal(1);
                                    headerColumn.Item().PaddingTop(20).Text("ACADEMIC PERFORMANCE").FontSize(14).Bold();
                                });

                                // Content section
                                column.Item()
                                .PaddingTop(10)
                                .PaddingLeft(10)
                                .PaddingRight(10)
                                .PaddingBottom(10)
                                .Column(contentColumn =>
                                {
                                    // Full table - will auto-break across pages
                                    AddSecondaryScoreTable(contentColumn, examData, true);

                                    contentColumn.Item().PaddingTop(20).LineHorizontal(1);
                                    contentColumn.Item().PageBreak();
                                    AddExamTotalsSecondaryJuniorTable(contentColumn, examData, allStudentsExamData);
                                    contentColumn.Item().PaddingTop(10).Text("Class Teacher's General Comment:").FontSize(14).Bold();

                                    // Use the teacher comment (manual or auto-generated)
                                    // var teacherComment = GenerateTeacherAssessment(examData, student.FirstName);

                                    // Use homeroom teacher for general comments
                                    var generalCommentTeacher = student.Grade?.HomeroomTeacher?.FullName ?? "Class Teacher";

                                    contentColumn.Item().PaddingTop(10).MinHeight(80)
                                        .Background(Colors.Grey.Lighten3)
                                        .Padding(10)
                                        .Text($"{generalCommentTeacher}:\n\n{capturedTeacherComment}")
                                        .FontSize(11)
                                        .LineHeight(1.3f);
                                });
                            });
                        });


                    // PAGE 3: Teacher Comments & Administrative

                    container.Page(page =>
                    {
                        ConfigureBasicPage(page, _schoolSettings.WatermarkLogoPath);

                        page.Content().Border(5).BorderColor(Colors.Black).Padding(20).Column(column =>
                        {
                            column.Item().PaddingTop(5).LineHorizontal(1);

                            var overallAverage = CalculateOverallAverage(examData);

                            AddSecondaryAdministrativeSection(column, student.Grade);
                            AddSecondaryJuniorGradingScale(column);

                            column.Item().PaddingTop(20).Column(contact =>
                            {
                                contact.Item().Text(_schoolSettings.Name).FontSize(14).Bold().AlignCenter();
                                contact.Item().Text("Plot 11289, Lusaka, Zambia").FontSize(8).AlignCenter();
                                contact.Item().Text("Tel: +260-955-876333  | +260-953-074465").FontSize(12).AlignCenter().FontSize(8);
                                contact.Item().Text($"Email: {_schoolSettings.Email}").AlignCenter().FontSize(8);
                                contact.Item().Text($"Website: {_schoolSettings.Website}").FontSize(8).AlignCenter();
                            });
                        });
                    });

                    // PAGE 4: Cover Page
                    AddCoverPageSec(container, student, academicYear, term, "JUNIOR-SECONDARY SCHOOL", _schoolSettings);

                }).GeneratePdf(ms);
                return ms.ToArray();
            });
        }



    private async Task<byte[]> GenerateSecondarySeniorReportCardAsync(Student student, List<StudentExamData> examData,
    AcademicYear academicYear, int term)
    {
        return await GenerateSecondarySeniorReportCardAsync(0, student, examData, academicYear, term);
    }

    private async Task<byte[]> GenerateSecondarySeniorReportCardAsync(int reportCardId, Student student, List<StudentExamData> examData,
    AcademicYear academicYear, int term)
        {
            return await Task.Run(async() =>
            {
                var halfCount = (examData.Count + 1) / 2;
                var allStudentsExamData = await FetchClassExamDataAsync(student.GradeId, academicYear.Id, term);
                
                // FETCH TEACHER COMMENT DATA
                var safeFirstName = student?.FirstName ?? "the student";
                
                // Use manual comment if available, otherwise fall back to auto-generation
                string teacherComment;
                if (reportCardId > 0)
                {
                    teacherComment = await GetGeneralCommentOrGenerateAsync(reportCardId, examData, safeFirstName);
                }
                else
                {
                    teacherComment = await GenerateBabyClassTeacherCommentAsync(student.Id, academicYear.Id, term, safeFirstName);
                }
                
                var generalCommentTeacher = student?.Grade?.HomeroomTeacher?.FullName ?? "Class Teacher";
                var safeTeacherComment = string.IsNullOrWhiteSpace(teacherComment) 
                    ? "Assessment comments are being prepared." 
                    : teacherComment;
                
                // Capture the teacher comment for use in the lambda
                var capturedTeacherComment = safeTeacherComment;
                
                using var ms = new MemoryStream();
                Document.Create(container =>
                {
                    // PAGE 1: Student Information & Scores (Part 1)

                    container.Page(page =>
                        {
                            ConfigureBasicPageSec(page, _schoolSettings.WatermarkLogoPath);

                            // Remove page.Header() and put everything in Content
                            page.Content()
                            .Border(5)
                            .BorderColor(Colors.Black)
                            .Column(column =>
                            {
                                // Header section (only on page 1)
                                column.Item()
                                .PaddingTop(20)
                                .PaddingBottom(5)
                                .PaddingLeft(20)
                                .PaddingRight(20)
                                .Column(headerColumn =>
                                {
                                    headerColumn.Item().Text(_schoolSettings.Name.ToUpper()).FontSize(18).Bold().AlignCenter();
                                    headerColumn.Item().Text("SENIOR-SECONDARY SCHOOL REPORT CARD").FontSize(14).AlignCenter();
                                    headerColumn.Item().PaddingTop(5).LineHorizontal(1);
                                    // AddStudentInformation(headerColumn, student, academicYear, term);
                                    // headerColumn.Item().PaddingTop(10).LineHorizontal(1);
                                    headerColumn.Item().PaddingTop(5).Text("ACADEMIC PERFORMANCE").FontSize(14).Bold();
                                });

                                // Content section
                                column.Item()
                                .PaddingTop(10)
                                .PaddingLeft(10)
                                .PaddingRight(10)
                                .PaddingBottom(10)
                                .Column(contentColumn =>
                                {
                                    // Full table - will auto-break across pages
                                    AddSecondaryScoreTable(contentColumn, examData, true);

                                    contentColumn.Item().PaddingTop(10).LineHorizontal(1);
                                    contentColumn.Item().PageBreak();
                                     AddExamTotalsSecondarySeniorTable(contentColumn, examData, allStudentsExamData);

                                    contentColumn.Item().PaddingTop(10).Text("Class Teacher's General Comment:").FontSize(14).Bold();

                                    // Use the teacher comment (manual or auto-generated)
                                    // var teacherComment = GenerateTeacherAssessment(examData, student.FirstName);

                                    // Use homeroom teacher for general comments
                                    var generalCommentTeacher = student.Grade?.HomeroomTeacher?.FullName ?? "Class Teacher";

                                    contentColumn.Item().PaddingTop(20).MinHeight(80)
                                        .Background(Colors.Grey.Lighten3)
                                        .Padding(10)
                                        .Text($"{generalCommentTeacher}:\n\n{capturedTeacherComment}")
                                        .FontSize(11)
                                        .LineHeight(1.3f);
                                });
                            });
                        });


                    // PAGE 3: Teacher Comments & Administrative

                    container.Page(page =>
                    {
                        ConfigureBasicPage(page, _schoolSettings.WatermarkLogoPath);

                        page.Content().Border(5).BorderColor(Colors.Black).Padding(20).Column(column =>
                        {
                            column.Item().PaddingTop(5).LineHorizontal(1);

                            var overallAverage = CalculateOverallAverage(examData);

                            AddSecondaryAdministrativeSection(column, student.Grade);
                            AddSecondarySeniorGradingScale(column);

                            column.Item().PaddingTop(20).Column(contact =>
                            {
                                contact.Item().Text(_schoolSettings.Name).FontSize(14).Bold().AlignCenter();
                                contact.Item().Text("Plot 11289, Lusaka, Zambia").FontSize(8).AlignCenter();
                                contact.Item().Text("Tel: +260-955-876333  | +260-953-074465").FontSize(12).AlignCenter().FontSize(8);
                                contact.Item().Text($"Email: {_schoolSettings.Email}").AlignCenter().FontSize(8);
                                contact.Item().Text($"Website: {_schoolSettings.Website}").FontSize(8).AlignCenter();
                            });
                        });
                    });

                    // PAGE 4: Cover Page
                    AddCoverPageSec(container, student, academicYear, term, "SENIOR-SECONDARY SCHOOL", _schoolSettings);

                }).GeneratePdf(ms);
                return ms.ToArray();
            });
        }






        // Helper Methods

     public async Task<List<List<StudentExamData>>> FetchClassExamDataAsync(int gradeId, int academicYearId, int term)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();

            // Get all student IDs in the grade/class
            var studentIds = await context.Students
                .Where(s => s.GradeId == gradeId)
                .Select(s => s.Id)
                .ToListAsync();

            // Fetch all exam scores for these students for the specified year and term
            var allScores = await context.ExamScores
                .Include(es => es.Subject)
                .Include(es => es.ExamType)
                .Include(es => es.RecordedByTeacher)
                .Include(es => es.CommentsUpdatedByTeacher)
                .Where(es => studentIds.Contains(es.StudentId) &&
                             es.AcademicYear == academicYearId &&
                             es.Term == term)
                .ToListAsync();

            // Group exam data per student - exclude students marked as absent
            var allStudentsExamData = studentIds
                .Select(id =>
                {
                    var studentScores = allScores.Where(es => es.StudentId == id).ToList();
                    
                    // Skip students with no exam records at all
                    if (!studentScores.Any())
                        return null;
                    
                    // Skip students who are marked as absent for ALL their exams
                    if (studentScores.All(s => s.IsAbsent))
                        return null;
                    
                    return studentScores
                        .GroupBy(s => s.Subject)
                        .Select(subjectGroup =>
                        {
                            var subject = subjectGroup.Key;
                            var subjectScores = subjectGroup.ToList();

                            var test1Score = subjectScores.FirstOrDefault(s => s.ExamType.Name == "Test-One");
                            var midTermScore = subjectScores.FirstOrDefault(s => s.ExamType.Name == "Term-Two");
                            var endTermScore = subjectScores.FirstOrDefault(s => s.ExamType.Name == "End-of-Term");

                            var comments = endTermScore?.Comments;

                            return new StudentExamData
                            {
                                SubjectId = subject.Id,
                                SubjectName = subject.Name,
                                Test1Score = test1Score?.Score ?? 0,
                                MidTermScore = midTermScore?.Score ?? 0,
                                EndTermScore = endTermScore?.Score ?? 0,
                                Comments = comments,
                                CommentsUpdatedAt = endTermScore?.CommentsUpdatedAt,
                                CommentsUpdatedBy = endTermScore?.CommentsUpdatedByTeacher?.FullName,
                                LastUpdated = endTermScore?.RecordedAt ?? System.DateTime.Now,
                                RecordedBy = endTermScore?.RecordedByTeacher?.FullName,
                                // Track absent status for each exam type
                                IsTest1Absent = test1Score?.IsAbsent ?? false,
                                IsMidTermAbsent = midTermScore?.IsAbsent ?? false,
                                IsEndTermAbsent = endTermScore?.IsAbsent ?? false
                            };
                        })
                        .OrderBy(e => e.SubjectName)
                        .ToList();
                })
                .Where(studentData => studentData != null) // Remove null entries (absent students)
                .Cast<List<StudentExamData>>() // Cast to remove nullability
                .ToList();

            return allStudentsExamData;
        }

        // Generate teacher comment specifically for baby class students based on skill assessments
        private async Task<string> GenerateBabyClassTeacherCommentAsync(int studentId, int academicYearId, int term, string firstName)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();

                // Fetch baby class skill assessments for the student
                var skillAssessments = await context.BabyClassSkillAssessments
                    .Include(a => a.SkillItem)
                        .ThenInclude(si => si.Skill)
                    .Include(a => a.AssessedByNavigation)
                    .Where(a => a.StudentId == studentId && 
                               a.AcademicYear == academicYearId && 
                               a.Term == term)
                    .ToListAsync();

                if (!skillAssessments.Any())
                {
                    return $"Throughout this term, {firstName} has been working on various developmental areas. " +
                           "Assessment data is currently being compiled. Please continue to support their learning journey.";
                }

                // Group assessments by skill category
                var assessmentsBySkill = skillAssessments
                    .Where(a => a != null && a.SkillItem != null && a.SkillItem.Skill != null)
                    .GroupBy(a => a.SkillItem.Skill)
                    .Where(g => g.Key != null && !string.IsNullOrWhiteSpace(g.Key.Name))
                    .ToList();

                if (!assessmentsBySkill.Any())
                {
                    return $"Throughout this term, {firstName} has been working on various developmental areas. " +
                           "Assessment data is currently being compiled. Please continue to support their learning journey.";
                }

                var safeFirstName = string.IsNullOrWhiteSpace(firstName) ? "the student" : firstName;
                var comment = $"Throughout this term, {safeFirstName} has demonstrated positive progress " +
                            $"across {assessmentsBySkill.Count} developmental areas. ";

                // Add specific comments about each skill area
                var skillAreas = assessmentsBySkill.Select(g => g.Key.Name).ToList();
                if (skillAreas.Any())
                {
                    comment += $"Areas of focus included {string.Join(", ", skillAreas)}. ";
                }

                // Check for any specific teacher comments
                var hasTeacherComments = skillAssessments.Any(a => !string.IsNullOrWhiteSpace(a.TeacherComment));
                if (hasTeacherComments)
                {
                    comment += "Detailed observations have been recorded for each skill area. ";
                }

                comment += "Continue to support their learning journey through play-based activities and positive reinforcement.";

                return comment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating baby class teacher comment for student {studentId}");
                var safeFirstName = string.IsNullOrWhiteSpace(firstName) ? "the student" : firstName;
                return $"Throughout this term, {safeFirstName} has been working on various developmental areas. " +
                       "Assessment data is currently being compiled. Please continue to support their learning journey.";
            }
        }

        private static void ConfigureBasicPage(PageDescriptor page, string watermarkLogoPath)
        {
            page.Size(PageSizes.A4);
            page.Margin(1.5f, Unit.Centimetre);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(10));


            page.Background()
            .AlignCenter()
            .AlignMiddle()
            .Width(500)
            .Image(watermarkLogoPath);
        }

         private static void ConfigureBasicPageSec(PageDescriptor page, string watermarkLogoPath)
        {
            page.Size(PageSizes.A4);
            page.Margin(1.5f, Unit.Centimetre);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(10));


            page.Background()
            .AlignCenter()
            .AlignMiddle()
            .Width(500)
            .Image(watermarkLogoPath);
        }

         private static void ElcConfigureBasicPage(PageDescriptor page, string watermarkLogoPath)
        {
            page.Size(PageSizes.A4);
            page.Margin(1.5f, Unit.Centimetre);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(10));


            page.Background()
            .AlignCenter()
            .AlignMiddle()
            .Width(500)
            .Image("./Media/ltwm2.png");
        }


        private static void AddStudentInformation(ColumnDescriptor column, Student student,
            AcademicYear academicYear, int term)
        {
            column.Item().PaddingTop(5).Text("STUDENT INFORMATION").FontSize(14).Bold();

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Name: {student?.FirstName ?? ""} {student?.LastName ?? ""}").Bold();
                    col.Item().Text($"Year: {academicYear?.Name ?? "Unknown Year"}").Bold();
                    //col.Item().Text($"Next Term Begins: {DateTime.Now.AddDays(30):dd/MM/yyyy}").Bold();
                    //col.Item().Text($"Student Number: {student.StudentNumber}");
                    //col.Item().Text($"Date of Birth: {student.DateOfBirth:dd/MM/yyyy}");
                });
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Class: {student?.Grade?.FullName ?? "Unknown Class"}").Bold();
                    col.Item().Text($"Term: {term}").Bold();
                });
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Class Teacher: {student?.Grade?.HomeroomTeacher?.FullName ?? "Not Assigned"}").Bold();
                    col.Item().Text($"Report Date: {DateTime.Now:dd-MM-yyyy}").Bold();

                });
            });
        }



        private static void AddPrimaryScoreTable(ColumnDescriptor column, List<StudentExamData> examData, bool showHeader, bool isLowerPrimary = false)
        {
            if (examData == null || !examData.Any())
            {
                column.Item().Text("No scores available").Italic().FontSize(10);
                return;
            }
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                if (showHeader)
                {
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).AlignCenter().AlignCenter().Text("SUBJECT").Bold();
                        header.Cell().Element(CellStyle).AlignCenter().AlignCenter().Text("TEST 1").Bold();
                        header.Cell().Element(CellStyle).AlignCenter().AlignCenter().Text("TEST 2").Bold();
                        header.Cell().Element(CellStyle).AlignCenter().AlignCenter().Text("END TERM").Bold();
                        header.Cell().Element(CellStyle).AlignCenter().AlignCenter().Text("TERM AVERAGE").Bold();
                        header.Cell().Element(CellStyle).AlignCenter().AlignCenter().Text("GRADE").Bold();
                    });
                }

                foreach (var exam in examData)
                {
                    var average = CalculateSubjectAverage(exam);
                    
                    // For Lower Primary: use End-of-Term score with Lower Primary grade scale
                    // For Upper Primary: use Term Average with Upper Primary grade scale
                    string grade;
                    if (isLowerPrimary)
                    {
                        grade = GetLowerPrimaryGrade(exam.EndTermScore);
                    }
                    else
                    {
                        grade = GetPrimaryUpperGrade(average);
                    }

                    table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).Text(exam.SubjectName);
                    table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(FormatScoreDisplay(exam.Test1Score, exam.IsTest1Absent));
                    table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(FormatScoreDisplay(exam.MidTermScore, exam.IsMidTermAbsent));
                    table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(FormatScoreDisplay(exam.EndTermScore, exam.IsEndTermAbsent));
                    table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(average.ToString("F1"));
                    table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(grade).Bold();

                    // Second row: Comments (if available)
                    if (!string.IsNullOrWhiteSpace(exam.Comments))
                    {
                        var commentAuthor = !string.IsNullOrEmpty(exam.CommentsUpdatedBy)
                                          ? exam.CommentsUpdatedBy
                                          : exam.RecordedBy ?? "System";

                        table.Cell().Element(CellStyle).Text($"Remark:");
                        table.Cell().ColumnSpan(5).Element(CellStyle).Column(commentCol =>
                        {

                            // Truncate long comments
                            var truncatedComment = exam.Comments.Length > 100 ?
                                exam.Comments.Substring(0, 100) + "..." : exam.Comments;
                            commentCol.Item().Text(truncatedComment).FontSize(10).FontColor(Colors.Black);


                        });


                    }


                }
            });
        }





        private static void AddBabyScoreTable(ColumnDescriptor column, List<StudentExamData> examData, bool showHeader)
        {
            // For Baby Class, we need to display skill assessments instead of subject scores
            // This method will be updated to fetch and display the 7 skill categories
            // with their sub-items and teacher comments
            
            column.Item().Text("BABY CLASS SKILL ASSESSMENT").FontSize(16).Bold().AlignCenter();
            column.Item().PaddingTop(10);
            
            // Display the 7 skill categories with their sub-items
            DisplaySkillCategory(column, "1. COMMUNICATION SKILLS", new[] {
                "• Speaks Clearly",
                "• Responds to direct questions"
            });
            
            DisplaySkillCategory(column, "2. SOCIAL EMOTIONAL SKILLS", new[] {
                "• Know first name",
                "• Follows Instruction",
                "• Shares well with others"
            });
            
            DisplaySkillCategory(column, "3. READING & WRITING", new[] {
                "• Know how to say letterland characters",
                "• Able to say sounds"
            });
            
            DisplaySkillCategory(column, "4. COLOUR & SHAPES", new[] {
                "• Know Primary Colours",
                "• Knows Shapes"
            });
            
            DisplaySkillCategory(column, "5. NUMBERS", new[] {
                "• Able to count",
                "• Orally from 1 - 10"
            });
            
            DisplaySkillCategory(column, "6. FINE-MOTOR SKILLS", new[] {
                "• Can hold and use a pencil",
                "• Can hold and use a Crayon",
                "• Able to Trace"
            });
            
            DisplaySkillCategory(column, "7. GROSS MOTOR SKILLS", new[] {
                "• Can jump up and down"
            });
        }
        
        private static void DisplaySkillCategory(ColumnDescriptor column, string categoryName, string[] skillItems)
        {
            // Ensure category name is not null
            var safeCategoryName = categoryName ?? "Unknown Category";
            
            column.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                });
                
                table.Cell().Element(CellStyle).Text(safeCategoryName).Bold().FontSize(12);
                table.Cell().Element(CellStyle).Text("Teacher Comments:").Bold().FontSize(10);
                
                // Ensure skillItems is not null and filter out null items
                var safeSkillItems = skillItems?.Where(item => !string.IsNullOrWhiteSpace(item)).ToArray() ?? new string[0];
                
                foreach (var item in safeSkillItems)
                {
                    // Ensure item is not null
                    var safeItem = item ?? "Unknown Skill";
                    table.Cell().Element(CellStyle).PaddingLeft(10).Text(safeItem).FontSize(10);
                    table.Cell().Element(CellStyle).MinHeight(30).Background(Colors.Grey.Lighten4)
                        .Padding(5).Text("_________________________________").FontSize(9);
                }
            });
        }

        // New method to display Baby Class skills with actual teacher comments from database
        private async Task AddBabyScoreTableWithCommentsAsync(ColumnDescriptor column, int studentId, int academicYearId, int term)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchoolDbContext>();

            try
            {
                _logger.LogInformation($"Fetching skill assessments for student {studentId}, year {academicYearId}, term {term}");
                
                // Fetch actual skill assessments from database with comprehensive null filtering
                var skillAssessments = await context.BabyClassSkillAssessments
                    .Include(a => a.SkillItem)
                        .ThenInclude(si => si.Skill)
                    .Include(a => a.AssessedByNavigation)
                    .Where(a => a.StudentId == studentId && 
                               a.AcademicYear == academicYearId && 
                               a.Term == term)
                    .ToListAsync();

                _logger.LogInformation($"Found {skillAssessments?.Count ?? 0} skill assessments");

                // Comprehensive null filtering and validation
                var validAssessments = skillAssessments?
                    .Where(a => a != null && 
                               a.SkillItem != null && 
                               a.SkillItem.Skill != null &&
                               !string.IsNullOrWhiteSpace(a.SkillItem.Name) &&
                               !string.IsNullOrWhiteSpace(a.SkillItem.Skill.Name))
                    .ToList() ?? new List<BabyClassSkillAssessment>();

                if (validAssessments.Any())
                {
                    // Group assessments by skill category with additional null safety
                    var assessmentsBySkill = validAssessments
                        .GroupBy(a => a.SkillItem.Skill)
                        .Where(g => g.Key != null && !string.IsNullOrWhiteSpace(g.Key.Name))
                        .OrderBy(g => g.Key.Order)
                        .ToList();

                    if (assessmentsBySkill.Any())
                    {
                        // Safe title rendering
                        var titleText = "BABY CLASS SKILL ASSESSMENT";
                        column.Item().Text(titleText).FontSize(16).Bold().AlignCenter();
                        column.Item().PaddingTop(10);

                        foreach (var skillGroup in assessmentsBySkill)
                        {
                            if (skillGroup?.Key == null) continue;
                            
                            var skill = skillGroup.Key;
                            var skillItems = skillGroup
                                .Where(a => a != null && a.SkillItem != null && !string.IsNullOrWhiteSpace(a.SkillItem.Name))
                                .OrderBy(a => a.SkillItem.Order)
                                .ToList();

                            if (!skillItems.Any()) continue;

                            column.Item().PaddingTop(8).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(2);
                                });

                                // Bulletproof skill header text
                                var skillName = (skill?.Name ?? "").Trim();
                                var skillOrder = skill?.Order ?? 0;
                                
                                if (string.IsNullOrWhiteSpace(skillName))
                                    skillName = "Unknown Skill";
                                
                                var skillHeaderText = $"{skillOrder}. {skillName.ToUpper()}";
                                table.Cell().Element(CellStyle).Text(skillHeaderText).Bold().FontSize(12);
                                table.Cell().Element(CellStyle).Text("Teacher Comments:").Bold().FontSize(10);

                                foreach (var assessment in skillItems)
                                {
                                    if (assessment?.SkillItem == null) continue;
                                    
                                    // Bulletproof skill item text
                                    var skillItemName = (assessment.SkillItem.Name ?? "").Trim();
                                    if (string.IsNullOrWhiteSpace(skillItemName))
                                        skillItemName = "Unknown Skill";
                                    
                                    var skillItemText = $"• {skillItemName}";
                                    table.Cell().Element(CellStyle).PaddingLeft(10).Text(skillItemText).FontSize(10);
                                    
                                    // Bulletproof comment text
                                    var commentText = (assessment.TeacherComment ?? "").Trim();
                                    if (string.IsNullOrWhiteSpace(commentText))
                                        commentText = "No comment provided";
                                    
                                    table.Cell().Element(CellStyle).MinHeight(30).Background(Colors.Grey.Lighten4)
                                        .Padding(5).Text(commentText).FontSize(9);
                                }
                            });
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"No valid skill groups found for student {studentId}. Using fallback display.");
                        AddBabyScoreTable(column, new List<StudentExamData>(), true);
                    }
                }
                else
                {
                    _logger.LogWarning($"No valid skill assessments found for student {studentId}. Using fallback display.");
                    AddBabyScoreTable(column, new List<StudentExamData>(), true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in database-driven comments for student {studentId}. Using fallback display.");
                try
                {
                    AddBabyScoreTable(column, new List<StudentExamData>(), true);
                }
                catch (Exception fallbackEx)
                {
                    _logger.LogError(fallbackEx, $"Error in fallback display for student {studentId}. Using minimal fallback.");
                    // Minimal fallback - just show a simple message
                    column.Item().Text("BABY CLASS SKILL ASSESSMENT").FontSize(16).Bold().AlignCenter();
                    column.Item().PaddingTop(10).Text("Assessment data temporarily unavailable.").FontSize(12).AlignCenter();
                }
            }
        }





        private static void AddElcScoreTable(ColumnDescriptor column, List<StudentExamData> examData, bool showHeader)
        {
            if (examData == null || !examData.Any())
            {
                column.Item().Text("No scores available").Italic().FontSize(10);
                return;
            }
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    //columns.RelativeColumn(1);
                });

                if (showHeader)
                {
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).AlignCenter().AlignCenter().Text("SUBJECT").Bold();
                        header.Cell().Element(CellStyle).AlignCenter().AlignCenter().Text("TEST 1").Bold();
                        header.Cell().Element(CellStyle).AlignCenter().AlignCenter().Text("TEST 2").Bold();
                        header.Cell().Element(CellStyle).AlignCenter().AlignCenter().Text("END OF TERM").Bold();
                        header.Cell().Element(CellStyle).AlignCenter().AlignCenter().Text("TERM AVERAGE").Bold();
                        //header.Cell().Element(CellStyle).AlignCenter().AlignCenter().Text("GRADE").Bold();
                    });
                }

                foreach (var exam in examData)
                {
                    var average = CalculateSubjectAverage(exam);
                    //var grade = GetGrade(average);
                    var grade = GetPrimaryUpperGrade(average);

                    table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).Text(exam.SubjectName);
                    table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(FormatScoreDisplay(exam.Test1Score, exam.IsTest1Absent));
                    table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(FormatScoreDisplay(exam.MidTermScore, exam.IsMidTermAbsent));
                    table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(FormatScoreDisplay(exam.EndTermScore, exam.IsEndTermAbsent));
                    table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(average.ToString("F1"));
                    //table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(grade).Bold();

                    // Second row: Comments (if available)
                    if (!string.IsNullOrWhiteSpace(exam.Comments))
                    {
                        var commentAuthor = !string.IsNullOrEmpty(exam.CommentsUpdatedBy)
                                          ? exam.CommentsUpdatedBy
                                          : exam.RecordedBy ?? "System";

                        table.Cell().Element(CellStyle).Text($"Remark:");
                        table.Cell().ColumnSpan(4).Element(CellStyle).Column(commentCol =>
                        {

                            // Truncate long comments
                            var truncatedComment = exam.Comments.Length > 100 ?
                                exam.Comments.Substring(0, 100) + "..." : exam.Comments;
                            commentCol.Item().Text(truncatedComment).FontSize(10).FontColor(Colors.Black);


                        });


                    }


                }
            });
        }



        private static void AddExamTotalsElcTable(
            ColumnDescriptor column, 
            List<StudentExamData> examData, 
            List<List<StudentExamData>> classExamData // List of each student's examData
                )
        {
            // Calculate pupil's total for selected subjects
            var selectedSubjects = examData
                .Where(e => 
                    !e.SubjectName.Equals("French", StringComparison.OrdinalIgnoreCase) &&
                    !e.SubjectName.Equals("Physical Education", StringComparison.OrdinalIgnoreCase)
                )
                .OrderByDescending(e => e.EndTermScore)
                .ToList();
           
            var totalEndTerm = selectedSubjects.Sum(e => e.EndTermScore);
        
            var studentTotals = classExamData
                .Select(studentData => SelectElcSubjects(studentData)
                    .Where(e => !e.IsEndTermAbsent) // Exclude absent students from class average
                    .Sum(e => e.EndTermScore))
                .Where(total => total > 0) // Only include students who had at least one non-absent score
                .ToList();

            decimal classAverage = studentTotals.Any()
                ? studentTotals.Average()
                : 0;

            
        
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                });
        
                table.Cell().Element(CellStyle).Text("Total Score").Bold();
                table.Cell().Element(CellStyle).AlignCenter().Text("400").Bold();
        
                table.Cell().Element(CellStyle).Text("Pupil's Mark").Bold();
                table.Cell().Element(CellStyle).AlignCenter().Text(totalEndTerm.ToString("F0")).Bold();
        
                // table.Cell().Element(CellStyle).Text("Class Average").Bold();
                // table.Cell().Element(CellStyle).AlignCenter().Text(classAverage.ToString("F0")).Bold();
            });
        }






        private static void AddExamTotalsTable(
            ColumnDescriptor column,
            List<StudentExamData> examData,
            List<List<StudentExamData>> classExamData // List of each student's examData
                )
        {
            // Calculate pupil's total for selected subjects
            var selectedSubjects = SelectPrimaryUpperSubjects(examData);
            var totalEndTerm = selectedSubjects.Sum(e => e.EndTermScore);

            // Calculate class average for selected subjects
            var studentTotals = classExamData
                .Select(studentData => SelectPrimaryUpperSubjects(studentData)
                    .Where(e => !e.IsEndTermAbsent) // Exclude absent students from class average
                    .Sum(e => e.EndTermScore))
                .Where(total => total > 0) // Only include students who had at least one non-absent score
                .ToList();

            decimal classAverage = studentTotals.Any()
                ? studentTotals.Average()
                : 0;
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                });

                table.Cell().Element(CellStyle).Text("Total Score").Bold();
                table.Cell().Element(CellStyle).AlignCenter().Text("900").Bold();

                table.Cell().Element(CellStyle).Text("Pupil's Mark").Bold();
                table.Cell().Element(CellStyle).AlignCenter().Text(totalEndTerm.ToString("F0")).Bold();

                table.Cell().Element(CellStyle).Text("Class Average").Bold();
                table.Cell().Element(CellStyle).AlignCenter().Text(classAverage.ToString("F0")).Bold();
            });
        }



         private static void AddExamTotalsLowerTable(
            ColumnDescriptor column, 
            List<StudentExamData> examData, 
            List<List<StudentExamData>> classExamData // List of each student's examData
                )
        {
            // Calculate pupil's total for selected subjects
            var selectedSubjects = SelectPrimaryLowerSubjects(examData);
            var totalEndTerm = selectedSubjects.Sum(e => e.EndTermScore);
        
            var studentTotals = classExamData
                .Select(studentData => SelectPrimaryLowerSubjects(studentData)
                    .Where(e => !e.IsEndTermAbsent) // Exclude absent students from class average
                    .Sum(e => e.EndTermScore))
                .Where(total => total > 0) // Only include students who had at least one non-absent score
                .ToList();

            decimal classAverage = studentTotals.Any()
                ? studentTotals.Average()
                : 0;

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                });
        
                table.Cell().Element(CellStyle).Text("Total Score").Bold();
                table.Cell().Element(CellStyle).AlignCenter().Text("600").Bold();
        
                table.Cell().Element(CellStyle).Text("Pupil's Mark").Bold();
                table.Cell().Element(CellStyle).AlignCenter().Text(totalEndTerm.ToString("F0")).Bold();
        
                table.Cell().Element(CellStyle).Text("Class Average").Bold();
                table.Cell().Element(CellStyle).AlignCenter().Text(classAverage.ToString("F0")).Bold();
            });
        }


        private static void AddExamTotalsSecondaryJuniorTable(
            ColumnDescriptor column, 
            List<StudentExamData> examData, 
            List<List<StudentExamData>> classExamData // List of each student's examData
                )
        {
            // Calculate pupil's total for selected subjects
            var selectedSubjects = SelectBestSixSubjects(examData);
            //var selectedSubjects = SelectPrimaryLowerSubjects(examData);
            var totalEndTerm = selectedSubjects.Sum(e => e.EndTermScore);
        
            var studentTotals = classExamData
                .Select(studentData => SelectBestSixSubjects(studentData)
                    .Where(e => !e.IsEndTermAbsent) // Exclude absent students from class average
                    .Sum(e => e.EndTermScore))
                .Where(total => total > 0) // Only include students who had at least one non-absent score
                .ToList();

            decimal classAverage = studentTotals.Any()
                ? studentTotals.Average()
                : 0;
        
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                });
        
                table.Cell().Element(CellStyle).Text("Total Score").Bold();
                table.Cell().Element(CellStyle).AlignCenter().Text("600").Bold();
        
                table.Cell().Element(CellStyle).Text("Pupil's Mark").Bold();
                table.Cell().Element(CellStyle).AlignCenter().Text(totalEndTerm.ToString("F0")).Bold();
        
                table.Cell().Element(CellStyle).Text("Class Average").Bold();
                table.Cell().Element(CellStyle).AlignCenter().Text(classAverage.ToString("F0")).Bold();
            });
        }

         private static void AddExamTotalsSecondarySeniorTable(
            ColumnDescriptor column, 
            List<StudentExamData> examData, 
            List<List<StudentExamData>> classExamData // List of each student's examData
                )
        {
            // Calculate pupil's total for selected subjects
            var selectedSubjects = SelectBestSixIncludingEngSubjects(examData);
            //var selectedSubjects = SelectPrimaryLowerSubjects(examData);
            var totalEndTerm = selectedSubjects.Sum(e => e.EndTermScore);
        
            var studentTotals = classExamData
                    .Select(studentData => SelectBestSixIncludingEngSubjects(studentData)
                        .Where(e => !e.IsEndTermAbsent) // Exclude absent students from class average
                        .Sum(e => e.EndTermScore))
                    .Where(total => total > 0) // Only include students who had at least one non-absent score
                    .ToList();

        decimal classAverage = studentTotals.Any()
            ? studentTotals.Average()
            : 0;
            // Calculate sum of best six grades (lowest grades are best)
            var bestSixGrades = selectedSubjects
                .Select(e => GetSecondarySeniorGrade(e.EndTermScore))
                .OrderBy(g => g)
                .Take(6)
                .ToList();
            int pointsObtained = bestSixGrades.Sum();

            // Calculate certificate type
             var certificateType = DetermineCertificateOrGce(examData);
        
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                });

              

                table.Cell().Element(CellStyle).Text("Best Six (Including English)").Bold();
                table.Cell().Element(CellStyle).AlignCenter().Text($"{pointsObtained} points").Bold();

                table.Cell().Element(CellStyle).Text("Certficate Type").Bold();
                table.Cell().Element(CellStyle).AlignCenter().Text(certificateType).Bold();
    
            });
        }




        private static void AddPrimaryUpperScoreTable(ColumnDescriptor column, List<StudentExamData> examData, bool showHeader,Student student)
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                     if (student.Grade?.Name == "Grade 7")
                        {
                            columns.RelativeColumn(1);  // TEST 3 (conditional)
                        }
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                if (showHeader)
                {
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).AlignCenter().Text("SUBJECT").Bold();
                        header.Cell().Element(CellStyle).AlignCenter().Text("TEST 1").Bold();
                        header.Cell().Element(CellStyle).AlignCenter().Text("TEST 2").Bold();
                        // Add TEST 3 column conditionally for Grade 7
                        if (student.Grade?.Name == "Grade 7")
                        {
                            header.Cell().Element(CellStyle).AlignCenter().Text("TEST 3").Bold();
                        }
                        
                        header.Cell().Element(CellStyle).AlignCenter().Text("END TERM").Bold();
                        header.Cell().Element(CellStyle).AlignCenter().Text("TERM AVERAGE").Bold();
                        header.Cell().Element(CellStyle).AlignCenter().Text("GRADE").Bold();
                    });
                }

                foreach (var exam in examData)
                {
                    var selectedSubjects = SelectPrimaryUpperSubjects(examData);
                    // Calculate average using proper method that handles absent tests
                    var average = student.Grade?.Name == "Grade 7" 
                        ? CalculateGrade7SubjectAverage(exam)
                        : CalculateSubjectAverage(exam);
                    var mark = exam.EndTermScore;
                    //var grade = GetGrade(average);
                    var grade = GetPrimaryUpperGrade(mark);

                    table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).Text(exam.SubjectName);
                    table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(FormatScoreDisplay(exam.Test1Score, exam.IsTest1Absent));
                    table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(FormatScoreDisplay(exam.MidTermScore, exam.IsMidTermAbsent));
                    // Add TEST 3 column conditionally for Grade 7
                    if (student.Grade?.Name == "Grade 7")
                    {
                        table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(FormatScoreDisplay(exam.Test3Score, exam.IsTest3Absent));
                    }
    
                    table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(FormatScoreDisplay(exam.EndTermScore, exam.IsEndTermAbsent));
                    table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(average.ToString("F1"));
                    table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(grade).Bold();

                    // Second row: Comments (if available)
                    if (!string.IsNullOrWhiteSpace(exam.Comments))
                    {
                        var commentAuthor = !string.IsNullOrEmpty(exam.CommentsUpdatedBy)
                                          ? exam.CommentsUpdatedBy
                                          : exam.RecordedBy ?? "System";

                        table.Cell().Element(CellStyle).Text($"Remark:");
                        // Adjust column span based on whether it's Grade 7 (7 columns) or other grades (6 columns)
                        var columnSpan = (uint)(student.Grade?.Name == "Grade 7" ? 6 : 5);
                        table.Cell().ColumnSpan(columnSpan).Element(CellStyle).Column(commentCol =>
                        {

                            // Truncate long comments
                            var truncatedComment = exam.Comments.Length > 100 ?
                                exam.Comments.Substring(0, 100) + "..." : exam.Comments;
                            commentCol.Item().Text(truncatedComment).FontSize(8).FontColor(Colors.Black);


                        });


                    }


                }
            });
        }

        

        private static void AddSecondaryScoreTable(ColumnDescriptor column, List<StudentExamData> examData, bool showHeader)
        {
            column.Item().Table(table =>
             {
                 table.ColumnsDefinition(columns =>
                 {
                     columns.RelativeColumn(2);
                     columns.RelativeColumn(1);
                     columns.RelativeColumn(1);
                     columns.RelativeColumn(1);
                     columns.RelativeColumn(1);
                     columns.RelativeColumn(1);
                 });

                 if (showHeader)
                 {
                     table.Header(header =>
                     {
                         header.Cell().Element(CellStyle).Text("SUBJECT").Bold();
                         header.Cell().Element(CellStyle).Text("TEST 1").Bold();
                         header.Cell().Element(CellStyle).Text("TEST 2").Bold();
                         header.Cell().Element(CellStyle).Text("END TERM").Bold();
                         header.Cell().Element(CellStyle).Text("TERM AVERAGE").Bold();
                         header.Cell().Element(CellStyle).Text("GRADE").Bold();
                     });
                 }

                 foreach (var exam in examData)
                 {
                     var average = CalculateSubjectAverage(exam);
                     var mark = exam.EndTermScore;
                     var grade = GetSecondarySeniorGrade(mark);
                     //var grade = GetGrade(average);

                     table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).Text(exam.SubjectName);
                     table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(FormatScoreDisplay(exam.Test1Score, exam.IsTest1Absent));
                     table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(FormatScoreDisplay(exam.MidTermScore, exam.IsMidTermAbsent));
                     table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(FormatScoreDisplay(exam.EndTermScore, exam.IsEndTermAbsent));
                     table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(average.ToString("F1"));
                     table.Cell().Background(Colors.Grey.Lighten2).Element(CellStyle).AlignCenter().Text(grade.ToString()).Bold();

                     // Second row: Comments (if available)
                     if (!string.IsNullOrWhiteSpace(exam.Comments))
                     {
                         var commentAuthor = !string.IsNullOrEmpty(exam.CommentsUpdatedBy)
                                           ? exam.CommentsUpdatedBy
                                           : exam.RecordedBy ?? "System";

                         table.Cell().Element(CellStyle).Text($"Remark:");
                         table.Cell().ColumnSpan(5).Element(CellStyle).Column(commentCol =>
                         {

                             // Truncate long comments
                             var truncatedComment = exam.Comments.Length > 100 ?
                                 exam.Comments.Substring(0, 100) + "..." : exam.Comments;
                             commentCol.Item().Text(truncatedComment).FontSize(8).FontColor(Colors.Black);


                         });


                     }


                 }
             });
        }

        private static void AddEndOfTermCommentsSection(ColumnDescriptor column, List<StudentExamData> examData, string part)
        {
            var subjectsWithComments = examData.Where(e => !string.IsNullOrWhiteSpace(e.Comments)).ToList();
            
            if (subjectsWithComments.Any())
            {
                column.Item().PaddingTop(15).Text($"END-OF-TERM COMMENTS - {part}").FontSize(12).Bold();
                
                foreach (var exam in subjectsWithComments)
                {
                    column.Item().PaddingTop(5).Column(col =>
                    {
                        col.Item().Text($"{exam.SubjectName}:").FontSize(10).Bold();
                        col.Item().Text(exam.Comments).FontSize(9);
                        if (exam.CommentsUpdatedAt.HasValue)
                        {
                            col.Item().Text($"Updated: {exam.CommentsUpdatedAt:dd/MM/yyyy} by {exam.CommentsUpdatedBy}")
                                .FontSize(8).Italic().FontColor(Colors.Grey.Darken1);
                        }
                    });
                }
            }
        }

        private static void AddOverallSummary(ColumnDescriptor column, List<StudentExamData> examData)
        {
            var overallAverage = CalculateOverallAverage(examData);
            
            column.Item().PaddingTop(15).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                table.Cell().Element(CellStyle).Text("OVERALL AVERAGE").Bold();
                table.Cell().Element(CellStyle).Text("");
                table.Cell().Element(CellStyle).Text("");
                table.Cell().Element(CellStyle).Text("");
                table.Cell().Element(CellStyle).AlignCenter().Text(overallAverage.ToString("F1")).Bold();
                table.Cell().Element(CellStyle).AlignCenter().Text(GetGrade(overallAverage)).Bold();
            });
        }

        private static void AddSecondaryOverallSummary(ColumnDescriptor column, List<StudentExamData> examData)
        {
            var averagePoints = CalculateAveragePoints(examData);
            
            column.Item().PaddingTop(15).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(1);
                });

                table.Cell().Element(CellStyle).Text("TOTAL/AVERAGE").Bold();
                table.Cell().Element(CellStyle).Text("");
                table.Cell().Element(CellStyle).Text("");
                table.Cell().Element(CellStyle).Text("");
                table.Cell().Element(CellStyle).Text("");
                table.Cell().Element(CellStyle).Text("");
                table.Cell().Element(CellStyle).AlignCenter().Text(averagePoints.ToString("F1")).Bold();
                table.Cell().Element(CellStyle).AlignCenter().Text(GetOverallRemark(averagePoints)).Bold();
            });
        }

        private static void AddClassStatistics(ColumnDescriptor column, int term, string academicYear)
        {
            column.Item().PaddingTop(15).Text("CLASS STATISTICS").FontSize(12).Bold();
            column.Item().PaddingTop(5).Column(col =>
            {
                col.Item().Text($"Class Size: 45 | Position in Class: {new Random().Next(1, 46)}");
                col.Item().Text($"Term: {term} | Academic Year: {academicYear}");
            });
        }
        
         private static void AddPreschoolAdministrativeSection(ColumnDescriptor column, Grade grade)
        {
            column.Item().PaddingTop(15).Text("HEADTEACHER'S APPROVAL").AlignCenter().FontSize(14).Bold();
            
            column.Item().PaddingHorizontal(100).PaddingVertical(60).Row(row =>
            {
                 row.RelativeItem().Background(Colors.Grey.Lighten3).Padding(10).Height(80).Column(col =>
                {
                    col.Item().Text("Signature:").AlignCenter().Bold();
                    col.Item().PaddingVertical(5).AlignCenter().Height(45).Image("./Media/pre-sig.png");
                    col.Item().LineHorizontal(1);
                });
            });
        }

        private static void AddPrimaryAdministrativeSection(ColumnDescriptor column, Grade grade)
        {
            column.Item().PaddingTop(15).Text("HEADTEACHER'S APPROVAL").AlignCenter().FontSize(14).Bold();

         
            column.Item().PaddingHorizontal(100).PaddingVertical(60).Row(row =>
            {
                row.RelativeItem().Background(Colors.Grey.Lighten3).Padding(10).Height(80).Column(col =>
               {
                   col.Item().Text("Signature:").AlignCenter().Bold();
                   col.Item().PaddingVertical(5).AlignCenter().Height(45).Image("./Media/pri-sig.png");
                   col.Item().LineHorizontal(1);
               });
            });
        }

        private static void AddSecondaryAdministrativeSection(ColumnDescriptor column, Grade grade)
        {
            column.Item().PaddingTop(15).Text("HEADTEACHER'S APPROVAL").AlignCenter().FontSize(14).Bold();
            
            column.Item().PaddingHorizontal(100).PaddingVertical(60).Row(row =>
            {
                
                
                row.RelativeItem().Background(Colors.Grey.Lighten3).Padding(10).Height(80).Column(col =>
                {
                    col.Item().Text("Signature:").AlignCenter().Bold();
                    col.Item().PaddingVertical(5).AlignCenter().Height(45).Image("./Media/sec-sig.png");
                    col.Item().LineHorizontal(1);
                });
            });
        }

        private static void AddPreschoolGradingScale(ColumnDescriptor column)
        {
            column.Item().PaddingTop(30).Text("PRESCHOOL ASSESSMENT SCALE").FontSize(12).Bold();
            column.Item().PaddingTop(10).Column(col =>
            {
                col.Item().Text("★★★★★ - Exceeds Expectations");
                col.Item().Text("★★★★☆ - Meets Expectations");
                col.Item().Text("★★★☆☆ - Approaching Expectations");
                col.Item().Text("★★☆☆☆ - Needs Support");
            });
        }

        private static void AddPrimaryLowerGradingScale(ColumnDescriptor column)
        {
            column.Item().PaddingTop(25).Text("PRIMARY SCHOOL GRADING SCALE").FontSize(12).AlignCenter().Bold();

            column.Item().PaddingTop(10).PaddingHorizontal(80).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                   {
                       columns.ConstantColumn(40);
                       columns.RelativeColumn(2);
                       columns.RelativeColumn(3);
                   });

                table.Cell().ColumnSpan(3)
               .Background(Colors.Grey.Lighten2).Element(CellStyle)
               .Text("Grading Scale").AlignCenter();

                table.Cell().Element(CellStyle).Text("Grade").SemiBold();
                table.Cell().Element(CellStyle).Text("Range").SemiBold();
                table.Cell().Element(CellStyle).Text("Remark").SemiBold();

                table.Cell().Element(CellStyle).Text("A");
                table.Cell().Element(CellStyle).Text("(80-100)");
                table.Cell().Element(CellStyle).Text("Excellent");

                table.Cell().Element(CellStyle).Text("B");
                table.Cell().Element(CellStyle).Text("(70-79)");
                table.Cell().Element(CellStyle).Text("Very Good");

                table.Cell().Element(CellStyle).Text("C");
                table.Cell().Element(CellStyle).Text("(60-69)");
                table.Cell().Element(CellStyle).Text("Good");

                table.Cell().Element(CellStyle).Text("D");
                table.Cell().Element(CellStyle).Text("(50-59)");
                table.Cell().Element(CellStyle).Text("Satisfactory");
                
                table.Cell().Element(CellStyle).Text("F");
                table.Cell().Element(CellStyle).Text("(0-49)");
                table.Cell().Element(CellStyle).Text("Needs Improvement");
            });

           
        }
        
        private static void AddPrimaryUpperGradingScale(ColumnDescriptor column)
        {
            column.Item().PaddingTop(25).Text("UPPER-PRIMARY SCHOOL GRADING SCALE").FontSize(12).AlignCenter().Bold();

            column.Item().PaddingTop(10).PaddingHorizontal(80).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                   {
                       columns.ConstantColumn(40);
                       columns.RelativeColumn(2);
                       columns.RelativeColumn(3);
                   });

                table.Cell().ColumnSpan(3)
               .Background(Colors.Grey.Lighten2).Element(CellStyle)
               .Text("Grading Scale").AlignCenter();

                table.Cell().Element(CellStyle).Text("Grade").SemiBold();
                table.Cell().Element(CellStyle).Text("Range").SemiBold();
                table.Cell().Element(CellStyle).Text("Remark").SemiBold();

                table.Cell().Element(CellStyle).Text("A");
                table.Cell().Element(CellStyle).Text("(135-150)");
                table.Cell().Element(CellStyle).Text("Excellent");

                table.Cell().Element(CellStyle).Text("B");
                table.Cell().Element(CellStyle).Text("(125-134)");
                table.Cell().Element(CellStyle).Text("Very Good");

                table.Cell().Element(CellStyle).Text("C");
                table.Cell().Element(CellStyle).Text("(110-124)");
                table.Cell().Element(CellStyle).Text("Good");

                table.Cell().Element(CellStyle).Text("D");
                table.Cell().Element(CellStyle).Text("(70-109)");
                table.Cell().Element(CellStyle).Text("Satisfactory");
                
                table.Cell().Element(CellStyle).Text("F");
                table.Cell().Element(CellStyle).Text("(1-69)");
                table.Cell().Element(CellStyle).Text("Needs Improvement");
            });

           
        }

        private static void AddSecondaryJuniorGradingScale(ColumnDescriptor column)
        {
            column.Item().PaddingTop(25).Text("JUNIOR-SECONDARY SCHOOL GRADING SCALE").FontSize(12).AlignCenter().Bold();

            column.Item().PaddingTop(10).PaddingHorizontal(80).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                   {
                       columns.ConstantColumn(40);
                       columns.RelativeColumn(2);
                       columns.RelativeColumn(3);
                   });

                table.Cell().ColumnSpan(3)
               .Background(Colors.Grey.Lighten2).Element(CellStyle)
               .Text("Grading Scale").AlignCenter();

                table.Cell().Element(CellStyle).Text("Grade").SemiBold();
                table.Cell().Element(CellStyle).Text("Range").SemiBold();
                table.Cell().Element(CellStyle).Text("Remark").SemiBold();

                table.Cell().Element(CellStyle).Text("1");
                table.Cell().Element(CellStyle).Text("(80-100)");
                table.Cell().Element(CellStyle).Text("Distinction");

                table.Cell().Element(CellStyle).Text("2");
                table.Cell().Element(CellStyle).Text("(75-79)");
                table.Cell().Element(CellStyle).Text("Distinction");

                table.Cell().Element(CellStyle).Text("3");
                table.Cell().Element(CellStyle).Text("(70-74)");
                table.Cell().Element(CellStyle).Text("Merit");

                table.Cell().Element(CellStyle).Text("4");
                table.Cell().Element(CellStyle).Text("(65-69)");
                table.Cell().Element(CellStyle).Text("Merit");

                table.Cell().Element(CellStyle).Text("5");
                table.Cell().Element(CellStyle).Text("(60-64)");
                table.Cell().Element(CellStyle).Text("Credit");

                table.Cell().Element(CellStyle).Text("6");
                table.Cell().Element(CellStyle).Text("(55-59)");
                table.Cell().Element(CellStyle).Text("Credit");

                table.Cell().Element(CellStyle).Text("7");
                table.Cell().Element(CellStyle).Text("(50-54)");
                table.Cell().Element(CellStyle).Text("Satisfactory");

                table.Cell().Element(CellStyle).Text("8");
                table.Cell().Element(CellStyle).Text("(45-49)");
                table.Cell().Element(CellStyle).Text("Satisfactory");

                table.Cell().Element(CellStyle).Text("9");
                table.Cell().Element(CellStyle).Text("(0-44)");
                table.Cell().Element(CellStyle).Text("Fail");
            });
        }


        private static void AddSecondarySeniorGradingScale(ColumnDescriptor column)
        {
            column.Item().PaddingTop(25).Text("SENIOR-SECONDARY SCHOOL GRADING SCALE").FontSize(12).AlignCenter().Bold();

            column.Item().PaddingTop(10).PaddingHorizontal(80).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                   {
                       columns.ConstantColumn(40);
                       columns.RelativeColumn(2);
                       columns.RelativeColumn(3);
                   });

                table.Cell().ColumnSpan(3)
               .Background(Colors.Grey.Lighten2).Element(CellStyle)
               .Text("Grading Scale").AlignCenter();

                table.Cell().Element(CellStyle).Text("Grade").SemiBold();
                table.Cell().Element(CellStyle).Text("Range").SemiBold();
                table.Cell().Element(CellStyle).Text("Remark").SemiBold();

               
                table.Cell().Element(CellStyle).Text("1");
                table.Cell().Element(CellStyle).Text("(80-100)");
                table.Cell().Element(CellStyle).Text("Distinction");

                table.Cell().Element(CellStyle).Text("2");
                table.Cell().Element(CellStyle).Text("(75-79)");
                table.Cell().Element(CellStyle).Text("Distinction");

                table.Cell().Element(CellStyle).Text("3");
                table.Cell().Element(CellStyle).Text("(70-74)");
                table.Cell().Element(CellStyle).Text("Merit");

                table.Cell().Element(CellStyle).Text("4");
                table.Cell().Element(CellStyle).Text("(65-69)");
                table.Cell().Element(CellStyle).Text("Merit");

                table.Cell().Element(CellStyle).Text("5");
                table.Cell().Element(CellStyle).Text("(60-64)");
                table.Cell().Element(CellStyle).Text("Credit");

                table.Cell().Element(CellStyle).Text("6");
                table.Cell().Element(CellStyle).Text("(55-59)");
                table.Cell().Element(CellStyle).Text("Credit");

                table.Cell().Element(CellStyle).Text("7");
                table.Cell().Element(CellStyle).Text("(50-54)");
                table.Cell().Element(CellStyle).Text("Satisfactory");

                table.Cell().Element(CellStyle).Text("8");
                table.Cell().Element(CellStyle).Text("(45-49)");
                table.Cell().Element(CellStyle).Text("Satisfactory");

                table.Cell().Element(CellStyle).Text("9");
                table.Cell().Element(CellStyle).Text("(0-44)");
                table.Cell().Element(CellStyle).Text("Fail");
            });
        }

        private static void AddContactInfo(ColumnDescriptor column, SchoolSettings schoolSettings)
        {
            column.Item().Background(Colors.Grey.Lighten4).PaddingTop(10).Column(contact =>
                    {
                        contact.Item().Text(schoolSettings.Name).FontSize(14).Bold().AlignCenter();
                        contact.Item().Text("Plot 11289, Lusaka, Zambia").FontSize(8).AlignCenter();
                        contact.Item().Text("Tel: +260-955-876333  | +260-953-074465").FontSize(12).AlignCenter().FontSize(8);
                        contact.Item().Text($"Email: {schoolSettings.Email}").AlignCenter().FontSize(8);
                        contact.Item().Text($"Website: {schoolSettings.Website}").FontSize(8).AlignCenter();
                    });

        }



        private static void AddCoverPage(IDocumentContainer container, Student student, AcademicYear academicYear, int term, string section, SchoolSettings schoolSettings)
        {

            container.Page(page =>
            {
                page.Background()
               .AlignCenter()
               .AlignMiddle()
               .Width(500)
               .Image(schoolSettings.WatermarkLogoPath);

                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));
                page.Content().Border(5).BorderColor(Colors.Black).Padding(20).Column(column =>
                {

                    // School Name
                    column.Item().PaddingTop(30).AlignCenter()
                        .Text(schoolSettings.Name.ToUpper()).FontSize(32).Bold().FontColor(Colors.Black);
                    // Logo
                    column.Item().AlignCenter().Height(120).Image(schoolSettings.LogoPath);

                    // School Motto
                    column.Item().PaddingTop(15).AlignCenter()
                        .Text("\"Towards A Brighter Future\"").FontSize(20).Italic().FontColor(Colors.Black);

                    // Report Title
                    column.Item().PaddingTop(80).AlignCenter()
                        .Text($"{section} REPORT CARD").FontSize(24).Bold().FontColor(Colors.Black);

                    // Exam Title
                    column.Item().PaddingTop(20).AlignCenter()
                        .Text($"End of Term {term} Examination {academicYear?.Name ?? "Unknown Year"}").FontSize(18).Bold().FontColor(Colors.Black);

                    column.Item().PaddingTop(100).Border(0).BorderColor(Colors.Transparent).PaddingHorizontal(60).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                       {
                           columns.ConstantColumn(80);
                           columns.RelativeColumn(125);

                       });


                        table.Cell().Element(CellStyle).Text($"Name:").FontSize(14);
                        table.Cell().Element(CellStyle).Text($"{student?.FullName ?? "Unknown Student"}").FontSize(14);

                        table.Cell().Element(CellStyle).Text($"Class:").FontSize(14);
                        table.Cell().Element(CellStyle).Text($"{student?.Grade?.FullName ?? "Unknown Class"}").FontSize(14);

                        table.Cell().Element(CellStyle).Text($"Teacher:").FontSize(14);
                        table.Cell().Element(CellStyle).Text($"{student?.Grade?.HomeroomTeacher?.FullName ?? "Not Assigned"}").FontSize(14);


                    });

                    // School Address and Contact
                    column.Item().PaddingTop(50).Column(contact =>
                    {
                        //contact.Item().Text("Chudleigh House School").FontSize(14).Bold().AlignCenter();
                        contact.Item().Text("Plot 11289, Lusaka, Zambia").FontSize(8).AlignCenter();
                        contact.Item().Text("Tel: +260-955-876333  | +260-953-074465").FontSize(12).AlignCenter().FontSize(8);
                        contact.Item().Text($"Email: {schoolSettings.Email}").AlignCenter().FontSize(8);
                        contact.Item().Text($"Website: {schoolSettings.Website}").FontSize(8).AlignCenter();
                    });


                });

            });
        }





        private static void AddCoverPageSec(IDocumentContainer container, Student student, AcademicYear academicYear, int term, string section, SchoolSettings schoolSettings)
        {

            container.Page(page =>
            {
                page.Background()
               .AlignCenter()
               .AlignMiddle()
               .Width(500)
               .Image(schoolSettings.WatermarkLogoPath);

                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));
                page.Content().Border(5).BorderColor(Colors.Black).Padding(20).Column(column =>
                {

                    // School Name
                    column.Item().PaddingTop(30).AlignCenter()
                        .Text(schoolSettings.Name.ToUpper()).FontSize(32).Bold().FontColor(Colors.Black);
                    // Logo
                    column.Item().AlignCenter().Height(120).Image("./Media/sec-logo.png");

                    // School Motto
                    column.Item().PaddingTop(15).AlignCenter()
                        .Text("\"Invest In Our Future\"").FontSize(20).Italic().FontColor(Colors.Black);

                    // Report Title
                    column.Item().PaddingTop(80).AlignCenter()
                        .Text($"{section} REPORT CARD").FontSize(24).Bold().FontColor(Colors.Black);

                    // Exam Title
                    column.Item().PaddingTop(20).AlignCenter()
                        .Text($"End of Term {term} Examination {academicYear?.Name ?? "Unknown Year"}").FontSize(18).Bold().FontColor(Colors.Black);

                    column.Item().PaddingTop(100).Border(0).BorderColor(Colors.Transparent).PaddingHorizontal(60).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                       {
                           columns.ConstantColumn(80);
                           columns.RelativeColumn(125);

                       });


                        table.Cell().Element(CellStyle).Text($"Name:").FontSize(14);
                        table.Cell().Element(CellStyle).Text($"{student?.FullName ?? "Unknown Student"}").FontSize(14);

                        table.Cell().Element(CellStyle).Text($"Class:").FontSize(14);
                        table.Cell().Element(CellStyle).Text($"{student?.Grade?.FullName ?? "Unknown Class"}").FontSize(14);

                        table.Cell().Element(CellStyle).Text($"Teacher:").FontSize(14);
                        table.Cell().Element(CellStyle).Text($"{student?.Grade?.HomeroomTeacher?.FullName ?? "Not Assigned"}").FontSize(14);


                    });

                    // School Address and Contact
                    column.Item().PaddingTop(50).Column(contact =>
                    {
                        //contact.Item().Text("Chudleigh House School").FontSize(14).Bold().AlignCenter();
                        contact.Item().Text("Plot 11289, Lusaka, Zambia").FontSize(8).AlignCenter();
                        contact.Item().Text("Tel: +260-955-876333  | +260-953-074465").FontSize(12).AlignCenter().FontSize(8);
                        contact.Item().Text($"Email: {schoolSettings.Email}").AlignCenter().FontSize(8);
                        contact.Item().Text($"Website: {schoolSettings.Website}").FontSize(8).AlignCenter();
                    });


                });

            });
        }




        private static void AddCoverPageELC(IDocumentContainer container, Student student, AcademicYear academicYear, int term, string section, SchoolSettings schoolSettings)
        {

            container.Page(page =>
            {
                page.Background()
               .AlignCenter()
               .AlignMiddle()
               .Width(500)
               .Image("./Media/ltwm2.png");

                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12));
                page.Content().Border(5).BorderColor(Colors.Blue.Darken2).Padding(20).Column(column =>
                {

                    // School Name
                    column.Item().PaddingTop(30).AlignCenter()
                        .Text(schoolSettings.Name.ToUpper()).FontSize(32).Bold().FontColor(Colors.Blue.Darken2);
                    // Logo
                    column.Item().AlignCenter().Height(120).Image(schoolSettings.LogoPath);

                    // School Motto
                    column.Item().PaddingTop(15).AlignCenter()
                        .Text("\"Towards A Brighter Future\"").FontSize(20).Italic().FontColor(Colors.Blue.Medium);

                    // Report Title
                    column.Item().PaddingTop(80).AlignCenter()
                        .Text($"{section} REPORT CARD").FontSize(24).Bold().FontColor(Colors.Green.Darken1);

                    // Exam Title
                    column.Item().PaddingTop(20).AlignCenter()
                        .Text($"End of Term {term} Examination {academicYear?.Name ?? "Unknown Year"}").FontSize(18).Bold().FontColor(Colors.Blue.Darken1);

                    column.Item().PaddingTop(100).Border(0).BorderColor(Colors.Transparent).PaddingHorizontal(60).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                       {
                           columns.ConstantColumn(80);
                           columns.RelativeColumn(125);

                       });


                        table.Cell().Element(CellStyle).Text($"Name:").FontSize(14);
                        table.Cell().Element(CellStyle).Text($"{student?.FullName ?? "Unknown Student"}").FontSize(14);

                        table.Cell().Element(CellStyle).Text($"Class:").FontSize(14);
                        table.Cell().Element(CellStyle).Text($"{student?.Grade?.FullName ?? "Unknown Class"}").FontSize(14);

                        table.Cell().Element(CellStyle).Text($"Teacher:").FontSize(14);
                        table.Cell().Element(CellStyle).Text($"{student?.Grade?.HomeroomTeacher?.FullName ?? "Not Assigned"}").FontSize(14);


                    });

                    // School Address and Contact
                    column.Item().PaddingTop(50).Column(contact =>
                    {
                        //contact.Item().Text("Chudleigh House School").FontSize(14).Bold().AlignCenter();
                        contact.Item().Text("Plot 11289, Lusaka, Zambia").FontSize(8).AlignCenter();
                        contact.Item().Text("Tel: +260-955-876333  | +260-953-074465").FontSize(12).AlignCenter().FontSize(8);
                        contact.Item().Text($"Email: {schoolSettings.Email}").AlignCenter().FontSize(8);
                        contact.Item().Text($"Website: {schoolSettings.Website}").FontSize(8).AlignCenter();
                    });


                });

            });
        }




        private static void AddPageFooter(PageDescriptor page, int pageNumber)
        {
            page.Footer().AlignCenter().Text($"Page {pageNumber} of 4 | Generated on {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8);
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container.Border(1).BorderColor(Colors.Black).Padding(5).AlignMiddle();
        }

        // Helper method to format score display with ABS notation
        private static string FormatScoreDisplay(decimal score, bool isAbsent)
        {
            return isAbsent ? "ABS" : score.ToString("F0");
        }

        // Helper method to calculate subject average for Grade 7 (includes Test3)
        private static decimal CalculateGrade7SubjectAverage(StudentExamData exam)
        {
            decimal total = 0;
            int count = 0;
            
            // Only include non-absent and non-zero scores in the average
            if (!exam.IsTest1Absent && exam.Test1Score > 0) { total += exam.Test1Score; count++; }
            if (!exam.IsMidTermAbsent && exam.MidTermScore > 0) { total += exam.MidTermScore; count++; }
            if (!exam.IsTest3Absent && exam.Test3Score > 0) { total += exam.Test3Score; count++; }
            if (!exam.IsEndTermAbsent && exam.EndTermScore > 0) { total += exam.EndTermScore; count++; }
            
            return count > 0 ? total / count : 0;
        }

        // Calculation Methods
        private static decimal CalculateOverallAverage(List<StudentExamData> examData)
        {
            if (examData == null || !examData.Any()) return 0;
            
            var averages = examData.Where(e => e != null).Select(e => 
            {
                decimal total = 0;
                int count = 0;
                
                // Only include non-absent and non-zero scores in the average
                if (!e.IsTest1Absent && e.Test1Score > 0) { total += e.Test1Score; count++; }
                if (!e.IsMidTermAbsent && e.MidTermScore > 0) { total += e.MidTermScore; count++; }
                if (!e.IsEndTermAbsent && e.EndTermScore > 0) { total += e.EndTermScore; count++; }
                
                return count > 0 ? total / count : 0;
            });
            
            return averages.Any() ? averages.Average() : 0;
        }

        private static decimal CalculateAveragePoints(List<StudentExamData> examData)
        {
            if (!examData.Any()) return 0;
            
            var totalPoints = examData.Select(e =>
            {
                decimal total = 0;
                int count = 0;
                
                // Only include non-absent and non-zero scores in the total
                if (!e.IsTest1Absent && e.Test1Score > 0) { total += e.Test1Score; count++; }
                if (!e.IsMidTermAbsent && e.MidTermScore > 0) { total += e.MidTermScore; count++; }
                if (!e.IsEndTermAbsent && e.EndTermScore > 0) { total += e.EndTermScore; count++; }
                
                // Calculate average score for this student, then get grade points
                var averageScore = count > 0 ? total / count : 0;
                var grade = GetSecondaryGrade(averageScore * count); // Scale back for grade calculation
                return GetGradePoints(grade);
            }).Sum();
            
            return totalPoints / examData.Count;
        }

        // Comment Generation Methods
        private static string GetEndOfTermComments(List<StudentExamData> examData, string firstName)
        {
            var commentsWithContent = examData.Where(e => !string.IsNullOrWhiteSpace(e.Comments)).ToList();
            
            if (commentsWithContent.Any())
            {
                var firstComment = commentsWithContent.First();
                return $"{firstName} shows progress in {firstComment.SubjectName.ToLower()}. {firstComment.Comments}";
            }
            
            return $"{firstName} has shown wonderful progress this term. Continue encouraging learning through play and exploration.";
        }

        private static string GetSkillComments(List<StudentExamData> examData, string skillArea)
        {
            // Map skill areas to subjects if available
            var relevantSubject = examData.FirstOrDefault(e => 
                e.SubjectName.ToLower().Contains(skillArea.Split(' ')[0].ToLower()));
            
            if (relevantSubject != null && !string.IsNullOrWhiteSpace(relevantSubject.Comments))
            {
                return relevantSubject.Comments.Length > 50 
                    ? relevantSubject.Comments.Substring(0, 47) + "..."
                    : relevantSubject.Comments;
            }
            
            return "Shows good development";
        }

        private static string GenerateProgressSummary(List<StudentExamData> examData, string firstName)
        {
            var overallAverage = CalculateOverallAverage(examData);
            var commentsCount = examData.Count(e => !string.IsNullOrWhiteSpace(e.Comments));
            
            return $"{firstName} continues to show remarkable growth in all developmental areas. " +
                   $"Based on {examData.Count} assessment areas with {commentsCount} detailed comments from teachers, " +
                   $"the overall performance shows {GetPerformanceLevel(overallAverage)} development. " +
                   $"Encouragement in creative activities would benefit further development.";
        }
        
            private const decimal EXCELLENT_THRESHOLD = 85m;
            private const decimal GOOD_THRESHOLD = 75m;
            private const decimal SATISFACTORY_THRESHOLD = 60m;

            // Upper Primary specific thresholds (150-point system)
            private const decimal UPPER_PRIMARY_EXCELLENT_THRESHOLD = 135m;
            private const decimal UPPER_PRIMARY_GOOD_THRESHOLD = 125m;
            private const decimal UPPER_PRIMARY_SATISFACTORY_THRESHOLD = 110m;

                    private static string GenerateTeacherAssessment(List<StudentExamData> examData, string firstName)
        {
            // Defensive programming: handle null or empty data
            if (examData == null || !examData.Any())
            {
                var safeFirstName = string.IsNullOrWhiteSpace(firstName) ? "the student" : firstName;
                return $"Throughout this term, {safeFirstName} has been working on various developmental areas. " +
                       "Assessment data is currently being compiled. Please continue to support their learning journey.";
            }

            var overallAverage = CalculateOverallAverage(examData);

            // Check if this is Upper Primary by looking at the grading scale used
            bool isUpperPrimary = examData.Any(e => {
                if (e == null) return false;
                var average = CalculateSubjectAverage(e);
                var grade = GetPrimaryUpperGrade(average);
                // If any subject gets an Upper Primary grade (A-F), this is Upper Primary
                return !string.IsNullOrEmpty(grade) && (grade == "A" || grade == "B" || grade == "C" || grade == "D" || grade == "F");
            });

            // Check if this is Secondary by looking for secondary grading
            bool isSecondary = examData.Any(e => {
                if (e == null) return false;
                var average = CalculateSubjectAverage(e);
                var grade = GetSecondarySeniorGrade(average);
                // If any subject gets a secondary grade (1-9), this is Secondary
                return grade >= 1 && grade <= 9;
            });

            // Check if this is ELC by looking for ELC characteristics (no grades, only scores)
            bool isELC = examData.Any() && !isUpperPrimary && !isSecondary && examData.All(e => 
                e != null &&
                CalculateSubjectAverage(e) <= 100 && // ELC typically has scores up to 100
                string.IsNullOrEmpty(GetPrimaryUpperGrade(CalculateSubjectAverage(e))) && // No letter grades
                GetSecondarySeniorGrade(CalculateSubjectAverage(e)) == 9); // No secondary grades

            if (isSecondary)
            {
                return GenerateSecondaryTeacherAssessment(examData, firstName);
            }
            else if (isUpperPrimary)
            {
                return GenerateUpperPrimaryTeacherAssessment(examData, firstName);
            }
            else if (isELC)
            {
                return GenerateELCTeacherAssessment(examData, firstName);
            }
            else
            {
                return GenerateStandardTeacherAssessment(examData, firstName);
            }
        }

        private static string GenerateUpperPrimaryTeacherAssessment(List<StudentExamData> examData, string firstName)
        {
            var overallAverage = CalculateOverallAverage(examData);

            var excellentSubjects = examData.Where(e => CalculateSubjectAverage(e) >= UPPER_PRIMARY_EXCELLENT_THRESHOLD).ToList();
            var goodSubjects = examData.Where(e => CalculateSubjectAverage(e) >= UPPER_PRIMARY_GOOD_THRESHOLD && CalculateSubjectAverage(e) < UPPER_PRIMARY_EXCELLENT_THRESHOLD).ToList();
            var satisfactorySubjects = examData.Where(e => CalculateSubjectAverage(e) >= UPPER_PRIMARY_SATISFACTORY_THRESHOLD && CalculateSubjectAverage(e) < UPPER_PRIMARY_GOOD_THRESHOLD).ToList();
            var improvementSubjects = examData.Where(e => CalculateSubjectAverage(e) < UPPER_PRIMARY_SATISFACTORY_THRESHOLD).ToList();

            var assessment = $"Throughout this term, this learner has demonstrated {GetUpperPrimaryPerformanceLevel(overallAverage)} progress " +
                           $"across all assessment areas. ";

            if (excellentSubjects.Any())
            {
                assessment += $"Excellent performance shown in {string.Join(", ", excellentSubjects.Select(s => s.SubjectName))}. ";
            }

            if (goodSubjects.Any())
            {
                assessment += $"Good progress demonstrated in {string.Join(", ", goodSubjects.Select(s => s.SubjectName))}. ";
            }

            if (satisfactorySubjects.Any())
            {
                assessment += $"Satisfactory development observed in {string.Join(", ", satisfactorySubjects.Select(s => s.SubjectName))}. ";
            }

            if (improvementSubjects.Any())
            {
                assessment += $"Areas requiring focused attention include {string.Join(", ", improvementSubjects.Select(s => s.SubjectName))}. ";
            }

            assessment += GetUpperPrimaryRecommendation(overallAverage, improvementSubjects.Count, examData.Count);

            return assessment;
        }

        private static string GenerateSecondaryTeacherAssessment(List<StudentExamData> examData, string firstName)
        {
            var overallAverage = CalculateOverallAverage(examData);

            var excellentSubjects = examData.Where(e => CalculateSubjectAverage(e) >= EXCELLENT_THRESHOLD).ToList();
            var goodSubjects = examData.Where(e => CalculateSubjectAverage(e) >= GOOD_THRESHOLD && CalculateSubjectAverage(e) < EXCELLENT_THRESHOLD).ToList();
            var satisfactorySubjects = examData.Where(e => CalculateSubjectAverage(e) >= SATISFACTORY_THRESHOLD && CalculateSubjectAverage(e) < GOOD_THRESHOLD).ToList();
            var improvementSubjects = examData.Where(e => CalculateSubjectAverage(e) < SATISFACTORY_THRESHOLD).ToList();

            var assessment = $"Throughout this term, {firstName} has demonstrated {GetPerformanceLevel(overallAverage)} progress " +
                           $"across all assessment areas. ";

            if (excellentSubjects.Any())
            {
                assessment += $"Excellent performance shown in {string.Join(", ", excellentSubjects.Select(s => s.SubjectName))}. ";
            }

            if (goodSubjects.Any())
            {
                assessment += $"Good progress demonstrated in {string.Join(", ", goodSubjects.Select(s => s.SubjectName))}. ";
            }

            if (satisfactorySubjects.Any())
            {
                assessment += $"Satisfactory development observed in {string.Join(", ", satisfactorySubjects.Select(s => s.SubjectName))}. ";
            }

            if (improvementSubjects.Any())
            {
                assessment += $"Areas requiring focused attention include {string.Join(", ", improvementSubjects.Select(s => s.SubjectName))}. ";
            }

            assessment += GetRecommendation(overallAverage, improvementSubjects.Count, examData.Count);

            return assessment;
        }

        private static string GenerateELCTeacherAssessment(List<StudentExamData> examData, string firstName)
        {
            // Defensive programming: handle null or empty data
            if (examData == null || !examData.Any())
            {
                var safeFirstName = string.IsNullOrWhiteSpace(firstName) ? "the student" : firstName;
                return $"Throughout this term, {safeFirstName} has been working on various developmental areas. " +
                       "Assessment data is currently being compiled. Please continue to support their learning journey.";
            }

            var overallAverage = CalculateOverallAverage(examData);

            var excellentSubjects = examData.Where(e => e != null && CalculateSubjectAverage(e) >= EXCELLENT_THRESHOLD).ToList();
            var goodSubjects = examData.Where(e => e != null && CalculateSubjectAverage(e) >= GOOD_THRESHOLD && CalculateSubjectAverage(e) < EXCELLENT_THRESHOLD).ToList();
            var satisfactorySubjects = examData.Where(e => e != null && CalculateSubjectAverage(e) >= SATISFACTORY_THRESHOLD && CalculateSubjectAverage(e) < GOOD_THRESHOLD).ToList();
            var improvementSubjects = examData.Where(e => e != null && CalculateSubjectAverage(e) < SATISFACTORY_THRESHOLD).ToList();

            var studentName = string.IsNullOrWhiteSpace(firstName) ? "the student" : firstName;
            var assessment = $"Throughout this term, {studentName} has demonstrated {GetPerformanceLevel(overallAverage)} progress " +
                           $"across all developmental areas. ";

            if (excellentSubjects.Any())
            {
                var subjectNames = excellentSubjects
                    .Where(s => s != null && !string.IsNullOrWhiteSpace(s.SubjectName))
                    .Select(s => s.SubjectName)
                    .ToList();
                if (subjectNames.Any())
                {
                    assessment += $"Excellent development shown in {string.Join(", ", subjectNames)}. ";
                }
            }

            if (goodSubjects.Any())
            {
                var subjectNames = goodSubjects
                    .Where(s => s != null && !string.IsNullOrWhiteSpace(s.SubjectName))
                    .Select(s => s.SubjectName)
                    .ToList();
                if (subjectNames.Any())
                {
                    assessment += $"Good progress demonstrated in {string.Join(", ", subjectNames)}. ";
                }
            }

            if (satisfactorySubjects.Any())
            {
                var subjectNames = satisfactorySubjects
                    .Where(s => s != null && !string.IsNullOrWhiteSpace(s.SubjectName))
                    .Select(s => s.SubjectName)
                    .ToList();
                if (subjectNames.Any())
                {
                    assessment += $"Satisfactory development observed in {string.Join(", ", subjectNames)}. ";
                }
            }

            if (improvementSubjects.Any())
            {
                var subjectNames = improvementSubjects
                    .Where(s => s != null && !string.IsNullOrWhiteSpace(s.SubjectName))
                    .Select(s => s.SubjectName)
                    .ToList();
                if (subjectNames.Any())
                {
                    assessment += $"Areas requiring focused attention include {string.Join(", ", subjectNames)}. ";
                }
            }

            assessment += GetELCRecommendation(overallAverage, improvementSubjects.Count, examData.Count);

            return assessment;
        }

        private static string GenerateStandardTeacherAssessment(List<StudentExamData> examData, string firstName)
        {
            // Defensive programming: handle null or empty data
            if (examData == null || !examData.Any())
            {
                var safeFirstName = string.IsNullOrWhiteSpace(firstName) ? "the student" : firstName;
                return $"Throughout this term, {safeFirstName} has been working on various assessment areas. " +
                       "Assessment data is currently being compiled. Please continue to support their learning journey.";
            }

            var overallAverage = CalculateOverallAverage(examData);

            var excellentSubjects = examData.Where(e => e != null && CalculateSubjectAverage(e) >= EXCELLENT_THRESHOLD).ToList();
            var goodSubjects = examData.Where(e => e != null && CalculateSubjectAverage(e) >= GOOD_THRESHOLD && CalculateSubjectAverage(e) < EXCELLENT_THRESHOLD).ToList();
            var satisfactorySubjects = examData.Where(e => e != null && CalculateSubjectAverage(e) >= SATISFACTORY_THRESHOLD && CalculateSubjectAverage(e) < GOOD_THRESHOLD).ToList();
            var improvementSubjects = examData.Where(e => e != null && CalculateSubjectAverage(e) < SATISFACTORY_THRESHOLD).ToList();

            var assessment = $"Throughout this term, this learner has demonstrated {GetPerformanceLevel(overallAverage)} progress " +
                           $"across all assessment areas. ";

            if (excellentSubjects.Any())
            {
                var subjectNames = excellentSubjects
                    .Where(s => s != null && !string.IsNullOrWhiteSpace(s.SubjectName))
                    .Select(s => s.SubjectName)
                    .ToList();
                if (subjectNames.Any())
                {
                    assessment += $"Excellent performance shown in {string.Join(", ", subjectNames)}. ";
                }
            }

            if (goodSubjects.Any())
            {
                var subjectNames = goodSubjects
                    .Where(s => s != null && !string.IsNullOrWhiteSpace(s.SubjectName))
                    .Select(s => s.SubjectName)
                    .ToList();
                if (subjectNames.Any())
                {
                    assessment += $"Good progress demonstrated in {string.Join(", ", subjectNames)}. ";
                }
            }

            if (satisfactorySubjects.Any())
            {
                var subjectNames = satisfactorySubjects
                    .Where(s => s != null && !string.IsNullOrWhiteSpace(s.SubjectName))
                    .Select(s => s.SubjectName)
                    .ToList();
                if (subjectNames.Any())
                {
                    assessment += $"Satisfactory development observed in {string.Join(", ", subjectNames)}. ";
                }
            }

            if (improvementSubjects.Any())
            {
                var subjectNames = improvementSubjects
                    .Where(s => s != null && !string.IsNullOrWhiteSpace(s.SubjectName))
                    .Select(s => s.SubjectName)
                    .ToList();
                if (subjectNames.Any())
                {
                    assessment += $"Areas requiring focused attention include {string.Join(", ", subjectNames)}. ";
                }
            }

            assessment += GetRecommendation(overallAverage, improvementSubjects.Count, examData.Count);

            return assessment;
        }

        private static decimal CalculateSubjectAverage(StudentExamData exam)
        {
            if (exam == null) return 0;
            
            decimal total = 0;
            int count = 0;
            
            // Only include non-absent and non-zero scores in the average
            if (!exam.IsTest1Absent && exam.Test1Score > 0) { total += exam.Test1Score; count++; }
            if (!exam.IsMidTermAbsent && exam.MidTermScore > 0) { total += exam.MidTermScore; count++; }
            if (!exam.IsEndTermAbsent && exam.EndTermScore > 0) { total += exam.EndTermScore; count++; }
            
            return count > 0 ? total / count : 0;
        }

        private static string GetRecommendation(decimal overallAverage, int improvementSubjectsCount, int totalSubjects)
        {
            if (overallAverage >= EXCELLENT_THRESHOLD)
            {
                return "I recommend continued challenge and enrichment opportunities to maintain this excellent trajectory.";
            }
            else if (overallAverage >= GOOD_THRESHOLD)
            {
                return "I recommend sustained effort and targeted practice to build upon this solid foundation.";
            }
            else if (improvementSubjectsCount >= totalSubjects / 2)
            {
                return "I recommend additional support and structured practice across multiple areas for comprehensive improvement.";
            }
            else
            {
                return "I recommend continued encouragement and focused support in identified areas for optimal development.";
            }
        }

        private static string GetUpperPrimaryRecommendation(decimal overallAverage, int improvementSubjectsCount, int totalSubjects)
        {
            if (overallAverage >= UPPER_PRIMARY_EXCELLENT_THRESHOLD)
            {
                return "I recommend continued challenge and enrichment opportunities to maintain this excellent trajectory.";
            }
            else if (overallAverage >= UPPER_PRIMARY_GOOD_THRESHOLD)
            {
                return "I recommend sustained effort and targeted practice to build upon this solid foundation.";
            }
            else if (improvementSubjectsCount >= totalSubjects / 2)
            {
                return "I recommend additional support and structured practice across multiple areas for comprehensive improvement.";
            }
            else
            {
                return "I recommend continued encouragement and focused support in identified areas for optimal development.";
            }
        }

        private static string GetELCRecommendation(decimal overallAverage, int improvementSubjectsCount, int totalSubjects)
        {
            if (overallAverage >= EXCELLENT_THRESHOLD)
            {
                return "I recommend continued encouragement and enrichment activities to maintain this excellent developmental trajectory.";
            }
            else if (overallAverage >= GOOD_THRESHOLD)
            {
                return "I recommend continued support and positive reinforcement to build upon this solid foundation.";
            }
            else if (improvementSubjectsCount >= totalSubjects / 2)
            {
                return "I recommend additional support and structured activities across multiple developmental areas.";
            }
            else
            {
                return "I recommend continued encouragement and focused support in identified areas for optimal development.";
            }
        }


   
        private static string GenerateDetailedTeacherComment(decimal average, string firstName, List<StudentExamData> examData)
        {
            // Check if this is Upper Primary by looking at the grading scale used
            bool isUpperPrimary = examData.Any(e => {
                var average = CalculateSubjectAverage(e);
                var grade = GetPrimaryUpperGrade(average);
                // If any subject gets an Upper Primary grade (A-F), this is Upper Primary
                return !string.IsNullOrEmpty(grade) && (grade == "A" || grade == "B" || grade == "C" || grade == "D" || grade == "F");
            });
            
            // Check if this is Grade 7 (has Test 3)
            bool isGrade7 = examData.Any(e => e.Test3Score > 0);
            
            var comment = $"Throughout this term, this learner has {(isUpperPrimary ? GetUpperPrimaryPerformanceDescription(average) : GetPerformanceDescription(average))} academic ability. ";
            
            // Add subject-by-subject analysis
            comment += "\n\nSubject Performance Analysis:\n";
            
            foreach (var exam in examData.OrderByDescending(e => CalculateSubjectAverage(e)))
            {
                var subjectAverage = CalculateSubjectAverage(exam);
                comment += $"\n{exam.SubjectName}: ";
                
                // Performance assessment based on grading system
                if (isUpperPrimary)
                {
                    // Upper Primary 150-point system
                    if (subjectAverage >= 135) // Grade A (90-100%)
                        comment += "Excellent performance with outstanding achievement (Grade A). ";
                    else if (subjectAverage >= 125) // Grade B (83-89%)
                        comment += "Very good performance showing strong understanding (Grade B). ";
                    else if (subjectAverage >= 110) // Grade C (73-82%)
                        comment += "Good performance with satisfactory progress (Grade C). ";
                    else if (subjectAverage >= 70) // Grade D (47-73%)
                        comment += "Satisfactory performance with room for improvement (Grade D). ";
                    else // Grade F (1-46%)
                        comment += "Needs significant improvement and additional support (Grade F). ";
                }
                else
                {
                    // Standard 100-point system
                    if (subjectAverage >= 80)
                        comment += "Excellent performance with consistent high achievement. ";
                    else if (subjectAverage >= 70)
                        comment += "Very good performance showing solid understanding. ";
                    else if (subjectAverage >= 60)
                        comment += "Good performance with satisfactory progress. ";
                    else if (subjectAverage >= 50)
                        comment += "Satisfactory performance with room for improvement. ";
                    else
                        comment += "Needs significant improvement and additional support. ";
                }
                
                // Add test scores breakdown
                comment += "Test scores: ";
                if (isGrade7)
                {
                    // Grade 7 has 4 tests
                    comment += $"Test 1: {FormatScoreDisplay(exam.Test1Score, exam.IsTest1Absent)}, ";
                    comment += $"Test 2: {FormatScoreDisplay(exam.MidTermScore, exam.IsMidTermAbsent)}, ";
                    comment += $"Test 3: {FormatScoreDisplay(exam.Test3Score, exam.IsTest3Absent)}, ";
                    comment += $"End Term: {FormatScoreDisplay(exam.EndTermScore, exam.IsEndTermAbsent)}. ";
                }
                else
                {
                    // Standard 3 tests
                    comment += $"Test 1: {FormatScoreDisplay(exam.Test1Score, exam.IsTest1Absent)}, ";
                    comment += $"Test 2: {FormatScoreDisplay(exam.MidTermScore, exam.IsMidTermAbsent)}, ";
                    comment += $"End Term: {FormatScoreDisplay(exam.EndTermScore, exam.IsEndTermAbsent)}. ";
                }
                
                comment += $"Term Average: {subjectAverage:F1}. ";
                
                // Add teacher comments if available
                if (!string.IsNullOrWhiteSpace(exam.Comments))
                {
                    var truncatedComment = exam.Comments.Length > 80 ? 
                        exam.Comments.Substring(0, 80) + "..." : 
                        exam.Comments;
                    comment += $"Teacher feedback: {truncatedComment} ";
                }
            }
            
            // Add overall advice
            comment += $"\n\nOverall Recommendation: ";
            comment += isUpperPrimary ? GetUpperPrimaryAdviceBasedOnPerformance(average) : GetAdviceBasedOnPerformance(average);
            
            return comment;
        }

        private static string GenerateSubjectRecommendations(List<StudentExamData> examData, string firstName)
        {
            var recommendations = new List<string>();
            
            // Check if this is Upper Primary by looking at the grading scale used
            bool isUpperPrimary = examData.Any(e => {
                var average = CalculateSubjectAverage(e);
                var grade = GetPrimaryUpperGrade(average);
                // If any subject gets an Upper Primary grade (A-F), this is Upper Primary
                return !string.IsNullOrEmpty(grade) && (grade == "A" || grade == "B" || grade == "C" || grade == "D" || grade == "F");
            });
            
            foreach (var exam in examData.Take(3))
            {
                var average = CalculateSubjectAverage(exam);
                var recommendation = "";
                
                if (isUpperPrimary)
                {
                    // Upper Primary uses 150-point system
                    if (average >= 135) // Grade A equivalent (90%)
                        recommendation = $"{exam.SubjectName}: Excellent performance - consider advanced challenges.";
                    else if (average >= 125) // Grade B equivalent (83%)
                        recommendation = $"{exam.SubjectName}: Very good progress - focus on consistency.";
                    else if (average >= 110) // Grade C equivalent (73%)
                        recommendation = $"{exam.SubjectName}: Good effort - strengthen weak areas.";
                    else
                        recommendation = $"{exam.SubjectName}: Needs improvement - seek additional support.";
                }
                else
                {
                    // Standard 100-point system
                    if (average >= 80)
                        recommendation = $"{exam.SubjectName}: Excellent performance - consider advanced challenges.";
                    else if (average >= 70)
                        recommendation = $"{exam.SubjectName}: Very good progress - focus on consistency.";
                    else if (average >= 60)
                        recommendation = $"{exam.SubjectName}: Good effort - strengthen weak areas.";
                    else
                        recommendation = $"{exam.SubjectName}: Needs improvement - seek additional support.";
                }
                
                // Add specific comments if available
                if (!string.IsNullOrWhiteSpace(exam.Comments))
                {
                    recommendation += $" Teacher notes: {exam.Comments.Substring(0, Math.Min(50, exam.Comments.Length))}";
                    if (exam.Comments.Length > 50) recommendation += "...";
                }
                
                recommendations.Add(recommendation);
            }
            
            return string.Join(" ", recommendations);
        }

        private static string GenerateSecondaryTeacherComment(decimal averagePoints, string firstName, List<StudentExamData> examData)
        {
            var subjectsWithComments = examData.Where(e => !string.IsNullOrWhiteSpace(e.Comments)).ToList();
            
            // Calculate top subjects using proper average calculation that handles absent tests
            var topSubjects = examData.OrderByDescending(e => CalculateSubjectAverage(e)).Take(2).ToList();
            
            var comment = $"This learner has {GetSecondaryPerformanceDescription(averagePoints)} this term ";
            
            if (topSubjects.Any())
            {
                comment += $"with outstanding results in {string.Join(" and ", topSubjects.Select(s => s.SubjectName))}. ";
            }
            
            if (subjectsWithComments.Any())
            {
                var firstComment = subjectsWithComments.First();
                if (!string.IsNullOrEmpty(firstComment.Comments))
                {
                    comment += $"Teacher feedback in {firstComment.SubjectName}: {firstComment.Comments.Substring(0, Math.Min(80, firstComment.Comments.Length))}";
                    if (firstComment.Comments.Length > 80) comment += "...";
                    comment += " ";
                }
            }
            
            comment += GetSecondaryAdvice(averagePoints);
            
            return comment;
        }

        private static string GenerateSecondaryPerformanceAnalysis(decimal averagePoints, string firstName)
        {
            if (averagePoints >= 10)
                return $"This learner demonstrates outstanding academic excellence with consistent high performance across all subjects. " +
                       $"This level of achievement reflects strong analytical skills, excellent study habits, and deep understanding of complex concepts. " +
                       $"Based on End-of-Term teacher feedback, continue to pursue academic challenges and consider leadership roles.";
            if (averagePoints >= 8)
                return $"This learner shows very commendable academic performance with strong understanding in most subject areas. " +
                       $"Teacher comments indicate clear potential for achieving excellence with continued focus and effort. " +
                       $"Consider developing stronger study strategies in subjects with lower performance.";
            if (averagePoints >= 6)
                return $"This learner demonstrates good academic progress overall but shows inconsistency across different subjects. " +
                       $"End-of-Term feedback suggests focusing on developing more effective study techniques and time management skills.";
            if (averagePoints >= 4)
                return $"This learner shows satisfactory performance but needs significant improvement to meet full academic potential. " +
                       $"Teacher comments recommend developing a structured study schedule and seeking additional support.";
                          return $"This learner requires immediate and intensive academic support across multiple subjects. " +
                   $"Based on teacher feedback, consider academic counseling and comprehensive support planning.";
        }

        private static string GenerateHeadTeacherComment(decimal averagePoints)
        {
            if (averagePoints >= 10)
                return "Outstanding academic achievement demonstrated across all subjects. This student shows exceptional ability, " +
                       "strong work ethic, and excellent potential for future academic success. Continue pursuing excellence.";
            if (averagePoints >= 8)
                return "Very commendable academic performance with strong potential evident. The student demonstrates good " +
                       "understanding and consistent effort. Continue working toward excellence through focused study.";
            if (averagePoints >= 6)
                return "Good overall academic effort shown with room for improvement in specific areas. The student should " +
                       "focus on developing stronger study strategies and seek support where needed for optimal performance.";
            if (averagePoints >= 4)
                return "Satisfactory performance overall, but more dedication and consistent effort are required. The student " +
                       "needs to develop better study habits and utilize available academic resources more effectively.";
            return "Immediate improvement required across all academic areas. Comprehensive support, close monitoring, " +
                   "and intervention strategies are essential to help the student meet educational standards.";
        }

        // Utility Methods
        private static string GetPreschoolRating()
        {
            var ratings = new[] { "★★★★★", "★★★★☆", "★★★☆☆", "★★☆☆☆" };
            return ratings[new Random().Next(ratings.Length)];
        }

        private static string GetGrade(decimal score)
        {
            if (score >= 80) return "A";
            if (score >= 70) return "B";
            if (score >= 60) return "C";
            if (score >= 50) return "D";
            return "F";
        }

    
        private static string GetLowerPrimaryGrade(decimal score)
        {
            if (score >= 80) return "A";
            if (score >= 70) return "B";
            if (score >= 60) return "C";
            if (score >= 50) return "D";
            return "F";
        }

        private static string GetPrimaryUpperGrade(decimal score)
        {
            if (score >= 135) return "A";
            if (score >= 125) return "B";
            if (score >= 110) return "C";
            if (score >= 70) return "D";
            return "F";
        }

        private static string GetSecondaryGrade(decimal totalScore)
        {
            if (totalScore >= 240) return "A";
            if (totalScore >= 210) return "B";
            if (totalScore >= 180) return "C";
            if (totalScore >= 150) return "D";
            return "F";
        }

         private static int GetSecondarySeniorGrade(decimal score)
        {
            if (score >= 80) return 1;
            if (score >= 75) return 2;
            if (score >= 70) return 3;
            if (score >= 65) return 4;
            if (score >= 60) return 5;
            if (score >= 55) return 6;
            if (score >= 50) return 7;
            if (score >= 45) return 8;
           
            return 9;
        }

        private static decimal GetGradePoints(string grade)
        {
            return grade switch
            {
                "A" => 12,
                "B" => 9,
                "C" => 6,
                "D" => 3,
                _ => 0
            };
        }

        private static string GetRemark(string grade)
        {
            return grade switch
            {
                "A" => "Excellent",
                "B" => "Very Good",
                "C" => "Good",
                "D" => "Satisfactory",
                _ => "Needs Improvement"
            };
        }

        private static string GetOverallRemark(decimal averagePoints)
        {
            if (averagePoints >= 10) return "EXCELLENT";
            if (averagePoints >= 8) return "VERY GOOD";
            if (averagePoints >= 6) return "GOOD";
            if (averagePoints >= 4) return "SATISFACTORY";
            return "NEEDS IMPROVEMENT";
        }

        private static string GetPerformanceLevel(decimal average)
        {
            if (average >= 80) return "excellent";
            if (average >= 70) return "very good";
            if (average >= 60) return "good";
            if (average >= 50) return "satisfactory";
            return "developing";
        }

        private static string GetUpperPrimaryPerformanceLevel(decimal average)
        {
            if (average >= 135) return "excellent";
            if (average >= 125) return "very good";
            if (average >= 110) return "good";
            if (average >= 70) return "satisfactory";
            return "developing";
        }

        private static string GetPerformanceDescription(decimal average)
        {
            if (average >= 80) return "consistently demonstrated exceptional";
            if (average >= 70) return "shown very good";
            if (average >= 60) return "displayed good";
            if (average >= 50) return "shown satisfactory";
            return "needs to develop stronger";
        }

        private static string GetUpperPrimaryPerformanceDescription(decimal average)
        {
            if (average >= 135) return "consistently demonstrated exceptional";
            if (average >= 125) return "shown very good";
            if (average >= 110) return "displayed good";
            if (average >= 70) return "shown satisfactory";
            return "needs to develop stronger";
        }

        private static string GetAdviceBasedOnPerformance(decimal average)
        {
            if (average >= 80) return "Continue with the same dedication and excellence.";
            if (average >= 70) return "With continued effort, even better results can be achieved.";
            if (average >= 60) return "Focus on areas that need improvement for better results.";
            if (average >= 50) return "More effort and dedication are needed to improve grades.";
            return "Significant improvement required. Seek additional support and study time.";
        }

        private static string GetUpperPrimaryAdviceBasedOnPerformance(decimal average)
        {
            if (average >= 135) return "Continue with the same dedication and excellence.";
            if (average >= 125) return "With continued effort, even better results can be achieved.";
            if (average >= 110) return "Focus on areas that need improvement for better results.";
            if (average >= 70) return "More effort and dedication are needed to improve grades.";
            return "Significant improvement required. Seek additional support and study time.";
        }

        private static string GetSecondaryPerformanceDescription(decimal averagePoints)
        {
            if (averagePoints >= 10) return "excelled academically";
            if (averagePoints >= 8) return "performed very well";
            if (averagePoints >= 6) return "shown good progress";
            if (averagePoints >= 4) return "demonstrated satisfactory performance";
            return "requires significant academic improvement";
        }

        private static string GetSecondaryAdvice(decimal averagePoints)
        {
            if (averagePoints >= 10) return "Maintain this excellent standard and continue challenging yourself.";
            if (averagePoints >= 8) return "Continue working hard to achieve excellence in all subjects.";
            if (averagePoints >= 6) return "Focus on weaker subjects for more balanced improvement.";
            if (averagePoints >= 4) return "More effort and dedicated study time are needed for improvement.";
            return "Immediate attention and comprehensive academic support are required.";
        }

        // Helper method to get general comment (manual or auto-generated)
        private async Task<string> GetGeneralCommentOrGenerateAsync(int reportCardId, List<StudentExamData> examData, string studentFirstName)
        {
            try
            {
                // First, try to get manual comment from database
                using var scope = _serviceProvider.CreateScope();
                var reportCardService = scope.ServiceProvider.GetRequiredService<IReportCardService>();
                
                var manualComment = await reportCardService.GetGeneralCommentAsync(reportCardId);
                
                if (!string.IsNullOrWhiteSpace(manualComment))
                {
                    _logger.LogInformation("Using manual general comment for report card {ReportCardId}", reportCardId);
                    return manualComment;
                }
                
                // Fall back to auto-generated comment
                _logger.LogInformation("No manual comment found, generating auto-comment for report card {ReportCardId}", reportCardId);
                return GenerateTeacherAssessment(examData, studentFirstName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting general comment for report card {ReportCardId}, falling back to auto-generation", reportCardId);
                return GenerateTeacherAssessment(examData, studentFirstName);
            }
        }

        // Helper method to get baby class skill assessments
        private async Task<List<BabyClassSkillAssessmentDto>> GetBabyClassSkillAssessmentsAsync(int studentId, int academicYearId, int term)
        {
            using var scope = _serviceProvider.CreateScope();
            var skillService = scope.ServiceProvider.GetRequiredService<IBabyClassSkillService>();
            
            var assessments = await skillService.GetStudentAssessmentsAsync(studentId, academicYearId, term);
            
            return assessments.Select(a => new BabyClassSkillAssessmentDto
            {
                Id = a.Id,
                StudentId = a.StudentId,
                SkillItemId = a.SkillItemId,
                AcademicYear = a.AcademicYear,
                Term = a.Term,
                TeacherComment = a.TeacherComment,
                AssessedAt = a.AssessedAt,
                AssessedBy = a.AssessedBy,
                StudentName = $"{a.Student.FirstName} {a.Student.LastName}",
                SkillItemName = a.SkillItem.Name,
                SkillName = a.SkillItem.Skill.Name,
                AssessedByTeacherName = null
            }).ToList();
        }

        // Helper method to add baby class score table with comments
        private static void AddBabyScoreTableWithComments(ColumnDescriptor column, List<BabyClassSkillAssessmentDto> skillAssessments)
        {
            if (skillAssessments == null || !skillAssessments.Any())
            {
                column.Item().PaddingTop(10).Text("Assessment data temporarily unavailable.").FontSize(12).AlignCenter();
                return;
            }

            // Group assessments by skill
            var groupedAssessments = skillAssessments.GroupBy(a => a.SkillName).ToList();

            column.Item().PaddingTop(2).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(3);
                });

                foreach (var skillGroup in groupedAssessments)
                {
                    table.Cell().Element(CellStyle).Text(skillGroup.Key).Bold().FontSize(10);
                    table.Cell().Element(CellStyle).Text("Teacher Comments:").Bold().FontSize(10);

                    foreach (var assessment in skillGroup)
                    {
                        table.Cell().Element(CellStyle).PaddingLeft(10).Text(assessment.SkillItemName).FontSize(10);
                        table.Cell().Element(CellStyle).MinHeight(10).Background(Colors.Grey.Lighten4)
                            .Padding(5).Text(assessment.TeacherComment ?? "_________________________________").FontSize(9);
                    }
                }
            });
        }
    }

    // Data Transfer Object for Student Exam Data
    public class StudentExamData
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public decimal Test1Score { get; set; }
        public decimal MidTermScore { get; set; }
        public decimal Test3Score { get; set; }
        public decimal EndTermScore { get; set; }
        public string? Comments { get; set; } // Only from End-of-Term exam
        public DateTime? CommentsUpdatedAt { get; set; }
        public string CommentsUpdatedBy { get; set; }
        public DateTime LastUpdated { get; set; }
        public string RecordedBy { get; set; }
        
        // Absent tracking for each exam type
        public bool IsTest1Absent { get; set; } = false;
        public bool IsMidTermAbsent { get; set; } = false;
        public bool IsTest3Absent { get; set; } = false;
        public bool IsEndTermAbsent { get; set; } = false;
    }

}