using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oop_s2_1_mvc_76214.Data;
using oop_s2_1_mvc_76214.Models;

namespace oop_s2_1_mvc_76214.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class StudentProfilesController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var students = await context.StudentProfiles
            .OrderBy(x => x.Name)
            .ToListAsync();

        return View(students);
    }

    public async Task<IActionResult> Details(int id)
    {
        var student = await context.StudentProfiles.SingleOrDefaultAsync(x => x.Id == id);
        if (student is null)
        {
            return NotFound();
        }

        return View(student);
    }

    public IActionResult Create()
    {
        return View(new StudentProfile { DOB = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StudentProfile model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await context.StudentProfiles.AnyAsync(x => x.StudentNumber == model.StudentNumber))
        {
            ModelState.AddModelError(nameof(StudentProfile.StudentNumber), "Student number already exists.");
            return View(model);
        }

        if (await context.StudentProfiles.AnyAsync(x => x.IdentityUserId == model.IdentityUserId))
        {
            ModelState.AddModelError(nameof(StudentProfile.IdentityUserId), "Identity user is already linked to a student profile.");
            return View(model);
        }

        context.StudentProfiles.Add(model);
        await context.SaveChangesAsync();
        TempData["StatusMessage"] = "Student profile created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var student = await context.StudentProfiles.SingleOrDefaultAsync(x => x.Id == id);
        if (student is null)
        {
            return NotFound();
        }

        return View(student);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(StudentProfile model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await context.StudentProfiles.AnyAsync(x => x.Id != model.Id && x.StudentNumber == model.StudentNumber))
        {
            ModelState.AddModelError(nameof(StudentProfile.StudentNumber), "Student number already exists.");
            return View(model);
        }

        if (await context.StudentProfiles.AnyAsync(x => x.Id != model.Id && x.IdentityUserId == model.IdentityUserId))
        {
            ModelState.AddModelError(nameof(StudentProfile.IdentityUserId), "Identity user is already linked to another student profile.");
            return View(model);
        }

        var student = await context.StudentProfiles.SingleOrDefaultAsync(x => x.Id == model.Id);
        if (student is null)
        {
            return NotFound();
        }

        student.IdentityUserId = model.IdentityUserId;
        student.Name = model.Name;
        student.Email = model.Email;
        student.Phone = model.Phone;
        student.Address = model.Address;
        student.DOB = model.DOB;
        student.StudentNumber = model.StudentNumber;

        await context.SaveChangesAsync();
        TempData["StatusMessage"] = "Student profile updated.";
        return RedirectToAction(nameof(Index));
    }
}
