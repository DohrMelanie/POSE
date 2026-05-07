using Microsoft.EntityFrameworkCore;

namespace AppServices.Importer;

public interface ISplitImporter
{
    Task<int> ImportFromCsvAsync(string csvFilePath, int laufbewerbId, bool isDryRun = false);
}

public class SplitImporter(
    IFileReader fileReader,
    ISplitCsvParser csvParser,
    ISplitDatabaseWriter databaseWriter,
    ApplicationDataContext context) : ISplitImporter
{
    private List<Teilnehmer> participants = [];

    public async Task<int> ImportFromCsvAsync(string csvFilePath, int laufbewerbId, bool isDryRun = false)
    {
        await databaseWriter.BeginTransactionAsync();

        try
        {
            var fileContent = await fileReader.ReadAllTextAsync(csvFilePath);
            var parsed = csvParser.ParseCsv(fileContent);
            var comp = await context.Laufbewerbe.FirstOrDefaultAsync(l => l.Id == laufbewerbId);
            var splits = parsed.Rows.Select(s => ConvertToSplit(s, comp!)).ToList();

            if (participants.Any(p => p.Splits.Count != comp!.Streckenlänge))
            {
                throw new ArgumentException("Split count mismatch");
            }
            
            await databaseWriter.ClearTeilnehmerAsync();
            await databaseWriter.WriteTeilnehmerAsync(participants);

            if (isDryRun)
            {
                await databaseWriter.RollbackTransactionAsync();
            }
            else
            {
                await databaseWriter.CommitTransactionAsync();
            }

            return participants.Count;
        }
        catch
        {
            await databaseWriter.RollbackTransactionAsync();
            throw;
        }
    }

    private Split ConvertToSplit(SplitRowData data, Laufbewerb comp)
    {
        var teilnehmer = participants.FirstOrDefault(p => p.Startnummer == data.Startnummer);

        if (teilnehmer == null)
        {
            teilnehmer = new Teilnehmer()
            {
                Startnummer = data.Startnummer,
                Vorname = data.Vorname,
                Nachname = data.Nachname,
                AngestrebteGesamtzeit = data.AngestrebteGesamtzeitSek,
                LaufbewerbId = comp.Id,
                Laufbewerb = comp
            };
            participants.Add(teilnehmer);
        }

        decimal segment;
        if (decimal.IsInteger(comp!.Streckenlänge) || Math.Floor(comp!.Streckenlänge) > data.KmNummer)
        {
            segment = 1m;
        }
        else
        {
            segment = comp!.Streckenlänge - data.KmNummer;
        }

        var split = new Split()
        {
            KmNummer = data.KmNummer,
            Teilnehmer = teilnehmer,
            TeilnehmerId = teilnehmer.Id,
            ZeitSekunden = data.ZeitSekunden,
            SegmentLaenge = segment
        };
        teilnehmer.Splits.Add(split);

        return split;
    }
}