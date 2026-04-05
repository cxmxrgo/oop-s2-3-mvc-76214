using System.ComponentModel.DataAnnotations;

namespace oop_s2_1_mvc_76214.Models;

public class ExamResult
{
    public int Id { get; set; }

    [Required]
    public int ExamId { get; set; }

    [Required]
    public int StudentProfileId { get; set; }

    [Range(0, 1000)]
    public decimal Score { get; set; }

    [Required, StringLength(5)]
    public string Grade { get; set; } = string.Empty;

    public Exam? Exam { get; set; }
    public StudentProfile? StudentProfile { get; set; }
}
