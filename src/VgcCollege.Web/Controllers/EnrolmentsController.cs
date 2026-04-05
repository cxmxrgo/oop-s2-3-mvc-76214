using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using oop_s2_1_mvc_76214.Data;
using oop_s2_1_mvc_76214.Models;
using oop_s2_1_mvc_76214.Models.ViewModels;

namespace oop_s2_1_mvc_76214.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class EnrolmentsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var enrolments = await context.CourseEnrolments
            .Select(x => new EnrolmentListItemViewModel
            {
                Id = x.Id,
                StudentName = x.StudentProfile!.Name,
                StudentNumber = x.StudentProfile.StudentNumber,
                CourseName = x.Course!.Name,
                BranchName = x.Course.Branch!.Name,
                EnrolDate = x.EnrolDate,
                Status = x.Status
            })
            .OrderBy(x => x.StudentName)
            .ThenBy(x => x.CourseName)
            .ToListAsync();

        return View(enrolments);
    }

    public async Task<IActionResult> Create()
    {
        var model = new EnrolmentUpsertViewModel();
        await PopulateSelectionsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EnrolmentUpsertViewModel model)
    {
        if (await context.CourseEnrolments.AnyAsync(x => x.StudentProfileId == model.StudentProfileId && x.CourseId == model.CourseId))
        {
            ModelState.AddModelError(string.Empty, "This student is already enrolled in the selected course.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(model);
            return View(model);
        }

        var enrolment = new CourseEnrolment
        {
            StudentProfileId = model.StudentProfileId,
            CourseId = model.CourseId,
            EnrolDate = model.EnrolDate,
            Status = model.Status.Trim()
        };

        context.CourseEnrolments.Add(enrolment);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var enrolment = await context.CourseEnrolments
            .Where(x => x.Id == id)
            .Select(x => new EnrolmentUpsertViewModel
            {
                Id = x.Id,
                StudentProfileId = x.StudentProfileId,
                CourseId = x.CourseId,
                EnrolDate = x.EnrolDate,
                Status = x.Status
            })
            .SingleOrDefaultAsync();

        if (enrolment is null)
        {
            return NotFound();
        }

        await PopulateSelectionsAsync(enrolment);
        return View(enrolment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EnrolmentUpsertViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return NotFound();
        }

        if (await context.CourseEnrolments.AnyAsync(x => x.Id != model.Id.Value && x.StudentProfileId == model.StudentProfileId && x.CourseId == model.CourseId))
        {
            ModelState.AddModelError(string.Empty, "This student is already enrolled in the selected course.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(model);
            return View(model);
        }

        var enrolment = await context.CourseEnrolments.SingleOrDefaultAsync(x => x.Id == model.Id.Value);
        if (enrolment is null)
        {
            return NotFound();
        }

        enrolment.StudentProfileId = model.StudentProfileId;
        enrolment.CourseId = model.CourseId;
        enrolment.EnrolDate = model.EnrolDate;
        enrolment.Status = model.Status.Trim();

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var enrolment = await context.CourseEnrolments
            .Where(x => x.Id == id)
            .Select(x => new EnrolmentListItemViewModel
            {
                Id = x.Id,
                StudentName = x.StudentProfile!.Name,
                StudentNumber = x.StudentProfile.StudentNumber,
                CourseName = x.Course!.Name,
                BranchName = x.Course.Branch!.Name,
                EnrolDate = x.EnrolDate,
                Status = x.Status
            })
            .SingleOrDefaultAsync();

        if (enrolment is null)
        {
            return NotFound();
        }

        return View(enrolment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var enrolment = await context.CourseEnrolments.SingleOrDefaultAsync(x => x.Id == id);
        if (enrolment is null)
        {
            return NotFound();
        }

        context.CourseEnrolments.Remove(enrolment);
        await context.SaveChangesAsync();

        TempData["StatusMessage"] = "Student enrolment deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateSelectionsAsync(EnrolmentUpsertViewModel model)
    {
        model.Students = await context.StudentProfiles
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = $"{x.Name} ({x.StudentNumber})"
            })
            .ToListAsync();

        model.Courses = await context.Courses
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = $"{x.Name} - {x.Branch!.Name}"
            })
            .ToListAsync();
    }
}
