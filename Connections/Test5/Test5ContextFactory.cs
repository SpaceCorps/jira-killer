using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Test5.Connections.Test5;

public class Test5ContextFactory(ServerArgs args) : IDbContextFactory<Test5Context>
{
    public Test5Context CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<Test5Context>();

        optionsBuilder.UseSqlite("Data Source=D:\\git\\test5\\db.sqlite");

        if (args.Verbose)
        {
            optionsBuilder
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information);
        }

        return new Test5Context(optionsBuilder.Options);
    }
}