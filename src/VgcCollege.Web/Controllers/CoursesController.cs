using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using oop_s2_1_mvc_76214.Data;
using oop_s2_1_mvc_76214.Models;
using oop_s2_1_mvc_76214.Models.ViewModels;

namespace oop_s2_1_mvc_76214.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class CoursesController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var courses = await context.Courses
            .OrderBy(x => x.Name)
            .Select(x => new CourseListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                BranchName = x.Branch!.Name,
                StartDate = x.StartDate,
                EndDate = x.EndDate
            })
            .ToListAsync();

        return View(courses);
    }

    public async Task<IActionResult> Create()
    {
        var model = new CourseUpsertViewModel
        {
            StartDate = DateTime.Today,
            EndDate = DateTime.Today
        };

        await PopulateBranchesAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CourseUpsertViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateBranchesAsync(model);
            return View(model);
        }

        context.Courses.Add(new Course
        {
            Name = model.Name.Trim(),
            BranchId = model.BranchId,
            StartDate = model.StartDate,
            EndDate = model.EndDate
        });

        await context.SaveChangesAsync();
        TempData["StatusMessage"] = "Course created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var course = await context.Courses
            .Where(x => x.Id == id)
            .Select(x => new CourseUpsertViewModel
            {
                Id = x.Id,
                Name = x.Name,
                BranchId = x.BranchId,
                StartDate = x.StartDate,
                EndDate = x.EndDate
            })
            .SingleOrDefaultAsync();

        if (course is null)
        {
            return NotFound();
        }

        await PopulateBranchesAsync(course);
        return View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CourseUpsertViewModel model)
    {
        if (!model.Id.HasValue)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await PopulateBranchesAsync(model);
            return View(model);
        }

        var course = await context.Courses.SingleOrDefaultAsync(x => x.Id == model.Id.Value);
        if (course is null)
        {
            return NotFound();
        }

        course.Name = model.Name.Trim();
        course.BranchId = model.BranchId;
        course.StartDate = model.StartDate;
        course.EndDate = model.EndDate;

        await context.SaveChangesAsync();
        TempData["StatusMessage"] = "Course updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await context.Courses.SingleOrDefaultAsync(x => x.Id == id);
        if (course is null)
        {
            return NotFound();
        }

        context.Courses.Remove(course);
        await context.SaveChangesAsync();
        TempData["StatusMessage"] = "Course deleted.";

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateBranchesAsync(CourseUpsertViewModel model)
    {
        model.Branches = await context.Branches
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToListAsync();
    }
}
