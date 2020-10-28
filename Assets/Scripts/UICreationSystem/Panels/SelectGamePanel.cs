using System.Collections.Generic;
using Extentions;
using UICreationSystem.Factories;
using UnityEngine;

namespace UICreationSystem.Panels
{
    public class SelectGamePanel
    {
        private readonly List<ChooseGameItemProps> m_CgiPropsList = new List<ChooseGameItemProps>
        {
            new ChooseGameItemProps(null, "POINT CLICKER", false, true, null),
            new ChooseGameItemProps(null, "FIGURE DRAWER", false, true, null),
            new ChooseGameItemProps(null, "MATH TRAIN", true, true, null),
            new ChooseGameItemProps(null, "TILE PATHER", true, false, null),
            new ChooseGameItemProps(null, "BALANCE DRAWER", true, false, null)
        };

        public RectTransform Create(IDialogViewer _DialogViewer)
        {
            GameObject selectGamePanel = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu",
                "select_game_panel");
            RectTransform content = selectGamePanel.GetComponentItem<RectTransform>("content");
            
            GameObject cgiObj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    content,
                    UiAnchor.Create(0, 1, 0, 1),
                    new Vector2(218f, -43f),
                    Vector2.one * 0.5f,
                    new Vector2(416f, 66f)),
                "main_menu",
                "select_game_item");
            
            foreach (var cgiProps in m_CgiPropsList)
            {
                var cgiObjClone = cgiObj.Clone();
                ChooseGameItem cgi = cgiObjClone.GetComponent<ChooseGameItem>();
                cgi.Init(cgiProps);
            }
            
            Object.Destroy(cgiObj);
            return selectGamePanel.RTransform();
        }
    }
}