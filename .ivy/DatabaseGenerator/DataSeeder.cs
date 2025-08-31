using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Ivy.Database.Generator.Toolkit;

namespace JiraKiller;

public class DataSeeder(DataContext context) : IDataSeeder
{
    public async Task SeedAsync()
    {
        var tagNames = new[] { "JavaScript", "TypeScript", "C#", "Python", "React", "Angular", "Vue", "ASP.NET", "Node.js", "Docker", "Kubernetes", "AWS", "Azure", "DevOps", "SQL", "NoSQL", "MongoDB", "PostgreSQL", "Redis", "GraphQL", "REST", "Microservices", "Architecture", "Testing", "Security", "Performance", "Mobile", "iOS", "Android", "Machine Learning", "AI" };
        var tags = new List<Tag>();
        foreach (var tagName in tagNames)
        {
            var tag = new Tag
            {
                Name = tagName,
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                UpdatedAt = DateTime.UtcNow.AddMonths(-6)
            };
            tags.Add(tag);
        }
        context.Tags.AddRange(tags);
        await context.SaveChangesAsync();

        var authorFaker = new Faker<Author>()
            .RuleFor(a => a.Name, f => f.Name.FullName())
            .RuleFor(a => a.Email, (f, a) => f.Internet.Email(a.Name))
            .RuleFor(a => a.CreatedAt, f => f.Date.Between(DateTime.UtcNow.AddYears(-2), DateTime.UtcNow.AddMonths(-6)))
            .RuleFor(a => a.UpdatedAt, (f, a) => f.Date.Between(a.CreatedAt, DateTime.UtcNow));

        var authors = authorFaker.Generate(50);
        context.Authors.AddRange(authors);
        await context.SaveChangesAsync();

        var postFaker = new Faker<Post>()
            .RuleFor(p => p.Title, f => f.Lorem.Sentence(3, 5).TrimEnd('.'))
            .RuleFor(p => p.Content, f => f.Lorem.Paragraphs(f.Random.Int(2, 8), "\n\n"))
            .RuleFor(p => p.AuthorId, f => f.PickRandom(authors).Id)
            .RuleFor(p => p.CreatedAt, f => f.Date.Between(DateTime.UtcNow.AddMonths(-5), DateTime.UtcNow.AddDays(-1)))
            .RuleFor(p => p.UpdatedAt, (f, p) => f.Date.Between(p.CreatedAt, DateTime.UtcNow));

        var posts = postFaker.Generate(200);
        context.Posts.AddRange(posts);
        await context.SaveChangesAsync();

        var postTags = new List<PostTag>();
        var random = new Random();
        foreach (var post in posts)
        {
            var tagCount = random.Next(1, 6);
            var selectedTags = tags.OrderBy(x => Guid.NewGuid()).Take(tagCount).ToList();
            foreach (var tag in selectedTags)
            {
                postTags.Add(new PostTag
                {
                    PostId = post.Id,
                    TagId = tag.Id
                });
            }
        }
        context.PostTags.AddRange(postTags);
        await context.SaveChangesAsync();

        var commentFaker = new Faker<Comment>()
            .RuleFor(c => c.Content, f => f.Lorem.Paragraph(f.Random.Int(1, 4)))
            .RuleFor(c => c.CreatedAt, f => f.Date.Between(DateTime.UtcNow.AddMonths(-4), DateTime.UtcNow))
            .RuleFor(c => c.UpdatedAt, (f, c) => f.Date.Between(c.CreatedAt, DateTime.UtcNow));

        var comments = new List<Comment>();
        foreach (var post in posts)
        {
            var commentCount = random.Next(0, 15);
            for (int i = 0; i < commentCount; i++)
            {
                var comment = commentFaker.Generate();
                comment.PostId = post.Id;
                comment.AuthorId = authors[random.Next(authors.Count)].Id;
                if (comment.CreatedAt < post.CreatedAt)
                {
                    comment.CreatedAt = post.CreatedAt.AddMinutes(random.Next(60, 10080));
                }
                comments.Add(comment);
            }
        }
        context.Comments.AddRange(comments);
        await context.SaveChangesAsync();
    }
}