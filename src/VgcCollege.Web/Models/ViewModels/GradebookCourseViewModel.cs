namespace oop_s2_1_mvc_76214.Models.ViewModels;

public class GradebookCourseViewModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public int AssignmentCount { get; set; }
    public int ExamCount { get; set; }
}
