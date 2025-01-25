using Amazon.DynamoDBv2;
using DivinitySoftworks.Functions.Identity.Data;

namespace DivinitySoftworks.Functions.Identity.Repositories;

/// <summary>
/// Provides methods to interact with the database for storing and retrieving user data.
/// </summary>
public interface IUsersRepository {
    /// <summary>
    /// Creates a new user asynchronously in the database.
    /// </summary>
    /// <param name="user">The user to be created.</param>
    /// <returns>A task representing the asynchronous operation. The task result is true if the creation is successful; otherwise, false.</returns>
    Task<bool> CreateAsync(User user);

    /// <summary>
    /// Reads a user asynchronously from the database based on the specified partition key.
    /// </summary>
    /// <param name="pk">The partition key of the user.</param>
    /// <returns>A task representing the asynchronous operation. The task result is the retrieved user if found; otherwise, null.</returns>
    Task<User?> ReadAsync(string pk);

    /// <summary>
    /// Deletes a user asynchronously from the database based on the specified partition key.
    /// </summary>
    /// <param name="pk">The partition key of the user to be deleted.</param>
    /// <returns>A task representing the asynchronous operation. The task result is true if the deletion is successful; otherwise, false.</returns>
    Task<bool> DeleteAsync(string pk);

    /// <summary>
    /// Updates a user asynchronously in the database.
    /// </summary>
    /// <param name="user">The user to be updated.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<bool> PutAsync(User user);
}

/// <summary>
/// Provides methods to interact with the database for storing and retrieving user data.
/// </summary>
/// <param name="amazonDynamoDB">The Amazon DynamoDB client instance.</param>
public sealed class UsersRepository(IAmazonDynamoDB amazonDynamoDB) : IUsersRepository {
    readonly string _tableName = "Identity.Users";
    readonly IAmazonDynamoDB _amazonDynamoDB = amazonDynamoDB;

    /// <inheritdoc/>
    public Task<bool> CreateAsync(User user) {
        return _amazonDynamoDB.CreateItemAsync(_tableName, user);
    }

    /// <inheritdoc/>
    public Task<User?> ReadAsync(string pk) {
        return _amazonDynamoDB.GetItemAsync<User?>(_tableName, pk.ToUpper());
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(string pk) {
        return _amazonDynamoDB.DeleteItemAsync(_tableName, pk.ToUpper());
    }

    /// <inheritdoc/>
    public Task<bool> PutAsync(User user) {
        return _amazonDynamoDB.PutItemAsync(_tableName, user);
    }
}
