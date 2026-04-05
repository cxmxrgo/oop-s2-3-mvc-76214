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
        return View(students);
    }
}
