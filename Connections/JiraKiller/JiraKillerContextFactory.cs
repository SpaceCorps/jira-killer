using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace JiraKiller.Connections.JiraKiller;

public class JiraKillerContextFactory(ServerArgs args) : IDbContextFactory<JiraKillerContext>
{
    public JiraKillerContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<JiraKillerContext>();

        optionsBuilder.UseSqlite("Data Source=D:\\git\\spacecorps\\jira-killer\\db.sqlite");

        if (args.Verbose)
        {
            optionsBuilder
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information);
        }

        return new JiraKillerContext(optionsBuilder.Options);
    }
}