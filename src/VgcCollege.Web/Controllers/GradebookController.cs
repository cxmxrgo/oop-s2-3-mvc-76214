using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oop_s2_1_mvc_76214.Data;
using oop_s2_1_mvc_76214.Models;
using oop_s2_1_mvc_76214.Models.ViewModels;

namespace oop_s2_1_mvc_76214.Controllers;

[Authorize(Roles = $"{RoleNames.Administrator},{RoleNames.Faculty}")]
public class GradebookController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var courses = await GetAccessibleCoursesQuery()
            .Select(x => new GradebookCourseViewModel
            {
                CourseId = x.Id,
                CourseName = x.Name,
                BranchName = x.Branch!.Name,
                StudentCount = x.CourseEnrolments.Count,
                AssignmentCount = x.Assignments.Count,
                ExamCount = x.Exams.Count
            })
            .OrderBy(x => x.CourseName)
            .ToListAsync();

        return View(courses);
    }

    public async Task<IActionResult> Assignments(int courseId)
    {
        if (!await HasCourseAccessAsync(courseId))
        {
            return Forbid();
        }

        ViewBag.CourseId = courseId;
        ViewBag.CourseName = await context.Courses.Where(x => x.Id == courseId).Select(x => x.Name).SingleAsync();

        var assignments = await context.Assignments
            .Where(x => x.CourseId == courseId)
            .OrderBy(x => x.DueDate)
            .Select(x => new AssignmentListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                MaxScore = x.MaxScore,
                DueDate = x.DueDate
            })
            .ToListAsync();

        return View(assignments);
    }

    public async Task<IActionResult> CreateAssignment(int courseId)
    {
        if (!await HasCourseAccessAsync(courseId))
        {
            return Forbid();
        }

        var model = new AssignmentEditViewModel
        {
            CourseId = courseId,
            CourseName = await context.Courses.Where(x => x.Id == courseId).Select(x => x.Name).SingleAsync(),
            DueDate = DateTime.Today
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAssignment(AssignmentEditViewModel model)
    {
        if (!await HasCourseAccessAsync(model.CourseId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            model.CourseName = await context.Courses.Where(x => x.Id == model.CourseId).Select(x => x.Name).SingleAsync();
            return View(model);
        }

        context.Assignments.Add(new Assignment
        {
            CourseId = model.CourseId,
            Title = model.Title.Trim(),
            MaxScore = model.MaxScore,
            DueDate = model.DueDate
        });

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Assignments), new { courseId = model.CourseId });
    }

    public async Task<IActionResult> EditAssignment(int id)
    {
        var assignment = await context.Assignments
            .Where(x => x.Id == id)
            .Select(x => new AssignmentEditViewModel
            {
                Id = x.Id,
                CourseId = x.CourseId,
                CourseName = x.Course!.Name,
                Title = x.Title,
                MaxScore = x.MaxScore,
                DueDate = x.DueDate
            })
            .SingleOrDefaultAsync();

        if (assignment is null)
        {
            return NotFound();
        }

        if (!await HasCourseAccessAsync(assignment.CourseId))
        {
            return Forbid();
        }

        return View(assignment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAssignment(AssignmentEditViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return NotFound();
        }

        if (!await HasCourseAccessAsync(model.CourseId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            model.CourseName = await context.Courses.Where(x => x.Id == model.CourseId).Select(x => x.Name).SingleAsync();
            return View(model);
        }

        var assignment = await context.Assignments.SingleOrDefaultAsync(x => x.Id == model.Id.Value);
        if (assignment is null)
        {
            return NotFound();
        }

        assignment.Title = model.Title.Trim();
        assignment.MaxScore = model.MaxScore;
        assignment.DueDate = model.DueDate;

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Assignments), new { courseId = model.CourseId });
    }

    public async Task<IActionResult> AssignmentResults(int assignmentId)
    {
        var assignment = await context.Assignments
            .Where(x => x.Id == assignmentId)
            .Select(x => new
            {
                x.Id,
                x.CourseId,
                x.Title,
                x.MaxScore,
                CourseName = x.Course!.Name
            })
            .SingleOrDefaultAsync();

        if (assignment is null)
        {
            return NotFound();
        }

        if (!await HasCourseAccessAsync(assignment.CourseId))
        {
            return Forbid();
        }

        var students = await context.CourseEnrolments
            .Where(x => x.CourseId == assignment.CourseId)
            .OrderBy(x => x.StudentProfile!.Name)
            .Select(x => new AssignmentResultRowViewModel
            {
                StudentProfileId = x.StudentProfileId,
                StudentName = x.StudentProfile!.Name,
                StudentNumber = x.StudentProfile.StudentNumber,
                Score = x.StudentProfile.AssignmentResults
                    .Where(r => r.AssignmentId == assignment.Id)
                    .Select(r => (decimal?)r.Score)
                    .FirstOrDefault(),
                Feedback = x.StudentProfile.AssignmentResults
                    .Where(r => r.AssignmentId == assignment.Id)
                    .Select(r => r.Feedback)
                    .FirstOrDefault() ?? string.Empty
            })
            .ToListAsync();

        return View(new AssignmentResultsViewModel
        {
            AssignmentId = assignment.Id,
            CourseId = assignment.CourseId,
            CourseName = assignment.CourseName,
            AssignmentTitle = assignment.Title,
            MaxScore = assignment.MaxScore,
            Students = students
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignmentResults(AssignmentResultsViewModel model)
    {
        if (!await HasCourseAccessAsync(model.CourseId))
        {
            return Forbid();
        }

        var assignment = await context.Assignments.SingleOrDefaultAsync(x => x.Id == model.AssignmentId && x.CourseId == model.CourseId);
        if (assignment is null)
        {
            return NotFound();
        }

        for (var i = 0; i < model.Students.Count; i++)
        {
            var row = model.Students[i];
            if (row.Score.HasValue && (row.Score.Value < 0 || row.Score.Value > assignment.MaxScore))
            {
                ModelState.AddModelError($"Students[{i}].Score", $"Score must be between 0 and {assignment.MaxScore:0.##}.");
            }
        }

        if (!ModelState.IsValid)
        {
            model.CourseName = await context.Courses.Where(x => x.Id == model.CourseId).Select(x => x.Name).SingleAsync();
            model.AssignmentTitle = assignment.Title;
            model.MaxScore = assignment.MaxScore;
            return View(model);
        }

        foreach (var row in model.Students)
        {
            var existing = await context.AssignmentResults
                .SingleOrDefaultAsync(x => x.AssignmentId == model.AssignmentId && x.StudentProfileId == row.StudentProfileId);

            if (!row.Score.HasValue)
            {
                if (existing is not null)
                {
                    context.AssignmentResults.Remove(existing);
                }
                continue;
            }

            if (existing is null)
            {
                context.AssignmentResults.Add(new AssignmentResult
                {
                    AssignmentId = model.AssignmentId,
                    StudentProfileId = row.StudentProfileId,
                    Score = row.Score.Value,
                    Feedback = row.Feedback.Trim()
                });
            }
            else
            {
                existing.Score = row.Score.Value;
                existing.Feedback = row.Feedback.Trim();
            }
        }

        await context.SaveChangesAsync();
        TempData["StatusMessage"] = "Assignment results saved.";
        return RedirectToAction(nameof(AssignmentResults), new { assignmentId = model.AssignmentId });
    }

    public async Task<IActionResult> Exams(int courseId)
    {
        if (!await HasCourseAccessAsync(courseId))
        {
            return Forbid();
        }

        ViewBag.CourseId = courseId;
        ViewBag.CourseName = await context.Courses.Where(x => x.Id == courseId).Select(x => x.Name).SingleAsync();
        ViewBag.IsAdmin = User.IsInRole(RoleNames.Administrator);

        var exams = await context.Exams
            .Where(x => x.CourseId == courseId)
            .OrderBy(x => x.Date)
            .Select(x => new ExamListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                MaxScore = x.MaxScore,
                Date = x.Date,
                ResultsReleased = x.ResultsReleased
            })
            .ToListAsync();

        return View(exams);
    }

    public async Task<IActionResult> CreateExam(int courseId)
    {
        if (!await HasCourseAccessAsync(courseId))
        {
            return Forbid();
        }

        var model = new ExamEditViewModel
        {
            CourseId = courseId,
            CourseName = await context.Courses.Where(x => x.Id == courseId).Select(x => x.Name).SingleAsync(),
            Date = DateTime.Today
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateExam(ExamEditViewModel model)
    {
        if (!await HasCourseAccessAsync(model.CourseId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            model.CourseName = await context.Courses.Where(x => x.Id == model.CourseId).Select(x => x.Name).SingleAsync();
            return View(model);
        }

        context.Exams.Add(new Exam
        {
            CourseId = model.CourseId,
            Title = model.Title.Trim(),
            Date = model.Date,
            MaxScore = model.MaxScore,
            ResultsReleased = false
        });

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Exams), new { courseId = model.CourseId });
    }

    public async Task<IActionResult> EditExam(int id)
    {
        var exam = await context.Exams
            .Where(x => x.Id == id)
            .Select(x => new ExamEditViewModel
            {
                Id = x.Id,
                CourseId = x.CourseId,
                CourseName = x.Course!.Name,
                Title = x.Title,
                Date = x.Date,
                MaxScore = x.MaxScore,
                ResultsReleased = x.ResultsReleased
            })
            .SingleOrDefaultAsync();

        if (exam is null)
        {
            return NotFound();
        }

        if (!await HasCourseAccessAsync(exam.CourseId))
        {
            return Forbid();
        }

        return View(exam);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditExam(ExamEditViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return NotFound();
        }

        if (!await HasCourseAccessAsync(model.CourseId))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            model.CourseName = await context.Courses.Where(x => x.Id == model.CourseId).Select(x => x.Name).SingleAsync();
            return View(model);
        }

        var exam = await context.Exams.SingleOrDefaultAsync(x => x.Id == model.Id.Value);
        if (exam is null)
        {
            return NotFound();
        }

        exam.Title = model.Title.Trim();
        exam.Date = model.Date;
        exam.MaxScore = model.MaxScore;

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Exams), new { courseId = model.CourseId });
    }

    public async Task<IActionResult> ExamResults(int examId)
    {
        var exam = await context.Exams
            .Where(x => x.Id == examId)
            .Select(x => new
            {
                x.Id,
                x.CourseId,
                x.Title,
                x.MaxScore,
                x.ResultsReleased,
                CourseName = x.Course!.Name
            })
            .SingleOrDefaultAsync();

        if (exam is null)
        {
            return NotFound();
        }

        if (!await HasCourseAccessAsync(exam.CourseId))
        {
            return Forbid();
        }

        var students = await context.CourseEnrolments
            .Where(x => x.CourseId == exam.CourseId)
            .OrderBy(x => x.StudentProfile!.Name)
            .Select(x => new ExamResultRowViewModel
            {
                StudentProfileId = x.StudentProfileId,
                StudentName = x.StudentProfile!.Name,
                StudentNumber = x.StudentProfile.StudentNumber,
                Score = x.StudentProfile.ExamResults
                    .Where(r => r.ExamId == exam.Id)
                    .Select(r => (decimal?)r.Score)
                    .FirstOrDefault(),
                Grade = x.StudentProfile.ExamResults
                    .Where(r => r.ExamId == exam.Id)
                    .Select(r => r.Grade)
                    .FirstOrDefault() ?? string.Empty
            })
            .ToListAsync();

        return View(new ExamResultsViewModel
        {
            ExamId = exam.Id,
            CourseId = exam.CourseId,
            CourseName = exam.CourseName,
            ExamTitle = exam.Title,
            MaxScore = exam.MaxScore,
            ResultsReleased = exam.ResultsReleased,
            Students = students
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExamResults(ExamResultsViewModel model)
    {
        if (!await HasCourseAccessAsync(model.CourseId))
        {
            return Forbid();
        }

        var exam = await context.Exams.SingleOrDefaultAsync(x => x.Id == model.ExamId && x.CourseId == model.CourseId);
        if (exam is null)
        {
            return NotFound();
        }

        for (var i = 0; i < model.Students.Count; i++)
        {
            var row = model.Students[i];
            if (row.Score.HasValue && (row.Score.Value < 0 || row.Score.Value > exam.MaxScore))
            {
                ModelState.AddModelError($"Students[{i}].Score", $"Score must be between 0 and {exam.MaxScore:0.##}.");
            }
        }

        if (!ModelState.IsValid)
        {
            model.CourseName = await context.Courses.Where(x => x.Id == model.CourseId).Select(x => x.Name).SingleAsync();
            model.ExamTitle = exam.Title;
            model.MaxScore = exam.MaxScore;
            model.ResultsReleased = exam.ResultsReleased;
            return View(model);
        }

        foreach (var row in model.Students)
        {
            var existing = await context.ExamResults
                .SingleOrDefaultAsync(x => x.ExamId == model.ExamId && x.StudentProfileId == row.StudentProfileId);

            if (!row.Score.HasValue)
            {
                if (existing is not null)
                {
                    context.ExamResults.Remove(existing);
                }
                continue;
            }

            if (existing is null)
            {
                context.ExamResults.Add(new ExamResult
                {
                    ExamId = model.ExamId,
                    StudentProfileId = row.StudentProfileId,
                    Score = row.Score.Value,
                    Grade = row.Grade.Trim()
                });
            }
            else
            {
                existing.Score = row.Score.Value;
                existing.Grade = row.Grade.Trim();
            }
        }

        await context.SaveChangesAsync();
        TempData["StatusMessage"] = "Exam results saved.";
        return RedirectToAction(nameof(ExamResults), new { examId = model.ExamId });
    }

    [Authorize(Roles = RoleNames.Administrator)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRelease(int examId)
    {
        var exam = await context.Exams.SingleOrDefaultAsync(x => x.Id == examId);
        if (exam is null)
        {
            return NotFound();
        }

        exam.ResultsReleased = !exam.ResultsReleased;
        await context.SaveChangesAsync();

        TempData["StatusMessage"] = exam.ResultsReleased
            ? "Exam results released to students."
            : "Exam results are no longer visible to students.";

        return RedirectToAction(nameof(Exams), new { courseId = exam.CourseId });
    }

    private IQueryable<Course> GetAccessibleCoursesQuery()
    {
        if (User.IsInRole(RoleNames.Administrator))
        {
            return context.Courses;
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return context.Courses.Where(x => false);
        }

        return context.CourseFacultyAssignments
            .Where(x => x.FacultyProfile!.IdentityUserId == userId)
            .Select(x => x.Course!);
    }

    private async Task<bool> HasCourseAccessAsync(int courseId)
    {
        if (User.IsInRole(RoleNames.Administrator))
        {
            return await context.Courses.AnyAsync(x => x.Id == courseId);
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        return await context.CourseFacultyAssignments.AnyAsync(x =>
            x.CourseId == courseId && x.FacultyProfile!.IdentityUserId == userId);
    }
}
