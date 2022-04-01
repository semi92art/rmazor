using System.Linq;
using Common.Extensions;
using Common.Helpers;

namespace RMAZOR.Views.UI
{
    public interface IViewUISubtitles
    {
        bool CanShowSubtitle { get; }
        void ShowSubtitle(string _Text, float _Seconds, params string[] _Args);
        void HideSubtitle();
    }
    
    public class ViewUISubtitles : InitBase, IViewUISubtitles
    {
        #region inject

        private ViewUISubtitleWithCharacterMonoBeh Beh { get; }

        public ViewUISubtitles(ViewUISubtitleWithCharacterMonoBeh _Beh)
        {
            Beh = _Beh;
        }

        #endregion

        #region api

        public bool CanShowSubtitle { get; }

        public void ShowSubtitle(string _Text, float _Seconds, params string[] _Args)
        {
            bool noCharacter = _Args.NullOrEmpty() || !_Args.Contains("character");
            Beh.ShowSubtitle(_Text, _Seconds, !noCharacter);
        }

        public void HideSubtitle()
        {
            Beh.HideSubtitle();
        }

        #endregion
    }
}