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
            .Select(c => c.Trim())
            .ToArray();

        if (lines[0].Length == 0)
        {
            throw new SplitParseException(SplitImportError.MissingDescription);
        }

        if (lines[0].Length > 100)
        {
            throw new SplitParseException(SplitImportError.DescriptionTooLong);
        }

        if (lines[1] != "")
        {
            throw new SplitParseException(SplitImportError.MissingEmptyLine);
        }

        if (lines[2] != "Startnummer,Vorname,Nachname,AngestrebteGesamtzeit,KmNummer,Zeit")
        {
            throw new SplitParseException(SplitImportError.InvalidCsvHeader);
        }

        List<SplitRowData> rows = [];

        
        lines = lines.Skip(3).SkipLast(1).ToArray();
        var currentKmNr = 0;
        var runnerData = (StartNr: -1, FirstName: "", LastName: "");
        
        foreach (var line in lines)
        {
            var cols = line.Split(',');
            if (cols.Length != 6)
            {
                throw new SplitParseException(SplitImportError.IncorrectColumnCount);
            }

            if (!int.TryParse(cols[0], out var startNr) || startNr <= 0)
            {
                throw  new SplitParseException(SplitImportError.InvalidStartnummer);
            }

            if (cols[1] == "")
            {
                throw new SplitParseException(SplitImportError.MissingVorname);
            }
            
            if (cols[2] == "")
            {
                throw new SplitParseException(SplitImportError.MissingNachname);
            }

            var totalString = cols[3].Split(':');
            var totalTime = 0;

            if (totalString.Length != 2 && totalString.Length != 3)
            {
                throw new SplitParseException(SplitImportError.InvalidAngestrebteGesamtzeit);
            }
            
            for (int i = 0; i < totalString.Length; i++)
            {
                if (!int.TryParse(totalString[i], out var current) || current < 0 || current > 59)
                {
                    throw new SplitParseException(SplitImportError.InvalidAngestrebteGesamtzeit);
                }
                totalTime += current * (int)Math.Pow(60, totalString.Length - 1 - i);
            }
            
            var timeString = cols[5].Split(':');
            
            if (timeString.Length != 2 && timeString.Length != 3)
            {
                throw new SplitParseException(SplitImportError.InvalidZeit);
            }
            
            var time = 0;
            for (var i = 0; i < timeString.Length; i++)
            {
                if (!int.TryParse(timeString[i], out var current) || current < 0 || current > 59)
                {
                    throw new SplitParseException(SplitImportError.InvalidZeit);
                }
                time += current * (int)Math.Pow(60, totalString.Length - 1 - i);
            }
            
            if (!int.TryParse(cols[4], out var kmNr) || kmNr <= 0)
            {
                throw new SplitParseException(SplitImportError.InvalidKmNummer);
            }
            
            currentKmNr++;
            if (runnerData.StartNr == startNr)
            {
                if (runnerData.FirstName != cols[1] || runnerData.LastName != cols[2])
                {
                    throw new SplitParseException(SplitImportError.InconsistentRunnerData);
                }
            } else if (runnerData.StartNr != -1)
            {
                currentKmNr = 1;
            }

            if (kmNr != currentKmNr)
            {
                throw new SplitParseException(SplitImportError.KmNummerNotConsecutive);
            }
            runnerData = (startNr, cols[1], cols[2]);

            rows.Add(new SplitRowData(startNr, cols[1], cols[2], totalTime, kmNr, time));
        }
        return new ParsedSplitData(lines[0], rows);
    }
}
