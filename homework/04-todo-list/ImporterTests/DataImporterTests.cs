using AppServices;
using AppServices.Importer;

namespace ImporterTests;

public class DataImporterTests
{
    private readonly IFileReader fileReader;
    private readonly IDummyCsvParser csvParser;
    private readonly ITodoItemsImportDatabaseWriter databaseWriter;
    private readonly TodoItemsImporter importer;

    public DataImporterTests()
    {
        fileReader = Substitute.For<IFileReader>();
        csvParser = Substitute.For<IDummyCsvParser>();
        databaseWriter = Substitute.For<ITodoItemsImportDatabaseWriter>();
        importer = new TodoItemsImporter(fileReader, csvParser, databaseWriter);
    }

    [Fact]
    public async Task ImportFromCsvAsync_SuccessfulImport_ReturnsCount()
    {
        // Arrange
        var csvFilePath = "test.csv";
        var csvContent = "Assignee: Rainer\nTodos:\n* Shopping";
        var dummies = new List<TodoItem>
        {
            new() { Assignee = "Rainer", Title = "Shopping" }
        };

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        csvParser.ParseCsv(csvContent).Returns(dummies);

        // Act
        var result = await importer.ImportFromCsvAsync(csvFilePath, isDryRun: false);

        // Assert
        Assert.Equal(1, result);
        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).ClearAllAsync();
        await databaseWriter.Received(1).WriteTodoItemsAsync(Arg.Is<IEnumerable<TodoItem>>(d => d.Count() == 1));
        await databaseWriter.Received(1).CommitTransactionAsync();
        await databaseWriter.DidNotReceive().RollbackTransactionAsync();
    }

    [Fact]
    public async Task ImportFromCsvAsync_DryRun_RollsBackTransaction()
    {
        // Arrange
        var csvFilePath = "test.csv";
        var csvContent = "Assignee: Rainer\nTodos:\n* Shopping\n* Prepare lecture\n---\nAssignee: Karin\n Todos:\n* Practice the Piano";
        var dummies = new List<TodoItem>
        {
            new() { Assignee = "Rainer", Title = "Shopping" },
            new() { Assignee = "Rainer", Title = "Prepare lecture" },
            new() { Assignee = "Karin", Title = "Practice the Piano" }
        };

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        csvParser.ParseCsv(csvContent).Returns(dummies);

        // Act
        var result = await importer.ImportFromCsvAsync(csvFilePath, isDryRun: true);

        // Assert
        Assert.Equal(2, result);
        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportFromCsvAsync_FileReaderThrows_RollsBackAndRethrows()
    {
        // Arrange
        var csvFilePath = "test.csv";
        var expectedException = new FileNotFoundException("File not found");
        fileReader.ReadAllTextAsync(csvFilePath).Throws(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            async () => await importer.ImportFromCsvAsync(csvFilePath));

        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportFromCsvAsync_CsvParserThrows_RollsBackAndRethrows()
    {
        // Arrange
        var csvFilePath = "test.csv";
        var csvContent = "Invalid content";
        var expectedException = new InvalidOperationException("Invalid CSV");

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        csvParser.ParseCsv(csvContent).Throws(expectedException);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await importer.ImportFromCsvAsync(csvFilePath));

        await databaseWriter.Received(1).BeginTransactionAsync();
        await databaseWriter.Received(1).RollbackTransactionAsync();
        await databaseWriter.DidNotReceive().CommitTransactionAsync();
    }
    

    [Fact]
    public async Task ImportFromCsvAsync_EmptyFile_ReturnsZero()
    {
        // Arrange
        var csvFilePath = "test.csv";
        var csvContent = "Assignee: Rainer\n";
        var dummies = new List<TodoItem>();

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        csvParser.ParseCsv(csvContent).Returns(dummies);

        // Act
        var result = await importer.ImportFromCsvAsync(csvFilePath, isDryRun: false);

        // Assert
        Assert.Equal(0, result);
        await databaseWriter.Received(1).WriteTodoItemsAsync(Arg.Is<IEnumerable<TodoItem>>(d => d.Count() == 0));
        await databaseWriter.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task ImportFromCsvAsync_CallsServicesInCorrectOrder()
    {
        // Arrange
        var csvFilePath = "test.csv";
        var csvContent = "Assignee: Rainer\nTodos:\n* Shopping";
        var dummies = new List<TodoItem> { new() { Assignee = "Test1", Title = "TestTitle" } };

        fileReader.ReadAllTextAsync(csvFilePath).Returns(Task.FromResult(csvContent));
        csvParser.ParseCsv(csvContent).Returns(dummies);

        // Act
        await importer.ImportFromCsvAsync(csvFilePath, isDryRun: false);

        // Assert - Verify order of calls
        Received.InOrder(async () =>
        {
            await databaseWriter.BeginTransactionAsync();
            await databaseWriter.ClearAllAsync();
            await fileReader.ReadAllTextAsync(csvFilePath);
            csvParser.ParseCsv(csvContent);
            await databaseWriter.WriteTodoItemsAsync(Arg.Any<IEnumerable<TodoItem>>());
            await databaseWriter.CommitTransactionAsync();
        });
    }
}
