//MRJ
using log4net;
using Spectre.Console;
using TdLib;
using TeleCleanSlate.Common;
using TeleCleanSlate.Telegram.Chat;

namespace TeleCleanSlate.Programs;

/// <summary>
/// Represents a program that cleans up Telegram chats by deleting specific messages and leaving non-essential chats.
/// </summary>
/// <param name="client">An instance of <see cref="TdClient"/> used to interact with the Telegram API.</param>
internal class CleanSlate(TdClient client)
{
    #region ELOG
    private static readonly ILog log = LogManager.GetLogger(typeof(CleanSlate));
    #endregion

    #region METHODS
    #region PRIVATE
    /// <summary>
    /// Evaluates a condition for a specific chat based on the provided flag.
    /// If the flag is true, it checks whether the chat meets the specified condition.
    /// </summary>
    /// <param name="X">Flag indicating whether to apply the condition check.</param>
    /// <param name="chatId">The ID of the chat to be evaluated.</param>
    /// <param name="isXchat">A function that determines if the chat meets the condition.</param>
    /// <returns>
    /// Returning true if the condition is met
    /// when the flag is true; otherwise, false.
    /// </returns>
    private async Task<bool> CheckChatCond(bool X, long chatId, Func<TdClient, long, Task<bool>> isXchat) => X && await isXchat(client, chatId);
    #endregion
    #region PUBLIC
    /// <summary>
    /// Executes the cleaning process, which involves loading chats, identifying messages that can be deleted or edited, and leaving non-essential chats.
    /// and maybe deleting you're the account...!
    /// </summary>
    /// <returns>A task that represents the asynchronous clean-up operation.</returns>
    public async Task Run(bool superGroup = true, bool basicGroup = true, bool botUser = true, bool chanel = true, bool regularUser = true, bool justClean = true)
    {
        await AnsiConsole.Status().StartAsync("Loading chats...", async ctx => await Helper.LoadChatsAsync(client));
        long meId = (await client.GetMeAsync()).Id;
        var keys = CommonData.MainChatList.Keys.ToArray();
        /*
         * The progress display is not thread safe,
         * and using it together with other interactive components such as prompts,
         * status displays or other progress displays are not supported.
         */
        await AnsiConsole.Progress().StartAsync(async ctx =>
        {
            var prog = ctx.AddTask("Scaning and removing chats...", maxValue: keys.Length);
            for (int i = 0; i < keys.Length; i++)
            {
                long chatId = keys[i];
                if (chatId == meId ||
                await CheckChatCond(!superGroup, chatId, ChatListGetter.IsSuperGroup) ||
                await CheckChatCond(!basicGroup, chatId, ChatListGetter.IsBasicGroup) ||
                await CheckChatCond(!botUser, chatId, ChatListGetter.IsBotUser) ||
                await CheckChatCond(!chanel, chatId, ChatListGetter.IsChanel) ||
                await CheckChatCond(!regularUser, chatId, ChatListGetter.IsRegularUser))
                {
                    prog.Value(i + 1);
                    continue;
                }
                try
                {
                    if (!await ChatListGetter.IsChanel(client, chatId))
                    {
                        ChatMessageReaderST reader = new(client, chatId);
                        var chat = await client.GetChatAsync(chatId);
                        var result = await reader.GetSenderIdMessagesFromLast(new TdApi.MessageSender.MessageSenderUser()
                        {
                            UserId = meId
                        }, maxCount: 0);
                        LinkedList<long> deletableMessages = [];
                        foreach (var message in result)
                        {
                            if (message.CanBeDeletedForAllUsers)
                                deletableMessages.AddLast(message.Id);
                            else if (message.CanBeEdited)
                                await client.EditMessageTextAsync(chatId, message.Id,
                                    inputMessageContent: new TdApi.InputMessageContent.InputMessageText()
                                    {
                                        Text = new TdApi.FormattedText() { Text = "." }
                                    });
                            else
                            {
                                log.Warn($"ct: {chat.Title} cid: {chatId} msgid: {message.Id} (The message was not deleted.)");
                            }
                        }
                        if (deletableMessages.Count > 0)
                            foreach (var deletableMessage in deletableMessages.ToArray().Split(CommonConstants.DefaultLimit))
                                await client.DeleteMessagesAsync(chatId, [.. deletableMessage], true);
                        if (chat.CanBeDeletedForAllUsers)
                            await client.DeleteChatAsync(chatId);
                    }
                    if (!await ChatListGetter.IsRegularUser(client, chatId) && !await ChatListGetter.IsBotUser(client, chatId))
                        try
                        {
                            await client.LeaveChatAsync(chatId);
                        }
                        catch { }
                }
                catch (Exception ex)
                {
                    log.Error(ex, ex);
                }
                prog.Value(i + 1);
            }
        });
        if (!justClean)
        {
#if true//DEBUG
            while (true)
            {
                try
                {
                    bool deleteByPassword = AnsiConsole.Prompt(new TextPrompt<bool>("All chats and remaining traces of your account have been deleted as much as possible and cannot be recovered([green]Except Archive messages...![/]).\r\nYou have the option to enter your Telegram password now to delete your account.\r\nIf you choose NOT to enter your password, your account deletion request will be submitted through the standard Telegram process.\r\nThis process takes 7 days, during which you have the option to cancel the deletion through Telegram itself.\r\n\r\nDo you want to enter your password? (if u don't have a password, enter \'n\'){[red]!!YOU CAN CLOSE THE APP IF YOU WANT!![/]} (y/n)")
                        .AddChoice(true)
                        .AddChoice(false)
                        .DefaultValue(true)
                        .WithConverter(choice => choice ? "y" : "n"));
                    string pass = "";
                    if (deleteByPassword)
                    {
                        pass = AnsiConsole.Prompt(new TextPrompt<string>("If you wish to complete the account deletion process, please enter your password:([red]!!YOU CAN CLOSE THE APP IF YOU WANT!![/]) ").Secret());
                        if (string.IsNullOrEmpty(pass.Trim()))
                            continue;
                    }
                    await client.DeleteAccountAsync("This account was deleted by me (the account's original owner) using the TeleCleanSlate tool!", pass);
                    break;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]{ex.Message}(try again...)[/]");
                }
            }
            Panel infoPanel = new Panel("Apparently, your account has been deleted. We're sorry you made this decision, and we hope it wasn't to erase evidence of a crime! ([green]If it hasn't been deleted, you can use the standard method.[/])").Header("[green bold rapidblink]INFO[/]");
            AnsiConsole.Write(infoPanel);
#endif
        }
        else
        {
            Panel infoPanel = new Panel("Apparently, your account has been deleted. We're sorry you made this decision, and we hope it wasn't to erase evidence of a crime! ([green]If it hasn't been deleted, you can use the standard method.[/])").Header("[green bold rapidblink]INFO[/]");
            AnsiConsole.Write(infoPanel);
        }
        AnsiConsole.WriteLine("Bye!");
    }
    #endregion
    #endregion

}

