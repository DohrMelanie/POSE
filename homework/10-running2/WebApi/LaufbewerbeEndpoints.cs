using AppServices;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static class LaufbewerbeEndpoints
{
    public static IEndpointRouteBuilder MapLaufbewerbeEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/laufkategorien", GetCategories)
            .WithName("GetCategories")
            .Produces<List<CategoryDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        
        app.MapGet("/laufbewerbe", GetCompetitions)
            .WithName("GetCompetitions")
            .Produces<List<CompetitionDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
        
        app.MapGet("/laufbewerbe/{id:int}", GetCompetitionById)
            .WithName("GetCompetitionById")
            .Produces<CompetitionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/laufbewerbe", CreateCompetition)
            .WithName("CreateCompetition")
            .Produces(StatusCodes.Status201Created);

        app.MapPut("/laufbewerbe", UpdateCompetition) // put instead of patch because in this context it makes more sense to update the entire resource
            .WithName("UpdateCompetition")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        
        app.MapDelete("/laufbewerbe/{id:int}", DeleteCompetitionById)
            .WithName("DeleteCompetitionById")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        
        return app;
    }

    private static async Task<IResult> GetCategories(ApplicationDataContext db)
    {
        var categories = await db.Laufkategorien.Select(k => new CategoryDto(k.Id, k.Bezeichnung)).ToListAsync();
        return Results.Ok(categories);
    }
    private static async Task<IResult> GetCompetitions(ApplicationDataContext db, string? name, int? categoryId)
    {
        var comps = await db.Laufbewerbe
            .Include(b => b.Laufkategorie)
            .Select(b => new CompetitionDto(b.Id, b.Name, b.LaufkategorieId, b.Streckenlänge, b.Datum, b.Ort))
            .ToListAsync();
        
        if (categoryId != null)
        {
            comps = comps.Where(c => c.CategoryId == categoryId).ToList();
        }

        if (name != null)
        {
            comps = comps.Where(c => c.Name == name).ToList();
        }

        return Results.Ok(comps.OrderByDescending(c => c.Date)); // according to the assignment in the frontend, it should return an empty list if no competitions are found
    }
    private static async Task<IResult> GetCompetitionById(ApplicationDataContext db, int id)
    {
        var b = await db.Laufbewerbe
            .Include(b => b.Laufkategorie)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (b == null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new CompetitionDto(b.Id, b.Name,b.LaufkategorieId, b.Streckenlänge, b.Datum, b.Ort));

    }
    private static async Task<IResult> CreateCompetition(ApplicationDataContext db, CompetitionReqDto comp)
    {
        if (comp.Name.Length is 0 or > 100)
        {
            return Results.BadRequest("Name should be between 0 and 100 characters!");
        }

        if (comp.Length < 0.01m || (comp.Length * 100m) % 1m != 0m)
        {
            return Results.BadRequest("Length should be min. 0.01 and 2 decimal places!");
        }
        
        if (comp.Place.Length is 0 or > 100)
        {
            return Results.BadRequest("Place should be between 0 and 100 characters!");
        }

        var cat = await db.Laufkategorien.FirstOrDefaultAsync(k => k.Id == comp.CategoryId);

        if (cat == null)
        {
            return Results.NotFound();
        }

        await db.Laufbewerbe.AddAsync(new Laufbewerb()
        {
            Datum = comp.Date,
            Laufkategorie = cat,
            LaufkategorieId = comp.CategoryId,
            Name = comp.Name,
            Ort = comp.Place,
            Streckenlänge = comp.Length
        });
        await db.SaveChangesAsync();
        return Results.Created();
    }
    private static async Task<IResult> UpdateCompetition(ApplicationDataContext db, CompetitionDto comp)
    {
        if (comp.Name.Length is 0 or > 100)
        {
            return Results.BadRequest("Name should be between 0 and 100 characters!");
        }

        if (comp.Length < 0.01m || (comp.Length * 100m) % 1m != 0m)
        {
            return Results.BadRequest("Length should be min. 0.01 and 2 decimal places!");
        }
        
        if (comp.Place.Length is 0 or > 100)
        {
            return Results.BadRequest("Place should be between 0 and 100 characters!");
        }    
        
        var cat = await db.Laufkategorien.FirstOrDefaultAsync(k => k.Id == comp.CategoryId);

        if (cat == null)
        {
            return Results.NotFound();
        }
        var b = await db.Laufbewerbe
            .Include(b => b.Laufkategorie)
            .FirstOrDefaultAsync(b => b.Id == comp.Id);

        if (b == null)
        {
            return Results.NotFound();
        }
        
        b.Name = comp.Name;
        b.Ort = comp.Place;
        b.Laufkategorie = cat;
        b.Datum = comp.Date;
        b.Streckenlänge = comp.Length;
        
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    private static async Task<IResult> DeleteCompetitionById(ApplicationDataContext db, int id)
    {
        var b = await db.Laufbewerbe
            .Include(b => b.Laufkategorie)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (b == null)
        {
            return Results.NotFound();
        }
        
        db.Laufbewerbe.Remove(b);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}


public record CategoryDto(int Id, string Name);

public record CompetitionReqDto(
    string Name,
    int CategoryId,
    decimal Length,
    DateOnly Date,
    string Place
    );

public record CompetitionDto(
    int Id,
    string Name,
    int CategoryId,
    decimal Length,
    DateOnly Date,
    string Place);
    