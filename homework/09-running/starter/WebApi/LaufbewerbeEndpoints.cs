using AppServices;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static class LaufbewerbeEndpoints
{
    public static IEndpointRouteBuilder MapLaufbewerbeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/laufkategorien", GetCategories)
            .WithName("GetCategories")
            .Produces<List<CategoryDto>>(StatusCodes.Status200OK);
        
        app.MapGet("/laufbewerbe", GetCompetitions)
            .WithName("GetCompetitions")
            .Produces<List<CompetitionDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        
        app.MapGet("/laufbewerbe/{id:int}", GetCompetitionById)
            .WithName("GetCompetitionById")
            .Produces<CompetitionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        
        app.MapPost("/laufbewerbe", CreateCompetition)
            .WithName("CreateCompetition");
        
        app.MapDelete("/laufbewerbe/{id:int}", DeleteCompetition)
            .WithName("DeleteCompetition");
        
        app.MapPut("/laufbewerbe", UpdateCompetition)
            .WithName("UpdateCompetition");

        return app;
    }

    private static async Task<IResult> GetCategories(ApplicationDataContext db)
    {
        var categories = await db.Laufkategorien
            .Select(k => new CategoryDto(k.Id, k.Bezeichnung))
            .ToListAsync();
        
        return Results.Ok(categories);
    }
    private static async Task<IResult> GetCompetitions(ApplicationDataContext db, string? name, int? categoryId)
    {
        var competitions = await db.Laufbewerbe
            .Include(l => l.Laufkategorie)
            .Select(k => new CompetitionDto(k.Id, k.Name, new CategoryDto(k.LaufkategorieId, k.Laufkategorie!.Bezeichnung), k.Streckenlänge, k.Datum, k.Ort))
            .ToListAsync();

        if (name != null)
        {
            competitions = competitions.Where(c => c.Name == name).ToList();
        }

        if (categoryId != null)
        {
            competitions = competitions.Where(c => c.Category.Id == categoryId).ToList();
        }

        competitions = competitions.OrderByDescending(dto => dto.Date).ToList();

        return competitions.Count == 0 ? Results.NotFound() : Results.Ok(competitions);
    }
    private static async Task<IResult> GetCompetitionById(ApplicationDataContext db, int id)
    {
        var k = await db.Laufbewerbe.Include(l => l.Laufkategorie).FirstOrDefaultAsync(k => k.Id == id);
        return k  == null ? Results.NotFound() : Results.Ok(new CompetitionDto(k.Id, k.Name, new CategoryDto(k.LaufkategorieId, k.Laufkategorie!.Bezeichnung), k.Streckenlänge, k.Datum, k.Ort));
    }
    private static async Task<IResult> CreateCompetition(ApplicationDataContext db, CompetitionReqDto dto)
    {
        if (dto.Name.Length > 100)
        {
            return Results.BadRequest("Name too long (max. 100 chars)");
        }

        if (dto.Length < 0.01m)
        {
            return Results.BadRequest("Length too short.");
        } // todo max 2 decimal places
        
        if (dto.Place.Length > 100)
        {
            return Results.BadRequest("Place too long (max. 100 chars)");
        }

        var category = await db.Laufkategorien.FirstOrDefaultAsync(c => c.Id == dto.Category.Id);

        await db.Laufbewerbe.AddAsync(new Laufbewerb()
        {
            Datum = DateOnly.FromDateTime(DateTime.Now),
            Name = dto.Name,
            Ort = dto.Place,
            Laufkategorie = category,
            LaufkategorieId = category!.Id,
            Streckenlänge = dto.Length
        });
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    private static async Task<IResult> DeleteCompetition(ApplicationDataContext db, int id)
    {
        var comp = await db.Laufbewerbe.FirstOrDefaultAsync(k => k.Id == id);
        if (comp == null)
        {
            return Results.NotFound();
        }
        db.Laufbewerbe.Remove(comp);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    
    private static async Task<IResult> UpdateCompetition(ApplicationDataContext db, CompetitionDto dto)
    {
        var comp = await db.Laufbewerbe.FirstOrDefaultAsync(k => k.Id == dto.Id);
        if (comp == null)
        {
            return Results.NotFound();
        }
        if (dto.Name.Length > 100)
        {
            return Results.BadRequest("Name too long (max. 100 chars)");
        }

        if (dto.Length < 0.01m)
        {
            return Results.BadRequest("Length too short.");
        } // todo max 2 decimal places
        
        if (dto.Place.Length > 100)
        {
            return Results.BadRequest("Place too long (max. 100 chars)");
        }
        var category = await db.Laufkategorien.FirstOrDefaultAsync(c => c.Id == dto.Category.Id);
        
        comp.Name = dto.Name;
        comp.Datum = dto.Date;
        comp.Laufkategorie = category;
        comp.LaufkategorieId = category!.Id;
        comp.Ort = dto.Place;
        comp.Streckenlänge = dto.Length;
        
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}

public record CategoryDto(int Id, string Name);
public record CompetitionDto(
    int Id,
    string Name,
    CategoryDto Category,
    decimal Length,
    DateOnly Date,
    string Place);
    
public record CompetitionReqDto(
    string Name,
    CategoryDto Category,
    decimal Length,
    string Place);
    