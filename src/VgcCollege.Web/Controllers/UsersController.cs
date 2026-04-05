using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using oop_s2_1_mvc_76214.Models;
using oop_s2_1_mvc_76214.Models.ViewModels;

namespace oop_s2_1_mvc_76214.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var users = userManager.Users.OrderBy(x => x.Email).ToList();
        var model = new List<UserListItemViewModel>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            model.Add(new UserListItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                Roles = roles.OrderBy(x => x).ToList()
            });
        }

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        var model = new UserCreateViewModel();
        await PopulateRolesAsync(model.Roles);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (!await roleManager.RoleExistsAsync(model.SelectedRole))
        {
            ModelState.AddModelError(nameof(UserCreateViewModel.SelectedRole), "Selected role is invalid.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateRolesAsync(model.Roles);
            return View(model);
        }

        var user = new IdentityUser
        {
            UserName = model.Email,
            Email = model.Email,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await PopulateRolesAsync(model.Roles);
            return View(model);
        }

        await userManager.AddToRoleAsync(user, model.SelectedRole);
        TempData["StatusMessage"] = "User created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> EditRole(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await userManager.GetRolesAsync(user);
        var model = new UserRoleEditViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? user.UserName ?? string.Empty,
            SelectedRole = roles.FirstOrDefault() ?? string.Empty
        };

        await PopulateRolesAsync(model.Roles);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRole(UserRoleEditViewModel model)
    {
        var user = await userManager.FindByIdAsync(model.UserId);
        if (user is null)
        {
            return NotFound();
        }

        if (!await roleManager.RoleExistsAsync(model.SelectedRole))
        {
            ModelState.AddModelError(nameof(UserRoleEditViewModel.SelectedRole), "Selected role is invalid.");
        }

        if (!ModelState.IsValid)
        {
            model.Email = user.Email ?? user.UserName ?? string.Empty;
            await PopulateRolesAsync(model.Roles);
            return View(model);
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            await userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        await userManager.AddToRoleAsync(user, model.SelectedRole);
        TempData["StatusMessage"] = "User role updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var currentUserId = userManager.GetUserId(User);
        if (id == currentUserId)
        {
            TempData["StatusMessage"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var result = await userManager.DeleteAsync(user);
        TempData["StatusMessage"] = result.Succeeded ? "User deleted." : "Failed to delete user.";

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateRolesAsync(List<SelectListItem> items)
    {
        var roles = roleManager.Roles
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Name!,
                Text = x.Name!
            })
            .ToList();

        await Task.CompletedTask;
        items.AddRange(roles);
    }
}
