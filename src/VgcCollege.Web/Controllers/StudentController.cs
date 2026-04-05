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

    public async Task<IActionResult> Attendance()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var studentProfileId = await context.StudentProfiles
            .Where(x => x.IdentityUserId == userId)
            .Select(x => (int?)x.Id)
            .SingleOrDefaultAsync();

        if (!studentProfileId.HasValue)
        {
            return Forbid();
        }

        var attendance = await context.AttendanceRecords
            .Where(x => x.CourseEnrolment!.StudentProfileId == studentProfileId.Value)
            .OrderBy(x => x.CourseEnrolment!.Course!.Name)
            .ThenBy(x => x.WeekNumber)
            .Select(x => new StudentAttendanceRecordViewModel
            {
                CourseName = x.CourseEnrolment!.Course!.Name,
                WeekNumber = x.WeekNumber,
                AttendanceDate = x.AttendanceDate,
                Present = x.Present
            })
            .ToListAsync();

        return View(attendance);
    }

    public async Task<IActionResult> Results()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var student = await context.StudentProfiles
            .Where(x => x.IdentityUserId == userId)
            .Select(x => new StudentResultsViewModel
            {
                StudentName = x.Name,
                AssignmentResults = x.AssignmentResults
                    .OrderBy(r => r.Assignment!.Course!.Name)
                    .ThenBy(r => r.Assignment!.Title)
                    .Select(r => new StudentAssignmentResultViewModel
                    {
                        CourseName = r.Assignment!.Course!.Name,
                        AssignmentTitle = r.Assignment.Title,
                        Score = r.Score,
                        MaxScore = r.Assignment.MaxScore,
                        Feedback = r.Feedback
                    })
                    .ToList(),
                ReleasedExamResults = x.ExamResults
                    .Where(r => r.Exam!.ResultsReleased)
                    .OrderBy(r => r.Exam!.Course!.Name)
                    .ThenBy(r => r.Exam!.Title)
                    .Select(r => new StudentExamResultViewModel
                    {
                        CourseName = r.Exam!.Course!.Name,
                        ExamTitle = r.Exam.Title,
                        Score = r.Score,
                        Grade = r.Grade
                    })
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
