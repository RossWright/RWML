namespace RossWright.MetalGuardian;

public interface IAuthenticationRepository
{
    /// <summary>
    /// Gets the user with the specified identity string (email, phone, username, etc.)
    /// </summary>
    /// <param name="userIdentity">The user identity provided by the client</param>
    /// <returns>The authentication user associated with the identity string, or null if not found</returns>
    Task<IAuthenticationUser?> LookupUser(string userIdentity, CancellationToken cancellationToken);

    /// <summary>
    /// Update the user
    /// </summary>
    /// <param name="userId">The user ID GUID for the authentication user</param>
    Task<IAuthenticationUser?> UpdateUser(Guid userId, Func<IAuthenticationUser, bool> update, CancellationToken cancellationToken);

    /// <summary>
    /// Store a refresh token as modified by the provided action.
    /// </summary>
    /// <param name="setProperties">the action which sets properties on the refresh token</param>
    Task AddRefreshToken(Action<IRefreshToken> setProperties, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieve and modify a refresh token associated with the specified user ID and refresh token value.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose refresh token is being retrieved.</param>
    /// <param name="refreshToken">The value of the refresh token to look up.</param>
    /// /// <param name="setProperties">the action which updates properties on the refresh token.</param>
    /// <returns>The refresh token or NULL if not found</returns>
    Task<IAuthenticationUser?> UpdateRefreshToken(Guid userId, string refreshToken, 
        Action<IRefreshToken> setProperties, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a refresh token for the specified user ID and refresh token value.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose refresh token is being deleted.</param>
    /// <param name="refreshToken">The value of the refresh token to delete.</param>
    Task DeleteRefreshToken(Guid userId, string refreshToken, CancellationToken cancellationToken);
}
