using Microsoft.EntityFrameworkCore;
using TrainingService.Models;

namespace TrainingService.Data;

public class TrainingDbContext : DbContext
{
    public TrainingDbContext(DbContextOptions<TrainingDbContext> options) : base(options) { }

    public DbSet<TrainingCourse> TrainingCourses => Set<TrainingCourse>();
    public DbSet<CourseSection> CourseSections => Set<CourseSection>();
    public DbSet<Test> Tests => Set<Test>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<UserTestResult> UserTestResults => Set<UserTestResult>();
    public DbSet<UserAnswer> UserAnswers => Set<UserAnswer>();
    public DbSet<FileRecord> FileRecords => Set<FileRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrainingCourse>(e =>
        {
            e.HasKey(x => x.id);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.NameAr).HasMaxLength(200);
            e.Property(x => x.UserId).HasMaxLength(100);
        });

        modelBuilder.Entity<CourseSection>(e =>
        {
            e.HasKey(x => x.id);
            e.HasOne(x => x.TrainingCourse)
             .WithMany(c => c.Sections)
             .HasForeignKey(x => x.TrainingCourseId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Test>(e =>
        {
            e.HasKey(x => x.id);
            e.HasOne(x => x.TrainingCourse)
             .WithMany(c => c.Tests)
             .HasForeignKey(x => x.TrainingCourseId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(e =>
        {
            e.HasKey(x => x.id);
            e.HasOne(x => x.Test)
             .WithMany(t => t.Questions)
             .HasForeignKey(x => x.TestId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Answer>(e =>
        {
            e.HasKey(x => x.id);
            e.HasOne(x => x.Question)
             .WithMany(q => q.Answers)
             .HasForeignKey(x => x.QuestionId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<UserTestResult>(e =>
        {
            e.HasKey(x => x.id);
            e.HasOne(x => x.Test)
             .WithMany()
             .HasForeignKey(x => x.TestId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FileRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.OriginalName).HasMaxLength(500);
            e.Property(x => x.ContentType).HasMaxLength(200);
            e.Property(x => x.StoredPath).HasMaxLength(1000);
            e.Property(x => x.UploadedBy).HasMaxLength(200);
        });

        modelBuilder.Entity<UserAnswer>(e =>
        {
            e.HasKey(x => x.id);
            e.HasOne(x => x.UserTestResult)
             .WithMany(r => r.UserAnswers)
             .HasForeignKey(x => x.UserTestResultId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Question).WithMany().HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.SelectedAnswer).WithMany().HasForeignKey(x => x.SelectedAnswerId).OnDelete(DeleteBehavior.NoAction);
        });
    }
}
