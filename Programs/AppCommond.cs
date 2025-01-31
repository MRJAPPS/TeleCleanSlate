//MRJ
using Spectre.Console;
using Spectre.Console.Cli;
using System.Reflection;
using TdLib;
using TdLib.Bindings;
using TeleCleanSlate.Common;
using TeleCleanSlate.Telegram.Security;
using static TeleCleanSlate.Common.AuthorizationHelper;

namespace TeleCleanSlate.Programs
{
    internal class AppCommond : Command<Settings>
    {
        private readonly string appVersion = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString() ?? "1.0.0";
        private TdClient? client;
        private readonly static Random rand = new();
        private readonly static string[] areuok = [":-\\", ":-/", ":-|", "Are u OK man?", "????"];

        public override int Execute(CommandContext context, Settings settings)
        {
            client = new();
            client.Bindings.SetLogVerbosityLevel(TdLogLevel.Fatal);
            AuthorizationHandler helper = helper = new(client, settings.DbName, appVersion, settings.Tell, settings.ApiHash, settings.ApiId, settings.DeviceName, settings.LangCode
           , Helper_OnNeedCode, Helper_OnNeedPassword, Helper_OnNeedPhoneNumber, Helper_OnUnknownError, Helper_OnError);
            client.UpdateReceived += CommonUpdateHandlerMethods.UpdateChatListHanlder;
            client.UpdateReceived += CommonUpdateHandlerMethods.UpdateFoldersHandler;
            helper.Wait();
            Panel panel = new Panel("By using the TeleCleanSlate tool, you acknowledge that you are solely responsible for your decision to delete your account and any consequences that may arise from it. The creator of this tool assumes no liability for any loss of data, account access, or unintended outcomes. Proceed only if you fully understand and accept this responsibility.").Header("[bold red rapidblink]Serious warning![/]");
            AnsiConsole.Write(panel);
            GetUserCommitment();
            CleanSlate cleanSlate = new(client);
            cleanSlate.Run().GetAwaiter().GetResult();
            try
            {
                client.LogOutAsync().GetAwaiter().GetResult();
            }
            catch { }
            return 0;
        }

        private static void GetUserCommitment()
        {
            while (true)
            {
                string response = AnsiConsole.Prompt(new TextPrompt<string>("Please write 'I want to delete my account' to continue the deletion process: "));
                if (response.Equals("I want to delete my account", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                AnsiConsole.WriteLine(areuok[rand.Next(areuok.Length)]);
            }
        }

    }
}
