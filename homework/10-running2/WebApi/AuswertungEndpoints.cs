using AppServices;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;

namespace WebApi;

public static class AuswertungEndpoints
{
    public static IEndpointRouteBuilder MapAuswertungEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/laufbewerbe/{compId:int}/teilnehmer", GetParticipants)
            .WithName("GetParticipants")
            .Produces<List<ParticipantDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        
        app.MapPost("/laufbewerbe/auswertung", ComputeEvaluation)
            .WithName("ComputeEvaluation")
            .Produces<EvaluationDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        return app;
    }

    private static async Task<IResult> GetParticipants(ApplicationDataContext db, int compId)
    {
        var people = await db.Teilnehmer
            .Where(t => t.LaufbewerbId == compId)
            .Select(t => new ParticipantDto(t.Startnummer, t.Id, t.Vorname, t.Nachname))
            .ToListAsync();

        return people.Count == 0 ? Results.NotFound() : Results.Ok(people.OrderBy(p => p.StartNr));
    }

    private static async Task<IResult> ComputeEvaluation(ApplicationDataContext db, ComputeReqDto req)
    {
        var participant = await db.Teilnehmer
            .Include(t => t.Laufbewerb)
            .Include(t => t.Splits)
            .FirstOrDefaultAsync(b => b.Id == req.ParticipantId);

        if (participant == null)
        {
            return Results.NotFound();
        }

        var totalTime = participant.Splits.Sum(s => s.ZeitSekunden);
        var targetVelocity = (participant.Laufbewerb!.Streckenlänge / participant.AngestrebteGesamtzeit) * 3600;
        var avgTime = (participant.Laufbewerb.Streckenlänge / totalTime) * 3600;
        var splits = participant!
                .Splits
            .Select(s => new SplitEvaluationDto(
                    s.KmNummer, s.SegmentLaenge, 
                    s.ZeitSekunden, 
                (s.SegmentLaenge / s.ZeitSekunden) * 3600, 
                (s.SegmentLaenge / s.ZeitSekunden) * 3600 <= targetVelocity))
            .ToList();
        
        return Results.Ok(new EvaluationDto(totalTime, avgTime, totalTime <= participant.AngestrebteGesamtzeit, splits));
    }
}

public record ParticipantDto(
    int StartNr,
    int Id,
    string FirstName,
    string LastName);

public record ComputeReqDto(
    int ParticipantId);

public record SplitEvaluationDto(
    int Km,
    decimal SegmentLength,
    int Time,
    decimal AvgVelocity,
    bool TargetReached);

public record EvaluationDto(
    decimal TotalTime,
    decimal AvgTime,
    bool TargetReached,
    List<SplitEvaluationDto> splits);
