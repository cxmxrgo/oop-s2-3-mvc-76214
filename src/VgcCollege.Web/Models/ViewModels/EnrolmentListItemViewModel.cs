namespace oop_s2_1_mvc_76214.Models.ViewModels;

public class EnrolmentListItemViewModel
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public DateTime EnrolDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
