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
    /// <summary>
    /// Executes the cleaning process, which involves loading chats, identifying messages that can be deleted or edited, and leaving non-essential chats.
    /// </summary>
    /// <param name="inputs">Optional input parameters for the program (currently not used).</param>
    /// <returns>A task that represents the asynchronous clean-up operation.</returns>
    public async Task Run()
    {
        await AnsiConsole.Status().StartAsync("Loading chats...", async ctx => await Helper.LoadChatsAsync(client));
        long meId = (await client.GetMeAsync()).Id;
        var keys = CommonData.MainChatList.Keys.ToArray();
        /*
        await AnsiConsole.Progress().StartAsync(async ctx =>
        {
            var prog = ctx.AddTask("Scaning and removing chats...", maxValue: keys.Length);
            for (int i = 0; i < keys.Length; i++)
            {
                long chatId = keys[i];
                if (chatId == meId) continue;
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
#if !DEBUG
        while (true)
        {
            try
            {
                string pass = AnsiConsole.Prompt(new TextPrompt<string>("All chats and remaining traces of your account have been deleted as much as possible and cannot be recovered([green]Except Archive messages...![/]). If you wish to complete the account deletion process, please enter your password again:([red]!!YOU CAN CLOSE THE APP IF YOU WANT!![/]) ").Secret());
                if (string.IsNullOrEmpty(pass.Trim())) 
                    continue;
                await client.DeleteAccountAsync("This account was deleted by me (the account's original owner) using the TeleCleanSlate tool!", pass);
                break;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]{ex.Message}(try again...)[/]");
            }
        }
#endif
        AnsiConsole.MarkupLine("Apparently, your account has been deleted. We're sorry you made this decision, and we hope it wasn't to erase evidence of a crime! ([green]If it hasn't been deleted, you can use the standard method.[/])\n Bye!");
        Environment.Exit(0);
    }
}

