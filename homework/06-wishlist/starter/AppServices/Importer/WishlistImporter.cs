namespace AppServices.Importer;

/// <summary>
/// Interface for importing wishlists from JSON files
/// </summary>
public interface IWishlistImporter
{
    /// <summary>
    /// Imports data from JSON files in the specified folder
    /// </summary>
    /// <param name="jsonFolderPath">Path to the folder with JSON files</param>
    /// <param name="isDryRun">If true, rollback transaction after import</param>
    /// <returns>Number of wishlists imported</returns>
    Task<int> ImportFromJsonAsync(string jsonFolderPath, bool isDryRun = false);
}

/// <summary>
/// Implementation for importing wishlists from JSON files
/// </summary>
public class WishlistImporter(
    IFileReader fileReader,
    IWishlistJsonParser jsonParser,
    IWishlistImportDatabaseWriter databaseWriter) : IWishlistImporter
{
    public async Task<int> ImportFromJsonAsync(string jsonFolderPath, bool isDryRun = false)
    {
        var listCount = 0;
        await databaseWriter.BeginTransactionAsync();
        try
        {
            var categoryCache = new Dictionary<string, GiftCategory>();
            var files = fileReader.GetAllJsonFiles(jsonFolderPath);

            foreach (var file in files)
            {
                var fileContent = await fileReader.ReadAllTextAsync(file);
                var parsed = jsonParser.ParseJson(file, fileContent);
            
                if (await databaseWriter.WishlistExistsAsync(parsed.Wishlist.Name))
                {
                    continue;
                }

                var items = await Task.WhenAll(parsed.Items.Select(async i => new WishlistItem
                    { Bought = i.Bought, ItemName = i.ItemName, Category = await GetCategory(i.Category, categoryCache) }));

                await databaseWriter.WriteWishlistAsync(new Wishlist
                {
                    Name = parsed.Wishlist.Name, ParentPin = parsed.Wishlist.ParentPin, ChildPin = parsed.Wishlist.ChildPin, Items = items.ToList()
                });
                listCount++;
            }
            
            if (isDryRun)
            {
                await databaseWriter.RollbackTransactionAsync();
            }
            else
            {
                await databaseWriter.CommitTransactionAsync();
            }

            return listCount;
        }
        catch
        {
            await databaseWriter.RollbackTransactionAsync();
            throw;
        }
    }

    private async Task<GiftCategory> GetCategory(string categoryName, Dictionary<string, GiftCategory> categoryCache)
    {
        if (categoryCache.TryGetValue(categoryName, out var category1))
        {
            return category1;
        }
        else
        {
            var category = await databaseWriter.GetOrCreateCategoryAsync(categoryName);
            categoryCache[categoryName] = category;
            return category;
        }
    }
}