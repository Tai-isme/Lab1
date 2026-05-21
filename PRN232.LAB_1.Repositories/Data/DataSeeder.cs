using Microsoft.EntityFrameworkCore;
using PRN232.LAB_1.Repositories.Entities;

namespace PRN232.LAB_1.Repositories.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(LmsDbContext context)
    {
        if (await context.Semesters.AnyAsync()) return;

        using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var semesters = new List<Semester>
            {
                new() { Code = "SP2025", Name = "Spring 2025", StartDate = new DateTime(2025, 1, 13), EndDate = new DateTime(2025, 5, 17), IsActive = false },
                new() { Code = "SU2025", Name = "Summer 2025", StartDate = new DateTime(2025, 5, 26), EndDate = new DateTime(2025, 9, 6), IsActive = false },
                new() { Code = "FA2025", Name = "Fall 2025", StartDate = new DateTime(2025, 9, 15), EndDate = new DateTime(2026, 1, 10), IsActive = false },
                new() { Code = "SP2026", Name = "Spring 2026", StartDate = new DateTime(2026, 1, 12), EndDate = new DateTime(2026, 5, 16), IsActive = true },
                new() { Code = "SU2026", Name = "Summer 2026", StartDate = new DateTime(2026, 5, 25), EndDate = new DateTime(2026, 9, 5), IsActive = true },
            };
            context.Semesters.AddRange(semesters);
            await context.SaveChangesAsync();

            var subjects = new List<Subject>
            {
                new() { Code = "PRN231", Name = "Web Application Development", Description = "Build dynamic web applications using ASP.NET Core MVC, Razor Pages, and Entity Framework.", Credits = 3 },
                new() { Code = "PRN232", Name = "REST API Design", Description = "Design and build RESTful APIs with ASP.NET Core, following clean architecture and layered design.", Credits = 3 },
                new() { Code = "PRN233", Name = "Mobile Application Development", Description = "Develop cross-platform mobile applications using .NET MAUI and Xamarin forms.", Credits = 3 },
                new() { Code = "PRN211", Name = "Basic Programming", Description = "Foundational programming concepts using C# including OOP, collections, and exception handling.", Credits = 3 },
                new() { Code = "PRN212", Name = "Data Structures and Algorithms", Description = "Study fundamental data structures and algorithms with practical implementations in C#.", Credits = 3 },
                new() { Code = "PRN221", Name = "Software Development", Description = "Learn software development lifecycle, version control, testing, and agile methodologies.", Credits = 3 },
                new() { Code = "PRN222", Name = "Web Frontend Development", Description = "Build responsive web interfaces using HTML, CSS, JavaScript, and modern frontend frameworks.", Credits = 2 },
                new() { Code = "PRN241", Name = "Database Systems", Description = "Design relational databases, write complex SQL queries, and understand normalization concepts.", Credits = 3 },
                new() { Code = "PRN242", Name = "Human-Computer Interaction", Description = "Study UI/UX principles, usability testing, and user-centered design processes.", Credits = 2 },
                new() { Code = "PRN251", Name = "System Analysis and Design", Description = "Analyze business requirements and design software systems using UML and design patterns.", Credits = 3 },
            };
            context.Subjects.AddRange(subjects);
            await context.SaveChangesAsync();

            var lastNames = new[] { "Nguyen", "Tran", "Le", "Pham", "Hoang", "Huynh", "Vu", "Vo", "Dang", "Bui", "Do", "Ho", "Ngo", "Duong", "Ly" };
            var firstNames = new[] { "Anh", "Bao", "Cuong", "Duc", "Giang", "Hai", "Hieu", "Hoang", "Khanh", "Lan", "Linh", "Mai", "Minh", "Nam", "Ngoc", "Phong", "Quang", "Son", "Tam", "Thanh", "Thao", "Thien", "Trang", "Tuan", "Van" };
            var middleNames = new[] { "Van", "Thi", "Duc", "Ngoc", "Minh", "Hoang" };
            var cities = new[] { "Ho Chi Minh City", "Hanoi", "Da Nang", "Can Tho", "Hai Phong", "Nha Trang", "Hue", "Vung Tau" };
            var streets = new[] { "Nguyen Hue", "Le Loi", "Tran Hung Dao", "Pham Van Dong", "Hoang Dieu", "Ly Thuong Kiet", "Vo Van Tan", "Dien Bien Phu" };
            var districts = new[] { "District 1", "District 2", "District 3", "District 7", "District 10", "Tan Binh", "Binh Thanh", "Go Vap" };

            var random = new Random(42);
            var students = new List<Student>();
            for (int i = 1; i <= 50; i++)
            {
                var lastName = lastNames[random.Next(lastNames.Length)];
                var middleName = middleNames[random.Next(middleNames.Length)];
                var firstName = firstNames[random.Next(firstNames.Length)];
                var city = cities[random.Next(cities.Length)];
                var street = streets[random.Next(streets.Length)];
                var district = districts[random.Next(districts.Length)];
                var day = random.Next(1, 29);
                var month = random.Next(1, 13);
                var year = random.Next(2000, 2006);

                students.Add(new Student
                {
                    Code = $"STU{i:D3}",
                    FullName = $"{lastName} {middleName} {firstName}",
                    Email = $"STU{i:D3}@fpt.edu.vn",
                    Phone = $"0{random.Next(3, 10)}{random.Next(10000000, 99999999):D8}",
                    DateOfBirth = new DateTime(year, month, day),
                    Address = $"{random.Next(1, 500)} {street} Street, {district}, {city}"
                });
            }
            context.Students.AddRange(students);
            await context.SaveChangesAsync();

            var instructors = new[] { "Nguyen Van A", "Tran Thi B", "Le Van C", "Pham Van D", "Hoang Thi E", "Vu Van F" };
            var rooms = new[] { "L101", "L202", "L303", "L404", "L505", "A101", "A202", "B101", "B201", "B301" };
            var schedules = new[] { "Mon 13:30-16:30", "Tue 08:00-11:00", "Wed 13:30-16:30", "Thu 08:00-11:00", "Fri 13:30-16:30", "Sat 08:00-11:00" };

            var courses = new List<Course>
            {
                new() { Code = "PRN231-1", SubjectId = subjects[0].Id, SemesterId = semesters[0].Id, Instructor = instructors[0], Room = rooms[0], MaxStudents = 35, Schedule = schedules[0] },
                new() { Code = "PRN232-1", SubjectId = subjects[1].Id, SemesterId = semesters[0].Id, Instructor = instructors[1], Room = rooms[1], MaxStudents = 40, Schedule = schedules[1] },
                new() { Code = "PRN211-1", SubjectId = subjects[3].Id, SemesterId = semesters[0].Id, Instructor = instructors[2], Room = rooms[2], MaxStudents = 50, Schedule = schedules[2] },
                new() { Code = "PRN241-1", SubjectId = subjects[7].Id, SemesterId = semesters[0].Id, Instructor = instructors[3], Room = rooms[3], MaxStudents = 30, Schedule = schedules[3] },
                new() { Code = "PRN233-1", SubjectId = subjects[2].Id, SemesterId = semesters[1].Id, Instructor = instructors[4], Room = rooms[4], MaxStudents = 35, Schedule = schedules[4] },
                new() { Code = "PRN212-1", SubjectId = subjects[4].Id, SemesterId = semesters[1].Id, Instructor = instructors[5], Room = rooms[5], MaxStudents = 45, Schedule = schedules[5] },
                new() { Code = "PRN222-1", SubjectId = subjects[6].Id, SemesterId = semesters[1].Id, Instructor = instructors[0], Room = rooms[6], MaxStudents = 40, Schedule = schedules[0] },
                new() { Code = "PRN232-2", SubjectId = subjects[1].Id, SemesterId = semesters[2].Id, Instructor = instructors[1], Room = rooms[7], MaxStudents = 35, Schedule = schedules[1] },
                new() { Code = "PRN221-1", SubjectId = subjects[5].Id, SemesterId = semesters[2].Id, Instructor = instructors[2], Room = rooms[8], MaxStudents = 40, Schedule = schedules[2] },
                new() { Code = "PRN251-1", SubjectId = subjects[9].Id, SemesterId = semesters[2].Id, Instructor = instructors[3], Room = rooms[9], MaxStudents = 30, Schedule = schedules[3] },
                new() { Code = "PRN242-1", SubjectId = subjects[8].Id, SemesterId = semesters[2].Id, Instructor = instructors[4], Room = rooms[0], MaxStudents = 45, Schedule = schedules[4] },
                new() { Code = "PRN231-2", SubjectId = subjects[0].Id, SemesterId = semesters[2].Id, Instructor = instructors[5], Room = rooms[1], MaxStudents = 35, Schedule = schedules[5] },
                new() { Code = "PRN211-2", SubjectId = subjects[3].Id, SemesterId = semesters[2].Id, Instructor = instructors[0], Room = rooms[2], MaxStudents = 50, Schedule = schedules[0] },
                new() { Code = "PRN233-2", SubjectId = subjects[2].Id, SemesterId = semesters[2].Id, Instructor = instructors[1], Room = rooms[3], MaxStudents = 30, Schedule = schedules[1] },
                new() { Code = "PRN241-2", SubjectId = subjects[7].Id, SemesterId = semesters[2].Id, Instructor = instructors[2], Room = rooms[4], MaxStudents = 40, Schedule = schedules[2] },
                new() { Code = "PRN232-3", SubjectId = subjects[1].Id, SemesterId = semesters[3].Id, Instructor = instructors[3], Room = rooms[5], MaxStudents = 35, Schedule = schedules[3] },
                new() { Code = "PRN221-2", SubjectId = subjects[5].Id, SemesterId = semesters[3].Id, Instructor = instructors[4], Room = rooms[6], MaxStudents = 40, Schedule = schedules[4] },
                new() { Code = "PRN212-2", SubjectId = subjects[4].Id, SemesterId = semesters[3].Id, Instructor = instructors[5], Room = rooms[7], MaxStudents = 45, Schedule = schedules[5] },
                new() { Code = "PRN222-2", SubjectId = subjects[6].Id, SemesterId = semesters[4].Id, Instructor = instructors[0], Room = rooms[8], MaxStudents = 35, Schedule = schedules[0] },
                new() { Code = "PRN251-2", SubjectId = subjects[9].Id, SemesterId = semesters[4].Id, Instructor = instructors[1], Room = rooms[9], MaxStudents = 30, Schedule = schedules[1] },
            };
            context.Courses.AddRange(courses);
            await context.SaveChangesAsync();

            var enrollments = new List<Enrollment>();
            var statuses = new[] { "Active", "Active", "Active", "Completed", "Dropped" };
            var grades = new[] { "A", "B+", "B", "C+", "C", "D+", "D", "F" };

            foreach (var course in courses)
            {
                var studentsForCourse = students.OrderBy(_ => random.Next()).Take(25).ToList();
                foreach (var student in studentsForCourse)
                {
                    var status = statuses[random.Next(statuses.Length)];
                    enrollments.Add(new Enrollment
                    {
                        StudentId = student.Id,
                        CourseId = course.Id,
                        EnrollmentDate = semesters.First(s => s.Id == course.SemesterId).StartDate.AddDays(random.Next(0, 14)),
                        Status = status,
                        Grade = status == "Completed" ? grades[random.Next(grades.Length)] : null
                    });
                }
            }

            context.Enrollments.AddRange(enrollments);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
