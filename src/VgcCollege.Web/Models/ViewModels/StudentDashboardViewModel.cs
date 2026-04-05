namespace oop_s2_1_mvc_76214.Models.ViewModels;

public class StudentDashboardViewModel
{
    public string Name { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public List<string> EnrolledCourses { get; set; } = new();
    public List<StudentExamResultViewModel> ReleasedExamResults { get; set; } = new();
}

public class StudentExamResultViewModel
{
    public string CourseName { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public string Grade { get; set; } = string.Empty;
}
