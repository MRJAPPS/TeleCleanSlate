using Spectre.Console.Cli;
using TeleCleanSlate.Common;

namespace TeleCleanSlate.Programs
{
    internal class AppCommond : Command<Settings>
    {
        public override int Execute(CommandContext context, Settings settings)
        {       
            return 0;
        }
    }
}
