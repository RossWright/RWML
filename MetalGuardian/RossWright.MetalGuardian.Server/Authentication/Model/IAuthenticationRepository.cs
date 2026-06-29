namespace RossWright.MetalGuardian;

/// <summary>
/// Stores and retrieves users and refresh tokens for MetalGuardian authentication.
/// </summary>
public interface IAuthenticationRepository
{
    /// <summary>
    /// Gets the user with the specified identity string (email, phone, username, etc.).
    /// </summary>
    /// <param name="userIdentity">The user identity provided by the client.</param>
    /// <param name="cancellationToken">A cancellation token for the lookup.</param>
    /// <returns>The authentication user associated with the identity string, or <c>null</c> if not found.</returns>
    Task<IAuthenticationUser?> LookupUser(string userIdentity, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the specified user.
    /// </summary>
    /// <param name="userId">The user ID GUID for the authentication user.</param>
    /// <param name="update">The update callback to apply to the user.</param>
    /// <param name="cancellationToken">A cancellation token for the update.</param>
    /// <returns>The updated user, or <c>null</c> when the user is not found.</returns>
    Task<IAuthenticationUser?> UpdateUser(Guid userId, Func<IAuthenticationUser, bool> update, CancellationToken cancellationToken);

    /// <summary>
    /// Stores a refresh token as modified by the provided action.
    /// </summary>
    /// <param name="setProperties">The action which sets properties on the refresh token.</param>
    /// <param name="cancellationToken">A cancellation token for the insert.</param>
    Task AddRefreshToken(Action<IRefreshToken> setProperties, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves and modifies a refresh token associated with the specified user ID and refresh token value.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose refresh token is being retrieved.</param>
    /// <param name="refreshToken">The value of the refresh token to look up.</param>
    /// <param name="setProperties">The action which updates properties on the refresh token.</param>
    /// <param name="cancellationToken">A cancellation token for the update.</param>
    /// <returns>The user associated with the refresh token, or <c>null</c> if not found.</returns>
    Task<IAuthenticationUser?> UpdateRefreshToken(Guid userId, string refreshToken,
        Action<IRefreshToken> setProperties, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a refresh token for the specified user ID and refresh token value.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose refresh token is being deleted.</param>
    /// <param name="refreshToken">The value of the refresh token to delete.</param>
    /// <param name="cancellationToken">A cancellation token for the delete.</param>
    Task DeleteRefreshToken(Guid userId, string refreshToken, CancellationToken cancellationToken);
}
