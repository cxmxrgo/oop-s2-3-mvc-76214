using System.ComponentModel.DataAnnotations;

namespace oop_s2_1_mvc_76214.Models.ViewModels;

public class AssignmentResultsViewModel
{
    public int AssignmentId { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public List<AssignmentResultRowViewModel> Students { get; set; } = new();
}

public class AssignmentResultRowViewModel
{
    public int StudentProfileId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;

    [Range(0, 100)]
    public decimal? Score { get; set; }

    [StringLength(500)]
    public string Feedback { get; set; } = string.Empty;
}
