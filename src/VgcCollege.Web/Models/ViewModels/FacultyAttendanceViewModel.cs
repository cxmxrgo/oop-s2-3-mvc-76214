using System.ComponentModel.DataAnnotations;

namespace oop_s2_1_mvc_76214.Models.ViewModels;

public class FacultyAttendanceViewModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;

    [Range(1, 52)]
    [Display(Name = "Week Number")]
    public int WeekNumber { get; set; } = 1;

    [DataType(DataType.Date)]
    [Display(Name = "Attendance Date")]
    public DateTime AttendanceDate { get; set; } = DateTime.Today;

    public List<FacultyAttendanceStudentItemViewModel> Students { get; set; } = new();
}

public class FacultyAttendanceStudentItemViewModel
{
    public int CourseEnrolmentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public bool Present { get; set; }
}
