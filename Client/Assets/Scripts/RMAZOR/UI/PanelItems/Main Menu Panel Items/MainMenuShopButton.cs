namespace RMAZOR.UI.PanelItems.Main_Menu_Panel_Items
{
    public class MainMenuShopButton : MainMenuButtonWithBadgeBase
    {
        public override bool CanBeVisible
        {
            get
            {
#if YANDEX_GAMES
                return false;
#else
                return true;
#endif
            }
        }
    }
}