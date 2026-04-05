using System.ComponentModel.DataAnnotations;

namespace oop_s2_1_mvc_76214.Models;

public class CourseEnrolment
{
    public int Id { get; set; }

    [Required]
    public int StudentProfileId { get; set; }

    [Required]
    public int CourseId { get; set; }

    [DataType(DataType.Date)]
    public DateTime EnrolDate { get; set; }

    [Required, StringLength(40)]
    public string Status { get; set; } = "Active";

    public StudentProfile? StudentProfile { get; set; }
    public Course? Course { get; set; }
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}
