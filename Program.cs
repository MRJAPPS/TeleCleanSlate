//MRJ
using Spectre.Console;
using Spectre.Console.Cli;
using TeleCleanSlate.Programs;
namespace TeleCleanSlate;

internal class Program
{
    static readonly Random rand = new();

    private static void InnerShowLogo(string asciiArt)
    {

        (int Left, int Top) lastPos = Console.GetCursorPosition();
        try
        {
            var (Left, Top) = Console.GetCursorPosition();
            foreach (var c in asciiArt)
            {
                Console.Write(c == '\n' || c == '\r' ? c : ' ');
            }
            lastPos = Console.GetCursorPosition();
            Console.SetCursorPosition(0, 0);
            var lines = asciiArt.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            List<int> lineIndexes = [.. lines.Select((s, i) => i)];
            int randDelay = rand.Next(3, 11);
            while (lineIndexes.Count > 0)
            {
                int lineIndex = rand.Next(0, lineIndexes.Count);
                string lineStr = lines[lineIndexes[lineIndex]];
                bool shouldWeHaveBlink = rand.Next() < int.MaxValue / 2;
                List<int> charIndexes = [.. lineStr.Select((c, i) => i)];
                while (charIndexes.Count > 0)
                {
                    int index = rand.Next(0, charIndexes.Count);
                    Console.SetCursorPosition(Left + charIndexes[index], Top + lineIndexes[lineIndex]);
                    AnsiConsole.Markup($"[bold {GenRandomColor()}]{lineStr[charIndexes[index]]}[/]");
                    if (charIndexes[index] < randDelay)//we don't have nansec sleep, so we do this :)
                        Thread.Sleep(10);
                    charIndexes.RemoveAt(index);
                }
                lineIndexes.RemoveAt(lineIndex);
            }
            Console.SetCursorPosition(lastPos.Left, lastPos.Top);
            Console.WriteLine();
        }
        catch
        {
            Console.SetCursorPosition(lastPos.Left, lastPos.Top);
            Console.WriteLine();
            throw;
        }
        //local methods:
        string GenRandomColor()
        {
            string[] colors = ["red", "green", "blue"];
            return colors[rand.Next(colors.Length)];
        }
    }

    private static void ShowLOGO()
    {
        string[] asciiArts =
            [
            "\r\n _____    _      _____ _                  _____ _       _       \r\n|_   _|  | |    /  __ \\ |                /  ___| |     | |      \r\n  | | ___| | ___| /  \\/ | ___  __ _ _ __ \\ `--.| | __ _| |_ ___ \r\n  | |/ _ \\ |/ _ \\ |   | |/ _ \\/ _` | '_ \\ `--. \\ |/ _` | __/ _ \\\r\n  | |  __/ |  __/ \\__/\\ |  __/ (_| | | | /\\__/ / | (_| | ||  __/\r\n  \\_/\\___|_|\\___|\\____/_|\\___|\\__,_|_| |_\\____/|_|\\__,_|\\__\\___|",
            "___________    .__         _________ .__                         _________.__          __          \r\n\\__    ___/___ |  |   ____ \\_   ___ \\|  |   ____ _____    ____  /   _____/|  | _____ _/  |_  ____  \r\n  |    |_/ __ \\|  | _/ __ \\/    \\  \\/|  | _/ __ \\\\__  \\  /    \\ \\_____  \\ |  | \\__  \\\\   __\\/ __ \\ \r\n  |    |\\  ___/|  |_\\  ___/\\     \\___|  |_\\  ___/ / __ \\|   |  \\/        \\|  |__/ __ \\|  | \\  ___/ \r\n  |____| \\___  >____/\\___  >\\______  /____/\\___  >____  /___|  /_______  /|____(____  /__|  \\___  >\r\n             \\/          \\/        \\/          \\/     \\/     \\/        \\/           \\/          \\/ ",
            "\r\n /$$$$$$$$        /$$            /$$$$$$  /$$                                /$$$$$$  /$$             /$$              \r\n|__  $$__/       | $$           /$$__  $$| $$                               /$$__  $$| $$            | $$              \r\n   | $$  /$$$$$$ | $$  /$$$$$$ | $$  \\__/| $$  /$$$$$$   /$$$$$$  /$$$$$$$ | $$  \\__/| $$  /$$$$$$  /$$$$$$    /$$$$$$ \r\n   | $$ /$$__  $$| $$ /$$__  $$| $$      | $$ /$$__  $$ |____  $$| $$__  $$|  $$$$$$ | $$ |____  $$|_  $$_/   /$$__  $$\r\n   | $$| $$$$$$$$| $$| $$$$$$$$| $$      | $$| $$$$$$$$  /$$$$$$$| $$  \\ $$ \\____  $$| $$  /$$$$$$$  | $$    | $$$$$$$$\r\n   | $$| $$_____/| $$| $$_____/| $$    $$| $$| $$_____/ /$$__  $$| $$  | $$ /$$  \\ $$| $$ /$$__  $$  | $$ /$$| $$_____/\r\n   | $$|  $$$$$$$| $$|  $$$$$$$|  $$$$$$/| $$|  $$$$$$$|  $$$$$$$| $$  | $$|  $$$$$$/| $$|  $$$$$$$  |  $$$$/|  $$$$$$$\r\n   |__/ \\_______/|__/ \\_______/ \\______/ |__/ \\_______/ \\_______/|__/  |__/ \\______/ |__/ \\_______/   \\___/   \\_______/\r\n",
            ];
        InnerShowLogo(asciiArts[rand.Next(asciiArts.Length)]);
    }

    static void Main(string[] args)
    {
        try
        {
            ShowLOGO();
        }
        catch { AnsiConsole.MarkupLine("[red rapidblink]\n\rThere is not enough space to display the logo...(pls clear the screen)[/]"); }
        var app = new CommandApp<AppCommond>();
        Environment.ExitCode = app.Run(args);
    }
}
