using AppServices.Importer;

namespace ImporterTests;

public class TodoItemsTxtParserTests
{
    private readonly TodoItemsTxtParser parser = new();

    [Fact]
    public void ParseTxt_ValidContent_ReturnsListOfDummies()
    {
        // Arrange
        var txtContent = "Assignee: Rainer\nTodos:\n* Shopping\n* Prepare";

        // Act
        var result = parser.ParseTxt(txtContent).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Rainer", result[0].Assignee);
        Assert.Equal("Shopping", result[0].Title);
        Assert.Equal("Rainer", result[1].Assignee);
        Assert.Equal("Prepare", result[1].Title);
    }

    [Fact]
    public void ParseTxt_EmptyContent_ThrowsInvalidOperationException()
    {
        // Arrange
        var txtContent = "";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => parser.ParseTxt(txtContent));
        Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ParseTxt_InvalidHeader_ThrowsInvalidOperationException()
    {
        // Arrange
        var txtContent = "Halloooo\nAssignee: Rainer\nTodos:\n* Shopping\n* Prepare";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => parser.ParseTxt(txtContent));
        Assert.Contains("Invalid TXT header", exception.Message);
    }

    [Fact]
    public void ParseTxt_InsufficientColumns_ThrowsInvalidOperationException()
    {
        // Arrange
        var txtContent = "Assignee: Test";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(
            () => parser.ParseTxt(txtContent));
        Assert.Contains("insufficient content", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public void ParseTxt_SkipsEmptyLines_ReturnsValidDummies()
    {
        // Arrange
        var txtContent = "Assignee: Test\n\n   \n* Test1\n* Test2";

        // Act
        var result = parser.ParseTxt(txtContent).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Test1", result[0].Title);
        Assert.Equal("Test2", result[1].Title);
    }
}
