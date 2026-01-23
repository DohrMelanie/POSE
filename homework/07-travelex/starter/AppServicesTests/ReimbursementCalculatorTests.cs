namespace AppServicesTests;

using AppServices;

public class ReimbursementCalculatorTests
{
    private readonly ReimbursementCalculator _reimbursementCalculator = new();

    [Fact]
    public void CalculatePerDiem_Validation_Null_travel_throws()
    {
        Assert.Throws<ArgumentNullException>(() => _reimbursementCalculator.CalculateReimbursement(null));
    }

    [Fact]
    public void CalculatePerDiem_Validation_end_before_start_zero()
    {
        var testTravel = new Travel(
            DateTimeOffset.Parse("2026-01-20 08:00:00Z"),
            DateTimeOffset.Parse("2026-01-20 07:59:59Z"),
            "Traveler",
            "Purpose",
            []);

        Assert.Equal(0, _reimbursementCalculator.CalculateReimbursement(testTravel).PerDiem);
    }
    
    [Fact]
    public void CalculatePerDiem_Threshold_3_hours_up_to_zero() // also includes exactly 3 hours
    {
        var testTravel = new Travel(
            DateTimeOffset.Parse("2026-01-20 08:00:00Z"),
            DateTimeOffset.Parse("2026-01-20 11:00:00Z"),
            "Traveler",
            "Purpose",
            []);

        Assert.Equal(0, _reimbursementCalculator.CalculateReimbursement(testTravel).PerDiem);
    }
    
    [Fact]
    public void CalculatePerDiem_Threshold_more_than_3_hours()
    {
        var testTravel = new Travel(
            DateTimeOffset.Parse("2026-01-20 08:00:00Z"),
            DateTimeOffset.Parse("2026-01-20 11:00:01Z"),
            "Traveler",
            "Purpose",
            []);

        Assert.Equal(10.00m, _reimbursementCalculator.CalculateReimbursement(testTravel).PerDiem);
    }

    [Fact]
    public void CalculatePerDiem_Threshold_just_started()
    {
        var testTravel = new Travel(
            DateTimeOffset.Parse("2026-01-20 08:00:00Z"),
            DateTimeOffset.Parse("2026-01-20 11:59:59Z"),
            "Traveler",
            "Purpose",
            []);

        Assert.Equal(10.00m, _reimbursementCalculator.CalculateReimbursement(testTravel).PerDiem);
    }
}