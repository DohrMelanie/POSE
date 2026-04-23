using System.Data;
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
    private static List<Teilnehmer> participants = [];
    
    public async Task<int> ImportFromCsvAsync(string csvFilePath, int laufbewerbId, bool isDryRun = false)
    {
        try
        {
            await databaseWriter.BeginTransactionAsync();
        
            var fileContent = await fileReader.ReadAllTextAsync(csvFilePath);
            var data = csvParser.ParseCsv(fileContent);

            var comp = await context.Laufbewerbe.FindAsync(laufbewerbId);
            var splits = data.Rows.Select(r => ConvertToSplit(r, data.Description, comp!)).ToList();
            
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
            /*
            if (participants.Any(p => p.Splits.Count != p.Laufbewerb!.Streckenlänge))
            {
                throw new NotImplementedException();        // 3. Validate that each runner has exactly ⌈Streckenlänge⌉ splits
            }*/

            return participants.Count;
        }
        catch
        {
            await databaseWriter.RollbackTransactionAsync();
            throw;
        }
    }

    private static Split ConvertToSplit(SplitRowData row, string description, Laufbewerb comp)
    {
        var participant = participants.FirstOrDefault(p => p.Startnummer == row.Startnummer);
        if (participant == null)
        {
            participant = new Teilnehmer()
            {
                Startnummer = row.Startnummer,
                AngestrebteGesamtzeit = row.AngestrebteGesamtzeitSek,
                Laufbewerb = comp,
                LaufbewerbId = comp.Id,
                Nachname = row.Nachname,
                Vorname = row.Vorname
            };
            participants.Add(participant);
        }

        var split = new Split()
        {
            KmNummer = row.KmNummer,
            SegmentLaenge = CalculateSegment(row, comp),
            ZeitSekunden = row.ZeitSekunden,
            Teilnehmer = participant,
            TeilnehmerId = participant.Id,
        };
        
        participant.Splits.Add(split);
        
        return split;
    }

    private static decimal CalculateSegment(SplitRowData row, Laufbewerb comp)
    {
        if (row.KmNummer <= comp.Streckenlänge || decimal.IsInteger(comp.Streckenlänge))
        {
            return 1m;
        }

        return row.KmNummer - comp.Streckenlänge;
    }
}
