using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oop_s2_1_mvc_76214.Data;
using oop_s2_1_mvc_76214.Models;
using oop_s2_1_mvc_76214.Models.ViewModels;

namespace oop_s2_1_mvc_76214.Controllers;

[Authorize(Roles = RoleNames.Administrator)]
public class AdminController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel
        {
            BranchCount = await context.Branches.CountAsync(),
            CourseCount = await context.Courses.CountAsync(),
            StudentCount = await context.StudentProfiles.CountAsync(),
            FacultyCount = await context.FacultyProfiles.CountAsync()
        };

        return View(model);
    }
}
