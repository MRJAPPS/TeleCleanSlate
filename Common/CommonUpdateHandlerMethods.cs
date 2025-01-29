using log4net;
using TdLib;

namespace TeleCleanSlate.Common;

/// <summary>
/// Provides methods for handling updates from the Telegram client.
/// </summary>
internal static class CommonUpdateHandlerMethods
{
    #region ELOG
    private static readonly ILog log = LogManager.GetLogger(typeof(CommonUpdateHandlerMethods));
    #endregion
    #region INTERNAL
    /// <summary>
    /// Handles updates related to the chats, such as new chats or changes in chat positions.
    /// </summary>
    /// <param name="sender">The source of the event, typically the <see cref="TdClient"/> instance.</param>
    /// <param name="e">The update event data from the Telegram API.</param>
    /// <remarks>
    /// This method processes updates such as when a new chat is created or when a chat's position changes. It updates the corresponding chat lists in the <see cref="CommonData"/> class.
    /// </remarks>
    public static async void UpdateChatListHanlder(object? sender, TdApi.Update e)
    {
        ArgumentNullException.ThrowIfNull(sender, nameof(sender));//better safe than sorry ;)
        var client = (TdClient)sender;
        switch (e)
        {
            case TdApi.Update.UpdateNewChat newChat:
                {
                    await UpdateNewChat(client, newChat.Chat.Id);
                    break;
                }
            case TdApi.Update.UpdateChatRemovedFromList removedChat:
                {
                    RemoveChatFromList(removedChat.ChatList, removedChat.ChatId);
                    break;
                }
            case TdApi.Update.UpdateChatPosition chatPos:
                {
                    if (chatPos.Position.Order == CommonConstants.TTdApi.ShouldRemoveChat)
                    {
                        RemoveChatFromList(chatPos.Position.List, chatPos.ChatId);
                    }
                    else
                    {
                        await UpdateNewChat(client, chatPos.ChatId);
                    }
                    break;
                }
        }
    }
    /// <summary>
    /// Handles the update event for chat folders and updates the local folder list.
    /// </summary>
    /// <param name="sender">The source of the event, typically the Telegram API client.</param>
    /// <param name="e">The update event containing information about the new or modified chat folders.</param>
    public static void UpdateFoldersHandler(object? sender, TdApi.Update e)
    {
        if (e is TdApi.Update.UpdateChatFolders folders)
        {
            CommonData.ChatFolderList.Clear();
            foreach (TdApi.ChatFolderInfo folder in folders.ChatFolders)
            {
                CommonData.ChatFolderList.AddOrUpdate(folder.Id, folder, (id, folder) => folder);
            }
        }
    }
    #endregion
    #region PRIVATE
    /// <summary>
    /// Updates(or add) the specified chat in the relevant chat list.
    /// </summary>
    /// <param name="client">The <see cref="TdClient"/> instance used to interact with the Telegram API.</param>
    /// <param name="newChatId">The ID of the new chat that needs to be updated.</param>
    /// <remarks>
    /// This method retrieves the chat information from Telegram using the <see cref="TdClient.GetChatAsync"/> method and then adds or updates the chat in the appropriate list (Main, Archive, or Unknown) in the <see cref="CommonData"/> class.
    /// </remarks>
    private static async Task UpdateNewChat(TdClient client, long newChatId)
    {
        try
        {
            var chat = await client.GetChatAsync(newChatId);
            if (chat.ChatLists.Any(list => list is TdApi.ChatList.ChatListArchive))
            {
                CommonData.ArchiveChatList.AddOrUpdate(chat.Id, chat, (id, chat) => chat);
            }
            else if (chat.ChatLists.Any(list => list is TdApi.ChatList.ChatListMain))
            {
                CommonData.MainChatList.AddOrUpdate(chat.Id, chat, (id, chat) => chat);
            }
            else
            {
                CommonData.UnkownChatList.AddOrUpdate(chat.Id, chat, (id, chat) => chat);
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, ex);
            throw;
        }
    }
    /// <summary>
    /// Removes a chat from the specified chat list.
    /// </summary>
    /// <param name="chatList">The chat list from which to remove the chat. Can be either <see cref="TdApi.ChatList.ChatListArchive"/> or <see cref="TdApi.ChatList.ChatListMain"/>.</param>
    /// <param name="chatId">The ID of the chat to be removed.</param>
    /// <remarks>
    /// This method checks the provided chat list and removes the specified chat by its ID from the corresponding list in <see cref="CommonData"/>.
    /// </remarks>
    private static void RemoveChatFromList(TdApi.ChatList chatList, long chatId)
    {
        switch (chatList)
        {
            case TdApi.ChatList.ChatListArchive:
                {
                    CommonData.ArchiveChatList.Remove(chatId, out _);
                    break;
                }
            case TdApi.ChatList.ChatListMain:
                {
                    CommonData.MainChatList.Remove(chatId, out _);
                    break;
                }
        }
    }
    #endregion
}

