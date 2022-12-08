using System.Collections;
using System.Collections.Generic;
using Common.Network;
using Common.Network.Packets;
using Lean.Localization;
using Newtonsoft.Json;
using UnityEngine.Device;
using UnityEngine.Networking;

namespace Common.Managers.Analytics
{
    public interface IMyOwnAnalyticsProvider : IAnalyticsProvider { }
    
    public class MyOwnAnalyticsProvider : AnalyticsProviderBase, IMyOwnAnalyticsProvider
    {
        private IGameClient GameClient { get; }

        public MyOwnAnalyticsProvider(IGameClient _GameClient)
        {
            GameClient = _GameClient;
        }
        
        protected override void SendAnalyticCore(string _AnalyticId, IDictionary<string, object> _EventData = null)
        {
            if (Application.isEditor)
                return;
            if (_AnalyticId == null)
                return;
            var packet = CreatePacket(_AnalyticId);
            GameClient.Send(packet);
        }

        protected override string GetRealAnalyticId(string _AnalyticId)
        {
            if (_AnalyticId != "session_start")
                return null;
            return _AnalyticId;
        }

        protected override string GetRealParameterId(string _ParameterId)
        {
            return null;
        }
        
        private static GameUserEventPacket CreatePacket(string _AnalyticId)
        {
            var gameUserDto = new GameUserDto
            {
                Action = _AnalyticId,
                Country = PreciseLocale.GetRegion(),
                Language = PreciseLocale.GetLanguage(),
                AppVersion = Application.version,
                Platform = Application.platform.ToString()
            };
            return new GameUserEventPacket(gameUserDto);
        }
    }
}