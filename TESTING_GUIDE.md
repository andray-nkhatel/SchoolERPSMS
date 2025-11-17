# Optional Subjects Workflow Testing Guide

## 🎯 Testing Overview

This guide will help you test the complete optional subject enrollment workflow, including:
- Super Admin enrollment management
- Teacher authorization for optional subjects
- Report card filtering
- Bulk operations

## 📋 Prerequisites

1. **API Access**: Ensure your API is running and accessible
2. **Authentication**: Have valid admin and teacher JWT tokens
3. **Test Data**: Secondary students and optional subjects in the database
4. **HTTP Client**: Use VS Code REST Client, Postman, or similar

## 🔧 Test Setup

### Step 1: Gather Test Data
```http
# Get all students to find secondary students
GET {{baseUrl}}/api/Students
Authorization: Bearer {{adminToken}}

# Get all grades to find secondary grades
GET {{baseUrl}}/api/Grades
Authorization: Bearer {{adminToken}}

# Get all subjects to find subjects we can make optional
GET {{baseUrl}}/api/Subjects
Authorization: Bearer {{adminToken}}
```

### Step 2: Identify Test Values
- **Grade ID**: Find a secondary grade (SecondaryJunior or SecondarySenior)
- **Student IDs**: Get 3-5 students from that grade
- **Subject IDs**: Find 2-3 subjects that can be made optional
- **Teacher ID**: Get a teacher assigned to teach one of those subjects

## 🧪 Test Scenarios

### Scenario 1: Check Current State
```http
# Check what optional subjects exist for the grade
GET {{baseUrl}}/api/Students/grade/{gradeId}/optional-subjects

# Check current enrollment status
GET {{baseUrl}}/api/Students/grade/{gradeId}/optional-subjects-enrollment

# Find unenrolled students
GET {{baseUrl}}/api/Students/grade/{gradeId}/unenrolled-students
```

**Expected Results:**
- Should show available optional subjects for the grade
- Should show enrollment statistics
- Should list students not enrolled in any optional subjects

### Scenario 2: Individual Student Enrollment
```http
# Assign optional subjects to a single student
POST {{baseUrl}}/api/Students/{studentId}/assign-optional-subjects
Content-Type: application/json

{
  "subjectIds": [subjectId1, subjectId2]
}

# Verify the assignment
GET {{baseUrl}}/api/Students/{studentId}/optional-subjects
```

**Expected Results:**
- Assignment should succeed
- Student should show enrolled subjects
- Should respect subject count limits (1 for Junior, 2 for Senior)

### Scenario 3: Bulk Enrollment
```http
# Bulk assign optional subjects to multiple students
POST {{baseUrl}}/api/Students/bulk-assign-optional-subjects
Content-Type: application/json

{
  "studentIds": [studentId1, studentId2, studentId3],
  "subjectIds": [subjectId1]
}

# Check updated enrollment status
GET {{baseUrl}}/api/Students/grade/{gradeId}/optional-subjects-enrollment
```

**Expected Results:**
- Should show success/error counts
- Should provide detailed results for each student
- Enrollment status should reflect the changes

### Scenario 4: Teacher Authorization
```http
# Get students enrolled in teacher's optional subject
GET {{baseUrl}}/api/Exams/optional-subject/{subjectId}/students?gradeId={gradeId}
Authorization: Bearer {{teacherToken}}

# Try to enter score for enrolled student (should succeed)
POST {{baseUrl}}/api/Exams/scores
Authorization: Bearer {{teacherToken}}
Content-Type: application/json

{
  "studentId": {enrolledStudentId},
  "subjectId": {subjectId},
  "examTypeId": {examTypeId},
  "score": 85,
  "academicYear": 2024,
  "term": 1
}

# Try to enter score for unenrolled student (should fail)
POST {{baseUrl}}/api/Exams/scores
Authorization: Bearer {{teacherToken}}
Content-Type: application/json

{
  "studentId": {unenrolledStudentId},
  "subjectId": {subjectId},
  "examTypeId": {examTypeId},
  "score": 85,
  "academicYear": 2024,
  "term": 1
}
```

**Expected Results:**
- Teacher should only see enrolled students
- Score entry for enrolled student should succeed
- Score entry for unenrolled student should fail with 403 error

### Scenario 5: Core Subject Authorization
```http
# Try to enter score for core subject (should work for all students)
POST {{baseUrl}}/api/Exams/scores
Authorization: Bearer {{teacherToken}}
Content-Type: application/json

{
  "studentId": {anyStudentId},
  "subjectId": {coreSubjectId},
  "examTypeId": {examTypeId},
  "score": 90,
  "academicYear": 2024,
  "term": 1
}
```

**Expected Results:**
- Should succeed for any student in the grade
- Core subjects should not be restricted by enrollment

### Scenario 6: Report Card Generation
```http
# Generate report card for enrolled student
POST {{baseUrl}}/api/ReportCards/generate/student/{studentId}
Authorization: Bearer {{adminToken}}
Content-Type: application/json

{
  "academicYear": 2024,
  "term": 1
}
```

**Expected Results:**
- Report card should only show subjects the student is enrolled in
- Optional subjects should not appear as "fails" for unenrolled students

### Scenario 7: Cleanup Operations
```http
# Remove optional subjects from single student
DELETE {{baseUrl}}/api/Students/{studentId}/remove-optional-subjects

# Bulk remove from multiple students
POST {{baseUrl}}/api/Students/bulk-remove-optional-subjects
Content-Type: application/json

{
  "studentIds": [studentId1, studentId2]
}

# Verify final state
GET {{baseUrl}}/api/Students/grade/{gradeId}/optional-subjects-enrollment
```

**Expected Results:**
- Students should be removed from optional subjects
- Enrollment status should reflect the changes

## 🚨 Common Issues & Solutions

### Issue 1: "Optional subjects can only be assigned to secondary students"
**Solution**: Ensure you're using students from SecondaryJunior or SecondarySenior grades

### Issue 2: "Maximum X optional subjects allowed"
**Solution**: 
- Junior Secondary: Maximum 1 optional subject
- Senior Secondary: Maximum 2 optional subjects

### Issue 3: "Some subjects are not optional for this grade"
**Solution**: Ensure the subjects are marked as optional (IsOptional = true) in the GradeSubjects table

### Issue 4: "You are not authorized to enter scores"
**Solution**: 
- Ensure teacher is assigned to teach the subject in that grade
- For optional subjects, ensure student is enrolled
- Check teacher token is valid

### Issue 5: Report cards showing all subjects
**Solution**: Ensure students are properly enrolled in optional subjects before generating report cards

## 📊 Success Criteria

✅ **Enrollment Management**: Super Admins can assign/remove optional subjects
✅ **Bulk Operations**: Bulk assignment/removal works with detailed results
✅ **Teacher Authorization**: Teachers can only score enrolled students for optional subjects
✅ **Core Subject Access**: Teachers can score all students for core subjects
✅ **Report Card Filtering**: Report cards show only enrolled subjects
✅ **Error Handling**: Proper validation and error messages
✅ **Data Integrity**: No orphaned enrollments or invalid assignments

## 🔍 Monitoring & Debugging

### Check Database State
```sql
-- Check student enrollments
SELECT s.FullName, sub.Name as SubjectName, sos.EnrolledAt
FROM Students s
JOIN StudentOptionalSubjects sos ON s.Id = sos.StudentId
JOIN Subjects sub ON sos.SubjectId = sub.Id
WHERE s.GradeId = {gradeId}

-- Check grade subjects
SELECT g.Name as GradeName, s.Name as SubjectName, gs.IsOptional
FROM Grades g
JOIN GradeSubjects gs ON g.Id = gs.GradeId
JOIN Subjects s ON gs.SubjectId = s.Id
WHERE g.Id = {gradeId}

-- Check teacher assignments
SELECT u.FullName as TeacherName, s.Name as SubjectName, g.Name as GradeName
FROM Users u
JOIN TeacherSubjectAssignments tsa ON u.Id = tsa.TeacherId
JOIN Subjects s ON tsa.SubjectId = s.Id
JOIN Grades g ON tsa.GradeId = g.Id
WHERE tsa.IsActive = 1
```

### API Response Monitoring
- Check HTTP status codes (200, 400, 403, 404, 500)
- Verify response structure matches expected DTOs
- Monitor error messages for clarity
- Check bulk operation success/error counts

## 🎉 Completion Checklist

- [ ] All test scenarios pass
- [ ] No unexpected errors
- [ ] Data integrity maintained
- [ ] Performance acceptable
- [ ] Error messages clear and helpful
- [ ] Documentation updated

## 📞 Support

If you encounter issues during testing:
1. Check the API logs for detailed error information
2. Verify database state using the SQL queries above
3. Ensure all required data exists (students, subjects, teachers)
4. Check authentication tokens are valid and have proper roles
