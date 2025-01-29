//MRJ
using TdLib;

namespace TeleCleanSlate.Common;

/// <summary>
/// Contains common constants used throughout the project.
/// </summary>
/// <remarks>
/// This static class holds various constants that are used globally across the application, including error codes, special values for TdLib, and default configuration settings.
/// </remarks>
internal static class CommonConstants
{
    #region TYPES
    /// <summary>
    /// The error codes related to the tdlib library.
    /// </summary>
    internal static class TdErrorCodes
    {
        public const int NotFound = 404;//LoadChats
    }
    #endregion

    #region DEFSETTINGS
    public const int DefaultLimit = 100;
    public const int DefaultLoadChatDelay = 250;
    #endregion
}

