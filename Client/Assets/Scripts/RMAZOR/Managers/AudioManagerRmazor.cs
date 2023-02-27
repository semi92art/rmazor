using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Settings;
using mazing.common.Runtime.Ticker;
using RMAZOR.Models;
using RMAZOR.Views;

namespace RMAZOR.Managers
{
    public interface IAudioManagerRmazor : IAudioManager, IOnLevelStageChanged { }
    
    public class AudioManagerRmazor : AudioManagerCommon, IAudioManagerRmazor
    {
        #region nonpublic members

        private readonly AudioClipInfo[] m_ClipInfos = new AudioClipInfo[500];
        
        #endregion
        
        #region inject

        private AudioManagerRmazor(
            IContainersGetter _ContainersGetter,
            IViewGameTicker   _GameTicker,
            IUITicker         _UIUiTicker,
            IMusicSetting     _MusicSetting,
            ISoundSetting     _SoundSetting,
            IPrefabSetManager _PrefabSetManager)
            : base(
                _ContainersGetter,
                _GameTicker,
                _UIUiTicker,
                _MusicSetting,
                _SoundSetting,
                _PrefabSetManager) { }

        #endregion

        #region api
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.Loaded) 
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
