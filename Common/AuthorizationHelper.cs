//MRJ
using Spectre.Console;
using TdLib;
using TeleCleanSlate.Telegram.Security;

namespace TeleCleanSlate.Common
{
    internal static class AuthorizationHelper
    {
        public static void Helper_OnUnknownError(object s, Exception obj)
        {
            ((AuthorizationHandler)s).Relase();
            AnsiConsole.MarkupLine("[red]FATAL[/] : {0}", obj.Message);
            Environment.Exit(3);
        }

        public static void Helper_OnError(object _, TdException obj)
        {
            AnsiConsole.MarkupLine($"[red]{obj.Message}[/]");
            if (obj.Message == nameof(CommonConstants.TdErrorCodes.API_ID_INVALID))
                Environment.Exit(3);
        }

        public static string Helper_OnNeedFirstName(object _) => AnsiConsole.Prompt(new TextPrompt<string>("Enter the FirstName: "));

        public static string Helper_OnNeedCode(object _) => AnsiConsole.Prompt(new TextPrompt<string>("Enter the Code: ").Secret());

        public static string Helper_OnNeedLastName(object _) => AnsiConsole.Prompt(new TextPrompt<string>("Enter the LastName: "));

        public static string Helper_OnNeedPassword(object _) => AnsiConsole.Prompt(new TextPrompt<string>("Enter the password: ").Secret());
    }
}
