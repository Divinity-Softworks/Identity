using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using DivinitySoftworks.Functions.Identity.Data;
using System.Reflection;
using System.Text.Json;

namespace DivinitySoftworks.Functions.Identity.Repositories;

/// <summary>
/// Provides methods to interact with the database for storing and retrieving OAuth authorization code data.
/// </summary>
public interface IOAuthAuthorizationCodesRepository {
    /// <summary>
    /// Creates a new OAuth authorization code asynchronously in the database.
    /// </summary>
    /// <param name="authorizationCode">The OAuth authorization code to be created.</param>
    /// <returns>A task representing the asynchronous operation. The task result is true if the creation is successful; otherwise, false.</returns>
    Task<bool> CreateAsync(OAuthAuthorizationCode authorizationCode);

    /// <summary>
    /// Reads an OAuth authorization code asynchronously from the database based on the specified partition key and sort key.
    /// </summary>
    /// <param name="pk">The partition key of the OAuth authorization code.</param>
    /// <param name="sk">The sort key of the OAuth authorization code.</param>
    /// <returns>A task representing the asynchronous operation. The task result is the retrieved OAuth authorization code if found; otherwise, null.</returns>
    Task<OAuthAuthorizationCode?> ReadAsync(string pk, string sk);

    /// <summary>
    /// Deletes an OAuth authorization code asynchronously from the database based on the specified partition key and sort key.
    /// </summary>
    /// <param name="pk">The partition key of the OAuth authorization code to be deleted.</param>
    /// <param name="sk">The sort key of the OAuth authorization code to be deleted.</param>
    /// <returns>A task representing the asynchronous operation. The task result is true if the deletion is successful; otherwise, false.</returns>
    Task<bool> DeleteAsync(string pk, string sk);

    /// <summary>
    /// Deletes the OAuth authorization code asynchronously from the database based on the partition key and sort key.
    /// </summary>
    /// <param name="oAuthAuthorizationCode">The OAuth authorization code to deleyte.</param>
    /// <returns>A task representing the asynchronous operation. The task result is true if the deletion is successful; otherwise, false.</returns>
    Task<bool> DeleteAsync(OAuthAuthorizationCode oAuthAuthorizationCode);

    /// <summary>
    /// Reads all the OAuth authorization code asynchronously from the database that have expired.
    /// </summary>
    /// <returns>A task representing the asynchronous operation. The task result is a list of all the OAuth authorization code asynchronously from 
    /// the database that have expired.</returns>
    Task<List<OAuthAuthorizationCode>> ReadExpiredCodesAsync();
}

/// <summary>
/// Provides methods to interact with the database for storing and retrieving OAuth authorization code data.
/// </summary>
/// <param name="amazonDynamoDB">The Amazon DynamoDB client instance.</param>
public sealed class OAuthAuthorizationCodesRepository(IAmazonDynamoDB amazonDynamoDB) : IOAuthAuthorizationCodesRepository {
    readonly string _tableName = "Identity.OAuthAuthorizationCodes";
    readonly IAmazonDynamoDB _amazonDynamoDB = amazonDynamoDB;

    /// <inheritdoc/>
    public Task<bool> CreateAsync(OAuthAuthorizationCode authorizationCode) {
        return _amazonDynamoDB.CreateItemAsync(_tableName, authorizationCode);
    }

    /// <inheritdoc/>
    public Task<OAuthAuthorizationCode?> ReadAsync(string pk, string sk) {
        return _amazonDynamoDB.GetItemAsync<OAuthAuthorizationCode?>(_tableName, pk, sk);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(string pk, string sk) {
        return _amazonDynamoDB.DeleteItemAsync(_tableName, pk, sk);
    }

    /// <inheritdoc/>
    public Task<List<OAuthAuthorizationCode>> ReadExpiredCodesAsync() {
        return _amazonDynamoDB.ScanAsync<OAuthAuthorizationCode>(_tableName, "Expiration < :expiration"
            , new {
                expiration = DateTime.UtcNow.Ticks
            });
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(OAuthAuthorizationCode oAuthAuthorizationCode) {
        return _amazonDynamoDB.DeleteItemAsync(_tableName, oAuthAuthorizationCode.PK, oAuthAuthorizationCode.SK);
    }
}