using Common.Entities;
using Common.Helpers;
using Common.Managers;
using Common.Ticker;
using GameHelpers;
using RMAZOR.Models;
using RMAZOR.Views;
using Settings;

namespace Managers.Audio
{
    public interface IAudioManagerRmazor : IAudioManager, IOnLevelStageChanged { }
    
    public class AudioManagerRmazor : AudioManagerCommon, IAudioManagerRmazor
    {
        #region nonpublic members

        private readonly AudioClipInfo[] m_ClipInfos = new AudioClipInfo[500];
        
        #endregion
        
        #region inject

        public AudioManagerRmazor(
            IContainersGetter _ContainersGetter,
            IViewGameTicker   _GameTicker,
            IUITicker         _UITicker,
            IMusicSetting     _MusicSetting,
            ISoundSetting     _SoundSetting,
            IPrefabSetManager _PrefabSetManager)
            : base(
                _ContainersGetter,
                _GameTicker,
                _UITicker,
                _MusicSetting,
                _SoundSetting,
                _PrefabSetManager) { }

        #endregion

        #region api
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage != ELevelStage.Loaded) 
                return;
            for (int i = 0; i < m_ClipInfos.Length; i++)
            {
                var info = m_ClipInfos[i];
                if (info == null)
                    continue;
                if (info.DestroyOnLevelChange)
                    m_ClipInfos[i] = null;
            }
        }

        #endregion
    }
}
