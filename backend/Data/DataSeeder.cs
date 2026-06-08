using Microsoft.EntityFrameworkCore;
using TrainingService.Models;

namespace TrainingService.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(TrainingDbContext db)
    {
        if (await db.TrainingCourses.AnyAsync()) return; // already seeded

        // ── Course 1: Quality Management System ──────────────────────────────
        var qms = new TrainingCourse
        {
            Name = "Quality Management System (QMS)",
            NameAr = "نظام إدارة الجودة",
            OrganizationId = 1,
            UserId = "admin",
            TargetedAudiance = "All Employees",
            IsActive = true,
            Creatd = DateTime.Now
        };

        qms.Sections.Add(new CourseSection
        {
            Name = "Introduction to QMS",
            NameAr = "مقدمة في نظام إدارة الجودة",
            OrganizationId = 1, UserId = "admin", IsActive = true, Creatd = DateTime.Now
        });
        qms.Sections.Add(new CourseSection
        {
            Name = "ISO 9001 Standards",
            NameAr = "معايير الآيزو 9001",
            OrganizationId = 1, UserId = "admin", IsActive = true, Creatd = DateTime.Now
        });
        qms.Sections.Add(new CourseSection
        {
            Name = "Continuous Improvement",
            NameAr = "التحسين المستمر",
            OrganizationId = 1, UserId = "admin", IsActive = true, Creatd = DateTime.Now
        });

        var qmsTest = new Test
        {
            Name = "QMS Assessment",
            OrganizationId = 1, UserId = "admin",
            PassScore = 60, ExpiryDuration = 365, IsActive = true
        };
        qmsTest.Questions.Add(new Question
        {
            Text = "What does QMS stand for?",
            IsActive = true, sorting = 1,
            Answers =
            [
                new Answer { Text = "Quality Management System", IsCorrect = true,  IsActive = true },
                new Answer { Text = "Quality Measurement Standard", IsCorrect = false, IsActive = true },
                new Answer { Text = "Quick Management Solution", IsCorrect = false, IsActive = true },
                new Answer { Text = "Quality Monitoring System",  IsCorrect = false, IsActive = true }
            ]
        });
        qmsTest.Questions.Add(new Question
        {
            Text = "Which ISO standard is related to QMS?",
            IsActive = true, sorting = 2,
            Answers =
            [
                new Answer { Text = "ISO 9001", IsCorrect = true,  IsActive = true },
                new Answer { Text = "ISO 14001", IsCorrect = false, IsActive = true },
                new Answer { Text = "ISO 45001", IsCorrect = false, IsActive = true },
                new Answer { Text = "ISO 27001", IsCorrect = false, IsActive = true }
            ]
        });
        qmsTest.Questions.Add(new Question
        {
            Text = "What is the PDCA cycle?",
            IsActive = true, sorting = 3,
            Answers =
            [
                new Answer { Text = "Plan, Do, Check, Act",     IsCorrect = true,  IsActive = true },
                new Answer { Text = "Prepare, Define, Control, Assess", IsCorrect = false, IsActive = true },
                new Answer { Text = "Plan, Develop, Check, Apply",     IsCorrect = false, IsActive = true },
                new Answer { Text = "Process, Data, Control, Action",  IsCorrect = false, IsActive = true }
            ]
        });
        qms.Tests.Add(qmsTest);
        db.TrainingCourses.Add(qms);

        // ── Course 2: Health & Safety Awareness ───────────────────────────────
        var hns = new TrainingCourse
        {
            Name = "Health & Safety Awareness",
            NameAr = "التوعية بالصحة والسلامة",
            OrganizationId = 1,
            UserId = "admin",
            TargetedAudiance = "All Staff",
            IsActive = true,
            Creatd = DateTime.Now
        };

        hns.Sections.Add(new CourseSection
        {
            Name = "Workplace Safety Fundamentals",
            NameAr = "أساسيات سلامة بيئة العمل",
            OrganizationId = 1, UserId = "admin", IsActive = true, Creatd = DateTime.Now
        });
        hns.Sections.Add(new CourseSection
        {
            Name = "Emergency Procedures",
            NameAr = "إجراءات الطوارئ",
            OrganizationId = 1, UserId = "admin", IsActive = true, Creatd = DateTime.Now
        });
        hns.Sections.Add(new CourseSection
        {
            Name = "Personal Protective Equipment",
            NameAr = "معدات الوقاية الشخصية",
            OrganizationId = 1, UserId = "admin", IsActive = true, Creatd = DateTime.Now
        });

        var hnsTest = new Test
        {
            Name = "Safety Awareness Assessment",
            OrganizationId = 1, UserId = "admin",
            PassScore = 70, ExpiryDuration = 180, IsActive = true
        };
        hnsTest.Questions.Add(new Question
        {
            Text = "What should you do first when you notice a fire?",
            IsActive = true, sorting = 1,
            Answers =
            [
                new Answer { Text = "Activate the fire alarm immediately", IsCorrect = true,  IsActive = true },
                new Answer { Text = "Try to extinguish it yourself",       IsCorrect = false, IsActive = true },
                new Answer { Text = "Wait to see if it grows bigger",      IsCorrect = false, IsActive = true },
                new Answer { Text = "Call a colleague first",              IsCorrect = false, IsActive = true }
            ]
        });
        hnsTest.Questions.Add(new Question
        {
            Text = "What does PPE stand for?",
            IsActive = true, sorting = 2,
            Answers =
            [
                new Answer { Text = "Personal Protective Equipment", IsCorrect = true,  IsActive = true },
                new Answer { Text = "Professional Protection Equipment", IsCorrect = false, IsActive = true },
                new Answer { Text = "Personal Prevention Equipment",    IsCorrect = false, IsActive = true },
                new Answer { Text = "Protective Personal Essentials",   IsCorrect = false, IsActive = true }
            ]
        });
        hnsTest.Questions.Add(new Question
        {
            Text = "How often should emergency drills be conducted?",
            IsActive = true, sorting = 3,
            Answers =
            [
                new Answer { Text = "At least twice a year",        IsCorrect = true,  IsActive = true },
                new Answer { Text = "Once every 5 years",           IsCorrect = false, IsActive = true },
                new Answer { Text = "Only when a new employee joins",IsCorrect = false, IsActive = true },
                new Answer { Text = "Never — drills are optional",  IsCorrect = false, IsActive = true }
            ]
        });
        hns.Tests.Add(hnsTest);
        db.TrainingCourses.Add(hns);

        await db.SaveChangesAsync();
    }
}
