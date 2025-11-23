using AppServices;
using AppServices.Importer;

namespace ImporterTests;

public class DataImporterTests
{
    private readonly IFileReader fileReader;
    private readonly ITodoItemsTxtParser txtParser;
    private readonly ITodoItemsImportDatabaseWriter databaseWriter;
    private readonly TodoItemsImporter importer;

    public DataImporterTests()
    {
        fileReader = Substitute.For<IFileReader>();
        txtParser = Substitute.For<ITodoItemsTxtParser>();
        databaseWriter = Substitute.For<ITodoItemsImportDatabaseWriter>();
        importer = new TodoItemsImporter(fileReader, txtParser, databaseWriter);
    }

    [Fact]
    public async Task ImportFromTxtAsync_SuccessfulImport_ReturnsCount()
    {
        // Arrange
        var txtFilePath = "test.txt";
        var txtContent = "Assignee: Rainer\nTodos:\n* Shopping";
        var dummies = new List<TodoItem>
        {
            new() { Assignee = "Rainer", Title = "Shopping" }
        };

        fileReader.ReadAllTextAsync(txtFilePath).Returns(Task.FromResult(txtContent));
        txtParser.ParseTxt(txtContent).Returns(dummies);

        // Act
        var result = await importer.ImportFromTxtAsync(txtFilePath, isDryRun: false);

        // Assert
        Assert.Equal(1, result);
        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).ClearAllAsync();
        await databaseWriter.Received(1).WriteTodoItemsAsync(Arg.Is<IEnumerable<TodoItem>>(d => d.Count() == 1));
        await databaseWriter.Received(1).CommitTransactionAsync();
        await databaseWriter.DidNotReceive().RollbackTransactionAsync();
    }

    [Fact]
    public async Task ImportFromTxtAsync_DryRun_RollsBackTransaction()
    {
        // Arrange
        var txtFilePath = "test.txt";
        var txtContent = "Assignee: Rainer\nTodos:\n* Shopping\n* Prepare lecture\n---\nAssignee: Karin\n Todos:\n* Practice the Piano";
        var dummies = new List<TodoItem>
        {
            new() { Assignee = "Rainer", Title = "Shopping" },
            new() { Assignee = "Rainer", Title = "Prepare lecture" },
            new() { Assignee = "Karin", Title = "Practice the Piano" }
        };

        fileReader.ReadAllTextAsync(txtFilePath).Returns(Task.FromResult(txtContent));
        txtParser.ParseTxt(txtContent).Returns(dummies);

        // Act
        var result = await importer.ImportFromTxtAsync(txtFilePath, isDryRun: true);

        // Assert
        Assert.Equal(3, result);
        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportFromTxtAsync_FileReaderThrows_RollsBackAndRethrows()
    {
        // Arrange
        var txtFilePath = "test.txt";
        var expectedException = new FileNotFoundException("File not found");
        fileReader.ReadAllTextAsync(txtFilePath).Throws(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            async () => await importer.ImportFromTxtAsync(txtFilePath));

        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportFromTxtAsync_TxtParserThrows_RollsBackAndRethrows()
    {
        // Arrange
        var txtFilePath = "test.txt";
        var txtContent = "Invalid content";
        var expectedException = new InvalidOperationException("Invalid TXT");

        fileReader.ReadAllTextAsync(txtFilePath).Returns(Task.FromResult(txtContent));
        txtParser.ParseTxt(txtContent).Throws(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await importer.ImportFromTxtAsync(txtFilePath));

        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }
    

    [Fact]
    public async Task ImportFromTxtAsync_EmptyFile_ReturnsZero()
    {
        // Arrange
        var txtFilePath = "test.txt";
        var txtContent = "Assignee: Rainer\n";
        var dummies = new List<TodoItem>();

        fileReader.ReadAllTextAsync(txtFilePath).Returns(Task.FromResult(txtContent));
        txtParser.ParseTxt(txtContent).Returns(dummies);

        // Act
        var result = await importer.ImportFromTxtAsync(txtFilePath, isDryRun: false);

        // Assert
        Assert.Equal(0, result);
        await databaseWriter.Received(1).WriteTodoItemsAsync(Arg.Is<IEnumerable<TodoItem>>(d => d.Count() == 0));
        await databaseWriter.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportFromTxtAsync_CallsServicesInCorrectOrder()
    {
        // Arrange
        var txtFilePath = "test.txt";
        var txtContent = "Assignee: Rainer\nTodos:\n* Shopping";
        var dummies = new List<TodoItem> { new() { Assignee = "Test1", Title = "TestTitle" } };

        fileReader.ReadAllTextAsync(txtFilePath).Returns(Task.FromResult(txtContent));
        txtParser.ParseTxt(txtContent).Returns(dummies);

        // Act
        await importer.ImportFromTxtAsync(txtFilePath, isDryRun: false);

        // Assert - Verify order of calls
        Received.InOrder(async () =>
        {
            await databaseWriter.BeginTransactionAsync();
            await databaseWriter.ClearAllAsync();
            await fileReader.ReadAllTextAsync(txtFilePath);
            txtParser.ParseTxt(txtContent);
            await databaseWriter.WriteTodoItemsAsync(Arg.Any<IEnumerable<TodoItem>>());
            await databaseWriter.CommitTransactionAsync();
        });
    }
}
