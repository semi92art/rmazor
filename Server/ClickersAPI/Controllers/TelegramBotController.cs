using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using ClickersAPI.DTO;
using ClickersAPI.Helpers;
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

        private static bool _showOnlyUsa = true;

        private static IEnumerable<string> AnalyticIdsToSkip => new[]
        {
            AnalyticIds.AdClicked,
            AnalyticIds.AdReward,
            AnalyticIds.AdClosed,
            AnalyticIds.AdFailedToShow,
            AnalyticIds.LevelReadyToStart,
            AnalyticIds.LevelStarted
        }.Concat(new long[]
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9,
            10, 20, 30, 40, 50, 60, 70, 80, 90,
            100, 200, 300, 400, 500, 600, 700, 800, 900, 1000
        }.Select(AnalyticIds.GetLevelFinishedAnalyticId))
            .ToArray();

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
            // cts.Cancel();
            return new OkObjectResult("Bot initialized");
        }

        [HttpPost("send_message")]
        public async Task<ActionResult<string>> SendAppEvent([FromBody] AppEventDto _AppEventDto)
        {
            if (_showOnlyUsa && !string.Equals(_AppEventDto.Country, "US", StringComparison.InvariantCultureIgnoreCase))
                return new EmptyResult();
            if (AnalyticIdsToSkip.Contains(_AppEventDto.Action))
                return new EmptyResult();
            var sb = new StringBuilder();
            sb.AppendLine($"<b><pre>{AnalyticIdFormatted(_AppEventDto.Action)}</pre></b>");
            sb.Append($"{IsoCountryCodeToFlagEmoji(_AppEventDto.Country)}{_AppEventDto.Country},{_AppEventDto.Language}, ");
            sb.Append($"{PlatformFormatted(_AppEventDto.Platform)}, v.{_AppEventDto.AppVersion}, ");
            sb.AppendLine($"IDFA: <b>{_AppEventDto.Idfa?.Substring(0, 5)}</b>");
            if (_AppEventDto.EventData != null)
            {
                sb.AppendLine("---------");
                foreach ((string key, var valueRaw) in _AppEventDto.EventData)
                {
                    string valueStr = AnalyticIds.GetGameParameterValueByAnalyticIdParameterValue(key, valueRaw);
                    sb.AppendLine(key + ": " + $"<b>{valueStr}</b>");
                }
            }
            string messageText = sb.ToString();
            await _telegramBotClient.SendTextMessageAsync(AdminChatId, messageText, ParseMode.Html);
            return new OkResult();
        }

        #endregion

        #region nonpublic methods
        
        public string IsoCountryCodeToFlagEmoji(string _CountryCode)
        {
            return string.Concat(_CountryCode.ToUpper().Select(_X => char.ConvertFromUtf32(_X + 0x1F1A5)));
        }

        private static string AnalyticIdFormatted(string _AnalyticId)
        {
            string emoji = _AnalyticId switch
            {
                AnalyticIds.SessionStart                  => "😀",
                AnalyticIds.AdShown                       => "🎥",
                AnalyticIds.LevelFinished                 => "🎮",
                AnalyticIds.LevelStageFinished            => "🕹",
                AnalyticIds.PlayMainLevelsButtonClick     => "▶",
                AnalyticIds.PlayDailyChallengeButtonClick => "📆",
                AnalyticIds.PlayPuzzleLevelsButtonClick   => "🧩",
                AnalyticIds.PlayRandomLevelsButtonClick   => "🎲",
                _                                         => string.Empty
            };
            return emoji + _AnalyticId.ToUpper();
        }

        private static string PlatformFormatted(string _Platform)
        {
            return _Platform switch
            {
                "Android" => "🤖",
                "iOS"     => "🍏",
                _         => _Platform
            };
        }

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
            var message        = _Update.Message;
            string messageText = message.Text;
            long chatId        = message.Chat.Id;

            _showOnlyUsa = messageText switch
            {
                "/usaonly" => true,
                "/world"   => false,
                _          => _showOnlyUsa
            };
            
            string echoText = messageText switch
            {
                "/usaonly" => "Show only USA",
                "/world"   => "Show world",
                _          => "You said:\n" + messageText
            };

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
            await _BotClient.SendTextMessageAsync(
                chatId,
                echoText,
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