//MRJ
using System.Collections.Concurrent;
using TdLib;

namespace TeleCleanSlate.Common;

/// <summary>
/// A static class that holds all the common data shared across the entire application.
/// </summary>
internal static class CommonData
{
    /// <summary>
    /// Gets or sets the main chat list.
    /// </summary>
    /// <value>
    /// A <see cref="ConcurrentDictionary{TKey, TValue}"/> containing the main chat list with chat ID as the key and <see cref="TdApi.Chat"/> as the value.
    /// </value>
    public static ConcurrentDictionary<long, TdApi.Chat> MainChatList { get; set; } = [];
    /// <summary>
    /// Gets or sets the archive chat list.
    /// </summary>
    /// <value>
    /// A <see cref="ConcurrentDictionary{TKey, TValue}"/> containing the archive chat list with chat ID as the key and <see cref="TdApi.Chat"/> as the value.
    /// </value>
    public static ConcurrentDictionary<long, TdApi.Chat> ArchiveChatList { get; set; } = [];
    /// <summary>
    /// Gets or sets the unknown chat list (e.g., channel comments).
    /// </summary>
    /// <value>
    /// A <see cref="ConcurrentDictionary{TKey, TValue}"/> containing the unknown chat list with chat ID as the key and <see cref="TdApi.Chat"/> as the value.
    /// </value>
    public static ConcurrentDictionary<long, TdApi.Chat> UnkownChatList { get; set; } = [];
    /// <summary>
    /// A thread-safe dictionary that stores information about chat folders.
    /// </summary>
    /// <value>
    /// A <see cref="ConcurrentDictionary{TKey, TValue}"/> containing the folder list with folder ID as the key and <see cref="TdApi.ChatFolderInfo"/> as the value.
    /// </value>
    public static ConcurrentDictionary<int, TdApi.ChatFolderInfo> ChatFolderList { get; set; } = [];
}

