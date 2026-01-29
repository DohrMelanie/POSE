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

    [Fact]
    public void CalculatePerDiem_FullRate_Exactly()
    {
        var testTravel = new Travel(
            DateTimeOffset.Parse("2026-01-20 08:00:00Z"),
            DateTimeOffset.Parse("2026-01-20 19:00:00Z"),
            "Traveler",
            "Purpose",
            []);

        Assert.Equal(27.50m, _reimbursementCalculator.CalculateReimbursement(testTravel).PerDiem);
    }
    
    [Fact]
    public void CalculatePerDiem_FullRate_Just_under()
    {
        var testTravel = new Travel(
            DateTimeOffset.Parse("2026-01-20 08:00:00Z"),
            DateTimeOffset.Parse("2026-01-20 18:59:59Z"),
            "Traveler",
            "Purpose",
            []);

        Assert.Equal(27.50m, _reimbursementCalculator.CalculateReimbursement(testTravel).PerDiem);
    }
    
    [Fact]
    public void CalculatePerDiem_FullRate_More_than()
    {
        var testTravel = new Travel(
            DateTimeOffset.Parse("2026-01-20 08:00:00Z"),
            DateTimeOffset.Parse("2026-01-20 19:00:01Z"),
            "Traveler",
            "Purpose",
            []);

        Assert.Equal(30.00m, _reimbursementCalculator.CalculateReimbursement(testTravel).PerDiem);
    }
    
    [Fact]
    public void CalculatePerDiem_Spanning_Multiple_Exactly()
    {
        var testTravel = new Travel(
            DateTimeOffset.Parse("2026-01-20 08:00:00Z"),
            DateTimeOffset.Parse("2026-01-21 08:00:00Z"),
            "Traveler",
            "Purpose",
            []);

        Assert.Equal(30.00m, _reimbursementCalculator.CalculateReimbursement(testTravel).PerDiem);
    }
    
    [Fact]
    public void CalculatePerDiem_Spanning_Multiple_Plus_2()
    {
        var testTravel = new Travel(
            DateTimeOffset.Parse("2026-01-20 08:00:00Z"),
            DateTimeOffset.Parse("2026-01-21 10:00:00Z"),
            "Traveler",
            "Purpose",
            []);

        Assert.Equal(35.00m, _reimbursementCalculator.CalculateReimbursement(testTravel).PerDiem);
    }
    
    [Fact]
    public void CalculatePerDiem_Spanning_Multiple_Plus_3()
    {
        var testTravel = new Travel(
            DateTimeOffset.Parse("2026-01-20 08:00:00Z"),
            DateTimeOffset.Parse("2026-01-21 11:00:00Z"),
            "Traveler",
            "Purpose",
            []);

        Assert.Equal(37.50m, _reimbursementCalculator.CalculateReimbursement(testTravel).PerDiem);
    }
    
    [Fact]
    public void CalculatePerDiem_Spanning_Multiple_Plus_3_1()
    {
        var testTravel = new Travel(
            DateTimeOffset.Parse("2026-01-20 08:00:00Z"),
            DateTimeOffset.Parse("2026-01-21 11:00:01Z"),
            "Traveler",
            "Purpose",
            []);

        Assert.Equal(40.00m, _reimbursementCalculator.CalculateReimbursement(testTravel).PerDiem);
    }

    
    [Fact]
    public void CalculatePerDiem_Spanning_Multiple_two_full()
    {
        var testTravel = new Travel(
            DateTimeOffset.Parse("2026-01-20 08:00:00Z"),
            DateTimeOffset.Parse("2026-01-22 08:00:00Z"),
            "Traveler",
            "Purpose",
            []);

        Assert.Equal(60.00m, _reimbursementCalculator.CalculateReimbursement(testTravel).PerDiem);
    }
    
    [Fact]
    public void CalculatePerDiem_Spanning_Multiple()
    {
        var testTravel = new Travel(
            DateTimeOffset.Parse("2026-01-19 07:00:00Z"),
            DateTimeOffset.Parse("2026-01-20 14:30:00Z"),
            "Traveler",
            "Purpose",
            []);

        Assert.Equal(50.00m, _reimbursementCalculator.CalculateReimbursement(testTravel).PerDiem);
    }
    
    [Fact]
    public void CalculateMileage()
    {
        var testTravel = new Travel(
            DateTimeOffset.Parse("2026-01-19 07:00:00Z"),
            DateTimeOffset.Parse("2026-01-20 14:30:00Z"),
            "Traveler",
            "Purpose",
            [
                new DriveWithPrivateCarReimbursement(75, "Test"),
                new DriveWithPrivateCarReimbursement(75, "Test"),
            ]);

        Assert.Equal(75.00m, _reimbursementCalculator.CalculateReimbursement(testTravel).Mileage);
    }
    
    [Fact]
    public void CalculateMileage_Zero()
    {
        var testTravel = new Travel(
            DateTimeOffset.Parse("2026-01-19 07:00:00Z"),
            DateTimeOffset.Parse("2026-01-20 14:30:00Z"),
            "Traveler",
            "Purpose",
            [
                new DriveWithPrivateCarReimbursement(20, "Test"),
                new ExpenseReimbursement(100, "Test")
            ]);

        Assert.Equal(0.00m, _reimbursementCalculator.CalculateReimbursement(testTravel).Expenses);
    }
    
    [Fact]
    public void CalculateMileage_Not_Zero()
    {
        var testTravel = new Travel(
            DateTimeOffset.Parse("2026-01-19 07:00:00Z"),
            DateTimeOffset.Parse("2026-01-20 14:30:00Z"),
            "Traveler",
            "Purpose",
            [
                new DriveWithPrivateCarReimbursement(0, "Test"),
                new ExpenseReimbursement(100, "Test")
            ]);

        Assert.Equal(100, _reimbursementCalculator.CalculateReimbursement(testTravel).Expenses);
    }
    
    [Fact]
    public void CalculateMileage_Sum()
    {
        var testTravel = new Travel(
            DateTimeOffset.Parse("2026-01-19 07:00:00Z"),
            DateTimeOffset.Parse("2026-01-20 14:30:00Z"),
            "Traveler",
            "Purpose",
            [
                new ExpenseReimbursement(498, "Test"),
                new ExpenseReimbursement(120, "Test")
            ]);

        Assert.Equal(618.00m, _reimbursementCalculator.CalculateReimbursement(testTravel).Expenses);
    }
}