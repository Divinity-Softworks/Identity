using Amazon.DynamoDBv2;
using DivinitySoftworks.Functions.Identity.Data;

namespace DivinitySoftworks.Functions.Identity.Repositories;

/// <summary>
/// Provides methods to interact with the database for storing and retrieving OAuth client data.
/// </summary>
public interface IOAuthClientsRepository {
    /// <summary>
    /// Creates a new OAuth client asynchronously in the database.
    /// </summary>
    /// <param name="client">The OAuth client to be created.</param>
    /// <returns>A task representing the asynchronous operation. The task result is true if the creation is successful; otherwise, false.</returns>
    Task<bool> CreateAsync(OAuthClient client);

    /// <summary>
    /// Reads an OAuth client asynchronously from the database based on the specified partition key and sort key.
    /// </summary>
    /// <param name="pk">The partition key of the OAuth client.</param>
    /// <returns>A task representing the asynchronous operation. The task result is the retrieved OAuth client if found; otherwise, null.</returns>
    Task<OAuthClient?> ReadAsync(string pk);

    /// <summary>
    /// Deletes an OAuth client asynchronously from the database based on the specified partition key and sort key.
    /// </summary>
    /// <param name="pk">The partition key of the OAuth client to be deleted.</param>
    /// <returns>A task representing the asynchronous operation. The task result is true if the deletion is successful; otherwise, false.</returns>
    Task<bool> DeleteAsync(string pk);
}

/// <summary>
/// Provides methods to interact with the database for storing and retrieving OAuth client data.
/// </summary>
/// <param name="amazonDynamoDB">The Amazon DynamoDB client instance.</param>
public sealed class OAuthClientsRepository(IAmazonDynamoDB amazonDynamoDB) : IOAuthClientsRepository {
    readonly string _tableName = "Identity.OAuthClients";
    readonly IAmazonDynamoDB _amazonDynamoDB = amazonDynamoDB;

    /// <inheritdoc/>
    public Task<bool> CreateAsync(OAuthClient client) {
        return _amazonDynamoDB.CreateItemAsync(_tableName, client);
    }

    /// <inheritdoc/>
    public Task<OAuthClient?> ReadAsync(string pk) {
        return _amazonDynamoDB.GetItemAsync<OAuthClient?>(_tableName, pk);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(string pk) {
        return _amazonDynamoDB.DeleteItemAsync(_tableName, pk);
    }
}
