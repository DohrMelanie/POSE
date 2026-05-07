namespace Importer;

public record CommandLineArgs(string CsvFilePath, int LaufbewerbId, bool IsDryRun);

public class CommandLineParser
{
    public static CommandLineArgs Parse(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("No arguments provided");
        }
        
        var isDryRun = args.Contains("--dry-run");
        var filePath = args[0];
        var compId = args.Contains("--laufbewerb-id");

        var id = args.FirstOrDefault(a => int.TryParse(a, out _));

        if (!compId || id is null)
        {
            throw new ArgumentException("Laufbewerb-Id is required");
        }
        return new CommandLineArgs(filePath, int.Parse(id), isDryRun);
    }
}
