//MRJ
using System.ComponentModel;
using Spectre.Console.Cli;

namespace TeleCleanSlate.Common;

internal sealed class Settings : CommandSettings
{
    [Description("The file path where the tdlib database should be stored.")]
    [DefaultValue("tdb")]
    [CommandOption("--db")]
    public string DbName { get; set; } = "tdb";
    [Description("The API hash provided by Telegram for the application.(https://core.telegram.org/api/obtaining_api_id)")]
    [CommandArgument(0, $"<{nameof(ApiHash)}>")]
    public required string ApiHash { get; set; }
    [Description("The API ID provided by Telegram for the application.(https://core.telegram.org/api/obtaining_api_id)")]
    [CommandArgument(1, $"<{nameof(ApiId)}>")]
    public required int ApiId { get; set; }
    [Description("The name of the device where the application is running.")]
    [DefaultValue("PC")]
    [CommandOption("--devname")]
    public string DeviceName { get; set; } = "PC";
    [Description("The code of the language used for the application (e.g., \"en\" for English).")]
    [DefaultValue("en")]
    [CommandOption("--lang")]
    public string LangCode { get; set; } = "en";
    [Description("The telegram phone number that you want to kill it :(")]
    [CommandOption("--phone")]
    [DefaultValue(null)]
    public string? Tell { get; set; }
}
