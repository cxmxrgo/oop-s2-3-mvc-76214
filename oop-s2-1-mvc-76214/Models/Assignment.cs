using System.ComponentModel.DataAnnotations;

namespace oop_s2_1_mvc_76214.Models;

public class Assignment
{
    public int Id { get; set; }

    [Required]
    public int CourseId { get; set; }

    [Required, StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Range(1, 1000)]
    public decimal MaxScore { get; set; }

    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; }

    public Course? Course { get; set; }
    public ICollection<AssignmentResult> AssignmentResults { get; set; } = new List<AssignmentResult>();
}
