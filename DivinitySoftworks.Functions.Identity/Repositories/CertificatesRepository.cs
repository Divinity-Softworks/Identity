using Amazon.DynamoDBv2;
using DivinitySoftworks.Functions.Identity.Data;

namespace DivinitySoftworks.Functions.Identity.Repositories;

/// <summary>
/// Provides methods to interact with the database for storing and retrieving certificate data.
/// </summary>
public interface ICertificatesRepository {
    /// <summary>
    /// Creates a new certificate asynchronously in the database.
    /// </summary>
    /// <param name="certificate">The certificate to be created.</param>
    /// <returns>A task representing the asynchronous operation. The task result is true if the creation is successful; otherwise, false.</returns>
    Task<bool> CreateAsync(Certificate certificate);
    /// <summary>
    /// Reads a certificate asynchronously from the database based on the specified partition key.
    /// </summary>
    /// <param name="pk">The partition key of the certificate.</param>
    /// <returns>A task representing the asynchronous operation. The task result is the retrieved certificate if found; otherwise, null.</returns>
    Task<Certificate?> ReadAsync(string pk);
    /// <summary>
    /// Deletes a certificate asynchronously from the database based on the specified partition key.
    /// </summary>
    /// <param name="pk">The partition key of the certificate to be deleted.</param>
    /// <returns>A task representing the asynchronous operation. The task result is true if the deletion is successful; otherwise, false.</returns>
    Task<bool> DeleteAsync(string pk);
}

/// <summary>
/// Provides methods to interact with the database for storing and retrieving certificate data.
/// </summary>
/// <param name="amazonDynamoDB">The Amazon DynamoDB client instance.</param>
public sealed class CertificatesRepository(IAmazonDynamoDB amazonDynamoDB) : ICertificatesRepository {
    readonly string _tableName = "Identity.Certificates";
    readonly IAmazonDynamoDB _amazonDynamoDB = amazonDynamoDB;

    /// <inheritdoc/>
    public Task<bool> CreateAsync(Certificate certificate) {
        return _amazonDynamoDB.CreateItemAsync(_tableName, certificate);
    }

    /// <inheritdoc/>
    public Task<Certificate?> ReadAsync(string pk) {
        return _amazonDynamoDB.GetItemAsync<Certificate?>(_tableName, pk);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(string pk) {
        return _amazonDynamoDB.DeleteItemAsync(_tableName, pk);
    }
}
