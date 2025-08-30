using Bogus;
using Microsoft.EntityFrameworkCore;
using Ivy.Database.Generator.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test5;

public class DataSeeder(DataContext context) : IDataSeeder
{
    public async Task SeedAsync()
    {
        var roles = new[] { "Developer", "Senior Developer", "Team Lead", "Project Manager", "QA Engineer", "DevOps Engineer", "Business Analyst", "Product Owner" };
        
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Name, f => f.Name.FullName())
            .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.Name.ToLower()))
            .RuleFor(u => u.Role, f => f.PickRandom(roles))
            .RuleFor(u => u.CreatedAt, f => f.Date.Between(DateTime.UtcNow.AddYears(-2), DateTime.UtcNow.AddMonths(-6)))
            .RuleFor(u => u.UpdatedAt, (f, u) => f.Date.Between(u.CreatedAt, DateTime.UtcNow));
        
        var users = userFaker.Generate(50);
        context.Users.AddRange(users);
        await context.SaveChangesAsync();

        var projectManagers = users.Where(u => u.Role == "Project Manager" || u.Role == "Product Owner" || u.Role == "Team Lead").ToList();
        if (!projectManagers.Any())
            projectManagers = users.Take(5).ToList();

        var projectFaker = new Faker<Project>()
            .RuleFor(p => p.Name, f => f.Commerce.ProductName() + " " + f.PickRandom("Platform", "System", "Application", "Service", "Portal", "Dashboard", "API", "Integration"))
            .RuleFor(p => p.Description, f => f.Lorem.Paragraph())
            .RuleFor(p => p.CreatedBy, f => f.PickRandom(projectManagers).Id)
            .RuleFor(p => p.CreatedAt, f => f.Date.Between(DateTime.UtcNow.AddMonths(-18), DateTime.UtcNow.AddMonths(-3)))
            .RuleFor(p => p.UpdatedAt, (f, p) => f.Date.Between(p.CreatedAt, DateTime.UtcNow));

        var projects = projectFaker.Generate(15);
        context.Projects.AddRange(projects);
        await context.SaveChangesAsync();

        var developers = users.Where(u => u.Role.Contains("Developer") || u.Role == "QA Engineer" || u.Role == "DevOps Engineer").ToList();
        if (!developers.Any())
            developers = users.ToList();

        var ticketTitles = new[] 
        {
            "Fix login authentication issue",
            "Implement user profile page",
            "Database connection timeout error",
            "Add email notification feature",
            "Optimize query performance",
            "Update API documentation",
            "Fix responsive layout on mobile",
            "Implement data export functionality",
            "Security vulnerability patch",
            "Add unit tests for service layer",
            "Refactor legacy code module",
            "Integrate third-party payment gateway",
            "Fix memory leak in background job",
            "Implement caching strategy",
            "Add logging and monitoring",
            "Update dependencies to latest versions",
            "Fix cross-browser compatibility issue",
            "Implement search functionality",
            "Add data validation rules",
            "Performance optimization for large datasets"
        };

        var tickets = new List<Ticket>();
        var faker = new Faker();
        
        foreach (var project in projects)
        {
            var ticketCount = faker.Random.Int(5, 30);
            for (int i = 0; i < ticketCount; i++)
            {
                var createdAt = faker.Date.Between(project.CreatedAt, DateTime.UtcNow.AddDays(-1));
                var status = faker.PickRandom<TicketStatus>();
                var priority = faker.PickRandom<TicketPriority>();
                
                var ticket = new Ticket
                {
                    Title = faker.PickRandom(ticketTitles) + " - " + faker.Random.AlphaNumeric(4).ToUpper(),
                    Description = faker.Lorem.Paragraphs(faker.Random.Int(1, 3)),
                    ProjectId = project.Id,
                    Priority = priority,
                    Status = status,
                    DueDate = faker.Random.Bool(0.7f) ? faker.Date.Between(createdAt.AddDays(1), DateTime.UtcNow.AddMonths(2)) : null,
                    CreatedBy = faker.PickRandom(users).Id,
                    AssignedTo = faker.Random.Bool(0.85f) ? faker.PickRandom(developers).Id : null,
                    CreatedAt = createdAt,
                    UpdatedAt = faker.Date.Between(createdAt, DateTime.UtcNow)
                };
                
                tickets.Add(ticket);
            }
        }
        
        context.Tickets.AddRange(tickets);
        await context.SaveChangesAsync();

        var timeEntries = new List<TimeEntry>();
        var assignedTickets = tickets.Where(t => t.AssignedTo.HasValue && (t.Status == TicketStatus.InProgress || t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed)).ToList();
        
        foreach (var ticket in assignedTickets)
        {
            var entryCount = faker.Random.Int(1, 8);
            var currentDate = ticket.CreatedAt;
            
            for (int i = 0; i < entryCount; i++)
            {
                currentDate = faker.Date.Between(currentDate, ticket.UpdatedAt);
                var loggedAt = faker.Date.Between(currentDate, currentDate.AddDays(3));
                
                var timeEntry = new TimeEntry
                {
                    TicketId = ticket.Id,
                    UserId = ticket.AssignedTo!.Value,
                    Hours = Math.Round((decimal)faker.Random.Double(0.5, 8.0), 1),
                    LoggedAt = loggedAt,
                    CreatedAt = loggedAt,
                    UpdatedAt = faker.Date.Between(loggedAt, DateTime.UtcNow)
                };
                
                timeEntries.Add(timeEntry);
                currentDate = loggedAt;
            }
        }
        
        context.TimeEntries.AddRange(timeEntries);
        await context.SaveChangesAsync();
    }
}