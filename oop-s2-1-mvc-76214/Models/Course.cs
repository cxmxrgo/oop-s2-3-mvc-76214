using System.ComponentModel.DataAnnotations;

namespace oop_s2_1_mvc_76214.Models;

public class Course
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int BranchId { get; set; }

    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<CourseEnrolment> CourseEnrolments { get; set; } = new List<CourseEnrolment>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<Exam> Exams { get; set; } = new List<Exam>();
    public ICollection<CourseFacultyAssignment> CourseFacultyAssignments { get; set; } = new List<CourseFacultyAssignment>();
}
