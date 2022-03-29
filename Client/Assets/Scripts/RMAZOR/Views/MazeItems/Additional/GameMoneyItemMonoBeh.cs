using Common.Constants;
using Common.Exceptions;
using RMAZOR.Models;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.Additional
{
    public class GameMoneyItemMonoBeh : MonoBehaviour, IOnLevelStageChanged
    {
        public                   SpriteRenderer icon;
        [SerializeField] private Animator       anim;

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            anim.speed = _Args.Stage == ELevelStage.Paused ? 0f : 1f;
            switch (_Args.Stage)
            {
                case ELevelStage.ReadyToStart:
                    anim.SetTrigger(AnimKeys.Anim);
                    break;
                case ELevelStage.Unloaded:
                    anim.SetTrigger(AnimKeys.Stop);
                    break;
                case ELevelStage.Loaded:
                case ELevelStage.StartedOrContinued:
                case ELevelStage.Paused:
                case ELevelStage.Finished:
                case ELevelStage.ReadyToUnloadLevel:
                case ELevelStage.CharacterKilled:
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.Stage);
            }
        }
        
        private void Update()
        {
            transform.rotation = Quaternion.Euler(Vector3.zero);
        }
    }
}