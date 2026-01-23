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
        decimal mileage = 0;
        decimal perDiem = 0;
        
        var hoursBetween = travel.End - travel.Start;
        var hours = hoursBetween.TotalHours;
        if (hours > 3.0)
        {
            while (hours > 24)
            {
                perDiem += 30;
                hours -= 24;
            }

            perDiem += 2.5m;
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
        
        return new ReimbursementResult(mileage, perDiem, expenses);
    }
}