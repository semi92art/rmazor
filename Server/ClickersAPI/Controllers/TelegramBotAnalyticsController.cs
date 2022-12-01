using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ClickersAPI.DTO;
using ClickersAPI.Entities;
using ClickersAPI.Helpers;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ClickersAPI.Controllers
{
    [ApiController]
    [Route("api/bot")]
    public class TelegramBotAnalyticsController : ControllerBaseImpl
    {
        private static TelegramBotClient m_TelegramBotClient;
        
        public TelegramBotAnalyticsController(
            ApplicationDbContext _Context,
            IMapper              _Mapper,
            IServiceProvider     _Provider) 
            : base(_Context, _Mapper, _Provider) { }

        [HttpPost("init_bot")]
        public async Task<ActionResult<string>> InitBot()
        {
            m_TelegramBotClient = new TelegramBotClient("5780426437:AAGsGdYfhOeEaLNcgMDpYT1Ev92w9SRSd7s");

            using var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };
            m_TelegramBotClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await m_TelegramBotClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();

            return Mapper.Map<string>("bot_initialized");
        }

        [HttpPost("send_message")]
        public async Task<ActionResult<string>> SendMessage([FromBody] GameUserDto _GameUserDto)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Action: " + _GameUserDto.Action);
            sb.AppendLine("Country: " + _GameUserDto.Country);
            sb.AppendLine("Language: " + _GameUserDto.Language);
            sb.AppendLine("Platform: " + _GameUserDto.Platform);
            sb.AppendLine("App Version: " + _GameUserDto.AppVersion);
            string messageText = sb.ToString();
            await m_TelegramBotClient.SendTextMessageAsync(
                266767924,
                messageText,
                disableNotification: true,
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithUrl(
                        "Check sendMessage method",
                        "https://core.telegram.org/bots/api#sendmessage")));
            return Mapper.Map<string>("message sent");
        }
        
        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            var message = update.Message;
            if (message == null)
                return;
            var messageText = message.Text;
            // Only process text messages
            if (messageText == null)
                return;

            var chatId = message.Chat.Id;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

            // Echo received message text
            Message sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "You said:\n" + messageText,
                cancellationToken: cancellationToken);
        }

        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
                                     CancellationToken  cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}