using System.Globalization;

namespace AppServices;

/// <summary>
/// Interface for parsing a travel file
/// </summary>
public interface ITravelFileParser
{
    /// <summary>
    /// Parses travel file content into a <see cref="Travel"/> object 
    /// </summary>
    /// <param name="textContent">Travel file content as string</param>
    /// <returns>Parsed <see cref="Travel"/> object</returns>
    Travel ParseTravel(string csvContent);
}

public record Reimbursement();

public record DriveWithPrivateCarReimbursement(int KM, string Description) : Reimbursement();

public record ExpenseReimbursement(int Amount, string Description) : Reimbursement();

public record Travel(
    DateTimeOffset Start,
    DateTimeOffset End,
    string TravelerName,
    string Purpose,
    IEnumerable<Reimbursement> Reimbursements
);

public enum TravelParseError
{
    EmptyFile,
    InvalidHeaderFieldCount,
    InvalidStartDateFormat,
    InvalidEndDateFormat,
    StartDateAfterEndDate,
    EmptyTravelerName,
    EmptyTripPurpose,
    InvalidDriveFieldCount,
    InvalidDriveDistance,
    EmptyDriveDescription,
    InvalidExpenseFieldCount,
    InvalidExpenseAmount,
    EmptyExpenseDescription,
    InvalidEntryType
}

public class TravelParseException(TravelParseError errorCode)
    : Exception(ErrorMessages.TryGetValue(errorCode, out var message) ? message : "Unknown parsing error.")
{
    private static readonly Dictionary<TravelParseError, string> ErrorMessages = new()
    {
        { TravelParseError.EmptyFile, "The travel file is empty." },
        { TravelParseError.InvalidHeaderFieldCount, "Invalid number of fields in header." },
        { TravelParseError.InvalidStartDateFormat, "Invalid start date format." },
        { TravelParseError.InvalidEndDateFormat, "Invalid end date format." },
        { TravelParseError.StartDateAfterEndDate, "Start date is after end date." },
        { TravelParseError.EmptyTravelerName, "Traveler's name is empty." },
        { TravelParseError.EmptyTripPurpose, "Trip purpose is empty." },
        { TravelParseError.InvalidDriveFieldCount, "Invalid number of fields in DRIVE entry." },
        { TravelParseError.InvalidDriveDistance, "Invalid distance in DRIVE entry (not a positive integer)." },
        { TravelParseError.EmptyDriveDescription, "Empty description in DRIVE entry." },
        { TravelParseError.InvalidExpenseFieldCount, "Invalid number of fields in EXPENSE entry." },
        { TravelParseError.InvalidExpenseAmount, "Invalid amount in EXPENSE entry (not a positive integer)." },
        { TravelParseError.EmptyExpenseDescription, "Empty description in EXPENSE entry." },
        { TravelParseError.InvalidEntryType, "Invalid entry type (must be DRIVE or EXPENSE)." }
    };

    public TravelParseError ErrorCode { get; } = errorCode;
}

/// <summary>
/// Implementation for parsing CSV content into Dummy objects
/// </summary>
public class TravelFileParser : ITravelFileParser
{
    public Travel ParseTravel(string csvContent)
    {
        var lines = csvContent.Split(["\r\n", "\n"], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries); // do both

        if (lines.Length == 0)
        {
            throw new TravelParseException(TravelParseError.EmptyFile);
        }

        var header = lines[0].Split('|');

        if (header.Length != 4)
        {
            throw new TravelParseException(TravelParseError.InvalidHeaderFieldCount);
        }
        
        if (!DateTimeOffset.TryParseExact(
                header[0],
                "yyyy-MM-dd'T'HH:mm:ss'Z'",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var start))
        {
            throw new TravelParseException(TravelParseError.InvalidStartDateFormat);
        }

        if (!DateTimeOffset.TryParseExact(
                header[1],
                "yyyy-MM-dd'T'HH:mm:ss'Z'",
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var end))
        {
            throw new TravelParseException(TravelParseError.InvalidEndDateFormat);
        }

        if (end < start)
        {
            throw new TravelParseException(TravelParseError.StartDateAfterEndDate);
        }

        var name = header[2].Trim();
        if (name.Length == 0)
        {
            throw new TravelParseException(TravelParseError.EmptyTravelerName);
        }

        var purpose = header[3].Trim();
        if (purpose.Length == 0)
        {
            throw new TravelParseException(TravelParseError.EmptyTripPurpose);
        }

        List<Reimbursement> reimbursements = [];
        
        lines = lines.Skip(1).ToArray();

        foreach (var line in lines)
        {
            var lineParts = line.Split('|');

            if (lineParts[0] == "DRIVE")
            {
                if (lineParts.Length != 3)
                {
                    throw new TravelParseException(TravelParseError.InvalidDriveFieldCount);
                }

                if (!int.TryParse(lineParts[1], out var km) || km <= 0)
                {
                    throw new TravelParseException(TravelParseError.InvalidDriveDistance);
                }
                var driveDescription = lineParts[2].Trim();
                if (driveDescription.Length == 0)
                {
                    throw new TravelParseException(TravelParseError.EmptyDriveDescription);
                }

                reimbursements.Add(new DriveWithPrivateCarReimbursement(km, driveDescription));
            }
            else if (lineParts[0] == "EXPENSE")
            {
                if (lineParts.Length != 3)
                {
                    throw new TravelParseException(TravelParseError.InvalidExpenseFieldCount);
                }
                
                if (!int.TryParse(lineParts[1], out var amount) || amount <= 0)
                {
                    throw new TravelParseException(TravelParseError.InvalidExpenseAmount);
                }
                var description = lineParts[2].Trim();
                
                if (description.Length == 0)
                {
                    throw new TravelParseException(TravelParseError.EmptyExpenseDescription);
                }
                reimbursements.Add(new ExpenseReimbursement(amount, description));
            }
            else
            {
                throw new TravelParseException(TravelParseError.InvalidEntryType);
            }
        }

        return new Travel(start, end, name, purpose, reimbursements);
    }
}