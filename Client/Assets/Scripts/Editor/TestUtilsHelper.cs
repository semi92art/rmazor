using Common.Utils;
using UnityEditor;
using Utils;

namespace Editor
{
    public class TestUtilsHelper : EditorWindow
    {
        [MenuItem("Tools/Test Utils Helper", false)]
        public static void ShowWindow()
        {
            GetWindow<TestUtilsHelper>("Test Utils Helper");
        }

        private void OnGUI()
        {
            EditorUtilsEx.GuiButtonAction(
                nameof(NetworkUtils.IsInternetConnectionAvailable),
                () => NetworkUtils.IsInternetConnectionAvailable());
        }
    }
}
