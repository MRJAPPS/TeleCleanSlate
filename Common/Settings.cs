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

    [Description("The code of the language used for the application (e.g., [bold]\"en\"[/] for English).")]
    [DefaultValue("en")]
    [CommandOption("--lang")]
    public string LangCode { get; set; } = "en";

    [Description("The telegram phone number that you want to [red]kill[/] it :(")]
    [CommandOption("--phone")]
    [DefaultValue(null)]
    public string? Tell { get; set; }

    [Description("If true, skips deletion of chats associated with supergroups.")]
    [CommandOption($"-u|--{nameof(SuperGroup)}")]
    [DefaultValue(false)]
    public bool SuperGroup { get; set; } = false;

    [Description("If true, skips deletion of chats associated with basic groups.")]
    [CommandOption($"-g|--{nameof(BasicGroup)}")]
    [DefaultValue(false)]
    public bool BasicGroup { get; set; } = false;

    [Description("If true, skips deletion of chats associated with bot users.")]
    [CommandOption($"-b|--{nameof(BotUser)}")]
    [DefaultValue(false)]
    public bool BotUser { get; set; } = false;

    [Description("If true, skips deletion of chats associated with channels.")]
    [CommandOption($"-c|--{nameof(Chanel)}")]
    [DefaultValue(false)]
    public bool Chanel { get; set; } = false;

    [Description("If true, skips deletion of chats associated with regular users.")]
    [CommandOption($"-r|--{nameof(RegularUser)}")]
    [DefaultValue(false)]
    public bool RegularUser { get; set; } = false;

    [Description($"If set to true(--{nameof(JustClean)}), the tool will NOT use Telegram's standard account deletion method[green] (I mean, by [link=https://core.telegram.org/tdlib/docs/classtd_1_1td__api_1_1delete_account.html]DeleteAccount(reason,password)[/] tdlib method)[/]")]
    [CommandOption($"-j|--{nameof(JustClean)}")]
    [DefaultValue(true)]
    public bool JustClean { get; set; } = true;

    [Description("If set to true, the current Telegram session will be preserved. This allows you to avoid logging in again every time you run the tool. If not set, the session will be cleared, requiring a fresh login on subsequent runs.")]
    [CommandOption($"-k|--{nameof(KeepSession)}")]
    [DefaultValue(false)]
    public bool KeepSession { get; set; } = false;


}
