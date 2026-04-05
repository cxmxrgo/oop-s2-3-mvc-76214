using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using oop_s2_1_mvc_76214.Models;

namespace oop_s2_1_mvc_76214.Data;

public static class ApplicationDbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

        await context.Database.MigrateAsync();

        var roles = new[] { RoleNames.Administrator, RoleNames.Faculty, RoleNames.Student };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var admin = await EnsureUserAsync(userManager, "admin@vgc.ie", "Admin123!", "admin@vgc.ie");
        await EnsureRoleAsync(userManager, admin, RoleNames.Administrator);

        var faculty1 = await EnsureUserAsync(userManager, "aoife.murphy@vgc.ie", "Faculty123!", "aoife.murphy@vgc.ie");
        var faculty2 = await EnsureUserAsync(userManager, "liam.kelly@vgc.ie", "Faculty123!", "liam.kelly@vgc.ie");
        await EnsureRoleAsync(userManager, faculty1, RoleNames.Faculty);
        await EnsureRoleAsync(userManager, faculty2, RoleNames.Faculty);

        var studentUsers = new[]
        {
            await EnsureUserAsync(userManager, "john.rowley@vgc.ie", "Student123!", "john.rowley@vgc.ie"),
            await EnsureUserAsync(userManager, "bernard.roche@vgc.ie", "Student123!", "bernard.roche@vgc.ie"),
            await EnsureUserAsync(userManager, "seamus.hickey@vgc.ie", "Student123!", "seamus.hickey@vgc.ie"),
            await EnsureUserAsync(userManager, "david.keane@vgc.ie", "Student123!", "david.keane@vgc.ie"),
            await EnsureUserAsync(userManager, "rod.haanappel@vgc.ie", "Student123!", "rod.haanappel@vgc.ie")
        };

        foreach (var studentUser in studentUsers)
        {
            await EnsureRoleAsync(userManager, studentUser, RoleNames.Student);
        }

        if (!await context.Branches.AnyAsync())
        {
            context.Branches.AddRange(
                new Branch { Name = "Dublin Branch", Address = "12 O'Connell Street, Dublin" },
                new Branch { Name = "Cork Branch", Address = "8 Patrick Street, Cork" },
                new Branch { Name = "Galway Branch", Address = "15 Eyre Square, Galway" });
            await context.SaveChangesAsync();
        }

        if (!await context.Courses.AnyAsync())
        {
            var dublinId = await context.Branches.Where(x => x.Name == "Dublin Branch").Select(x => x.Id).SingleAsync();
            var corkId = await context.Branches.Where(x => x.Name == "Cork Branch").Select(x => x.Id).SingleAsync();
            var galwayId = await context.Branches.Where(x => x.Name == "Galway Branch").Select(x => x.Id).SingleAsync();

            context.Courses.AddRange(
                new Course { Name = "Software Engineering Fundamentals", BranchId = dublinId, StartDate = new DateTime(2026, 9, 1), EndDate = new DateTime(2027, 1, 31) },
                new Course { Name = "Cloud Development", BranchId = corkId, StartDate = new DateTime(2026, 9, 15), EndDate = new DateTime(2027, 2, 15) },
                new Course { Name = "Data Analytics", BranchId = galwayId, StartDate = new DateTime(2026, 10, 1), EndDate = new DateTime(2027, 3, 1) });
            await context.SaveChangesAsync();
        }

        await EnsureFacultyProfileAsync(context, faculty1.Id, "Aoife Murphy", "aoife.murphy@vgc.ie", "+353-85-100-2000");
        await EnsureFacultyProfileAsync(context, faculty2.Id, "Liam Kelly", "liam.kelly@vgc.ie", "+353-85-100-3000");

        await EnsureStudentProfileAsync(context, studentUsers[0].Id, "John Rowley", "john.rowley@vgc.ie", "+353-86-111-1111", "1 Green Lane, Dublin", new DateTime(2003, 2, 14), "VGC1001");
        await EnsureStudentProfileAsync(context, studentUsers[1].Id, "Bernard Roche", "bernard.roche@vgc.ie", "+353-86-111-1112", "2 Green Lane, Cork", new DateTime(2002, 7, 10), "VGC1002");
        await EnsureStudentProfileAsync(context, studentUsers[2].Id, "Seamus Hickey", "seamus.hickey@vgc.ie", "+353-86-111-1113", "3 Green Lane, Galway", new DateTime(2001, 11, 22), "VGC1003");
        await EnsureStudentProfileAsync(context, studentUsers[3].Id, "David Keane", "david.keane@vgc.ie", "+353-86-111-1114", "4 Green Lane, Dublin", new DateTime(2004, 4, 9), "VGC1004");
        await EnsureStudentProfileAsync(context, studentUsers[4].Id, "Rod Haanappel", "rod.haanappel@vgc.ie", "+353-86-111-1115", "5 Green Lane, Cork", new DateTime(2003, 12, 3), "VGC1005");

        await context.SaveChangesAsync();

        if (!await context.CourseFacultyAssignments.AnyAsync())
        {
            var facultyOneId = await context.FacultyProfiles.Where(x => x.Email == "aoife.murphy@vgc.ie").Select(x => x.Id).SingleAsync();
            var facultyTwoId = await context.FacultyProfiles.Where(x => x.Email == "liam.kelly@vgc.ie").Select(x => x.Id).SingleAsync();

            var softwareCourseId = await context.Courses.Where(x => x.Name == "Software Engineering Fundamentals").Select(x => x.Id).SingleAsync();
            var cloudCourseId = await context.Courses.Where(x => x.Name == "Cloud Development").Select(x => x.Id).SingleAsync();
            var analyticsCourseId = await context.Courses.Where(x => x.Name == "Data Analytics").Select(x => x.Id).SingleAsync();

            context.CourseFacultyAssignments.AddRange(
                new CourseFacultyAssignment { CourseId = softwareCourseId, FacultyProfileId = facultyOneId },
                new CourseFacultyAssignment { CourseId = cloudCourseId, FacultyProfileId = facultyTwoId },
                new CourseFacultyAssignment { CourseId = analyticsCourseId, FacultyProfileId = facultyOneId });
        }

        if (!await context.CourseEnrolments.AnyAsync())
        {
            var studentIds = await context.StudentProfiles.Select(x => new { x.Id, x.StudentNumber }).ToListAsync();
            var courses = await context.Courses.Select(x => new { x.Id, x.Name }).ToListAsync();

            context.CourseEnrolments.AddRange(
                new CourseEnrolment { StudentProfileId = studentIds.Single(x => x.StudentNumber == "VGC1001").Id, CourseId = courses.Single(x => x.Name == "Software Engineering Fundamentals").Id, EnrolDate = new DateTime(2026, 8, 20), Status = "Active" },
                new CourseEnrolment { StudentProfileId = studentIds.Single(x => x.StudentNumber == "VGC1002").Id, CourseId = courses.Single(x => x.Name == "Cloud Development").Id, EnrolDate = new DateTime(2026, 8, 22), Status = "Active" },
                new CourseEnrolment { StudentProfileId = studentIds.Single(x => x.StudentNumber == "VGC1003").Id, CourseId = courses.Single(x => x.Name == "Data Analytics").Id, EnrolDate = new DateTime(2026, 8, 25), Status = "Active" },
                new CourseEnrolment { StudentProfileId = studentIds.Single(x => x.StudentNumber == "VGC1004").Id, CourseId = courses.Single(x => x.Name == "Software Engineering Fundamentals").Id, EnrolDate = new DateTime(2026, 8, 24), Status = "Active" },
                new CourseEnrolment { StudentProfileId = studentIds.Single(x => x.StudentNumber == "VGC1005").Id, CourseId = courses.Single(x => x.Name == "Cloud Development").Id, EnrolDate = new DateTime(2026, 8, 21), Status = "Active" });
        }

        await context.SaveChangesAsync();

        if (!await context.Assignments.AnyAsync())
        {
            var softwareCourseId = await context.Courses.Where(x => x.Name == "Software Engineering Fundamentals").Select(x => x.Id).SingleAsync();
            var cloudCourseId = await context.Courses.Where(x => x.Name == "Cloud Development").Select(x => x.Id).SingleAsync();
            var analyticsCourseId = await context.Courses.Where(x => x.Name == "Data Analytics").Select(x => x.Id).SingleAsync();

            context.Assignments.AddRange(
                new Assignment { CourseId = softwareCourseId, Title = "MVC Fundamentals", MaxScore = 100, DueDate = new DateTime(2026, 11, 1) },
                new Assignment { CourseId = cloudCourseId, Title = "Azure Deployment", MaxScore = 100, DueDate = new DateTime(2026, 11, 8) },
                new Assignment { CourseId = analyticsCourseId, Title = "Data Visualization", MaxScore = 100, DueDate = new DateTime(2026, 11, 15) });
        }

        if (!await context.Exams.AnyAsync())
        {
            var softwareCourseId = await context.Courses.Where(x => x.Name == "Software Engineering Fundamentals").Select(x => x.Id).SingleAsync();
            var cloudCourseId = await context.Courses.Where(x => x.Name == "Cloud Development").Select(x => x.Id).SingleAsync();

            context.Exams.AddRange(
                new Exam { CourseId = softwareCourseId, Title = "Software Fundamentals Final", Date = new DateTime(2027, 1, 15), MaxScore = 100, ResultsReleased = true },
                new Exam { CourseId = cloudCourseId, Title = "Cloud Development Final", Date = new DateTime(2027, 2, 10), MaxScore = 100, ResultsReleased = false });
        }

        await context.SaveChangesAsync();

        if (!await context.AssignmentResults.AnyAsync())
        {
            var assignmentId = await context.Assignments.Where(x => x.Title == "MVC Fundamentals").Select(x => x.Id).SingleAsync();
            var johnId = await context.StudentProfiles.Where(x => x.StudentNumber == "VGC1001").Select(x => x.Id).SingleAsync();

            context.AssignmentResults.Add(new AssignmentResult
            {
                AssignmentId = assignmentId,
                StudentProfileId = johnId,
                Score = 89,
                Feedback = "Good work"
            });
        }

        if (!await context.ExamResults.AnyAsync())
        {
            var releasedExamId = await context.Exams.Where(x => x.Title == "Software Fundamentals Final").Select(x => x.Id).SingleAsync();
            var unreleasedExamId = await context.Exams.Where(x => x.Title == "Cloud Development Final").Select(x => x.Id).SingleAsync();
            var johnId = await context.StudentProfiles.Where(x => x.StudentNumber == "VGC1001").Select(x => x.Id).SingleAsync();
            var bernardId = await context.StudentProfiles.Where(x => x.StudentNumber == "VGC1002").Select(x => x.Id).SingleAsync();

            context.ExamResults.AddRange(
                new ExamResult { ExamId = releasedExamId, StudentProfileId = johnId, Score = 84, Grade = "B" },
                new ExamResult { ExamId = unreleasedExamId, StudentProfileId = bernardId, Score = 78, Grade = "C" });
        }

        var enrolmentIds = await context.CourseEnrolments.OrderBy(x => x.Id).Select(x => x.Id).ToListAsync();
        foreach (var enrolmentId in enrolmentIds)
        {
            if (!await context.AttendanceRecords.AnyAsync(x => x.CourseEnrolmentId == enrolmentId && x.WeekNumber == 1))
            {
                context.AttendanceRecords.Add(new AttendanceRecord
                {
                    CourseEnrolmentId = enrolmentId,
                    WeekNumber = 1,
                    AttendanceDate = new DateTime(2026, 9, 7),
                    Present = true
                });
            }
        }

        if (enrolmentIds.Count > 0 && !await context.AttendanceRecords.AnyAsync(x => x.CourseEnrolmentId == enrolmentIds[0] && x.WeekNumber == 2))
        {
            context.AttendanceRecords.Add(new AttendanceRecord
            {
                CourseEnrolmentId = enrolmentIds[0],
                WeekNumber = 2,
                AttendanceDate = new DateTime(2026, 9, 14),
                Present = true
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task<IdentityUser> EnsureUserAsync(UserManager<IdentityUser> userManager, string email, string password, string userName)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            return user;
        }

        user = new IdentityUser
        {
            UserName = userName,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(x => x.Description));
            throw new InvalidOperationException($"Failed to create user {email}: {errors}");
        }

        return user;
    }

    private static async Task EnsureRoleAsync(UserManager<IdentityUser> userManager, IdentityUser user, string roleName)
    {
        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            await userManager.AddToRoleAsync(user, roleName);
        }
    }

    private static async Task EnsureStudentProfileAsync(
        ApplicationDbContext context,
        string identityUserId,
        string name,
        string email,
        string phone,
        string address,
        DateTime dob,
        string studentNumber)
    {
        if (await context.StudentProfiles.AnyAsync(x => x.IdentityUserId == identityUserId))
        {
            return;
        }

        context.StudentProfiles.Add(new StudentProfile
        {
            IdentityUserId = identityUserId,
            Name = name,
            Email = email,
            Phone = phone,
            Address = address,
            DOB = dob,
            StudentNumber = studentNumber
        });
    }

    private static async Task EnsureFacultyProfileAsync(ApplicationDbContext context, string identityUserId, string name, string email, string phone)
    {
        if (await context.FacultyProfiles.AnyAsync(x => x.IdentityUserId == identityUserId))
        {
            return;
        }

        context.FacultyProfiles.Add(new FacultyProfile
        {
            IdentityUserId = identityUserId,
            Name = name,
            Email = email,
            Phone = phone
        });
    }
}
