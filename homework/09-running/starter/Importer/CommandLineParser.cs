namespace Importer;

public record CommandLineArgs(string CsvFilePath, int LaufbewerbId, bool IsDryRun);

public class CommandLineParser
{
    public static CommandLineArgs Parse(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Please provide a product file path as a command line argument.\nUsage: Importer <file-path> [--laufbewerb-id] [--dry-run]");
        }

        var filePath = args[0];
        var isDryRun = args.Any(arg => arg == "--dry-run");
        var compId = args.FirstOrDefault(arg => int.TryParse(arg, out _));
        if (compId == null || args.All(arg => arg != "--laufbewerb-id"))
        {
            throw new ArgumentException("Please provide a valid id");
        }
        return new CommandLineArgs(filePath, int.Parse(compId), isDryRun);
    }
}
