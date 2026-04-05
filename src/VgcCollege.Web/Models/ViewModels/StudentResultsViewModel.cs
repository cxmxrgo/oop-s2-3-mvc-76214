namespace oop_s2_1_mvc_76214.Models.ViewModels;

public class StudentResultsViewModel
{
    public string StudentName { get; set; } = string.Empty;
    public List<StudentAssignmentResultViewModel> AssignmentResults { get; set; } = new();
    public List<StudentExamResultViewModel> ReleasedExamResults { get; set; } = new();
}

public class StudentAssignmentResultViewModel
{
    public string CourseName { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public decimal MaxScore { get; set; }
    public string Feedback { get; set; } = string.Empty;
}
