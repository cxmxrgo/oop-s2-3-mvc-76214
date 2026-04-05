# VGC Student & Course Management System

## Prerequisites

- .NET SDK 10
- MySQL Server running locally 

## Configuration
- `server=localhost;port=3306;database=vgc_student_course_m1;user=root;password=admin`

## Setup Steps
1. Open a terminal:
2. Restore packages:
   - `dotnet restore`
3. Apply migrations:
   - `dotnet ef database update --project src/VgcCollege.Web/oop-s2-1-mvc-76214.csproj --startup-project src/VgcCollege.Web/oop-s2-1-mvc-76214.csproj`
4. Run the application:
   - `dotnet run --project src/VgcCollege.Web/oop-s2-1-mvc-76214.csproj`

## How to Run Tests

- Run all tests:
  - `dotnet test VGC.slnx --configuration Release`

## Seeded Demo Accounts

### Login Credentials

### Administrator

- Email/Username: `admin@vgc.ie`
- Password: `Admin123!`

### Faculty

- Email/Username: `aoife.murphy@vgc.ie`
- Password: `Faculty123!`
- Email/Username: `liam.kelly@vgc.ie`
- Password: `Faculty123!`

### Students

- Email/Username: `john.rowley@vgc.ie` | Password: `Student123!`
- Email/Username: `bernard.roche@vgc.ie` | Password: `Student123!`
- Email/Username: `seamus.hickey@vgc.ie` | Password: `Student123!`
- Email/Username: `david.keane@vgc.ie` | Password: `Student123!`
- Email/Username: `rod.haanappel@vgc.ie` | Password: `Student123!`

## Design Decisions / Assumptions

- Role-based authorization is enforced server-side at controller/action level using `[Authorize]` and role checks.
- Faculty access is scoped by `CourseFacultyAssignment`; faculty can only manage data for assigned courses.
- Student access is scoped by `IdentityUserId`; students can only view their own profile, attendance, and results.
- Exam visibility for students is filtered by `Exam.ResultsReleased == true`; unreleased exam results are hidden.
- Scores are constrained to `0-100` and max score values are constrained to `1-100`.
- Seed data is idempotent and includes branches, courses, role accounts, profiles, assignments, enrolments, attendance, and results.

