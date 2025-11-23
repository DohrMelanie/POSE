namespace Importer;

/// <summary>
/// Result of command line parsing
/// </summary>
public record CommandLineArgs(string TxtFilePath, bool IsDryRun);

/// <summary>
/// Parser for command line arguments
/// </summary>
public class CommandLineParser
{
    public CommandLineArgs Parse(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentException("Please provide a TXT file path as a command line argument.\nUsage: Importer <txt-file-path> [--dry-run]");
        }

        var txtFilePath = args[0];
        var isDryRun = args.Any(arg => arg == "--dry-run");

        return new CommandLineArgs(txtFilePath, isDryRun);
    }
}
