using BluebirdCore.Entities;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using BluebirdCore.Data;

namespace BluebirdCore.Services
{
    public class ExamAnalysisPdfService
    {
        private readonly SchoolDbContext _context;
        private readonly ILogger<ExamAnalysisPdfService> _logger;

        public ExamAnalysisPdfService(SchoolDbContext context, ILogger<ExamAnalysisPdfService> logger)
        {
            _context = context;
            _logger = logger;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        private List<(Student Student, List<decimal> SubjectScores, List<int> SubjectPoints, decimal Total, decimal TotalPoints, string Certificate)> GetExamAnalysisRows(
            List<Student> students,
            List<Subject> subjects,
            List<ExamScore> scores,
            string examTypeName,
            SchoolSection gradeSection)
        {
            var rows = new List<(Student, List<decimal>, List<int>, decimal, decimal, string)>();
            foreach (var student in students)
            {
                var subjectScores = new List<decimal>();
                var subjectPoints = new List<int>();
                var subjectScoreMap = new Dictionary<string, decimal>();
                
                foreach (var subject in subjects)
                {
                    var score = scores.FirstOrDefault(s => s.StudentId == student.Id && s.SubjectId == subject.Id && s.ExamType.Name == examTypeName);
                    var scoreValue = score?.Score ?? 0;
                    subjectScores.Add(scoreValue);
                    
                    // Calculate points for Senior Secondary only
                    int points = 0;
                    if (gradeSection == SchoolSection.SecondarySenior)
                    {
                        points = GetSecondarySeniorGrade(scoreValue);
                    }
                    subjectPoints.Add(points);
                    subjectScoreMap[subject.Name] = scoreValue;
                }
                
                decimal total;
                decimal totalPoints = 0;
                string certificate = "";
                
                if (gradeSection == SchoolSection.SecondaryJunior)
                {
                    // For Junior Secondary: 6 best scores
                    var bestSixScores = subjectScores
                        .OrderByDescending(score => score)
                        .Take(6)
                        .Sum();
                    total = bestSixScores;
                    
                    // Certificate determination for Junior Secondary
                    certificate = total >= 300 ? "SC" : "SOR";
                }
                else if (gradeSection == SchoolSection.SecondarySenior)
                {
                    // For Senior Secondary: English + 5 highest scoring subjects (excluding English)
                    var englishScore = subjectScoreMap.GetValueOrDefault("English", 0);
                    var englishPoints = GetSecondarySeniorGrade(englishScore);
                    
                    // Get other subjects (excluding English) and sort by score descending
                    var otherSubjects = subjectScoreMap
                        .Where(kvp => kvp.Key != "English")
                        .OrderByDescending(kvp => kvp.Value)
                        .Take(5)
                        .ToList();
                    
                    var otherSubjectsTotal = otherSubjects.Sum(kvp => kvp.Value);
                    var otherSubjectsPoints = otherSubjects.Sum(kvp => GetSecondarySeniorGrade(kvp.Value));
                    
                    total = englishScore + otherSubjectsTotal;
                    totalPoints = englishPoints + otherSubjectsPoints;
                    
                    // Certificate determination for Senior Secondary
                    certificate = (totalPoints >= 37 && englishPoints <= 8) ? "SC" : "GCE";
                }
                else
                {
                    // For other sections: sum all subjects
                    total = subjectScores.Sum();
                    certificate = "N/A";
                }
                
                rows.Add((student, subjectScores, subjectPoints, total, totalPoints, certificate));
            }
            return rows;
        }

        private static int GetSecondarySeniorGrade(decimal score)
        {
            if (score >= 80) return 1;
            if (score >= 75) return 2;
            if (score >= 74) return 3;
            if (score >= 65) return 4;
            if (score >= 60) return 5;
            if (score >= 55) return 6;
            if (score >= 50) return 7;
            if (score >= 45) return 8;
            return 9;
        }

        private static string AbbreviateSubjectName(string subjectName)
        {
            if (subjectName.Length <= 3) return subjectName;
            
            // Handle special cases
            switch (subjectName.ToUpper())
            {
                case "MATHEMATICS": return "MATH";
                case "ENGLISH": return "ENG";
                case "INTEGRATED SCIENCE": return "SCI";
                case "SOCIAL STUDIES": return "SS";
                case "PHYSICAL EDUCATION": return "PE";
                case "ART & DESIGN": return "ART";
                case "COMPUTER STUDIES": return "CS";
                case "BUSINESS STUDIES": return "BS";
                case "RELIGIOUS STUDIES": return "RS";
                case "CREATIVE AND TECHNOLOGY STUDIES (CTS)": return "CTS";
                case "CREATIVE ACTIVITIES": return "CA";
                default:
                    // Take first 3 letters for other subjects
                    return subjectName.Substring(0, 3).ToUpper();
            }
        }

        public async Task<byte[]> GenerateExamAnalysisPdfAsync(int academicYearId, int term, string examTypeName)
        {
            // 0. Fetch academic year for footer
            var academicYear = await _context.AcademicYears
                .FirstOrDefaultAsync(ay => ay.Id == academicYearId);
            
            if (academicYear == null)
                throw new ArgumentException($"Academic year with ID {academicYearId} not found");

            // 1. Fetch only secondary grades
            var grades = await _context.Grades
                .Where(g => g.IsActive && (g.Section == SchoolSection.SecondaryJunior || g.Section == SchoolSection.SecondarySenior))
                .OrderBy(g => g.Level)
                .ThenBy(g => g.Name)
                .ThenBy(g => g.Stream)
                .ToListAsync();

            // 2. Fetch all subjects (for subject names)
            var allSubjects = await _context.Subjects.ToListAsync();

            // 3. Fetch all GradeSubjects (to get subjects per grade)
            var allGradeSubjects = await _context.GradeSubjects.Include(gs => gs.Subject).Where(gs => gs.IsActive).ToListAsync();

            // Fetch all students for all grades
            var allStudents = await _context.Students
                .Where(s => s.IsActive)
                .ToListAsync();

            // Fetch all scores for the academic year, term, and exam type
            var allScores = await _context.ExamScores
                .Where(es => es.AcademicYear == academicYearId
                    && es.Term == term
                    && es.ExamType.Name == examTypeName)
                .Include(es => es.Subject)
                .Include(es => es.ExamType)
                .ToListAsync();

            using var ms = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9));
                    
                    // Add footer
                    page.Footer().AlignCenter().Text($"Term: {term}, Year: {academicYear.Name}, Exam Type: {examTypeName}").FontSize(8);
                    
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Exam Analysis - Academic Year {academicYearId}, Term {term}, Exam: {examTypeName}").FontSize(16).Bold().AlignCenter();

                        bool isFirstGrade = true;
                        foreach (var grade in grades)
                        {
                            var students = allStudents
                                .Where(s => s.GradeId == grade.Id)
                                .OrderBy(s => s.LastName)
                                .ThenBy(s => s.FirstName)
                                .ToList();

                            if (students.Count == 0)
                                continue;

                            var gradeSubjectIds = allGradeSubjects
                                .Where(gs => gs.GradeId == grade.Id)
                                .Select(gs => gs.SubjectId)
                                .ToList();

                            var studentIds = students.Select(s => s.Id).ToList();
                            
                            var subjects = allSubjects.Where(sub => gradeSubjectIds.Contains(sub.Id) && 
                                sub.Name != "MDE" && (allGradeSubjects.First(gs => gs.SubjectId == sub.Id && gs.GradeId == grade.Id).IsOptional == false || 
                                _context.StudentOptionalSubjects.Any(sos => sos.SubjectId == sub.Id && studentIds.Contains(sos.StudentId)))).OrderBy(s => s.Name).ToList();

                            if (subjects.Count == 0)
                                continue;
                            var scores = allScores
                                .Where(es => es.GradeId == grade.Id && studentIds.Contains(es.StudentId))
                                .ToList();

                            // Use helper to build rows
                            var studentRows = GetExamAnalysisRows(students, subjects, scores, examTypeName, grade.Section);

                            // Assign positions
                            var rankedRows = studentRows
                                .OrderByDescending(r => r.Total)
                                .Select((r, idx) => new {
                                    Student = r.Student,
                                    SubjectScores = r.SubjectScores,
                                    SubjectPoints = r.SubjectPoints,
                                    Total = r.Total,
                                    TotalPoints = r.TotalPoints,
                                    Certificate = r.Certificate,
                                    Position = idx + 1
                                }).ToList();

                            // Add page break for grades after the first one
                            if (!isFirstGrade)
                            {
                                col.Item().PageBreak();
                            }
                            isFirstGrade = false;

                            // Table for this grade
                            col.Item().PaddingTop(20).Text($"{grade.FullName}").FontSize(13).Bold().AlignLeft();
                            col.Item().Table(table =>
                            {
                                bool isSeniorSecondary = grade.Section == SchoolSection.SecondarySenior;
                                
                                // Define columns: Student Name + (Subjects) + Total + (Points for Senior Secondary only) + Certificate + Position
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2); // Student Name
                                    foreach (var subject in subjects)
                                        columns.RelativeColumn(1); // Each subject
                                    columns.RelativeColumn(1); // Total
                                    if (isSeniorSecondary)
                                        columns.RelativeColumn(1); // Points
                                    columns.RelativeColumn(1); // Certificate
                                    columns.RelativeColumn(1); // Position
                                });

                                // Header row
                                table.Header(header =>
                                {
                                    header.Cell().Border(1).Background(Colors.Grey.Lighten3).Text("Student Name").Bold();
                                    foreach (var subject in subjects)
                                        header.Cell().Border(1).Background(Colors.Grey.Lighten3).Text(AbbreviateSubjectName(subject.Name)).Bold();
                                    header.Cell().Border(1).Background(Colors.Grey.Lighten3).Text("Total").Bold();
                                    if (isSeniorSecondary)
                                        header.Cell().Border(1).Background(Colors.Grey.Lighten3).Text("Points").Bold();
                                    header.Cell().Border(1).Background(Colors.Grey.Lighten3).Text("Certificate").Bold();
                                    header.Cell().Border(1).Background(Colors.Grey.Lighten3).Text("Position").Bold();
                                });

                                // Data rows
                                foreach (var row in rankedRows)
                                {
                                    table.Cell().Border(1).AlignCenter().Text($"{row.Student.LastName}, {row.Student.FirstName}");
                                    
                                    if (isSeniorSecondary)
                                    {
                                        // For Senior Secondary: show scores and points
                                        for (int i = 0; i < subjects.Count; i++)
                                        {
                                            var score = row.SubjectScores[i];
                                            var points = row.SubjectPoints[i];
                                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text($"{score:F0}({points})");
                                        }
                                    }
                                    else
                                    {
                                        // For Junior Secondary: show only scores
                                        foreach (var score in row.SubjectScores)
                                            table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text(score.ToString("F0"));
                                    }
                                    
                                    table.Cell().Border(1).AlignCenter().Text(row.Total.ToString("F0"));
                                    if (isSeniorSecondary)
                                        table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text(row.TotalPoints.ToString("F0"));
                                    table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text(row.Certificate).Bold();
                                    table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text(row.Position.ToString());
                                }

                                // Average row (footer)
                                if (rankedRows.Count > 0)
                                {
                                    table.Cell().Border(1).Background(Colors.Blue.Lighten4).AlignCenter().Text("AVERAGE").Bold();
                                    
                                    // Calculate averages for each subject
                                    for (int i = 0; i < subjects.Count; i++)
                                    {
                                        var subjectAverages = rankedRows.Select(row => row.SubjectScores[i]).ToList();
                                        var average = subjectAverages.Average();
                                        
                                        if (isSeniorSecondary)
                                        {
                                            var pointAverages = rankedRows.Select(row => row.SubjectPoints[i]).ToList();
                                            var averagePoints = pointAverages.Average();
                                            table.Cell().Border(1).Background(Colors.Blue.Lighten4).AlignCenter().Text($"{average:F1}({averagePoints:F1})").Bold();
                                        }
                                        else
                                        {
                                            table.Cell().Border(1).Background(Colors.Blue.Lighten4).AlignCenter().Text(average.ToString("F1")).Bold();
                                        }
                                    }
                                    
                                    // Average total
                                    var totalAverages = rankedRows.Select(row => row.Total).ToList();
                                    var averageTotal = totalAverages.Average();
                                    table.Cell().Border(1).Background(Colors.Blue.Lighten4).AlignCenter().Text(averageTotal.ToString("F1")).Bold();
                                    
                                    if (isSeniorSecondary)
                                    {
                                        var totalPointsAverages = rankedRows.Select(row => row.TotalPoints).ToList();
                                        var averageTotalPoints = totalPointsAverages.Average();
                                        table.Cell().Border(1).Background(Colors.Blue.Lighten4).AlignCenter().Text(averageTotalPoints.ToString("F1")).Bold();
                                    }
                                    
                                    // Empty cell for certificate column
                                    table.Cell().Border(1).Background(Colors.Blue.Lighten4).AlignCenter().Text("-");
                                    
                                    // Empty cell for position column
                                    table.Cell().Border(1).Background(Colors.Blue.Lighten4).AlignCenter().Text("-");
                                }

                                // Certificate count rows
                                var scCount = rankedRows.Count(r => r.Certificate == "SC");
                                var gceCount = rankedRows.Count(r => r.Certificate == "GCE");
                                var sorCount = rankedRows.Count(r => r.Certificate == "SOR");

                                // SC count row
                                if (scCount > 0)
                                {
                                    table.Cell().Border(1).Background(Colors.Green.Lighten4).AlignCenter().Text($"SC: {scCount}").Bold();
                                    for (int i = 0; i < subjects.Count; i++)
                                        table.Cell().Border(1).Background(Colors.Green.Lighten4).AlignCenter().Text("-");
                                    table.Cell().Border(1).Background(Colors.Green.Lighten4).AlignCenter().Text("-");
                                    if (isSeniorSecondary)
                                        table.Cell().Border(1).Background(Colors.Green.Lighten4).AlignCenter().Text("-");
                                    table.Cell().Border(1).Background(Colors.Green.Lighten4).AlignCenter().Text($"SC: {scCount}").Bold();
                                    table.Cell().Border(1).Background(Colors.Green.Lighten4).AlignCenter().Text("-");
                                }

                                // GCE count row
                                if (gceCount > 0)
                                {
                                    table.Cell().Border(1).Background(Colors.Orange.Lighten4).AlignCenter().Text($"GCE: {gceCount}").Bold();
                                    for (int i = 0; i < subjects.Count; i++)
                                        table.Cell().Border(1).Background(Colors.Orange.Lighten4).AlignCenter().Text("-");
                                    table.Cell().Border(1).Background(Colors.Orange.Lighten4).AlignCenter().Text("-");
                                    if (isSeniorSecondary)
                                        table.Cell().Border(1).Background(Colors.Orange.Lighten4).AlignCenter().Text("-");
                                    table.Cell().Border(1).Background(Colors.Orange.Lighten4).AlignCenter().Text($"GCE: {gceCount}").Bold();
                                    table.Cell().Border(1).Background(Colors.Orange.Lighten4).AlignCenter().Text("-");
                                }

                                // SOR count row
                                if (sorCount > 0)
                                {
                                    table.Cell().Border(1).Background(Colors.Red.Lighten4).AlignCenter().Text($"SOR: {sorCount}").Bold();
                                    for (int i = 0; i < subjects.Count; i++)
                                        table.Cell().Border(1).Background(Colors.Red.Lighten4).AlignCenter().Text("-");
                                    table.Cell().Border(1).Background(Colors.Red.Lighten4).AlignCenter().Text("-");
                                    if (isSeniorSecondary)
                                        table.Cell().Border(1).Background(Colors.Red.Lighten4).AlignCenter().Text("-");
                                    table.Cell().Border(1).Background(Colors.Red.Lighten4).AlignCenter().Text($"SOR: {sorCount}").Bold();
                                    table.Cell().Border(1).Background(Colors.Red.Lighten4).AlignCenter().Text("-");
                                }
                            });
                        }
                    });
                });
            }).GeneratePdf(ms);

            return ms.ToArray();
        }

        public async Task<byte[]> GenerateExamAnalysisPdfForGradeAsync(int gradeId, int academicYearId, int term, string examTypeName)
        {
            // 0. Fetch academic year for footer
            var academicYear = await _context.AcademicYears
                .FirstOrDefaultAsync(ay => ay.Id == academicYearId);
            
            if (academicYear == null)
                throw new ArgumentException($"Academic year with ID {academicYearId} not found");

            // 1. Fetch the specific grade
            var grade = await _context.Grades
                .FirstOrDefaultAsync(g => g.Id == gradeId && g.IsActive && (g.Section == SchoolSection.SecondaryJunior || g.Section == SchoolSection.SecondarySenior));
            
            if (grade == null)
                throw new ArgumentException($"Secondary grade with ID {gradeId} not found or is not active");

            // 2. Fetch all subjects and grade-subject relationships
            var allSubjects = await _context.Subjects.ToListAsync();
            var allGradeSubjects = await _context.GradeSubjects
                .Where(gs => gs.IsActive)
                .ToListAsync();

            // 3. Fetch all exam scores for the specific grade
            var scores = await _context.ExamScores
                .Include(es => es.ExamType)
                .Where(es => es.GradeId == gradeId && es.AcademicYear == academicYearId && es.Term == term)
                .ToListAsync();

            if (!scores.Any())
                throw new ArgumentException($"No exam scores found for grade {gradeId}, academic year {academicYearId}, term {term}. Please ensure that exam scores have been recorded for this grade, term, and exam type.");

            // 4. Fetch students for this grade
            var students = await _context.Students
                .Where(s => s.GradeId == gradeId && s.IsActive)
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();

            if (!students.Any())
                throw new ArgumentException($"No active students found in grade {gradeId}");

            // 5. Get subject IDs for this grade
            var gradeSubjectIds = allGradeSubjects
                .Where(gs => gs.GradeId == gradeId)
                .Select(gs => gs.SubjectId)
                .ToList();

            // 6. Filter subjects based on grade section
            var subjects = allSubjects.Where(sub => gradeSubjectIds.Contains(sub.Id) && 
                sub.Name != "MDE" && (allGradeSubjects.First(gs => gs.SubjectId == sub.Id && gs.GradeId == gradeId).IsOptional == false || 
                _context.StudentOptionalSubjects.Any(sos => sos.SubjectId == sub.Id && students.Select(s => s.Id).Contains(sos.StudentId)))).OrderBy(s => s.Name).ToList();

            if (subjects.Count == 0)
                throw new ArgumentException($"No subjects found for grade {gradeId}");

            // 7. Get student IDs for optional subject filtering
            var studentIds = students.Select(s => s.Id).ToList();

            // 8. Generate PDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(9));
                    
                    // Add footer
                    page.Footer().AlignCenter().Text($"Term: {term}, Year: {academicYear.Name}, Exam Type: {examTypeName}").FontSize(8);
                    
                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Exam Analysis - {grade.Name} {grade.Stream} - Academic Year {academicYearId}, Term {term}, Exam: {examTypeName}").FontSize(16).Bold().AlignCenter();
                        col.Item().Height(20);
                        
                        // Table for this specific grade
                        col.Item().Table(table =>
                        {
                            bool isSeniorSecondary = grade.Section == SchoolSection.SecondarySenior;
                            
                            // Define columns: Student Name + (Subjects) + Total + (Points for Senior Secondary only) + Certificate + Position
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Student Name
                                foreach (var subject in subjects)
                                    columns.RelativeColumn(1); // Each subject
                                columns.RelativeColumn(1); // Total
                                if (isSeniorSecondary)
                                    columns.RelativeColumn(1); // Points
                                columns.RelativeColumn(1); // Certificate
                                columns.RelativeColumn(1); // Position
                            });

                            // Header row
                            table.Header(header =>
                            {
                                header.Cell().Border(1).Background(Colors.Grey.Lighten3).Text("Student Name").Bold();
                                foreach (var subject in subjects)
                                    header.Cell().Border(1).Background(Colors.Grey.Lighten3).Text(AbbreviateSubjectName(subject.Name)).Bold();
                                header.Cell().Border(1).Background(Colors.Grey.Lighten3).Text("Total").Bold();
                                if (isSeniorSecondary)
                                    header.Cell().Border(1).Background(Colors.Grey.Lighten3).Text("Points").Bold();
                                header.Cell().Border(1).Background(Colors.Grey.Lighten3).Text("Certificate").Bold();
                                header.Cell().Border(1).Background(Colors.Grey.Lighten3).Text("Position").Bold();
                            });

                            // Data rows
                            var studentRows = GetExamAnalysisRows(students, subjects, scores, examTypeName, grade.Section);
                            var rankedRows = studentRows.OrderByDescending(r => r.Total).ToList();
                            
                            foreach (var row in rankedRows)
                            {
                                table.Cell().Border(1).AlignCenter().Text($"{row.Student.LastName}, {row.Student.FirstName}");
                                
                                if (isSeniorSecondary)
                                {
                                    // For Senior Secondary: show scores and points
                                    for (int i = 0; i < subjects.Count; i++)
                                    {
                                        var score = row.SubjectScores[i];
                                        var points = row.SubjectPoints[i];
                                        table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text($"{score:F0}({points})");
                                    }
                                }
                                else
                                {
                                    // For Junior Secondary: show only scores
                                    foreach (var score in row.SubjectScores)
                                        table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text(score.ToString("F0"));
                                }
                                
                                table.Cell().Border(1).AlignCenter().Text(row.Total.ToString("F0"));
                                if (isSeniorSecondary)
                                    table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text(row.TotalPoints.ToString("F0"));
                                table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text(row.Certificate).Bold();
                                                                    table.Cell().Border(1).Background(Colors.Grey.Lighten3).AlignCenter().Text((rankedRows.IndexOf(row) + 1).ToString());
                            }

                            // Average row (footer)
                            if (rankedRows.Count > 0)
                            {
                                table.Cell().Border(1).Background(Colors.Blue.Lighten4).AlignCenter().Text("AVERAGE").Bold();
                                
                                // Calculate averages for each subject
                                for (int i = 0; i < subjects.Count; i++)
                                {
                                    var subjectAverages = rankedRows.Select(row => row.SubjectScores[i]).ToList();
                                    var average = subjectAverages.Average();
                                    
                                    if (isSeniorSecondary)
                                    {
                                        var pointAverages = rankedRows.Select(row => row.SubjectPoints[i]).ToList();
                                        var averagePoints = pointAverages.Average();
                                        table.Cell().Border(1).Background(Colors.Blue.Lighten4).AlignCenter().Text($"{average:F1}({averagePoints:F1})").Bold();
                                    }
                                    else
                                    {
                                        table.Cell().Border(1).Background(Colors.Blue.Lighten4).AlignCenter().Text(average.ToString("F1")).Bold();
                                    }
                                }
                                
                                // Average total
                                var totalAverages = rankedRows.Select(row => row.Total).ToList();
                                var averageTotal = totalAverages.Average();
                                table.Cell().Border(1).Background(Colors.Blue.Lighten4).AlignCenter().Text(averageTotal.ToString("F1")).Bold();
                                
                                if (isSeniorSecondary)
                                {
                                    var totalPointsAverages = rankedRows.Select(row => row.TotalPoints).ToList();
                                    var averageTotalPoints = totalPointsAverages.Average();
                                    table.Cell().Border(1).Background(Colors.Blue.Lighten4).AlignCenter().Text(averageTotalPoints.ToString("F1")).Bold();
                                }
                                
                                // Empty cell for certificate column
                                table.Cell().Border(1).Background(Colors.Blue.Lighten4).AlignCenter().Text("-");
                                
                                // Empty cell for position column
                                table.Cell().Border(1).Background(Colors.Blue.Lighten4).AlignCenter().Text("-");
                            }

                            // Certificate count rows
                            var scCount = rankedRows.Count(r => r.Certificate == "SC");
                            var gceCount = rankedRows.Count(r => r.Certificate == "GCE");
                            var sorCount = rankedRows.Count(r => r.Certificate == "SOR");

                            // SC count row
                            if (scCount > 0)
                            {
                                table.Cell().Border(1).Background(Colors.Green.Lighten4).AlignCenter().Text($"SC: {scCount}").Bold();
                                for (int i = 0; i < subjects.Count; i++)
                                    table.Cell().Border(1).Background(Colors.Green.Lighten4).AlignCenter().Text("-");
                                table.Cell().Border(1).Background(Colors.Green.Lighten4).AlignCenter().Text("-");
                                if (isSeniorSecondary)
                                    table.Cell().Border(1).Background(Colors.Green.Lighten4).AlignCenter().Text("-");
                                table.Cell().Border(1).Background(Colors.Green.Lighten4).AlignCenter().Text($"SC: {scCount}").Bold();
                                table.Cell().Border(1).Background(Colors.Green.Lighten4).AlignCenter().Text("-");
                            }

                            // GCE count row
                            if (gceCount > 0)
                            {
                                table.Cell().Border(1).Background(Colors.Orange.Lighten4).AlignCenter().Text($"GCE: {gceCount}").Bold();
                                for (int i = 0; i < subjects.Count; i++)
                                    table.Cell().Border(1).Background(Colors.Orange.Lighten4).AlignCenter().Text("-");
                                table.Cell().Border(1).Background(Colors.Orange.Lighten4).AlignCenter().Text("-");
                                if (isSeniorSecondary)
                                    table.Cell().Border(1).Background(Colors.Orange.Lighten4).AlignCenter().Text("-");
                                table.Cell().Border(1).Background(Colors.Orange.Lighten4).AlignCenter().Text($"GCE: {gceCount}").Bold();
                                table.Cell().Border(1).Background(Colors.Orange.Lighten4).AlignCenter().Text("-");
                            }

                            // SOR count row
                            if (sorCount > 0)
                            {
                                table.Cell().Border(1).Background(Colors.Red.Lighten4).AlignCenter().Text($"SOR: {sorCount}").Bold();
                                for (int i = 0; i < subjects.Count; i++)
                                    table.Cell().Border(1).Background(Colors.Red.Lighten4).AlignCenter().Text("-");
                                table.Cell().Border(1).Background(Colors.Red.Lighten4).AlignCenter().Text("-");
                                if (isSeniorSecondary)
                                    table.Cell().Border(1).Background(Colors.Red.Lighten4).AlignCenter().Text("-");
                                table.Cell().Border(1).Background(Colors.Red.Lighten4).AlignCenter().Text($"SOR: {sorCount}").Bold();
                                table.Cell().Border(1).Background(Colors.Red.Lighten4).AlignCenter().Text("-");
                            }
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
} 