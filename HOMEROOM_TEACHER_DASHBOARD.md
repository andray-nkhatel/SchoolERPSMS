# Homeroom Teacher Dashboard Implementation

## Overview
This implementation provides a dedicated API for homeroom teachers to manage their students' subject assignments. The system ensures secure access where teachers can only manage students in their assigned homeroom grades.

## Security & Authorization

### Three-Layer Security Model

1. **JWT Role-Based Authorization**
   - Only users with `Teacher` role can access endpoints
   - Implemented via `[Authorize(Roles = "Teacher")]`

2. **Homeroom Teacher Validation**
   - Verifies teacher is assigned as homeroom teacher
   - Checks `Grade.HomeroomTeacherId` relationship

3. **Student Access Control**
   - Teachers can only access students in their homeroom grades
   - Validates `Student.GradeId` is in teacher's homeroom grades

## API Endpoints

### Base URL: `/api/homeroom`

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| GET | `/students` | Get homeroom students with subjects | Teacher (Homeroom) |
| GET | `/grade-info` | Get homeroom grade information | Teacher (Homeroom) |
| GET | `/available-subjects` | Get available subjects for grade | Teacher (Homeroom) |
| POST | `/students/{id}/subjects` | Assign subject to student | Teacher (Homeroom) |
| DELETE | `/students/{id}/subjects/{subjectId}` | Remove subject from student | Teacher (Homeroom) |
| POST | `/bulk-assign-subjects` | Bulk assign subject to multiple students | Teacher (Homeroom) |
| GET | `/debug/status` | Debug homeroom teacher status | Teacher (Homeroom) |

## Data Transfer Objects (DTOs)

### HomeroomStudentDto
```csharp
public class HomeroomStudentDto
{
    public int Id { get; set; }
    public required string StudentNumber { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string FullName { get; set; }
    public required string GradeName { get; set; }
    public int GradeId { get; set; }
    public List<StudentSubjectDto> Subjects { get; set; } = new List<StudentSubjectDto>();
}
```

### StudentSubjectDto
```csharp
public class StudentSubjectDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int SubjectId { get; set; }
    public required string SubjectName { get; set; }
    public required string SubjectCode { get; set; }
    public DateTime EnrolledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }
    public string? AssignedBy { get; set; }
}
```

## Service Implementation

### IHomeroomService Interface
- `GetTeacherHomeroomGradesAsync(int teacherId)` - Get teacher's homeroom grades
- `CanAccessStudentAsync(int teacherId, int studentId)` - Validate student access
- `GetHomeroomStudentsAsync(int teacherId)` - Get homeroom students
- `GetHomeroomGradeInfoAsync(int teacherId)` - Get grade information
- `AssignSubjectToStudentAsync()` - Assign subject to student
- `RemoveSubjectFromStudentAsync()` - Remove subject from student
- `BulkAssignSubjectsAsync()` - Bulk assign subjects

## Security Flow

### 1. Authentication Check
```csharp
[Authorize(Roles = "Teacher")] // ASP.NET Core validates JWT token
```

### 2. Extract Teacher ID
```csharp
var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
var teacherId = int.Parse(userIdClaim.Value);
```

### 3. Validate Homeroom Teacher Status
```csharp
var homeroomGrades = await _context.Grades
    .Where(g => g.HomeroomTeacherId == teacherId && g.IsActive)
    .Select(g => g.Id)
    .ToListAsync();
```

### 4. Validate Student Access
```csharp
var canAccess = await _context.Students
    .Where(s => s.Id == studentId && homeroomGrades.Contains(s.GradeId))
    .AnyAsync();
```

## Usage Examples

### Get Homeroom Students
```http
GET /api/homeroom/students
Authorization: Bearer {jwt_token}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "studentNumber": "STU001",
      "firstName": "John",
      "lastName": "Doe",
      "fullName": "John Doe",
      "gradeName": "Form 1 Blue",
      "gradeId": 1,
      "subjects": [
        {
          "id": 1,
          "studentId": 1,
          "subjectId": 5,
          "subjectName": "Mathematics",
          "subjectCode": "MATH",
          "enrolledDate": "2024-01-15T10:30:00Z",
          "isActive": true,
          "assignedBy": "Mr. Smith"
        }
      ]
    }
  ],
  "message": "Retrieved 1 homeroom students"
}
```

### Assign Subject to Student
```http
POST /api/homeroom/students/1/subjects
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "subjectId": 6,
  "notes": "Assigned for new semester"
}
```

### Bulk Assign Subjects
```http
POST /api/homeroom/bulk-assign-subjects
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "studentIds": [1, 2, 3],
  "subjectId": 7,
  "notes": "Bulk assignment for all students"
}
```

## Error Handling

### Common Error Responses

1. **Unauthorized Access**
```json
{
  "success": false,
  "message": "You can only manage students in your homeroom"
}
```

2. **Not a Homeroom Teacher**
```json
{
  "success": false,
  "message": "You are not assigned as a homeroom teacher"
}
```

3. **Subject Already Assigned**
```json
{
  "success": false,
  "message": "Subject is already assigned to this student"
}
```

## Frontend Integration

### Required Frontend Components

1. **Homeroom Dashboard Page**
   - List of homeroom students
   - Student subject assignments
   - Available subjects for assignment

2. **Student Management Interface**
   - Individual student subject management
   - Bulk operations for multiple students
   - Subject assignment/removal forms

3. **Grade Information Panel**
   - Homeroom grade details
   - Available subjects for the grade
   - Student count and statistics

### API Integration Points

```javascript
// Get homeroom students
const response = await fetch('/api/homeroom/students', {
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
});

// Assign subject to student
const assignResponse = await fetch(`/api/homeroom/students/${studentId}/subjects`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    subjectId: subjectId,
    notes: notes
  })
});
```

## Testing

Use the provided `HomeroomController.http` file to test all endpoints. Make sure to:

1. Login as a homeroom teacher to get JWT token
2. Test all endpoints with proper authorization
3. Test unauthorized access scenarios
4. Verify security restrictions work correctly

## Database Requirements

The implementation uses existing database relationships:
- `Users` table (teachers)
- `Grades` table (with `HomeroomTeacherId`)
- `Students` table (with `GradeId`)
- `StudentSubjects` table (subject assignments)
- `Subjects` table (available subjects)

## Future Enhancements

1. **Audit Trail**: Track all subject assignment changes
2. **Notifications**: Notify when subjects are assigned/removed
3. **Bulk Operations**: More advanced bulk management features
4. **Reporting**: Generate reports on subject assignments
5. **Mobile Support**: Mobile-friendly interface for teachers
