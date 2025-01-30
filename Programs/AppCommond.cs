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
            try
            {
                File.Delete(settings.DbName);
            }
            catch { }
            settings.Tell = GetPhoneNumber(settings.Tell);
            client = new();
            client.Bindings.SetLogVerbosityLevel(TdLogLevel.Fatal);
            AuthorizationHandler helper = helper = new(client, settings.DbName, appVersion, settings.Tell, settings.ApiHash, settings.ApiId, settings.DeviceName, settings.LangCode
           , Helper_OnNeedFirstName, Helper_OnNeedLastName, Helper_OnNeedCode, Helper_OnNeedPassword, Helper_OnUnknownError, Helper_OnError);
            client.UpdateReceived += CommonUpdateHandlerMethods.UpdateChatListHanlder;
            client.UpdateReceived += CommonUpdateHandlerMethods.UpdateFoldersHandler;
            helper.Wait();
            Panel panel = new Panel("By using the TeleCleanSlate tool, you acknowledge that you are solely responsible for your decision to delete your account and any consequences that may arise from it. The creator of this tool assumes no liability for any loss of data, account access, or unintended outcomes. Proceed only if you fully understand and accept this responsibility.").Header("[bold red rapidblink]Serious warning![/]");
            AnsiConsole.Write(panel);
        Commitment:
            string str = AnsiConsole.Prompt(new TextPrompt<string>("Please write 'I want to delete my account' to continue the deletion process: "));
            if (!str.Equals("I want to delete my account", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.WriteLine(areuok[rand.Next(areuok.Length)]);
                goto Commitment;
            }

            try
            {
                client.LogOutAsync().GetAwaiter().GetResult();
            }
            catch { }
            return 0;
        }


        private static string GetPhoneNumber(string? phoneNumber)
        {
            if (phoneNumber == null)
            {
                return AnsiConsole.Prompt<string>(new TextPrompt<string>("\n\rPlease enter the telegram phone number [red]that you want to kill it[/] : "));
            }
            else
            {
                return phoneNumber;
            }
        }
    }
}
