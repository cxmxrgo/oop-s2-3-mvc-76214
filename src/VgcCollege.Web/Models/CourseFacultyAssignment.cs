namespace oop_s2_1_mvc_76214.Models;

public class CourseFacultyAssignment
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int FacultyProfileId { get; set; }

    public Course? Course { get; set; }
    public FacultyProfile? FacultyProfile { get; set; }
}
