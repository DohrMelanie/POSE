using AppServices;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static class AuswertungEndpoints
{
    public static IEndpointRouteBuilder MapAuswertungEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/laufbewerbe/{id:int}/teilnehmer", GetParticipants)
            .Produces<ParticipantDto[]>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetParticipants");
        
        app.MapPost("/laufbewerbe/auswertung", ComputeEvaluation)
            .Produces<EvaluationDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("ComputeEvaluation");

        return app;
    }

    private static async Task<IResult> GetParticipants(ApplicationDataContext db, int id)
    {
        var parts = await db.Teilnehmer
            .Where(t => t.LaufbewerbId == id)
            .Select(t => new ParticipantDto(t.Id, t.Startnummer, t.Vorname, t.Nachname))
            .ToListAsync();

        if (parts.Count == 0)
        {
            return Results.NotFound();
        }
        
        parts = parts.OrderBy(p => p.StartNr).ToList();
        return Results.Ok(parts);
    }

    private static async Task<IResult> ComputeEvaluation(ApplicationDataContext db, EvalReqDto req)
    {
        var participant = await db.Teilnehmer
            .Include(t => t.Laufbewerb)
            .Include(t => t.Splits)
            .FirstOrDefaultAsync(t => t.LaufbewerbId == req.CompId && t.Id == req.ParticipantId);

        if (participant == null ||  participant.Splits.Count == 0)
        {
            return Results.NotFound();
        }

        var targetVelocity = (participant.Laufbewerb!.Streckenlänge / participant.AngestrebteGesamtzeit) * 3600;

        var splits = participant.Splits.Select(s => new SplitDto(
            s.KmNummer,
            s.SegmentLaenge,
            s.ZeitSekunden,
            (s.SegmentLaenge / s.ZeitSekunden) * 3600,
            (s.SegmentLaenge / s.ZeitSekunden) * 3600 <= targetVelocity
        )).ToList();

        var totalTime = splits.Sum(s => s.Time);
        
        var calculation = new EvaluationDto(
            totalTime, 
            (int)(participant.Laufbewerb.Streckenlänge / totalTime) * 3600,
            totalTime <= participant.AngestrebteGesamtzeit, 
            splits);
        
        return Results.Ok(calculation);
    }
}

public record EvalReqDto(
    int ParticipantId,
    int CompId);

public record ParticipantDto(
    int Id,
    int StartNr,
    string FirstName,
    string LastName);
    
public record EvaluationDto(
    int TotalTime,
    int AvgVelocity,
    bool GoalAchieved,
    List<SplitDto> Splits);
    
public record SplitDto(
    int Km,
    decimal Length,
    int Time,
    decimal AvgVelocity,
    bool GoalAchieved);