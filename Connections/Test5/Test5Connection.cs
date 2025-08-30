using Ivy.Connections;

namespace Test5.Connections.Test5;

public class Test5Connection : IConnection
{
    public string GetContext(string connectionPath)
    {
        var connectionFile = nameof(Test5Connection) + ".cs";
        var contextFactoryFile = nameof(Test5ContextFactory) + ".cs";
        var files = Directory.GetFiles(connectionPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(f => !f.EndsWith(connectionFile) && !f.EndsWith(contextFactoryFile))
            .Select(File.ReadAllText)
            .ToArray();
        return string.Join(Environment.NewLine, files);
    }

    public string GetName() => nameof(Test5);

    public string GetNamespace() => typeof(Test5Connection).Namespace;
    
    public ConnectionEntity[] GetEntities()
    {
        return typeof(Test5Context)
            .GetProperties()
            .Where(e => e.PropertyType.IsGenericType && e.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(e => new ConnectionEntity(e.PropertyType.GenericTypeArguments[0].Name, e.Name))
            .ToArray();
    }

    public void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<Test5ContextFactory>();
    }
}