using DialogViewers;
using Extensions;
using Helpers;
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
    public class ProfilePanel : IDialogPanel
    {
        #region private members
        
        private IDialogViewer m_DialogViewer;
        private TextMeshProUGUI m_WorldRankNum;
        private TextMeshProUGUI m_CountryRankNum;
        
        #endregion
        
        #region api
        
        public UiCategory Category => UiCategory.Profile;
        public RectTransform Panel { get; private set; }

        public ProfilePanel(IDialogViewer _DialogViewer)
        {
            m_DialogViewer = _DialogViewer;
        }
        
        public void Show()
        {
            Panel = Create();
            m_DialogViewer.Show(this);
        }
        
        #endregion
        
        #region private methods

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
            TextMeshProUGUI bestResultTitle = pp.GetCompItem<TextMeshProUGUI>("best_result_title");
            TextMeshProUGUI bestResultNum = pp.GetCompItem<TextMeshProUGUI>("best_result_number");
            TextMeshProUGUI worldRankTitle = pp.GetCompItem<TextMeshProUGUI>("world_rank_title");
            m_WorldRankNum = pp.GetCompItem<TextMeshProUGUI>("world_rank_number");
            Button worldRankListButton = pp.GetCompItem<Button>("world_rank_list_button");
            TextMeshProUGUI countryRankTitle = pp.GetCompItem<TextMeshProUGUI>("country_rank_title");
            m_CountryRankNum = pp.GetCompItem<TextMeshProUGUI>("country_rank_number");
            Button countryRankListButton = pp.GetCompItem<Button>("country_rank_list_button");
            TextMeshProUGUI gamesPlayedTitle = pp.GetCompItem<TextMeshProUGUI>("games_played_title");
            TextMeshProUGUI gamesPlayedNum = pp.GetCompItem<TextMeshProUGUI>("games_played_number");
            TextMeshProUGUI levelsPlayedTitle = pp.GetCompItem<TextMeshProUGUI>("levels_played_title");
            TextMeshProUGUI levelsPlayedNum = pp.GetCompItem<TextMeshProUGUI>("levels_played_number");
            TextMeshProUGUI awardsTitle = pp.GetCompItem<TextMeshProUGUI>("awards_title");

            bool isLogined = !string.IsNullOrEmpty(GameClient.Instance.Login);
            toSeeMessage.gameObject.SetActive(!isLogined);
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
                GameId = GameClient.Instance.GameId
            });
            var countryPacket = new RankPacket(new RankRequestArgs
            {
                Global = false,
                AccountId = GameClient.Instance.AccountId,
                GameId = GameClient.Instance.GameId
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