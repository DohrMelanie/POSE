using AppServices.Importer;

namespace ImporterTests;

public class TodoItemsTxtParserTests
{
    private readonly TodoItemsTxtParser parser = new();

    [Fact]
    public void ParseCsv_ValidContent_ReturnsListOfDummies()
    {
        // Arrange
        var csvContent = "Assignee: Rainer\nTodos:\n* Shopping\n* Prepare";

        // Act
        var result = parser.ParseCsv(csvContent).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Test1", result[0].Assignee);
        Assert.Equal("Shopping", result[0].Title);
        Assert.Equal("Test2", result[1].Assignee);
        Assert.Equal("Prepare", result[1].Title);
    }

    [Fact]
    public void ParseCsv_EmptyContent_ThrowsInvalidOperationException()
    {
        // Arrange
        var csvContent = "";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => parser.ParseCsv(csvContent));
        Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseCsv_InvalidHeader_ThrowsInvalidOperationException()
    {
        // Arrange
        var csvContent = "Assignee: Rainer\nTodos:\n* Shopping\n* Prepare";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => parser.ParseCsv(csvContent));
        Assert.Contains("Invalid CSV header", exception.Message);
    }

    [Fact]
    public void ParseCsv_InsufficientColumns_ThrowsInvalidOperationException()
    {
        // Arrange
        var csvContent = "Assignee: Test";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => parser.ParseCsv(csvContent));
        Assert.Contains("insufficient content", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public void ParseCsv_SkipsEmptyLines_ReturnsValidDummies()
    {
        // Arrange
        var csvContent = "Assignee: Test\n\n   \n* Test1\n* Test2";

        // Act
        var result = parser.ParseCsv(csvContent).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Test1", result[0].Title);
        Assert.Equal("Test2", result[1].Title);
    }
}
