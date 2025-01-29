//MRJ
using TdLib;

namespace TeleCleanSlate.Telegram.Chat;
/// <summary>
/// Represents a Telegram user and provides various methods to interact with and retrieve information about the user.
/// </summary>
/// <param name="client">The TdClient instance used for making requests to the Telegram API.</param>
/// <param name="uid">The ID of the user.</param>
internal class User(TdClient client, long uid)
{
    #region METHODS
    #region PRIVATE
    /// <summary>
    /// Updates the user information by fetching the latest details from the Telegram API.
    /// </summary>
    private async Task UpdateUserInfoAsync()
    {
        await client.GetUserFullInfoAsync(uid);
        await client.GetUserAsync(uid);
        Thread.Sleep(120);
    }
    /// <summary>
    /// Determines whether the user is of a specified <see cref="UserType"/> type.
    /// </summary>
    /// <typeparam name="T">The user type to check against.</typeparam>
    /// <param name="throwExp">Whether to throw an exception if the operation fails.</param>
    /// <returns>True if the user is of the specified type; otherwise, false. Null if an error occurs and exceptions are not thrown.</returns>
    private async Task<bool?> IsTUser<T>(bool throwExp) where T : TdApi.UserType
    {
        try
        {
            return (await GetTdUser())?.Type is T;
        }
        catch
        {
            if (throwExp)
                throw;
            return null;
        }
    }
    #endregion
    #region PUBLIC
    /// <summary>
    /// Checks if the user is a regular user.
    /// </summary>
    /// <param name="throwExp">Whether to throw an exception if the operation fails.</param>
    /// <returns>True if the user is a regular user; otherwise, false. Null if an error occurs and exceptions are not thrown.</returns>
    public async Task<bool?> IsRegularUser(bool throwExp = false) => await IsTUser<TdApi.UserType.UserTypeRegular>(throwExp);
    /// <summary>
    /// Checks if the user is a bot.
    /// </summary>
    /// <param name="thorwExp">Whether to throw an exception if the operation fails.</param>
    /// <returns>True if the user is a bot; otherwise, false. Null if an error occurs and exceptions are not thrown.</returns>
    public async Task<bool?> IsBotUser(bool thorwExp = false) => await IsTUser<TdApi.UserType.UserTypeBot>(thorwExp);
    /// <summary>
    /// Checks if the user is deleted.
    /// </summary>
    /// <param name="thorwExp">Whether to throw an exception if the operation fails.</param>
    /// <returns>True if the user is deleted; otherwise, false. Null if an error occurs and exceptions are not thrown.</returns>
    public async Task<bool?> IsDeletedUser(bool thorwExp = false) => await IsTUser<TdApi.UserType.UserTypeDeleted>(thorwExp);
    /// <summary>
    /// Checks if the user is unknown.
    /// </summary>
    /// <param name="thorwExp">Whether to throw an exception if the operation fails.</param>
    /// <returns>True if the user is unknown; otherwise, false. Null if an error occurs and exceptions are not thrown.</returns>
    public async Task<bool?> IsUnknownUser(bool thorwExp = false) => await IsTUser<TdApi.UserType.UserTypeUnknown>(thorwExp);
    /// <summary>
    /// Retrieves the basic Telegram user information.
    /// </summary>
    /// <returns>The <see cref="TdApi.User"/> object representing the user. Null if the operation fails.</returns>
    public async Task<TdApi.User?> GetTdUser()
    {
        try
        {
            return await client.GetUserAsync(uid);
        }
        catch
        {
            return null;
        }
    }
    /// <summary>
    /// Retrieves the full Telegram user information.
    /// </summary>
    /// <returns>The <see cref="UserFullInfo"/> object containing additional details about the user. Null if the operation fails.</returns>
    public async Task<TdApi.UserFullInfo?> GetTdUserFullInfo()
    {
        try
        {
            return await client.GetUserFullInfoAsync(uid);
        }
        catch
        {
            return null;
        }
    }
    /// <summary>
    /// Updates and retrieves the basic Telegram user information.
    /// </summary>
    /// <returns>The <see cref="TdApi.User"/> object representing the user after updating the information. Null if the operation fails.</returns>
    public async Task<TdApi.User?> GetTdUserWithUpdate()
    {
        try
        {
            await UpdateUserInfoAsync();
            return await client.GetUserAsync(uid);
        }
        catch
        {
            return null;
        }
    }
    /// <summary>
    /// Updates and retrieves the full Telegram user information.
    /// </summary>
    /// <returns>The <see cref="UserFullInfo"/> object containing additional details about the user after updating the information. Null if the operation fails.</returns>
    public async Task<TdApi.UserFullInfo?> GetTdUserFullInfoWithUpdate()
    {
        try
        {
            await UpdateUserInfoAsync();
            return await client.GetUserFullInfoAsync(uid);
        }
        catch
        {
            return null;
        }
    }
    /// <summary>
    /// Retrieves the full user information as a JSON string.
    /// </summary>
    /// <returns>A JSON string representing the user's basic and full information.</returns>
   #endregion
    #region STATIC
    /// <summary>
    /// Retrieves a Telegram user by their ID.
    /// </summary>
    /// <param name="client">The TdClient instance used for making requests to the Telegram API.</param>
    /// <param name="uid">The ID of the user.</param>
    /// <returns>The <see cref="TdApi.User"/> object representing the user. Null if the operation fails.</returns>
    public static async Task<TdApi.User?> GetUserById(TdClient client, long uid)
    {
        try
        {
            return await client.GetUserAsync(uid);
        }
        catch
        {
            return null;
        }
    }
    #endregion
    #endregion
}