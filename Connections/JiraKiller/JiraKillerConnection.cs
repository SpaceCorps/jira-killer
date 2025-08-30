using Ivy.Connections;

namespace JiraKiller.Connections.JiraKiller;

public class JiraKillerConnection : IConnection
{
    public string GetContext(string connectionPath)
    {
        var connectionFile = nameof(JiraKillerConnection) + ".cs";
        var contextFactoryFile = nameof(JiraKillerContextFactory) + ".cs";
        var files = Directory.GetFiles(connectionPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(f => !f.EndsWith(connectionFile) && !f.EndsWith(contextFactoryFile))
            .Select(File.ReadAllText)
            .ToArray();
        return string.Join(Environment.NewLine, files);
    }

    public string GetName() => nameof(JiraKiller);

    public string GetNamespace() => typeof(JiraKillerConnection).Namespace;
    
    public ConnectionEntity[] GetEntities()
    {
        return typeof(JiraKillerContext)
            .GetProperties()
            .Where(e => e.PropertyType.IsGenericType && e.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(e => new ConnectionEntity(e.PropertyType.GenericTypeArguments[0].Name, e.Name))
            .ToArray();
    }

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<JiraKillerContextFactory>();
    }
}