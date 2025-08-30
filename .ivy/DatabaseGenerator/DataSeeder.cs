using Bogus;
using Ivy.Database.Generator.Toolkit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JiraKiller;

public class DataSeeder(DataContext context) : IDataSeeder
{
    public async Task SeedAsync()
    {
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Name.FullName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Name))
            .RuleFor(u => u.CreatedAt, f => f.Date.Between(DateTime.UtcNow.AddYears(-2), DateTime.UtcNow.AddMonths(-6)))
            .RuleFor(u => u.UpdatedAt, (f, u) => f.Date.Between(u.CreatedAt, DateTime.UtcNow));

        var users = userFaker.Generate(25);
        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        var projectNames = new[] 
        { 
            "Customer Portal Redesign", "Mobile App Development", "Data Migration Project", 
            "API Integration Platform", "Security Audit Implementation", "Cloud Infrastructure Setup",
            "E-commerce Platform", "Analytics Dashboard", "Payment Gateway Integration",
            "Inventory Management System", "HR Management Suite", "Marketing Automation Tool"
        };
        
        var projectFaker = new Faker<Project>()
            .RuleFor(p => p.Name, f => f.PickRandom(projectNames) + " " + f.Random.AlphaNumeric(3).ToUpper())
            .RuleFor(p => p.Description, f => f.Lorem.Paragraph())
            .RuleFor(p => p.StartDate, f => f.Date.Between(DateTime.UtcNow.AddMonths(-18), DateTime.UtcNow.AddMonths(-3)))
            .RuleFor(p => p.EndDate, (f, p) => f.Random.Bool(0.7f) ? f.Date.Between(p.StartDate!.Value.AddMonths(1), DateTime.UtcNow.AddMonths(6)) : null)
            .RuleFor(p => p.CreatedAt, (f, p) => f.Date.Between(p.StartDate ?? DateTime.UtcNow.AddMonths(-12), DateTime.UtcNow.AddMonths(-2)))
            .RuleFor(p => p.UpdatedAt, (f, p) => f.Date.Between(p.CreatedAt, DateTime.UtcNow));

        var projects = projectFaker.Generate(12);
        context.Projects.AddRange(projects);
        await context.SaveChangesAsync();

        var ticketTitles = new[]
        {
            "Fix login authentication bug", "Update user profile page", "Implement search functionality",
            "Database optimization needed", "Add export to PDF feature", "Mobile responsive issues",
            "Performance improvements required", "Security vulnerability patch", "API endpoint not responding",
            "Data validation errors", "Email notifications not sending", "Dashboard loading slowly",
            "Integration test failures", "Memory leak in production", "Update documentation",
            "Refactor legacy code", "Add unit tests", "Deploy to staging environment",
            "Configure CI/CD pipeline", "Migrate to new database", "Update third-party libraries",
            "Fix cross-browser compatibility", "Add user permissions", "Implement caching strategy"
        };

        var tickets = new List<Ticket>();
        var faker = new Faker();
        
        foreach (var project in projects)
        {
            var ticketCount = faker.Random.Int(8, 30);
            
            for (int i = 0; i < ticketCount; i++)
            {
                var createdAt = faker.Date.Between(project.CreatedAt, DateTime.UtcNow.AddDays(-1));
                var status = faker.PickRandom<TicketStatus>();
                var priority = faker.PickRandom<Priority>();
                
                var ticket = new Ticket
                {
                    ProjectId = project.Id,
                    AssignedUserId = faker.Random.Bool(0.85f) ? faker.PickRandom(users).Id : null,
                    Title = faker.PickRandom(ticketTitles) + " - " + faker.Random.AlphaNumeric(4).ToUpper(),
                    Description = faker.Lorem.Paragraphs(faker.Random.Int(1, 3)),
                    Priority = priority,
                    Status = status,
                    DueDate = faker.Random.Bool(0.6f) ? faker.Date.Between(createdAt.AddDays(1), DateTime.UtcNow.AddMonths(2)) : null,
                    CreatedAt = createdAt,
                    UpdatedAt = faker.Date.Between(createdAt, DateTime.UtcNow)
                };
                
                tickets.Add(ticket);
            }
        }
        
        context.Tickets.AddRange(tickets);
        await context.SaveChangesAsync();

        var timeLogs = new List<TimeLog>();
        
        foreach (var ticket in tickets.Where(t => t.AssignedUserId.HasValue && (t.Status == TicketStatus.in_progress || t.Status == TicketStatus.completed || t.Status == TicketStatus.closed)))
        {
            var logCount = faker.Random.Int(1, 8);
            var availableUsers = users.Where(u => u.Id == ticket.AssignedUserId.Value || faker.Random.Bool(0.3f)).ToList();
            
            for (int i = 0; i < logCount; i++)
            {
                var loggedAt = faker.Date.Between(ticket.CreatedAt, ticket.UpdatedAt);
                var timeLog = new TimeLog
                {
                    TicketId = ticket.Id,
                    UserId = faker.PickRandom(availableUsers).Id,
                    Hours = faker.Random.Decimal(0.5M, 8.0M),
                    LoggedAt = loggedAt,
                    CreatedAt = loggedAt,
                    UpdatedAt = faker.Date.Between(loggedAt, DateTime.UtcNow)
                };
                
                timeLogs.Add(timeLog);
            }
        }
        
        context.TimeLogs.AddRange(timeLogs);
        await context.SaveChangesAsync();
    }
}