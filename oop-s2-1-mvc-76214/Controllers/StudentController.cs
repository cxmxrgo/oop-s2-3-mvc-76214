using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oop_s2_1_mvc_76214.Data;
using oop_s2_1_mvc_76214.Models;
using oop_s2_1_mvc_76214.Models.ViewModels;

namespace oop_s2_1_mvc_76214.Controllers;

[Authorize(Roles = RoleNames.Student)]
public class StudentController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var student = await context.StudentProfiles
            .Where(x => x.IdentityUserId == userId)
            .Select(x => new StudentDashboardViewModel
            {
                Name = x.Name,
                StudentNumber = x.StudentNumber,
                Email = x.Email,
                Phone = x.Phone,
                EnrolledCourses = x.CourseEnrolments.Select(e => e.Course!.Name).OrderBy(n => n).ToList(),
                ReleasedExamResults = x.ExamResults
                    .Where(er => er.Exam!.ResultsReleased)
                    .Select(er => new StudentExamResultViewModel
                    {
                        CourseName = er.Exam!.Course!.Name,
                        ExamTitle = er.Exam.Title,
                        Score = er.Score,
                        Grade = er.Grade
                    })
                    .OrderBy(er => er.CourseName)
                    .ThenBy(er => er.ExamTitle)
                    .ToList()
            })
            .SingleOrDefaultAsync();

        if (student is null)
        {
            return Forbid();
        }

        return View(student);
    }
}
