namespace VgcCollege.Tests;

public class FeatureRulesTests
{
    [Fact]
    public void Assignment_MaxScore_Above100_ShouldFailValidation()
    {
        var model = new Assignment { CourseId = 1, Title = "A1", MaxScore = 101, DueDate = DateTime.Today };
        var results = Validate(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(Assignment.MaxScore)));
    }

    [Fact]
    public void ExamResult_Grade_Required_ShouldFailValidation()
    {
        var model = new ExamResult { ExamId = 1, StudentProfileId = 1, Score = 80, Grade = string.Empty };
        var results = Validate(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(ExamResult.Grade)));
    }

    [Fact]
    public void CourseEnrolment_Status_Required_ShouldFailValidation()
    {
        var model = new CourseEnrolment { StudentProfileId = 1, CourseId = 1, EnrolDate = DateTime.Today, Status = string.Empty };
        var results = Validate(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(CourseEnrolment.Status)));
    }

    [Fact]
    public async Task Student_Results_ShouldContainOnlyReleasedExamResults()
    {
        await using var context = CreateContext();

        var course = new Course { Id = 1, Name = "Course", BranchId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(10) };
        var student = new StudentProfile { Id = 1, IdentityUserId = "stu-1", Name = "Student One", Email = "s1@vgc.ie", Phone = "1", Address = "A", DOB = DateTime.Today.AddYears(-20), StudentNumber = "S001" };

        var releasedExam = new Exam { Id = 1, CourseId = 1, Course = course, Title = "Released", Date = DateTime.Today, MaxScore = 100, ResultsReleased = true };
        var provisionalExam = new Exam { Id = 2, CourseId = 1, Course = course, Title = "Provisional", Date = DateTime.Today, MaxScore = 100, ResultsReleased = false };

        context.Courses.Add(course);
        context.StudentProfiles.Add(student);
        context.Exams.AddRange(releasedExam, provisionalExam);
        context.ExamResults.AddRange(
            new ExamResult { ExamId = 1, StudentProfileId = 1, Score = 75, Grade = "B" },
            new ExamResult { ExamId = 2, StudentProfileId = 1, Score = 65, Grade = "C" });
        await context.SaveChangesAsync();

        var controller = new StudentController(context);
        SetUser(controller, "stu-1", RoleNames.Student);

        var result = await controller.Results();
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<StudentResultsViewModel>(view.Model);

        Assert.Single(model.ReleasedExamResults);
        Assert.Equal("Released", model.ReleasedExamResults[0].ExamTitle);
    }

    [Fact]
    public async Task Student_Attendance_ShouldContainOnlyOwnRecords()
    {
        await using var context = CreateContext();

        var course = new Course { Id = 1, Name = "Course", BranchId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(10) };
        var studentOne = new StudentProfile { Id = 1, IdentityUserId = "stu-1", Name = "Student One", Email = "s1@vgc.ie", Phone = "1", Address = "A", DOB = DateTime.Today.AddYears(-20), StudentNumber = "S001" };
        var studentTwo = new StudentProfile { Id = 2, IdentityUserId = "stu-2", Name = "Student Two", Email = "s2@vgc.ie", Phone = "2", Address = "B", DOB = DateTime.Today.AddYears(-21), StudentNumber = "S002" };

        var enrolOne = new CourseEnrolment { Id = 1, CourseId = 1, StudentProfileId = 1, EnrolDate = DateTime.Today, Status = "Active" };
        var enrolTwo = new CourseEnrolment { Id = 2, CourseId = 1, StudentProfileId = 2, EnrolDate = DateTime.Today, Status = "Active" };

        context.Courses.Add(course);
        context.StudentProfiles.AddRange(studentOne, studentTwo);
        context.CourseEnrolments.AddRange(enrolOne, enrolTwo);
        context.AttendanceRecords.AddRange(
            new AttendanceRecord { CourseEnrolmentId = 1, WeekNumber = 1, AttendanceDate = DateTime.Today, Present = true },
            new AttendanceRecord { CourseEnrolmentId = 2, WeekNumber = 1, AttendanceDate = DateTime.Today, Present = false });
        await context.SaveChangesAsync();

        var controller = new StudentController(context);
        SetUser(controller, "stu-1", RoleNames.Student);

        var result = await controller.Attendance();
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<StudentAttendanceRecordViewModel>>(view.Model);

        Assert.Single(model);
        Assert.True(model.First().Present);
    }

    [Fact]
    public async Task AssignmentResults_Post_WhenScoreAboveMax_ShouldAddFieldError()
    {
        await using var context = CreateContext();

        context.Courses.Add(new Course { Id = 1, Name = "Course", BranchId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(10) });
        context.Assignments.Add(new Assignment { Id = 1, CourseId = 1, Title = "A1", MaxScore = 100, DueDate = DateTime.Today });
        await context.SaveChangesAsync();

        var controller = new GradebookController(context);
        SetUser(controller, "admin-1", RoleNames.Administrator);

        var model = new AssignmentResultsViewModel
        {
            AssignmentId = 1,
            CourseId = 1,
            Students = new List<AssignmentResultRowViewModel>
            {
                new() { StudentProfileId = 1, StudentName = "Student", StudentNumber = "S001", Score = 101 }
            }
        };

        var result = await controller.AssignmentResults(model);
        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains("Students[0].Score", controller.ModelState.Keys);
    }

    [Fact]
    public async Task ExamResults_Post_WhenScoreAboveMax_ShouldAddFieldError()
    {
        await using var context = CreateContext();

        context.Courses.Add(new Course { Id = 1, Name = "Course", BranchId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(10) });
        context.Exams.Add(new Exam { Id = 1, CourseId = 1, Title = "Final", MaxScore = 100, Date = DateTime.Today, ResultsReleased = false });
        await context.SaveChangesAsync();

        var controller = new GradebookController(context);
        SetUser(controller, "admin-1", RoleNames.Administrator);

        var model = new ExamResultsViewModel
        {
            ExamId = 1,
            CourseId = 1,
            Students = new List<ExamResultRowViewModel>
            {
                new() { StudentProfileId = 1, StudentName = "Student", StudentNumber = "S001", Score = 101, Grade = "A" }
            }
        };

        var result = await controller.ExamResults(model);
        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains("Students[0].Score", controller.ModelState.Keys);
    }

    [Fact]
    public async Task ToggleRelease_ShouldFlipExamResultsReleasedFlag()
    {
        await using var context = CreateContext();

        context.Courses.Add(new Course { Id = 1, Name = "Course", BranchId = 1, StartDate = DateTime.Today, EndDate = DateTime.Today.AddDays(10) });
        context.Exams.Add(new Exam { Id = 1, CourseId = 1, Title = "Final", Date = DateTime.Today, MaxScore = 100, ResultsReleased = false });
        await context.SaveChangesAsync();

        var controller = new GradebookController(context);
        SetUser(controller, "admin-1", RoleNames.Administrator);

        var actionResult = await controller.ToggleRelease(1);

        Assert.IsType<RedirectToActionResult>(actionResult);
        var exam = await context.Exams.SingleAsync(x => x.Id == 1);
        Assert.True(exam.ResultsReleased);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static List<ValidationResult> Validate<T>(T model)
    {
        var context = new ValidationContext(model!);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model!, context, results, validateAllProperties: true);
        return results;
    }

    private static void SetUser(Controller controller, string userId, string role)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, role)
        }, "TestAuth");

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        controller.TempData = new TempDataDictionary(controller.HttpContext, new TestTempDataProvider());
    }

    private sealed class TestTempDataProvider : ITempDataProvider
    {
        public IDictionary<string, object> LoadTempData(HttpContext context) => new Dictionary<string, object>();

        public void SaveTempData(HttpContext context, IDictionary<string, object> values)
        {
        }
    }
}
