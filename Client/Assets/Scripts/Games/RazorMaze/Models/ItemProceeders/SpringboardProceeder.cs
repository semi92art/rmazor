using System;
using System.Linq;
using UnityGameLoopDI;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public class SpringboardEventArgs : EventArgs
    {
        public MazeItem Item { get; }

        public SpringboardEventArgs(MazeItem _Item)
        {
            Item = _Item;
        }
    }

    public delegate void SpringboardEventHandler(SpringboardEventArgs _Args);

    public interface ISpringboardProceeder : IOnMazeChanged, ICharacterMoveContinued
    {
        event SpringboardEventHandler SpringboardEvent;
    }
    
    public class SpringboardProceeder : ItemsProceederBase, ISpringboardProceeder
    {
        #region nonpbulic members

        private SpringboardEventArgs m_LastArgs;
        protected override EMazeItemType[] Types => new[] {EMazeItemType.Springboard};
        
        #endregion
        
        #region inject
        
        public SpringboardProceeder(ModelSettings _Settings, IModelMazeData _Data, ITicker _Ticker)
            : base (_Settings, _Data, _Ticker) { }
        
        #endregion
        
        #region api
        
        public event SpringboardEventHandler SpringboardEvent;
        
        public void OnMazeChanged(MazeInfo _Info)
        {
            CollectItems(_Info);
        }

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            foreach (var type in Types)
            {
                var infos = GetProceedInfos(type);
                var springboardItem = (from info in infos.Values where 
                        info.Item.Position == _Args.Current select info.Item)
                    .FirstOrDefault();

                if (springboardItem == null)
                {
                    m_LastArgs = null;
                    return;
                }

                if (m_LastArgs != null)
                    return;
                
                m_LastArgs = new SpringboardEventArgs(springboardItem);
                SpringboardEvent?.Invoke(m_LastArgs);
            }
        }
        
        #endregion
    }
}