using System.ComponentModel.DataAnnotations;

namespace oop_s2_1_mvc_76214.Models;

public class StudentProfile
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

    [Required, StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime DOB { get; set; }

    [Required, StringLength(20)]
    public string StudentNumber { get; set; } = string.Empty;

    public ICollection<CourseEnrolment> CourseEnrolments { get; set; } = new List<CourseEnrolment>();
    public ICollection<AssignmentResult> AssignmentResults { get; set; } = new List<AssignmentResult>();
    public ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();
}
