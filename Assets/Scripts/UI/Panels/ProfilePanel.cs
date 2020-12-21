using System.Collections.Generic;
using Constants;
using DialogViewers;
using Entities;
using Extensions;
using GameHelpers;
using Network;
using Network.PacketArgs;
using Network.Packets;
using TMPro;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Panels
{
    public class ProfilePanel : GameObservable, IMenuDialogPanel
    {
        #region private members
        
        private readonly IMenuDialogViewer m_DialogViewer;
        private TextMeshProUGUI m_WorldRankNum;
        private TextMeshProUGUI m_CountryRankNum;
        
        #endregion
        
        #region api
        
        public MenuUiCategory Category => MenuUiCategory.Profile;
        public RectTransform Panel { get; private set; }

        public ProfilePanel(IMenuDialogViewer _DialogViewer)
        {
            m_DialogViewer = _DialogViewer;
        }
        
        public void Show()
        {
            Panel = Create();
            m_DialogViewer.Show(this);
        }

        public void OnEnable() { }

        #endregion
        
        #region nonpublic methods

        private RectTransform Create()
        {
            GameObject pp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu", "profile_panel");
            GameObject infoContainer = pp.GetContentItem("info_container");
            TextMeshProUGUI toSeeMessage = pp.GetCompItem<TextMeshProUGUI>("to_see_message");

            TextMeshProUGUI user = pp.GetCompItem<TextMeshProUGUI>("user_text");
            m_WorldRankNum = pp.GetCompItem<TextMeshProUGUI>("world_rank_number");
            Button worldRankListButton = pp.GetCompItem<Button>("world_rank_list_button");
            m_CountryRankNum = pp.GetCompItem<TextMeshProUGUI>("country_rank_number");
            Button countryRankListButton = pp.GetCompItem<Button>("country_rank_list_button");
            TextMeshProUGUI bestResultNum = pp.GetCompItem<TextMeshProUGUI>("best_result_number");
            TextMeshProUGUI gamesPlayedNum = pp.GetCompItem<TextMeshProUGUI>("games_played_number");
            TextMeshProUGUI levelsPlayedNum = pp.GetCompItem<TextMeshProUGUI>("levels_played_number");

            bool isLogined = !string.IsNullOrEmpty(GameClient.Instance.Login);
            toSeeMessage.SetGoActive(!isLogined);
            infoContainer.SetActive(isLogined);

            if (!isLogined)
                return pp.RTransform();
            
            user.text = GameClient.Instance.Login;
            SetRankNums();
            
            
            worldRankListButton.SetOnClick(ShowWorldRankings);
            countryRankListButton.SetOnClick(ShowCountryRankings);

            
            
            
            return pp.RTransform();
        }

        private void ShowWorldRankings()
        {
            
        }

        private void ShowCountryRankings()
        {
            
        }

        private void SetRankNums()
        {
            var globalPacket = new RankPacket(new RankRequestArgs
            {
                Global = true,
                AccountId = GameClient.Instance.AccountId,
                GameId = GameClient.Instance.GameId,
                Type = ScoreTypes.MaxScore
            });
            var countryPacket = new RankPacket(new RankRequestArgs
            {
                Global = false,
                AccountId = GameClient.Instance.AccountId,
                GameId = GameClient.Instance.GameId,
                Type = ScoreTypes.MaxScore
            });
            
            globalPacket.OnSuccess(() =>
            {
                m_WorldRankNum.text = globalPacket.Response.Rank.ToNumeric();
            }).OnFail(() => Debug.LogError(globalPacket.ErrorMessage));
            countryPacket.OnSuccess(() =>
            {
                m_CountryRankNum.text = countryPacket.Response.Rank.ToNumeric();
            }).OnFail(() => Debug.LogError(countryPacket.ErrorMessage));
            
            GameClient.Instance.Send(globalPacket);
            GameClient.Instance.Send(countryPacket);
    }
        
        #endregion
    }
}