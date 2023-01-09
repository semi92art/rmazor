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

namespace ClickersAPI.Controllers
{
    [ApiController]
    [Route("api/bot")]
    public class TelegramBotController : ControllerBaseImpl
    {
        #region constants

        private const long AdminChatId = 266767924; 

        #endregion
        
        #region nonpublic members

        private static TelegramBotClient _telegramBotClient;

        #endregion

        #region inject

        public TelegramBotController(
            ApplicationDbContext _Context,
            IMapper              _Mapper,
            IServiceProvider     _Provider) 
            : base(_Context, _Mapper, _Provider) { }

        #endregion

        #region api

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
            return new OkResult();
        }

        [HttpPost("send_message")]
        public async Task<ActionResult<string>> SendAppEvent([FromBody] AppEventDto _AppEventDto)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Action: " + _AppEventDto.Action);
            sb.AppendLine($"Country: {_AppEventDto.Country}, Language: {_AppEventDto.Language}");
            sb.AppendLine($"Platform: {_AppEventDto.Platform}, App Ver.: {_AppEventDto.AppVersion}");
            if (_AppEventDto.EventData != null)
            {
                foreach ((string key, var value) in _AppEventDto.EventData)
                    sb.AppendLine(key + ": " + value);
            }
            string messageText = sb.ToString();
            await _telegramBotClient.SendTextMessageAsync(AdminChatId, messageText);
            return new OkResult();
        }

        #endregion

        #region nonpublic methods

        private static async Task HandleUpdateAsync(
            ITelegramBotClient _BotClient,
            Update             _Update,
            CancellationToken  _CancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message
            if (_Update.Message == null)
                return;
            // Only process text messages
            if (string.IsNullOrEmpty(_Update.Message.Text))
                return;
            var message = _Update.Message;
            string messageText = message.Text;
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

        #endregion
    }
}