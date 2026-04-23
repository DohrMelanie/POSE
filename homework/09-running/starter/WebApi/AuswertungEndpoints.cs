using AppServices;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static class AuswertungEndpoints
{
    public static IEndpointRouteBuilder MapAuswertungEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/laufbewerbe/{id:int}/teilnehmer", GetParticipants)
            .Produces<ParticipantDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        
        app.MapPost("/laufbewerbe/auswertung", ComputeEvaluation)
            .Produces<EvaluationDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        
        // TODO   POST /laufbewerbe/auswertung       - Compute split evaluation for a Teilnehmer

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
        throw new NotImplementedException();
    }
}

public record EvalReqDto(
    int ParticipantId);

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
    int Length,
    string Time,
    string AvgVelocity,
    bool GoalAchieved);