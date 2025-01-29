//MRJ
using log4net;
using TdLib;

namespace TeleCleanSlate.Common;

/// <summary>
/// Provides helper methods for various operations.
/// </summary>
/// <remarks>
/// This static class includes utility methods that can be used throughout the program. Some methods are designed for future use and might not be currently utilized.
/// </remarks>
internal static class Helper
{
    #region ELOG
    private static readonly ILog log = LogManager.GetLogger(typeof(Helper));
    #endregion
    #region PRIVATE
    /// <summary>
    /// Loads chats asynchronously in a loop until no more chats are found or an error occurs.
    /// </summary>
    /// <param name="client">The <see cref="TdClient"/> instance used to interact with the Telegram API.</param>
    /// <param name="chatList">The chat list from which to load chats, such as <see cref="TdApi.ChatList.ChatListMain"/> or <see cref="TdApi.ChatList.ChatListArchive"/>.</param>
    /// <param name="delay">The delay in milliseconds between each load operation.</param>
    /// <remarks>
    /// This method continuously loads chats from the specified chat list with a delay between each operation. If no more chats are found (i.e., a <see cref="CommonConstants.TdErrorCodes.NotFound"/> error occurs), the method stops loading. If any other exception occurs, it is re-thrown.
    /// </remarks>
    /// <exception cref="Exception">Thrown when an unexpected exception occurs.</exception>
    private static async Task InnerLoadChatsAsync(TdClient client, TdApi.ChatList chatList, int delay)
    {
        try
        {
            while (true)
            {
                await client.LoadChatsAsync(chatList, CommonConstants.DefaultLimit);
                Thread.Sleep(delay);
            }
        }
        catch (Exception ex)
        {
            log.Error(ex, ex);
            if (ex is TdException tde && tde.Error.Code == CommonConstants.TdErrorCodes.NotFound)
                return;
            else
                throw;
        }
    }
    #endregion

    #region PUBLIC
    /// <summary>
    /// Loads both archived and main chat lists asynchronously with an optional delay between operations.
    /// </summary>
    /// <param name="client">The <see cref="TdClient"/> instance used to interact with the Telegram API.</param>
    /// <param name="delay">The delay in milliseconds between each load operation. Defaults to <see cref="CommonConstants.DefaultLoadChatDelay"/>.</param>
    /// <remarks>
    /// This method loads chats from both the archive and main chat lists with an optional delay between each operation. It is useful for initializing or refreshing chat data.
    /// </remarks>
    public static async Task LoadChatsAsync(TdClient client, int delay = CommonConstants.DefaultLoadChatDelay)
    {
        await InnerLoadChatsAsync(client, new TdApi.ChatList.ChatListArchive(), delay);
        await InnerLoadChatsAsync(client, new TdApi.ChatList.ChatListMain(), delay);
    }
    #region EXTENSIONS
    /// <summary>
    /// Adds a range of <see cref="TdApi.Message"/> objects to a <see cref="LinkedList{T}"/>, with options to add at the beginning or end,
    /// check for duplicates, and validate against specific types. A callback can be used to perform additional
    /// actions on each message, and can terminate the process early if needed.
    /// </summary>
    /// <param name="messages">The <see cref="LinkedList{T}"/> to which the messages will be added.</param>
    /// <param name="tdMessages">The collection of messages to add to the <see cref="LinkedList{T}"/>.</param>
    /// <param name="addFirst">If true, messages are added to the beginning of the list; otherwise, to the end.</param>
    /// <param name="checkDup">If true, checks for duplicate messages based on the message ID and skips them if found.</param>
    /// <param name="towLayerCheck">If true, performs an additional duplicate check against the second-to-last message in the list.</param>
    /// <param name="types">An array of valid message types to add to the list. If null, all message types are allowed.</param>
    /// <param name="callback">A function to execute on each message. If this function returns true, the method exits early and returns true.</param>
    /// <returns>Returns true if the callback function returns true for any message; otherwise, returns false.</returns>
    public static async Task<bool> CustomAddRange(this LinkedList<TdApi.Message> messages, IEnumerable<TdApi.Message> tdMessages,
        bool addFirst, bool checkDup, bool towLayerCheck, Type[]? types, Func<TdApi.Message, Task<bool>> callback)
    {
        foreach (var tdmsg in tdMessages)
        {
            bool isDup = checkDup && messages.Last?.Value != null && messages.Last.Value.Id == tdmsg.Id ||
                towLayerCheck && messages.Last?.Previous?.Value != null && messages.Last.Previous.Value.Id == tdmsg.Id;
            if (!isDup && (types == null || types.Any(c => c.Equals(tdmsg.Content.GetType()))))
            {
                if (addFirst)
                    messages.AddFirst(tdmsg);
                else
                    messages.AddLast(tdmsg);

                if (await callback(tdmsg))
                    return true;
            }
        }
        return false;
    }
    /// <summary>
    /// Splits an array into several smaller arrays.
    /// </summary>
    /// <typeparam name="T">The type of the array.</typeparam>
    /// <param name="array">The array to split.</param>
    /// <param name="size">The size of the smaller arrays.</param>
    /// <returns>An array containing smaller arrays.</returns>
    /// https://stackoverflow.com/questions/18986129/how-can-i-split-an-array-into-n-parts#:~:text=///%20%3Csummary%3E%0A///%20Splits%20an,size).Take(size)%3B%0A%20%20%20%20%7D%0A%7D
    public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
    {
        for (var i = 0; i < (float)array.Length / size; i++)
            yield return array.Skip(i * size).Take(size);
    }
    /// <summary>
    /// Creates all directories and subdirectories in the specified path unless they
    /// already exist.
    /// </summary>
    /// <param name="path">Directory path</param>
    public static void CreateDirectory(string path)
    {
        try { Directory.CreateDirectory(path); } catch { }
    }
    #endregion
    #endregion
}