using System.ComponentModel.DataAnnotations;

namespace oop_s2_1_mvc_76214.Models.ViewModels;

public class ExamResultsViewModel
{
    public int ExamId { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public bool ResultsReleased { get; set; }
    public List<ExamResultRowViewModel> Students { get; set; } = new();
}

public class ExamResultRowViewModel
{
    public int StudentProfileId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;

    [Range(0, 100)]
    public decimal? Score { get; set; }

    [StringLength(5)]
    public string Grade { get; set; } = string.Empty;
}
