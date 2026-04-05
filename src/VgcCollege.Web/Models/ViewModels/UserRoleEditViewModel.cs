using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace oop_s2_1_mvc_76214.Models.ViewModels;

public class UserRoleEditViewModel
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Role")]
    public string SelectedRole { get; set; } = string.Empty;

    public List<SelectListItem> Roles { get; set; } = new();
}
