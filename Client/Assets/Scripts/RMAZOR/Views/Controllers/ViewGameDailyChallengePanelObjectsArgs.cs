using TMPro;
using UnityEngine;

namespace RMAZOR.Views.Controllers
{
    public class ViewGameDailyChallengePanelObjectsArgs
    {
        public GameObject     PanelObject               { get; set; }
        public SpriteRenderer IconChallenge             { get; set; }
        public SpriteRenderer IconSuccessOrFail         { get; set; }
        public Animator       IconSuccessOrFailAnimator { get; set; }
        public TextMeshPro    Text                      { get; set; }
    }
}