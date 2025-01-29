//MRJ
using TdLib;

namespace TeleCleanSlate.Telegram.Chat;

/// <summary>
/// This class provides methods to retrieve different types of chats and users from Telegram using <see cref="TdClient"/>.
/// It allows for filtering chats based on user types, blocked users, and specific chat types like groups or channels.
/// </summary>
/// <param name="client">The <see cref="TdClient"/> instance used to communicate with the Telegram API.</param>
/// <param name="limit">The maximum number of chats to retrieve in a single request.</param>
/// <param name="blockListLimit">The maximum number of blocked users to retrieve in a single request.</param>
internal class ChatListGetter(TdClient client, int limit, int blockListLimit)
{
    #region TYPES
    /// <summary>
    /// Enum representing different types of regular users.
    /// </summary>
    private enum RegularUserType
    {
        /// <summary>
        /// Known users.
        /// </summary>
        Known,
        /// <summary>
        /// Unknown users.
        /// </summary>
        UnKnow,
        /// <summary>
        /// All Chats.
        /// </summary>
        All,
    }
    #endregion

    #region METHODS
    #region PRIVATE
    /// <summary>
    /// Retrieves chat IDs for users of the specified type and user category (<see cref="RegularUserType.Known"/>, <see cref="RegularUserType.UnKnow"/>, <see cref="RegularUserType.All"/>).
    /// </summary>
    /// <typeparam name="T">Type of user (e.g., <see cref="TdApi.UserType.UserTypeRegular"/>, <see cref="TdApi.UserType.UserTypeBot"/>, <see cref="TdApi.UserType.UserTypeDeleted"/>, <see cref="TdApi.UserType.UserTypeUnknown"/>).</typeparam>
    /// <param name="utype">The category of the user.</param>
    /// <returns>A set of chat IDs matching the specified user type and category.</returns>
    private async Task<HashSet<long>> GetTUserChatsHelperAsync<T>(RegularUserType utype) where T : TdApi.UserType
    {
        HashSet<long> allChats = await GetChatsAsync();
        HashSet<long> Chats = [];
        foreach (var chatId in allChats)
        {
            TdApi.Chat chat = await client.GetChatAsync(chatId);
            if (chat.Type is TdApi.ChatType.ChatTypePrivate pv)
            {
                TdApi.User user = await client.GetUserAsync(pv.UserId);
                bool cond = utype == RegularUserType.All ||//Use All for bots!
                    utype == RegularUserType.Known && user.IsContact ||
                    utype == RegularUserType.UnKnow && !user.IsContact;
                if (user.Type is T && cond)
                    Chats.Add(chatId);
            }
        }
        return Chats;
    }
    /// <summary>
    /// Retrieves blocked chat IDs for users of the specified type and user category (<see cref="RegularUserType.Known"/>, <see cref="RegularUserType.UnKnow"/>, <see cref="RegularUserType.All"/>).
    /// </summary>
    /// <typeparam name="T">Type of user (e.g., <see cref="TdApi.UserType.UserTypeRegular"/>, <see cref="TdApi.UserType.UserTypeBot"/>, <see cref="TdApi.UserType.UserTypeDeleted"/>, <see cref="TdApi.UserType.UserTypeUnknown"/>).</typeparam>
    /// <param name="utype">The category of the user.</param>
    /// <returns>A set of blocked chat IDs matching the specified user type and category.</returns>
    private async Task<HashSet<long>> GetBlockedTUserChatsHelperAsync<T>(RegularUserType utype) where T : TdApi.UserType
    {
        HashSet<long> ret = [];
        var blockedChats = (await client.GetBlockedMessageSendersAsync(new TdApi.BlockList.BlockListMain(), 0, blockListLimit)).Senders;
        var chats = await GetChatsAsync();
        foreach (var chatId in chats)
        {
            TdApi.Chat chat = await client.GetChatAsync(chatId);
            if (chat.Type is TdApi.ChatType.ChatTypePrivate pv)
            {
                TdApi.User user = await client.GetUserAsync(pv.UserId);
                if (user.Type is T)
                {
                    bool cond = utype == RegularUserType.All//Use All for bots!
                        || utype == RegularUserType.Known && user.IsContact
                        || utype == RegularUserType.UnKnow && !user.IsContact;
                    foreach (var sender in blockedChats)
                    {
                        if (sender is TdApi.MessageSender.MessageSenderUser msgUser && user.Id == msgUser.UserId ||
                            sender is TdApi.MessageSender.MessageSenderChat msgChat && chat.Id == msgChat.ChatId && cond)
                        {
                            ret.Add(chatId);
                            break;
                        }
                    }
                }
            }
        }
        return ret;
    }
    /// <summary>
    /// Retrieves chat IDs for chats of the specified type (e.g., <see cref="TdApi.ChatType.ChatTypeBasicGroup"/>, <see cref="TdApi.ChatType.ChatTypeSupergroup"/>, etc.).
    /// </summary>
    /// <typeparam name="T">Type of chat (e.g., <see cref="TdApi.ChatType.ChatTypeBasicGroup"/>, <see cref="TdApi.ChatType.ChatTypeSupergroup"/>).</typeparam>
    /// <param name="chanel">Indicates whether to retrieve channels or not.</param>
    /// <returns>A set of chat IDs matching the specified chat type.</returns>
    private async Task<HashSet<long>> GetTChatAsync<T>(bool chanel = false) where T : TdApi.ChatType
    {
        HashSet<long> chats = await GetChatsAsync();
        HashSet<long> groups = [];
        foreach (var cid in chats)
        {
            TdApi.Chat chat = await client.GetChatAsync(cid);
            if (chat.Type is T)
                if (chat.Type is TdApi.ChatType.ChatTypeBasicGroup || chat.Type is TdApi.ChatType.ChatTypeSupergroup sp && (chanel ? sp.IsChannel : !sp.IsChannel))
                    groups.Add(cid);
        }
        return groups;
    }
    #region STATIC
    private static async Task<bool> IsTUser<T>(TdClient client, long chatId) where T : TdApi.UserType
    {
        var chat = await client.GetChatAsync(chatId);
        if (chat.Type is TdApi.ChatType.ChatTypePrivate pv)
        {
            var user = await client.GetUserAsync(pv.UserId);
            return user.Type is T;
        }
        return false;
    }
    #endregion
    #endregion
    #region PUBLIC
    #region GENERAL
    /// <summary>
    /// Retrieves chat IDs from the specified chat folder.
    /// </summary>
    /// <param name="chatList">The chat folder from which to retrieve the chat IDs. The value of this parameter should not be null!</param>
    /// <returns>A set of folder chat IDs.</returns>
    public async Task<HashSet<long>> GetChatsFromFolderAsync(TdApi.ChatList.ChatListFolder chatList)
    {
        ArgumentNullException.ThrowIfNull(chatList, nameof(chatList));
        return [.. (await client.GetChatsAsync(chatList, limit)).ChatIds];
    }
    /// <summary>
    /// Retrieves chat IDs from both the main list and the archive.
    /// </summary>
    /// <returns>A set of all chat IDs.</returns>
    public async Task<HashSet<long>> GetChatsAsync() => [.. (await GetArchiveChatsAsync()), .. (await GetMainListChatsAsync())];
    /// <summary>
    /// Retrieves chat IDs from the archive list.
    /// </summary>
    /// <returns>A set of archived chat IDs.</returns>
    public async Task<HashSet<long>> GetArchiveChatsAsync() => [.. (await client.GetChatsAsync(new TdApi.ChatList.ChatListArchive(), limit)).ChatIds];
    /// <summary>
    /// Retrieves chat IDs from the main chat list.
    /// </summary>
    /// <returns>A set of chat IDs in the main list.</returns>
    public async Task<HashSet<long>> GetMainListChatsAsync() => [.. (await client.GetChatsAsync(limit: limit)).ChatIds];
    /// <summary>
    /// Retrieves blocked chat IDs.
    /// </summary>
    /// <returns>A set of blocked chat IDs.</returns>
    public async Task<HashSet<long>> GetBlockedChatsAsync()
    {
        HashSet<long> chatIds = await GetChatsAsync();
        //We need use a userManager class! this line is not correct.
        var blockedUsers = (await client.GetBlockedMessageSendersAsync(new TdApi.BlockList.BlockListMain(), 0, blockListLimit)).Senders;
        HashSet<long> blockedChatIds = [];
        foreach (long chatId in chatIds)
        {
            TdApi.Chat chat = await client.GetChatAsync(chatId);
            if (chat.Type is TdApi.ChatType.ChatTypePrivate pv)
            {
                TdApi.User user = await client.GetUserAsync(pv.UserId);
                foreach (var messageSender in blockedUsers)
                {
                    if (messageSender is TdApi.MessageSender.MessageSenderUser msgUser && user.Id == msgUser.UserId ||
                            messageSender is TdApi.MessageSender.MessageSenderChat msgChat && chat.Id == msgChat.ChatId)
                    {
                        blockedChatIds.Add(chatId);
                        break;
                    }
                }
            }
        }
        return blockedChatIds;
    }
    #endregion

    #region REGULARUSER
    /// <summary>
    /// Retrieves chat IDs of all regular users.
    /// </summary>
    /// <returns>A set of regular user chat IDs.</returns>
    public async Task<HashSet<long>> GetRegularUserChatsAsync() => await GetTUserChatsHelperAsync<TdApi.UserType.UserTypeRegular>(RegularUserType.All);
    /// <summary>
    /// Retrieves chat IDs of known regular users (contacts).
    /// </summary>
    /// <returns>A set of chat IDs for known regular users.</returns>
    public async Task<HashSet<long>> GetKnownRegularUserChatsAsync() => await GetTUserChatsHelperAsync<TdApi.UserType.UserTypeRegular>(RegularUserType.Known);
    /// <summary>
    /// Retrieves chat IDs of unknown regular users (non-contacts).
    /// </summary>
    /// <returns>A set of chat IDs for unknown regular users.</returns>
    public async Task<HashSet<long>> GetUnKnownRegularUserChatsAsync() => await GetTUserChatsHelperAsync<TdApi.UserType.UserTypeRegular>(RegularUserType.UnKnow);
    /// <summary>
    /// Retrieves blocked chat IDs of unknown regular users (non-contacts).
    /// </summary>
    /// <returns>A set of blocked chat IDs for unknown regular users.</returns>
    public async Task<HashSet<long>> GetBlockedUnknownRegularUserChatsAsync() => await GetBlockedTUserChatsHelperAsync<TdApi.UserType.UserTypeRegular>(RegularUserType.UnKnow);
    /// <summary>
    /// Retrieves blocked chat IDs of known regular users (contacts).
    /// </summary>
    /// <returns>A set of blocked chat IDs for known regular users.</returns>
    public async Task<HashSet<long>> GetBlockedKnownRegularUserChatsAsync() => await GetBlockedTUserChatsHelperAsync<TdApi.UserType.UserTypeRegular>(RegularUserType.UnKnow);
    #endregion

    #region BOTS
    /// <summary>
    /// Retrieves chat IDs of bot users.
    /// </summary>
    /// <returns>A set of bot user chat IDs.</returns>
    public async Task<HashSet<long>> GetBotUserChatsAsync() => await GetTUserChatsHelperAsync<TdApi.UserType.UserTypeBot>(RegularUserType.All);
    //public async Task<HashSet<long>> GetBlockedBotUserChatsAsync() => await GetBlockedTUserChatsHelperAsync<TdApi.UserType.UserTypeBot>(RegularUserType.All);
    #endregion

    #region DELETED_ACCOUNT
    /// <summary>
    /// Retrieves chat IDs of deleted accounts.
    /// </summary>
    /// <returns>A set of chat IDs for deleted accounts.</returns>
    public async Task<HashSet<long>> GetDeletedAccountAsync() => await GetTUserChatsHelperAsync<TdApi.UserType.UserTypeDeleted>(RegularUserType.All);
    #endregion

    #region GROUPS
    /// <summary>
    /// Retrieves chat IDs of basic groups.
    /// </summary>
    /// <returns>A set of basic group chat IDs.</returns>
    public async Task<HashSet<long>> GetBasicGroupsAsync() => await GetTChatAsync<TdApi.ChatType.ChatTypeBasicGroup>();
    /// <summary>
    /// Retrieves chat IDs of supergroups.
    /// </summary>
    /// <returns>A set of supergroup chat IDs.</returns>
    public async Task<HashSet<long>> GetSuperGroupsAsync() => await GetTChatAsync<TdApi.ChatType.ChatTypeSupergroup>();
    #endregion

    #region CHANELS
    /// <summary>
    /// Retrieves chat IDs of channels.
    /// </summary>
    /// <returns>A set of channel chat IDs.</returns>
    public async Task<HashSet<long>> GetChanelsAsync() => await GetTChatAsync<TdApi.ChatType.ChatTypeSupergroup>(true);
    #endregion

    #region STATIC
    /// <summary>
    /// Retrieves a chat by its ID.
    /// </summary>
    /// <param name="client">The <see cref="TdClient"/> to use for the request.</param>
    /// <param name="chatId">The ID of the chat to retrieve.</param>
    /// <returns>The chat corresponding to the given ID, or null if the chat could not be found.</returns>
    public static async Task<TdApi.Chat?> GetChatById(TdClient client, long chatId)
    {
        try
        {
            return await client.GetChatAsync(chatId);
        }
        catch
        {
            return null;
        }

    }
    /// <summary>
    /// Maps chat IDs to <see cref="TdApi.Chat"/> array.
    /// </summary>
    /// <param name="client">The <see cref="TdClient"/> to use for the request.</param>
    /// <param name="chatIds">An array of chat IDs to retrieve.</param>
    /// <returns>A set of chats corresponding to the given IDs.</returns>
    public static async Task<TdApi.Chat?[]> GetChatsById(TdClient client, long[] chatIds)
    {
        TdApi.Chat?[] chats = new TdApi.Chat[chatIds.Length];
        for (int i = 0; i < chats.Length; ++i)
        {
            chats[i] = await GetChatById(client, chatIds[i]);
        }
        return [.. chats];
    }
    public static async Task<bool> IsSuperGroup(TdClient client, long chatId) => (await client.GetChatAsync(chatId)).Type is TdApi.ChatType.ChatTypeSupergroup supergroup && !supergroup.IsChannel;
    public static async Task<bool> IsRegularUser(TdClient client, long chatId) => await IsTUser<TdApi.UserType.UserTypeRegular>(client, chatId);
    public static async Task<bool> IsBotUser(TdClient client, long chatId) => await IsTUser<TdApi.UserType.UserTypeBot>(client, chatId);
    public static async Task<bool> IsBasicGroup(TdClient client, long chatId) => (await client.GetChatAsync(chatId)).Type is TdApi.ChatType.ChatTypeBasicGroup;
    public static async Task<bool> IsChanel(TdClient client, long chatId) => (await client.GetChatAsync(chatId)).Type is TdApi.ChatType.ChatTypeSupergroup supergroup && supergroup.IsChannel;
    public static async Task<bool> IsDeletedAccount(TdClient client, long chatId) => await IsTUser<TdApi.UserType.UserTypeDeleted>(client, chatId);
    #endregion
    #endregion
    #endregion
}
