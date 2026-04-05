using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using oop_s2_1_mvc_76214.Models;

namespace oop_s2_1_mvc_76214.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
        public DbSet<FacultyProfile> FacultyProfiles => Set<FacultyProfile>();
        public DbSet<CourseEnrolment> CourseEnrolments => Set<CourseEnrolment>();
        public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
        public DbSet<Assignment> Assignments => Set<Assignment>();
        public DbSet<AssignmentResult> AssignmentResults => Set<AssignmentResult>();
        public DbSet<Exam> Exams => Set<Exam>();
        public DbSet<ExamResult> ExamResults => Set<ExamResult>();
        public DbSet<CourseFacultyAssignment> CourseFacultyAssignments => Set<CourseFacultyAssignment>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Branch>(entity =>
            {
                entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Address).HasMaxLength(200).IsRequired();
            });

            builder.Entity<Course>(entity =>
            {
                entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
                entity.HasOne(x => x.Branch)
                    .WithMany(x => x.Courses)
                    .HasForeignKey(x => x.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<StudentProfile>(entity =>
            {
                entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
                entity.Property(x => x.Email).HasMaxLength(120).IsRequired();
                entity.Property(x => x.Phone).HasMaxLength(20).IsRequired();
                entity.Property(x => x.Address).HasMaxLength(200).IsRequired();
                entity.Property(x => x.StudentNumber).HasMaxLength(20).IsRequired();
                entity.HasIndex(x => x.IdentityUserId).IsUnique();
                entity.HasIndex(x => x.StudentNumber).IsUnique();
            });

            builder.Entity<FacultyProfile>(entity =>
            {
                entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
                entity.Property(x => x.Email).HasMaxLength(120).IsRequired();
                entity.Property(x => x.Phone).HasMaxLength(20).IsRequired();
                entity.HasIndex(x => x.IdentityUserId).IsUnique();
            });

            builder.Entity<CourseEnrolment>(entity =>
            {
                entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
                entity.HasOne(x => x.StudentProfile)
                    .WithMany(x => x.CourseEnrolments)
                    .HasForeignKey(x => x.StudentProfileId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Course)
                    .WithMany(x => x.CourseEnrolments)
                    .HasForeignKey(x => x.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(x => new { x.StudentProfileId, x.CourseId }).IsUnique();
            });

            builder.Entity<AttendanceRecord>(entity =>
            {
                entity.HasOne(x => x.CourseEnrolment)
                    .WithMany(x => x.AttendanceRecords)
                    .HasForeignKey(x => x.CourseEnrolmentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(x => new { x.CourseEnrolmentId, x.WeekNumber }).IsUnique();
            });

            builder.Entity<Assignment>(entity =>
            {
                entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
                entity.HasOne(x => x.Course)
                    .WithMany(x => x.Assignments)
                    .HasForeignKey(x => x.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AssignmentResult>(entity =>
            {
                entity.HasOne(x => x.Assignment)
                    .WithMany(x => x.AssignmentResults)
                    .HasForeignKey(x => x.AssignmentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.StudentProfile)
                    .WithMany(x => x.AssignmentResults)
                    .HasForeignKey(x => x.StudentProfileId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(x => new { x.AssignmentId, x.StudentProfileId }).IsUnique();
            });

            builder.Entity<Exam>(entity =>
            {
                entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
                entity.HasOne(x => x.Course)
                    .WithMany(x => x.Exams)
                    .HasForeignKey(x => x.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ExamResult>(entity =>
            {
                entity.Property(x => x.Grade).HasMaxLength(5).IsRequired();
                entity.HasOne(x => x.Exam)
                    .WithMany(x => x.ExamResults)
                    .HasForeignKey(x => x.ExamId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.StudentProfile)
                    .WithMany(x => x.ExamResults)
                    .HasForeignKey(x => x.StudentProfileId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(x => new { x.ExamId, x.StudentProfileId }).IsUnique();
            });

            builder.Entity<CourseFacultyAssignment>(entity =>
            {
                entity.HasOne(x => x.Course)
                    .WithMany(x => x.CourseFacultyAssignments)
                    .HasForeignKey(x => x.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.FacultyProfile)
                    .WithMany(x => x.CourseFacultyAssignments)
                    .HasForeignKey(x => x.FacultyProfileId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(x => new { x.CourseId, x.FacultyProfileId }).IsUnique();
            });
        }
    }
}
