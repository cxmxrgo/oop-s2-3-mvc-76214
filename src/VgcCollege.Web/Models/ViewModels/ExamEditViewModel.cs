using System.ComponentModel.DataAnnotations;

namespace oop_s2_1_mvc_76214.Models.ViewModels;

public class ExamEditViewModel
{
    public int? Id { get; set; }

    [Required]
    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Range(1, 100)]
    public decimal MaxScore { get; set; }

    public bool ResultsReleased { get; set; }
}
