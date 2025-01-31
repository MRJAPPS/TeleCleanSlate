//MRJ
using Spectre.Console;
using TdLib;
using TeleCleanSlate.Telegram.Security;

namespace TeleCleanSlate.Common
{
    internal static class AuthorizationHelper
    {
        //For avoid unwanted AnsiConsole exceptions...
        private readonly static object locker = new();

        public static void Helper_OnUnknownError(object s, Exception obj)
        {
            lock (locker)
            {
                ((AuthorizationHandler)s).Relase();
                AnsiConsole.MarkupLine("[red]FATAL[/] : {0}", obj.Message);
                Environment.Exit(3);
            }
        }

        public static void Helper_OnError(object _, TdException obj)
        {
            lock (locker)
            {
                AnsiConsole.MarkupLine($"[red]{obj.Message}[/]");
                if (obj.Message == nameof(CommonConstants.TdErrorCodes.API_ID_INVALID) || obj.Error.Code == CommonConstants.TdErrorCodes.UNREGISTERED)
                    Environment.Exit(obj.Error.Code);

            }
        }

        public static string Helper_OnNeedCode(object _)
        {
            lock (locker)
                return AnsiConsole.Prompt(new TextPrompt<string>("Enter the Code: ").Secret());
        }

        public static string Helper_OnNeedPassword(object _)
        {
            lock (locker)
                return AnsiConsole.Prompt(new TextPrompt<string>("Enter the password: ").Secret());
        }
    }
}
