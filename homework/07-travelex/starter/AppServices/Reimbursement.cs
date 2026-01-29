namespace AppServices;

public interface IReimbursementCalculator
{
    ReimbursementResult CalculateReimbursement(Travel travel);
}

public record ReimbursementResult(
    decimal Mileage,
    decimal PerDiem,
    decimal Expenses
);

public class ReimbursementCalculator : IReimbursementCalculator
{
    public ReimbursementResult CalculateReimbursement(Travel travel)
    {
        if (travel == null)
        {
            throw new ArgumentNullException(nameof(travel));
        }

        decimal mileage = 0;
        decimal perDiem = 0;

        var hoursBetween = travel.End - travel.Start;
        var hours = Convert.ToDecimal(hoursBetween.TotalHours);

        if (hours > 3.0m)
        {
            while (hours >= 24)
            {
                perDiem += 30;
                hours -= 24;
            }

            perDiem += 2.5m * Math.Ceiling(hours);
        }

        decimal expenses = 0;

        foreach (var reimbursement in travel.Reimbursements)
        {
            if (reimbursement is DriveWithPrivateCarReimbursement driveReimbursement)
            {
                mileage += Convert.ToDecimal(driveReimbursement.KM) / 2;
            }
            else if (reimbursement is ExpenseReimbursement expenseReimbursement)
            {
                expenses += expenseReimbursement.Amount;
            }
        }

        if (mileage != 0)
        {
            expenses = 0;
        }

        return new ReimbursementResult(mileage, Math.Round(perDiem, 2), expenses);
    }
}