using AppServices;
using Microsoft.EntityFrameworkCore;

namespace WebApi;

public static class TimeTrackingEndpoints
{
    public static IEndpointRouteBuilder MapTimeTrackingEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/employees", (ApplicationDataContext db) => db.Employees)
            .Produces<List<Employee>>()
            .WithName("GetEmployees")
            .WithDescription("Gets all employees");
        
        app.MapGet("/projects", (ApplicationDataContext db) => db.Projects)
            .Produces<List<Project>>()
            .WithName("GetProjects")
            .WithDescription("Gets all projects");

        app.MapGet("/timeentries", async (ApplicationDataContext db, int? employeeId, int? projectId) =>
        {
            // TODO
        }).Produces<List<TimeEntry>>()
        .Produces(StatusCodes.Status404NotFound) // when filters are not matched
        .WithName("GetTimeEntries")
        .WithDescription("Gets all or filtered timeentries");

        app.MapPut("/timeentries/{id}", async (int id, TimeEntryUpdateDto dto, ApplicationDataContext db) =>
        {
            // TODO (also validate all fields)
        }).Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("UpdateTimeEntry")
        .WithDescription("Updates time entry");

        app.MapGet("/timeentries/{id}", async (int id, ApplicationDataContext db) =>
        {
            var entry =  await db.TimeEntries
                .Include(te => te.Employee)
                .Include(te => te.Project)
                .FirstOrDefaultAsync(te => te.Id == id);
            return entry == null ? Results.NotFound() : Results.Ok(entry);
        }).Produces<TimeEntry>()
        .Produces(StatusCodes.Status404NotFound)
        .WithName("GetTimeEntry")
        .WithDescription("Gets time entry");
        
        app.MapDelete("/timeentries/{id:int}", async (int id, ApplicationDataContext db) =>
        {
            var project = await db.Projects.FindAsync(id);
            if (project == null)
            {
                return Results.NotFound();
            }
            db.Projects.Remove(project);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).Produces(StatusCodes.Status204NoContent);
        return app;
    }
}

public record TimeEntryUpdateDto(
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Description,
    int EmployeeId,
    int ProjectId);

