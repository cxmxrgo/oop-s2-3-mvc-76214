using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace oop_s2_1_mvc_76214.Models.ViewModels;

public class StaffAssignmentUpsertViewModel
{
    public int? Id { get; set; }

    [Required]
    [Display(Name = "Course")]
    public int CourseId { get; set; }

    [Required]
    [Display(Name = "Faculty")]
    public int FacultyProfileId { get; set; }

    public List<SelectListItem> Courses { get; set; } = new();
    public List<SelectListItem> FacultyMembers { get; set; } = new();
}
