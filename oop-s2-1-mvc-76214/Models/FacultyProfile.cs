using System.ComponentModel.DataAnnotations;

namespace oop_s2_1_mvc_76214.Models;

public class FacultyProfile
{
    public int Id { get; set; }

    [Required]
    public string IdentityUserId { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    public string Phone { get; set; } = string.Empty;

    public ICollection<CourseFacultyAssignment> CourseFacultyAssignments { get; set; } = new List<CourseFacultyAssignment>();
}
