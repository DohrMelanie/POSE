using System.Diagnostics.CodeAnalysis;

namespace AppServices.Importer;

/// <summary>
/// Interface for parsing timesheet files into objects
/// </summary>
public interface ITimesheetParser
{
    /// <summary>
    /// Parses CSV content into a list of TimeEntry objects
    /// </summary>
    /// <param name="csvContent">CSV content as string</param>
    /// <param name="existingEmployees">Existing employees in the database</param>
    /// <param name="existingProjects">Existing projects in the database</param>
    /// <returns>List of parsed TimeEntry objects</returns>
    /// <exception cref="TimesheetParseException">
    /// Thrown when file content is invalid.
    /// </exception>
    /// <remarks>
    /// Note that this method must link TimeEntry objects to existing
    /// Employee and Project entities from the database where possible.
    /// 
    /// If an Employee (based on Employee ID) exists in the database, the
    /// employee's name will be updated if it differs from the CSV content.
    /// If a Project from the CSV does not exist in the database, a new
    /// entity must be created.
    /// 
    /// Multiple time entries with the same project code MUST reference 
    /// the same Project object. You must ensure that duplicate Project
    /// entities are not created for the same project code.
    /// </remarks>
    IEnumerable<TimeEntry> ParseCsv(string csvContent, IEnumerable<Employee> existingEmployees, IEnumerable<Project> existingProjects);
}

/// <summary>
/// Represents all possible validation errors for the time tracking import file format.
/// </summary>
public enum ImportFileError
{
    // Employee Identification Errors
    MissingEmployeeId,
    MissingEmployeeName,
    DuplicateEmployeeId,
    DuplicateEmployeeName,
    EmployeeIdTooLong,              // Max 5 characters
    EmployeeNameTooLong,            // Max 100 characters
    EmployeeIdNotNumeric,

    // Field Format Errors
    InvalidKeyValueFormat,          // Missing ": " separator
    LeadingWhitespace,
    TrailingWhitespace,
    UnknownKey,                     // e.g., DEPARTMENT, TIMESHEET (singular)
    EmptyValue,

    // Timesheet Section Errors
    MissingTimesheetSection,        // No TIMESHEETS sections found
    TimesheetSectionBeforeEmployeeData,
    EmptyTimesheetSection,          // TIMESHEETS section with no time entries

    // Date Errors
    InvalidDate,                    // Not YYYY-MM-DD or invalid date

    // Time Entry Field Errors
    IncorrectFieldCount,            // Not exactly 4 semicolon-delimited fields
    EmptyField,                     // One or more fields are empty

    // Time Format Errors
    InvalidTime,                    // Not HH:MM or invalid time
    EndTimeBeforeStartTime,         // Logical validation

    // Description Errors
    DescriptionNotQuoted,           // Missing opening or closing quotes
    DescriptionTooLong,             // Max 200 characters

    // Project Errors
    ProjectTooLong,                 // Max 20 characters
    ProjectQuoted,                  // Project should NOT be quoted
}

public class TimesheetParseException(ImportFileError errorCode)
    : Exception(ErrorMessages.TryGetValue(errorCode, out var message) ? message : "Unknown parsing error.")
{
    private static readonly Dictionary<ImportFileError, string> ErrorMessages = new()
    {
        { ImportFileError.MissingEmployeeId, "Employee ID is missing." },
        { ImportFileError.MissingEmployeeName, "Employee name is missing." },
        { ImportFileError.DuplicateEmployeeId, "Duplicate employee ID found." },
        { ImportFileError.DuplicateEmployeeName, "Duplicate employee name found." },
        { ImportFileError.EmployeeIdTooLong, "Employee ID exceeds maximum length of 5 characters." },
        { ImportFileError.EmployeeNameTooLong, "Employee name exceeds maximum length of 100 characters." },
        { ImportFileError.EmployeeIdNotNumeric, "Employee ID must be numeric." },
        { ImportFileError.InvalidKeyValueFormat, "Invalid key-value format; missing ': ' separator." },
        { ImportFileError.LeadingWhitespace, "Leading whitespace detected in field." },
        { ImportFileError.TrailingWhitespace, "Trailing whitespace detected in field." },
        { ImportFileError.UnknownKey, "Unknown key found in the file." },
        { ImportFileError.EmptyValue, "Field value cannot be empty." },
        { ImportFileError.MissingTimesheetSection, "No TIMESHEETS section found in the file." },
        { ImportFileError.TimesheetSectionBeforeEmployeeData, "TIMESHEETS section appears before employee data." },
        { ImportFileError.EmptyTimesheetSection, "TIMESHEETS section is empty." },
        { ImportFileError.InvalidDate, "Invalid date format; expected YYYY-MM-DD." },
        { ImportFileError.IncorrectFieldCount, "Incorrect number of fields in time entry; expected 4 fields." },
        { ImportFileError.EmptyField, "One or more fields in time entry are empty." },
        { ImportFileError.InvalidTime, "Invalid time format; expected HH:MM." },
        { ImportFileError.EndTimeBeforeStartTime, "End time is before start time." },
        { ImportFileError.DescriptionNotQuoted, "Description field must be enclosed in double quotes." },
        { ImportFileError.DescriptionTooLong, "Description exceeds maximum length of 200 characters." },
        { ImportFileError.ProjectTooLong, "Project code exceeds maximum length of 20 characters." },
        { ImportFileError.ProjectQuoted, "Project code must not be quoted." },
    };

    public ImportFileError ErrorCode { get; } = errorCode;
}

/// <summary>
/// Implementation for parsing CSV content into TimeEntry objects
/// </summary>
public class TimesheetParser : ITimesheetParser
{
    /// <inheritdoc/>
    public IEnumerable<TimeEntry> ParseCsv(string csvContent, IEnumerable<Employee> existingEmployees, IEnumerable<Project> existingProjects)
    {
        var lines = csvContent.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries);
        List<TimeEntry> entries = [];
        var empId = string.Empty;
        var empName = string.Empty;
        DateOnly? date = null;
        var projectCache = new Dictionary<string, Project>(StringComparer.Ordinal);
        foreach (var p in existingProjects)
        {
            if (p.ProjectCode is { } code)
            {
                projectCache.TryAdd(code, p);
            }
        }

        if (!lines.Any(line => line.Contains("TIMESHEETS")))
        {
            throw new TimesheetParseException(ImportFileError.MissingTimesheetSection);
        }

        if (csvContent.IndexOf("TIMESHEETS", StringComparison.Ordinal) < csvContent.IndexOf("EMP-ID:", StringComparison.Ordinal))
        {
            throw new TimesheetParseException(ImportFileError.TimesheetSectionBeforeEmployeeData);
        }

        Employee? currentEmployee = null;
        
        foreach (var line in lines)
        {
            if (!(line.StartsWith("TIMESHEETS: ") || line.StartsWith("EMP-ID: ") || line.StartsWith("EMP-NAME: ") ||
                  line.Contains(';')) && line.Contains(':'))
            {
                throw new TimesheetParseException(ImportFileError.UnknownKey);
            }
            
            if (line.StartsWith("EMP-ID:"))
            {
                CheckForFieldErrors(line);
                if (!string.IsNullOrEmpty(empId))
                {
                    throw new TimesheetParseException(ImportFileError.DuplicateEmployeeId);
                }
                
                empId = line.Replace("EMP-ID: ", "");
                if (empId.Length > 5)
                {
                    throw new TimesheetParseException(ImportFileError.EmployeeIdTooLong);
                }
                
                if (!int.TryParse(empId, out _))
                {
                    throw new TimesheetParseException(ImportFileError.EmployeeIdNotNumeric);
                }
            }

            if (line.StartsWith("EMP-NAME:"))
            {
                CheckForFieldErrors(line);
                if (!string.IsNullOrEmpty(empName))
                {
                    throw new TimesheetParseException(ImportFileError.DuplicateEmployeeName);
                }
                
                empName = line.Replace("EMP-NAME: ", "");
                if (empName.Length > 100)
                {
                    throw new TimesheetParseException(ImportFileError.EmployeeNameTooLong);
                }
            }

            if (line.StartsWith("TIMESHEETS:"))
            {
                // date not null but entries empty
                if (date != null && entries.Count == 0)
                {
                    throw new TimesheetParseException(ImportFileError.EmptyTimesheetSection);
                }
                CheckForFieldErrors(line);
                CheckForEmpErrors(empId, empName);
                try
                {
                    var dateParts = line.Replace("TIMESHEETS: ", "").Split("-");
                    var dateNumbers = dateParts.Select(int.Parse).ToArray();
                    date = new DateOnly(dateNumbers[0], dateNumbers[1], dateNumbers[2]);
                }
                catch (Exception e)
                {
                    throw new TimesheetParseException(ImportFileError.InvalidDate);
                }
            }

            if (line.Contains(';'))
            {
                var parts = line.Split(';');
                if (parts.Length != 4)
                {
                    throw new TimesheetParseException(ImportFileError.IncorrectFieldCount);
                }
                var startTime = GetTime(parts[0]);
                var endTime = GetTime(parts[1]);
                
                if (startTime > endTime)
                {
                    throw new TimesheetParseException(ImportFileError.EndTimeBeforeStartTime);
                }

                if (parts.Any(string.IsNullOrEmpty))
                {
                    throw new TimesheetParseException(ImportFileError.EmptyField);
                }

                if (!(parts[2].StartsWith('"') && parts[2].EndsWith('"')))
                {
                    throw new TimesheetParseException(ImportFileError.DescriptionNotQuoted);
                }
                var description = parts[2].Trim('"');
                if (description.Length > 200)
                {
                    throw new TimesheetParseException(ImportFileError.DescriptionTooLong);
                }

                var projectCode = parts[3];
                if (projectCode.StartsWith('"') || projectCode.EndsWith('"'))
                {
                    throw new TimesheetParseException(ImportFileError.ProjectQuoted);
                }
                if (projectCode.Length > 20)
                {
                    throw new TimesheetParseException(ImportFileError.ProjectTooLong);
                }

                var emp = GetEmployee(existingEmployees, empId, empName, ref currentEmployee);

                if (!projectCache.TryGetValue(projectCode, out var project))
                {
                    project = new Project { ProjectCode = projectCode };
                    projectCache[projectCode] = project;
                }

                var entry = new TimeEntry()
                {
                    Date = (DateOnly)date!,
                    Description = description,
                    Employee = emp,
                    Project = project,
                    StartTime = startTime,
                    EndTime = endTime
                };
                entries.Add(entry);
            }
        }
        return entries.Count == 0 ? throw new TimesheetParseException(ImportFileError.EmptyTimesheetSection) : entries;
    }

    private static Employee GetEmployee(IEnumerable<Employee> existingEmployees, string empId, string empName,
        [AllowNull] ref Employee currentEmployee)
    {
        Employee? emp;
        if (currentEmployee is null || currentEmployee.EmployeeId != empId)
        {
            emp = existingEmployees.FirstOrDefault(e => e.EmployeeId == empId);
            if (emp == null)
            {
                emp = new Employee() { EmployeeId = empId, EmployeeName = empName };
            } else
            {
                emp.EmployeeName = empName;
            }
            currentEmployee = emp;
        }
        else
        {
            currentEmployee.EmployeeName = empName;
            emp = currentEmployee;
        }

        return emp;
    }

    private static TimeOnly GetTime(string time)
    {
        var timeParts = time.Split(':');
        if (timeParts.Length != 2 
            || !int.TryParse(timeParts[0], out var hours) 
            || !int.TryParse(timeParts[1], out var minutes)
            || hours < 0 || hours > 23 || minutes < 0 || minutes > 59)
        {
            throw new TimesheetParseException(ImportFileError.InvalidTime);
        }
        return new TimeOnly(hours, minutes);
    }
    
    private static void CheckForFieldErrors(string line)
    {
        if (!line.Contains(": "))
        {
            throw new TimesheetParseException(ImportFileError.InvalidKeyValueFormat);
        }
        
        var fields = line.Split(": ");
        if (fields.Length != 2)
        {
            throw new TimesheetParseException(ImportFileError.IncorrectFieldCount);
        }
        if (fields[0].TrimStart().Length != fields[0].Length)
        {
            throw new TimesheetParseException(ImportFileError.LeadingWhitespace);
        }

        if (fields[1].TrimEnd().Length != fields[1].Length)
        {
            throw new TimesheetParseException(ImportFileError.TrailingWhitespace);
        }
        
        if (fields[0].Trim().Length == 0 || fields[1].Trim().Length == 0)
        {
            throw new TimesheetParseException(ImportFileError.EmptyField);
        }
    }

    private static void CheckForEmpErrors(string empId, string empName)
    {
        if (string.IsNullOrEmpty(empId))
        {
            throw new TimesheetParseException(ImportFileError.MissingEmployeeId);
        }

        if (string.IsNullOrEmpty(empName))
        {
            throw new TimesheetParseException(ImportFileError.MissingEmployeeName);
        }
    }
}
