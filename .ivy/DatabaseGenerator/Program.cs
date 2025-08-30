using Test5;
using System.ComponentModel;
using Spectre.Console.Cli;

var app = new CommandApp<GenerateCommand>();

app.Configure(config =>
{
    config.PropagateExceptions();
});

await app.RunAsync(args);

public class GenerateCommand : AsyncCommand<GenerateCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--verbose")]
        [Description("Enable verbose output")]
        public bool Verbose { get; set; } = false;

        [CommandOption("--output")]
        [Description("Output directory for generated files.")]
        public string? OutputDirectory { get; set; } = null;

        [CommandOption("--data-provider")]
        [Description("The database provider")]
        public DatabaseProvider? DataProvider { get; set; }

        [CommandOption("--connection-string")]
        [Description("Connection string for the database.")]
        public string? ConnectionString { get; set; } = null;

        [CommandOption("--yes-to-all")]
        [Description("Automatically answer yes to all questions.")]
        public bool YesToAll { get; set; } = false;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var generator = new DatabaseGenerator();
        try {
            return await generator.GenerateAsync(
                        typeof(DataContext),
                        typeof(DataSeeder),
                        settings.Verbose,
                        settings.YesToAll,
                        settings.OutputDirectory,
                        settings.ConnectionString,
                        settings.DataProvider
                    );
        } catch (Exception ex) {
            System.Console.Error.WriteLine($"{ex.Message}");
            return 1;
        }
    }
}