using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using oop_s2_1_mvc_76214.Data;
using oop_s2_1_mvc_76214.Models;
using oop_s2_1_mvc_76214.Models.ViewModels;

namespace oop_s2_1_mvc_76214.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class StaffAssignmentsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var assignments = await context.CourseFacultyAssignments
            .OrderBy(x => x.Course!.Name)
            .Select(x => new StaffAssignmentListItemViewModel
            {
                Id = x.Id,
                CourseName = x.Course!.Name,
                BranchName = x.Course.Branch!.Name,
                FacultyName = x.FacultyProfile!.Name,
                FacultyEmail = x.FacultyProfile.Email
            })
            .ToListAsync();

        return View(assignments);
    }

    public async Task<IActionResult> Create()
    {
        var model = new StaffAssignmentUpsertViewModel();
        await PopulateSelectionsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffAssignmentUpsertViewModel model)
    {
        if (await context.CourseFacultyAssignments.AnyAsync(x => x.CourseId == model.CourseId && x.FacultyProfileId == model.FacultyProfileId))
        {
            ModelState.AddModelError(string.Empty, "This faculty member is already assigned to the selected course.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateSelectionsAsync(model);
            return View(model);
        }

        context.CourseFacultyAssignments.Add(new CourseFacultyAssignment
        {
            CourseId = model.CourseId,
            FacultyProfileId = model.FacultyProfileId
        });

        await context.SaveChangesAsync();
        TempData["StatusMessage"] = "Staff assignment created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var assignment = await context.CourseFacultyAssignments.SingleOrDefaultAsync(x => x.Id == id);
        if (assignment is null)
        {
            return NotFound();
        }

        context.CourseFacultyAssignments.Remove(assignment);
        await context.SaveChangesAsync();
        TempData["StatusMessage"] = "Staff assignment removed.";

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateSelectionsAsync(StaffAssignmentUpsertViewModel model)
    {
        model.Courses = await context.Courses
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = $"{x.Name} - {x.Branch!.Name}"
            })
            .ToListAsync();

        model.FacultyMembers = await context.FacultyProfiles
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = $"{x.Name} ({x.Email})"
            })
            .ToListAsync();
    }
}
