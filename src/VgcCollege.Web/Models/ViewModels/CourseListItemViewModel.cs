namespace oop_s2_1_mvc_76214.Models.ViewModels;

public class CourseListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
