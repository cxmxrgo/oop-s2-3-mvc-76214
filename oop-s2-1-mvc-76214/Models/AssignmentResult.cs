using System.ComponentModel.DataAnnotations;

namespace oop_s2_1_mvc_76214.Models;

public class AssignmentResult
{
    public int Id { get; set; }

    [Required]
    public int AssignmentId { get; set; }

    [Required]
    public int StudentProfileId { get; set; }

    [Range(0, 1000)]
    public decimal Score { get; set; }

    [StringLength(500)]
    public string Feedback { get; set; } = string.Empty;

    public Assignment? Assignment { get; set; }
    public StudentProfile? StudentProfile { get; set; }
}
