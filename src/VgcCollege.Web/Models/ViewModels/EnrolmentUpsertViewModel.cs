using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace oop_s2_1_mvc_76214.Models.ViewModels;

public class EnrolmentUpsertViewModel
{
    public int? Id { get; set; }

    [Required]
    [Display(Name = "Student")]
    public int StudentProfileId { get; set; }

    [Required]
    [Display(Name = "Course")]
    public int CourseId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Enrol Date")]
    public DateTime EnrolDate { get; set; } = DateTime.Today;

    [Required]
    [StringLength(40)]
    public string Status { get; set; } = "Active";

    public List<SelectListItem> Students { get; set; } = new();
    public List<SelectListItem> Courses { get; set; } = new();
}
