namespace oop_s2_1_mvc_76214.Models.ViewModels;

public class AssignmentListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public DateTime DueDate { get; set; }
}

public class ExamListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public DateTime Date { get; set; }
    public bool ResultsReleased { get; set; }
}
