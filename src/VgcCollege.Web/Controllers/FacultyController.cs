using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oop_s2_1_mvc_76214.Data;
using oop_s2_1_mvc_76214.Models;
using oop_s2_1_mvc_76214.Models.ViewModels;

namespace oop_s2_1_mvc_76214.Controllers;

[Authorize(Roles = RoleNames.Faculty)]
public class FacultyController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var facultyProfileId = await context.FacultyProfiles
            .Where(x => x.IdentityUserId == userId)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();

        if (facultyProfileId == 0)
        {
            return Forbid();
        }

        var model = await context.CourseFacultyAssignments
            .Where(x => x.FacultyProfileId == facultyProfileId)
            .Select(x => new FacultyCourseViewModel
            {
                CourseId = x.CourseId,
                CourseName = x.Course!.Name,
                BranchName = x.Course.Branch!.Name,
                StudentCount = x.Course.CourseEnrolments.Count
            })
            .OrderBy(x => x.CourseName)
            .ToListAsync();

        return View(model);
    }

    public async Task<IActionResult> Students(int courseId)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var facultyProfileId = await context.FacultyProfiles
            .Where(x => x.IdentityUserId == userId)
            .Select(x => x.Id)
            .SingleOrDefaultAsync();

        var hasAccess = await context.CourseFacultyAssignments
            .AnyAsync(x => x.CourseId == courseId && x.FacultyProfileId == facultyProfileId);

        if (!hasAccess)
        {
            return Forbid();
        }

        var students = await context.CourseEnrolments
            .Where(x => x.CourseId == courseId)
            .Select(x => new FacultyStudentViewModel
            {
                StudentName = x.StudentProfile!.Name,
                StudentNumber = x.StudentProfile.StudentNumber,
                Email = x.StudentProfile.Email,
                Phone = x.StudentProfile.Phone
            })
            .OrderBy(x => x.StudentName)
            .ToListAsync();

        ViewBag.CourseName = await context.Courses.Where(x => x.Id == courseId).Select(x => x.Name).SingleAsync();
        ViewBag.CourseId = courseId;
        return View(students);
    }

    public async Task<IActionResult> Attendance(int courseId, int weekNumber = 1)
    {
        var facultyProfileId = await GetCurrentFacultyProfileIdAsync();
        if (!facultyProfileId.HasValue)
        {
            return Challenge();
        }

        var hasAccess = await context.CourseFacultyAssignments
            .AnyAsync(x => x.CourseId == courseId && x.FacultyProfileId == facultyProfileId.Value);

        if (!hasAccess)
        {
            return Forbid();
        }

        var courseName = await context.Courses.Where(x => x.Id == courseId).Select(x => x.Name).SingleOrDefaultAsync();
        if (courseName is null)
        {
            return NotFound();
        }

        var week = Math.Clamp(weekNumber, 1, 52);

        var model = new FacultyAttendanceViewModel
        {
            CourseId = courseId,
            CourseName = courseName,
            WeekNumber = week,
            AttendanceDate = DateTime.Today,
            Students = await context.CourseEnrolments
                .Where(x => x.CourseId == courseId)
                .OrderBy(x => x.StudentProfile!.Name)
                .Select(x => new FacultyAttendanceStudentItemViewModel
                {
                    CourseEnrolmentId = x.Id,
                    StudentName = x.StudentProfile!.Name,
                    StudentNumber = x.StudentProfile.StudentNumber,
                    Present = x.AttendanceRecords
                        .Where(ar => ar.WeekNumber == week)
                        .Select(ar => ar.Present)
                        .FirstOrDefault()
                })
                .ToListAsync()
        };

        var existingDate = await context.AttendanceRecords
            .Where(x => x.CourseEnrolment!.CourseId == courseId && x.WeekNumber == week)
            .Select(x => x.AttendanceDate)
            .FirstOrDefaultAsync();

        if (existingDate != default)
        {
            model.AttendanceDate = existingDate;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Attendance(FacultyAttendanceViewModel model)
    {
        model.Students ??= new List<FacultyAttendanceStudentItemViewModel>();

        if (!ModelState.IsValid)
        {
            return await Attendance(model.CourseId, model.WeekNumber);
        }

        var facultyProfileId = await GetCurrentFacultyProfileIdAsync();
        if (!facultyProfileId.HasValue)
        {
            return Challenge();
        }

        var hasAccess = await context.CourseFacultyAssignments
            .AnyAsync(x => x.CourseId == model.CourseId && x.FacultyProfileId == facultyProfileId.Value);

        if (!hasAccess)
        {
            return Forbid();
        }

        var validEnrolmentIds = await context.CourseEnrolments
            .Where(x => x.CourseId == model.CourseId)
            .Select(x => x.Id)
            .ToListAsync();

        foreach (var student in model.Students.Where(x => validEnrolmentIds.Contains(x.CourseEnrolmentId)))
        {
            var existingRecord = await context.AttendanceRecords
                .SingleOrDefaultAsync(x => x.CourseEnrolmentId == student.CourseEnrolmentId && x.WeekNumber == model.WeekNumber);

            if (existingRecord is null)
            {
                context.AttendanceRecords.Add(new AttendanceRecord
                {
                    CourseEnrolmentId = student.CourseEnrolmentId,
                    WeekNumber = model.WeekNumber,
                    AttendanceDate = model.AttendanceDate,
                    Present = student.Present
                });
            }
            else
            {
                existingRecord.AttendanceDate = model.AttendanceDate;
                existingRecord.Present = student.Present;
            }
        }

        await context.SaveChangesAsync();
        TempData["StatusMessage"] = "Attendance saved.";
        return RedirectToAction(nameof(Attendance), new { courseId = model.CourseId, weekNumber = model.WeekNumber });
    }

    private async Task<int?> GetCurrentFacultyProfileIdAsync()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var facultyProfileId = await context.FacultyProfiles
            .Where(x => x.IdentityUserId == userId)
            .Select(x => (int?)x.Id)
            .SingleOrDefaultAsync();

        return facultyProfileId;
    }
}
