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
        public const int API_ID_INVALID = 400;//Helper_OnError
        public const int PHONE_NUMBER_INVALID = 406;//Helper_OnError
        public const int PHONE_NUMBER_BANNED = 400;//Oh no...
        public const int UNREGISTERED = -111;//custom code
    }

    /// <summary>
    /// Contains constants with special meaning in TdLib.
    /// </summary>
    internal static class TTdApi
    {
        public const long ShouldRemoveChat = 0;//UpdateChatPosition(Position.Order)
    }
    #endregion

    #region DEFSETTINGS
    public const int DefaultLimit = 100;
    public const int DefaultLoadChatDelay = 250;
    public const int DefOffsetFromMsgID = 0;
    public const string DefSearchQ = "+";
    public const long DefFromMsgIDLast = 0;
    public const int DefaultLimitSearch = 100;
    public const int DefMaxCount = 100;
    public const string DefInvalidPhoneNumber = "+1999999999999999999999999";
    #endregion
}

