namespace AppServices.Importer;

public record SplitRowData(int Startnummer, string Vorname, string Nachname, int AngestrebteGesamtzeitSek, int KmNummer, int ZeitSekunden);
public record ParsedSplitData(string Description, List<SplitRowData> Rows);

public interface ISplitCsvParser
{
    ParsedSplitData ParseCsv(string csvContent);
}

public enum SplitImportError
{
    MissingDescription,
    DescriptionTooLong,
    MissingEmptyLine,
    MissingCsvHeader,
    InvalidCsvHeader,
    IncorrectColumnCount,
    InvalidStartnummer,
    MissingVorname,
    MissingNachname,
    InvalidAngestrebteGesamtzeit,
    InconsistentRunnerData,
    InvalidKmNummer,
    KmNummerNotConsecutive,
    InvalidZeit,
}

public class SplitParseException(SplitImportError errorCode)
    : Exception(ErrorMessages.TryGetValue(errorCode, out var message) ? message : "Unknown parsing error.")
{
    private static readonly Dictionary<SplitImportError, string> ErrorMessages = new()
    {
        { SplitImportError.MissingDescription, "Description (line 1) is missing or empty." },
        { SplitImportError.DescriptionTooLong, "Description (line 1) exceeds maximum length of 100 characters." },
        { SplitImportError.MissingEmptyLine, "Line 2 must be empty." },
        { SplitImportError.MissingCsvHeader, "CSV header (line 3) is missing." },
        { SplitImportError.InvalidCsvHeader, "CSV header (line 3) must be exactly: Startnummer,Vorname,Nachname,AngestrebteGesamtzeit,KmNummer,Zeit (in this order)." },
        { SplitImportError.IncorrectColumnCount, "Incorrect number of columns in data row." },
        { SplitImportError.InvalidStartnummer, "Invalid Startnummer; must be a positive integer." },
        { SplitImportError.MissingVorname, "Vorname is missing." },
        { SplitImportError.MissingNachname, "Nachname is missing." },
        { SplitImportError.InvalidAngestrebteGesamtzeit, "Invalid AngestrebteGesamtzeit; expected (H:)MM:SS with seconds 0-59." },
        { SplitImportError.InconsistentRunnerData, "Runner metadata (Vorname, Nachname, or AngestrebteGesamtzeit) are inconsistent for the same Startnummer." },
        { SplitImportError.InvalidKmNummer, "Invalid KmNummer; must be a positive integer." },
        { SplitImportError.KmNummerNotConsecutive, "KmNummern are not consecutively ascending starting from 1." },
        { SplitImportError.InvalidZeit, "Invalid Zeit; expected MM:SS with seconds 0-59 and value > 0." },
    };

    public SplitImportError ErrorCode { get; } = errorCode;
}

public class SplitCsvParser : ISplitCsvParser
{
    public ParsedSplitData ParseCsv(string csvContent)
    {
        var lines = csvContent.Split(["\r\n", "\n"], StringSplitOptions.None)
            .Select(line =>  line.Trim())
            .ToArray();
        
        ValidateHeader(lines);
        
        var currentRunner = (StartNr: -1, FirstName: "", LastName: "", TotalTime: -1, NextKm: 1);

        List<SplitRowData> rows = [];
        for (var i = 3; i < lines.Length - 1; i++)
        {
            var parts = lines[i].Split(",");
            if (parts.Length != 6)
            {
                throw new SplitParseException(SplitImportError.IncorrectColumnCount);
            }

            if (!int.TryParse(parts[0], out var startNr) || startNr < 0)
            {
                throw new SplitParseException(SplitImportError.InvalidStartnummer);
            }

            var firstName = parts[1];
            if (firstName == string.Empty)
            {
                throw new SplitParseException(SplitImportError.MissingVorname);
            }

            var lastName = parts[2];
            if (lastName == string.Empty)
            {
                throw new SplitParseException(SplitImportError.MissingNachname);
            }

            var totalTime = GetTime(parts[3].Split(":"), SplitImportError.InvalidAngestrebteGesamtzeit);
            var time = GetTime(parts[5].Split(":"), SplitImportError.InvalidZeit);

            if (currentRunner.StartNr != -1 && currentRunner.StartNr == startNr)
            {
                if (currentRunner.FirstName != firstName
                     || currentRunner.LastName != lastName
                     || currentRunner.TotalTime != totalTime)
                {
                    throw new SplitParseException(SplitImportError.InconsistentRunnerData);
                }
            }
            else
            {
                currentRunner.NextKm = 1;
            }
            
            if (!int.TryParse(parts[4], out var kmNr) || kmNr < 0)
            {
                throw new SplitParseException(SplitImportError.InvalidKmNummer);
            }

            if (kmNr != currentRunner.NextKm)
            {
                throw new SplitParseException(SplitImportError.KmNummerNotConsecutive);
            }

            var row = new SplitRowData(startNr, firstName, lastName, totalTime, kmNr, time);
            rows.Add(row);

            currentRunner = (row.Startnummer, row.Vorname, row.Nachname, row.AngestrebteGesamtzeitSek, row.KmNummer + 1);
        }
        return new ParsedSplitData(lines[0], rows);
    }

    private static void ValidateHeader(string[] lines)
    {
        if (lines[0] == string.Empty)
        {
            throw new SplitParseException(SplitImportError.MissingDescription);
        }
        
        if (lines[0].Length > 100)
        {
            throw new SplitParseException(SplitImportError.DescriptionTooLong);
        }
        
        if (lines[1] != string.Empty)
        {
            throw new SplitParseException(SplitImportError.MissingEmptyLine);
        }

        if (lines[2] != "Startnummer,Vorname,Nachname,AngestrebteGesamtzeit,KmNummer,Zeit")
        {
            throw new SplitParseException(SplitImportError.InvalidCsvHeader);
        }
    }

    private static int GetTime(string[] timeParts, SplitImportError error)
    {
        if (timeParts.Length is < 2 or > 3)
        {
            throw new SplitParseException(error);
        }

        var totalTime = 0;
        for (var j = 0; j < timeParts.Length; j++)
        {
            var current = int.Parse(timeParts[j]);
            if (current is < 0 or > 59)
            {
                throw new SplitParseException(error);
            }

            totalTime += current * (int) Math.Pow(60, timeParts.Length - j - 1);
        }

        return totalTime;
    }
}
