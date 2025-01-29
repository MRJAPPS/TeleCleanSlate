//MRJ
namespace TeleCleanSlate.Common;

/// <summary>
/// Provides helper methods for throwing exceptions related to invalid arguments.
/// </summary>
internal static class ExceptionHelper
{
    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified condition is true.
    /// </summary>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    /// <param name="msg">The error message to be included with the exception.</param>
    /// <param name="cond">The condition that, if true, will cause the exception to be thrown.</param>
    public static void ThrowIfArgumentOutOfRange(string paramName, string msg, bool cond)
    {
        if (cond) throw new ArgumentOutOfRangeException(paramName, msg);
    }
}
