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
            var entries = await db.TimeEntries
                .Include(te => te.Employee)
                .Include(te => te.Project)
                .Where(te => employeeId == null || te.EmployeeId == employeeId)
                .Where(te => projectId == null || te.ProjectId == projectId)
                .Select(te => new TimeEntryDto(te.Id, te.Date, te.StartTime, te.EndTime, te.Description, te.EmployeeId, te.Employee!.EmployeeName, te.ProjectId, te.Project!.ProjectCode))
                .ToListAsync();
            return entries;
        }).Produces<List<TimeEntryDto>>()
        .Produces(StatusCodes.Status404NotFound) // when filters are not matched
        .WithName("GetTimeEntries")
        .WithDescription("Gets all or filtered timeentries");

        app.MapPut("/timeentries/{id:int}", async (int id, TimeEntryUpdateDto dto, ApplicationDataContext db) =>
        {
            var entry = await db.TimeEntries.FindAsync(id);
            if (entry == null)
            {
                return Results.NotFound();
            }
            entry.Date = dto.Date;
            entry.StartTime = dto.StartTime;
            entry.EndTime = dto.EndTime;
            entry.Description = dto.Description;
            entry.EmployeeId = dto.EmployeeId;
            entry.ProjectId = dto.ProjectId;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithName("UpdateTimeEntry")
        .WithDescription("Updates time entry");

        app.MapGet("/timeentries/{id:int}", async (int id, ApplicationDataContext db) =>
        {
            var entry =  await db.TimeEntries
                .Include(te => te.Employee)
                .Include(te => te.Project)
                .FirstOrDefaultAsync(te => te.Id == id);
            if (entry == null)
            {
                return Results.NotFound();
            }
            var entryDto = new TimeEntryDto(
                entry.Id, 
                entry.Date, 
                entry.StartTime, 
                entry.EndTime, 
                entry.Description, 
                entry.EmployeeId, 
                entry.Employee!.EmployeeName, 
                entry.ProjectId, 
                entry.Project!.ProjectCode);
            
            return Results.Ok(entryDto);
        }).Produces<TimeEntryDto>()
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

public record TimeEntryDto(
    int Id,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Description,
    int EmployeeId,
    string EmployeeName,
    int ProjectId,
    string ProjectCode);