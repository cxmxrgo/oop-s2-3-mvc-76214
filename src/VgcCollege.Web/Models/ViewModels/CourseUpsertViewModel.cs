using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace oop_s2_1_mvc_76214.Models.ViewModels;

public class CourseUpsertViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Branch")]
    public int BranchId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "End Date")]
    public DateTime EndDate { get; set; }

    public List<SelectListItem> Branches { get; set; } = new();
}
