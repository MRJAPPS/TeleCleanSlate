//MRJ
using TdLib;
using TeleCleanSlate.Common;
using static TeleCleanSlate.Common.CommonConstants;

namespace TeleCleanSlate.Telegram.Chat;

/// <summary>
/// This class is responsible for reading messages from a specific chat in a single-threaded context. 
/// </summary>
/// <param name="client">The Telegram client used for interacting with the Telegram API.</param>
/// <param name="chatId">The ID of the chat from which messages will be read.</param>
internal class ChatMessageReaderST(TdClient client, long chatId)
{
    #region METHODS
    #region PRIVATE
    /// <summary>
    /// Invokes the callback function on the input message.
    /// </summary>
    /// <param name="callback">The callback function to invoke.</param>
    /// <param name="input">The input message.</param>
    /// <returns>A boolean indicating if the callback returned true.</returns>
    private static async Task<bool> Call(Func<TdApi.Message, Task<bool>>? callback, TdApi.Message input) => callback != null && await callback(input);

    /// <summary>
    /// Searches for messages in a chat based on the specified parameters.
    /// </summary>
    /// <param name="fromMessageId">The ID of the message to start searching from.</param>
    /// <param name="limit">The number of messages to retrieve in each request.</param>
    /// <param name="sender">The sender of the messages to filter by.</param>
    /// <param name="offset">The offset from the message ID to start searching from.</param>
    /// <returns>The search result containing the found messages.</returns>
    private async Task<TdApi.FoundChatMessages> SearchChatMessages(long fromMessageId, int limit, TdApi.MessageSender? sender, int offset = DefOffsetFromMsgID) => await client.ExecuteAsync(new TdApi.SearchChatMessages()
    { ChatId = chatId, Query = DefSearchQ, FromMessageId = fromMessageId, Limit = limit, SenderId = sender, Offset = offset });

    /// <summary>
    /// Retrieves a linked list of messages from a chat based on specified filters.
    /// </summary>
    /// <param name="types">The types of messages to include.</param>
    /// <param name="maxCount">The maximum number of messages to retrieve in total.</param>
    /// <param name="limit">The number of messages to retrieve in each request.</param>
    /// <param name="sender">The sender of the messages to filter by.</param>
    /// <param name="callback">A callback function to invoke for each message.</param>
    /// <returns>A linked list of messages matching the filters.</returns>
    private async Task<LinkedList<TdApi.Message>> GetMessagesByFilter(Type[]? types, int maxCount, int limit,
        TdApi.MessageSender? sender, Func<TdApi.Message, Task<bool>>? callback)
    {
        ExceptionHelper.ThrowIfArgumentOutOfRange(nameof(limit), $"Invalid value for '{nameof(limit)}'. The value must be greater than 0 and less than 101. ", limit <= 0 || limit > 100);
        long fromMessageId = DefFromMsgIDLast;
        LinkedList<TdApi.Message> messages = [];
        while (true)
        {
            var result = await SearchChatMessages(fromMessageId, limit, sender);
            if (await messages.CustomAddRange(result.Messages, true, false, false, types, async (msg) =>
                await Call(callback, msg) || maxCount > 0 && messages.Count >= maxCount) || result.NextFromMessageId == 0)
                break;
            fromMessageId = result.NextFromMessageId;
        }
        return messages;
    }
    #endregion
    #region PUBLIC
    /// <summary>
    /// Retrieves a linked list of messages from the chat's history, filtered by a specific sender, starting from the last message.
    /// </summary>
    /// <param name="sender">The sender whose messages to retrieve.</param>
    /// <param name="types">The types of messages to include.</param>
    /// <param name="limit">The limit of messages to retrieve in each request.</param>
    /// <param name="maxCount">The maximum number of messages to include in the result.</param>
    /// <param name="callback">A callback function to invoke for each message.</param>
    /// <returns>A linked list of messages.</returns>
    public async Task<LinkedList<TdApi.Message>> GetSenderIdMessagesFromLast(TdApi.MessageSender sender, Type[]? types = null,
        int limit = DefaultLimitSearch, int maxCount = DefMaxCount, Func<TdApi.Message, Task<bool>>? callback = null)
        => await GetMessagesByFilter(types, maxCount, limit, sender, callback);
    #endregion
    #endregion
}

