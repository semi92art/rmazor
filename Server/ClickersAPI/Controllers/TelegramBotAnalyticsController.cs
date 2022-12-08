using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ClickersAPI.DTO;
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
        private static TelegramBotClient _telegramBotClient;
        
        public TelegramBotAnalyticsController(
            ApplicationDbContext _Context,
            IMapper              _Mapper,
            IServiceProvider     _Provider) 
            : base(_Context, _Mapper, _Provider) { }

        [HttpPost("init_bot")]
        public async Task<ActionResult<string>> InitBot()
        {
            _telegramBotClient = new TelegramBotClient("5780426437:AAGsGdYfhOeEaLNcgMDpYT1Ev92w9SRSd7s");
            using var cts = new CancellationTokenSource();
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };
            _telegramBotClient.StartReceiving(
                HandleUpdateAsync,
                HandlePollingErrorAsync,
                receiverOptions,
                cts.Token
            );
            var me = await _telegramBotClient.GetMeAsync(cts.Token);
            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
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
            await _telegramBotClient.SendTextMessageAsync(
                266767924,
                messageText,
                disableNotification: true,
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithUrl(
                        "Check sendMessage method",
                        "https://core.telegram.org/bots/api#sendmessage")));
            return Mapper.Map<string>("message sent");
        }
        
        private static async Task HandleUpdateAsync(
            ITelegramBotClient _BotClient,
            Update             _Update,
            CancellationToken  _CancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            var message = _Update.Message;
            if (message == null)
                return;
            string messageText = message.Text;
            // Only process text messages
            if (messageText == null)
                return;
            long chatId = message.Chat.Id;
            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
            // Echo received message text
            await _BotClient.SendTextMessageAsync(
                chatId,
                "You said:\n" + messageText,
                cancellationToken: _CancellationToken);
        }

        private static Task HandlePollingErrorAsync(
            ITelegramBotClient _BotClient,
            Exception          _Exception,
            CancellationToken  _CancellationToken)
        {
            string errorMessage = _Exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => _Exception.ToString()
            };
            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
    }
}