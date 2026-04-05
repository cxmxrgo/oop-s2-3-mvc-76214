namespace oop_s2_1_mvc_76214.Models.ViewModels;

public class StudentAttendanceRecordViewModel
{
    public string CourseName { get; set; } = string.Empty;
    public int WeekNumber { get; set; }
    public DateTime AttendanceDate { get; set; }
    public bool Present { get; set; }
}
