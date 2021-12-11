using UnityEngine;
using UnityEditor;
using System.IO;

namespace MTAssets.NativeAndroidToolkit.Editor{

    /*
     * This class is responsible for creating the menu for this asset. 
     */

    public class Menu : MonoBehaviour
    {
        //Menu items

        [MenuItem("Tools/MT Assets/Native Android Toolkit/Open Setup", false, 10)]
        static void SetupAsset()
        {
            Setup.OpenWindow();
        }

        [MenuItem("Tools/MT Assets/Native Android Toolkit/Preferences", false, 10)]
        static void AssetPreferences()
        {
            Preferences.OpenWindow(false);
        }

        [MenuItem("Tools/MT Assets/Native Android Toolkit/Changelog", false, 10)]
        static void OpenChangeLog()
        {
            string filePath = "Assets/MT Assets/Native Android Toolkit/List Of Changes.txt";

            if (File.Exists(filePath) == true)
            {
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(filePath, typeof(TextAsset)));
            }
            if (File.Exists(filePath) == false)
            {
                EditorUtility.DisplayDialog("Error", "Unable to open file. The file has been deleted, or moved. Please, to correct this problem and avoid future problems with this tool, remove all files from this asset and install it again.", "Ok");
            }
        }

        [MenuItem("Tools/MT Assets/Native Android Toolkit/Read Documentation", false, 30)]
        static void ReadDocumentation()
        {
            EditorUtility.DisplayDialog("Read Documentation", "The documentation is located inside the \n\"MT Assets/Native Android Toolkit\" folder. Just unzip \"Documentation.zip\" on your desktop and open the \"Documentation.html\" file with your preferred browser.", "Cool!");
        }

        [MenuItem("Tools/MT Assets/Native Android Toolkit/More Assets", false, 30)]
        static void MoreAssets()
        {
            Application.OpenURL("https://assetstore.unity.com/publishers/40306");
        }

        [MenuItem("Tools/MT Assets/Native Android Toolkit/Support", false, 30)]
        static void GetSupport()
        {
            EditorUtility.DisplayDialog("Support", "If you have any questions, problems or want to contact me, just contact me by email (mtassets@windsoft.xyz).", "Got it!");
        }
    }
}