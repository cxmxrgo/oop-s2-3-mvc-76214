using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oop_s2_1_mvc_76214.Data;
using oop_s2_1_mvc_76214.Models;

namespace oop_s2_1_mvc_76214.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class BranchesController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var branches = await context.Branches
            .OrderBy(x => x.Name)
            .ToListAsync();

        return View(branches);
    }

    public IActionResult Create()
    {
        return View(new Branch());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Branch model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        context.Branches.Add(model);
        await context.SaveChangesAsync();
        TempData["StatusMessage"] = "Branch created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var branch = await context.Branches.SingleOrDefaultAsync(x => x.Id == id);
        if (branch is null)
        {
            return NotFound();
        }

        return View(branch);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Branch model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var branch = await context.Branches.SingleOrDefaultAsync(x => x.Id == model.Id);
        if (branch is null)
        {
            return NotFound();
        }

        branch.Name = model.Name;
        branch.Address = model.Address;
        await context.SaveChangesAsync();

        TempData["StatusMessage"] = "Branch updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var branch = await context.Branches.SingleOrDefaultAsync(x => x.Id == id);
        if (branch is null)
        {
            return NotFound();
        }

        context.Branches.Remove(branch);

        try
        {
            await context.SaveChangesAsync();
            TempData["StatusMessage"] = "Branch deleted.";
        }
        catch (DbUpdateException)
        {
            TempData["StatusMessage"] = "Branch cannot be deleted because it is linked to existing courses.";
        }

        return RedirectToAction(nameof(Index));
    }
}
