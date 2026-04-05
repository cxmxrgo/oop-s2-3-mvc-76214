using System.ComponentModel.DataAnnotations;

namespace oop_s2_1_mvc_76214.Models;

public class AttendanceRecord
{
    public int Id { get; set; }

    [Required]
    public int CourseEnrolmentId { get; set; }

    [Range(1, 52)]
    public int WeekNumber { get; set; }

    [DataType(DataType.Date)]
    public DateTime AttendanceDate { get; set; }

    public bool Present { get; set; }

    public CourseEnrolment? CourseEnrolment { get; set; }
}
