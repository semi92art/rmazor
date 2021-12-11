using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System;

namespace MTAssets.NativeAndroidToolkit.Editor
{
    /*
     * This class is responsible for assisting the installation of Native Android Toolkit and its handler.
     */

    [InitializeOnLoad]
    public class Setup : EditorWindow
    {
        //Variables of setup
        private bool setupAutoStarted = false;
        private int rendersCount = 0;
        public static NativeAndroidPreferences natPreferences;

        //After unity compilation, run setup verification 

        static Setup()
        {
            //Run the verifier after unity compiles
            EditorApplication.delayCall += SetupVerifier;
        }

        static void SetupVerifier()
        {
            //Load the preferences 
            LoadThePreferences();

            //If the NAT is not installed, run the setup
            if(natPreferences.natInstalledInFirstScene == false)
            {
                int confirmation = EditorUtility.DisplayDialogComplex("Native Android Toolkit Setup",
                        "Native Android Toolkit has detected that \"NAT Data Handler\" is not installed in the first scene of your project. This GameObject is responsible for communicating between Android Native and your game. If it is not installed, NAT will not function correctly.\n\nDo you want to start the installation process now ? ",
                        "Yes",
                        "Don't ask again",
                        "No");
                switch (confirmation)
                {
                    case 0:
                        //Open the window
                        OpenWindow();   
                        break;
                    case 1:
                        //Register that installed
                        natPreferences.natInstalledInFirstScene = true;
                        SaveThePreferences();
                        EditorUtility.DisplayDialog("Tip", "You can always start the NAT installer by going to the \"Tools > Native Android Toolkit > Open Setup\" tab.", "Ok");
                        break;
                    case 2:
                        break;
                }
            }
        }

        static void LoadThePreferences()
        {
            //Create the default directory, if not exists
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData"))
                AssetDatabase.CreateFolder("Assets/MT Assets", "_AssetsData");
            if (!AssetDatabase.IsValidFolder("Assets/MT Assets/_AssetsData/Preferences"))
                AssetDatabase.CreateFolder("Assets/MT Assets/_AssetsData", "Preferences");

            //Try to load the preferences file
            natPreferences = (NativeAndroidPreferences)AssetDatabase.LoadAssetAtPath("Assets/MT Assets/_AssetsData/Preferences/NativeAndroidToolkit.asset", typeof(NativeAndroidPreferences));
            //Validate the preference file. if this preference file is of another project, delete then
            if (natPreferences != null)
            {
                if (natPreferences.projectName != Application.productName)
                {
                    AssetDatabase.DeleteAsset("Assets/MT Assets/_AssetsData/Preferences/NativeAndroidToolkit.asset");
                    natPreferences = null;
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }
            //If null, create and save a preferences file
            if (natPreferences == null)
            {
                natPreferences = ScriptableObject.CreateInstance<NativeAndroidPreferences>();
                natPreferences.projectName = Application.productName;
                AssetDatabase.CreateAsset(natPreferences, "Assets/MT Assets/_AssetsData/Preferences/NativeAndroidToolkit.asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        static void SaveThePreferences()
        {
            //Save the preferences in Prefs.asset
            natPreferences.projectName = Application.productName;

            EditorUtility.SetDirty(natPreferences);
            AssetDatabase.SaveAssets();
        }

        //Setup window code

        public static void OpenWindow()
        {
            //Method to open the Window
            var window = GetWindow<Setup>("Setup");
            window.minSize = new Vector2(360, 220);
            window.maxSize = new Vector2(360, 220);
            var position = window.position;
            position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            window.position = position;
            window.Show();
        }

        void OnGUI()
        {
            //Start the undo event support, draw default inspector and monitor of changes
            EditorGUI.BeginChangeCheck();

            //Try to load needed assets
            Texture iconOfUi = (Texture)AssetDatabase.LoadAssetAtPath("Assets/MT Assets/Native Android Toolkit/Editor/Images/Icon.png", typeof(Texture));
            //If fails on load needed assets, locks ui
            if (iconOfUi == null)
            {
                EditorGUILayout.HelpBox("Unable to load required files. Please reinstall Native Android Toolkit to correct this problem.", MessageType.Error);
                return;
            }

            //Run install wizard, if is not installed (after rendered the window) 
            if (natPreferences.natInstalledInFirstScene == false && setupAutoStarted == false)
            {
                rendersCount += 1;
                if (rendersCount >= 5)
                {
                    setupAutoStarted = true;
                    RunInstallWizard();
                }
            }

            GUILayout.BeginVertical("box");

            GUIStyle tituloBox = new GUIStyle();
            tituloBox.fontStyle = FontStyle.Bold;
            tituloBox.alignment = TextAnchor.MiddleCenter;

            //Topbar
            GUILayout.BeginHorizontal();
            GUILayout.Space(8);
            GUILayout.BeginVertical();
            GUILayout.Space(8);
            GUIStyle estiloIcone = new GUIStyle();
            estiloIcone.border = new RectOffset(0, 0, 0, 0);
            estiloIcone.margin = new RectOffset(4, 0, 4, 0);
            GUILayout.Box(iconOfUi, estiloIcone, GUILayout.Width(48), GUILayout.Height(44));
            GUILayout.Space(6);
            GUILayout.EndVertical();
            GUILayout.Space(8);
            GUILayout.Space(-55);
            GUILayout.BeginVertical();
            GUILayout.Space(20);
            GUIStyle titulo = new GUIStyle();
            titulo.fontSize = 25;
            titulo.normal.textColor = Color.black;
            titulo.alignment = TextAnchor.MiddleLeft;
            EditorGUILayout.LabelField("Native Android Toolkit", titulo);
            GUILayout.Space(4);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            //Setup 
            EditorGUILayout.LabelField("Setup of Asset", tituloBox);
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Setup will install \"NAT Data Handler\" in the main scene of your game. The GameObject \"NAT Data Handler\" will persist even if other scenes are loaded and it is responsible for communication between your game and Native Android. Click the button below to start the installation.", MessageType.Info);
            GUILayout.Space(10);
            if (GUILayout.Button("Start Installation", GUILayout.Height(40)))
            {
                RunInstallWizard();
            }
            GUILayout.Space(3);

            GUILayout.EndVertical();

            //Apply changes on script, case is not playing in editor
            if (GUI.changed == true && Application.isPlaying == false)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            if (EditorGUI.EndChangeCheck() == true)
            {

            }
        }

        void RunInstallWizard()
        {
            //Start the install
            EditorUtility.DisplayDialog("Installation Wizard", "The installation wizard will assist you in the process of installing GameObject \"NAT Data Handler\" in the primary scene of your game.\n\nThis GameObject acts as a bridge for communicating your game with Native Android code. For functions such as dialogs to work, this GameObject must be installed correctly. This GameObject must remain in the first scene of your game (the scene your game plays when it is opened), as this GameObject is indestructible and will persist even if another scene is loaded, so the communication bridge will always remain active and all Native Android Toolkit functions will work.\n\nIf your game does not have a first scene registered on the \"Build Settings\" screen, you can install this GameObject on the first scene your game plays when it opens.", "Next");

            //Get data of scenes
            int registeredScenes = SceneManager.sceneCountInBuildSettings;
            int activeSceneId = SceneManager.GetActiveScene().buildIndex;

            //Set registered scenes to zero, if the main is deleted
            if (registeredScenes > 0 && AssetDatabase.LoadAssetAtPath(SceneUtility.GetScenePathByBuildIndex(0), typeof(object)) == null)
            {
                registeredScenes = 0;
            }

            //If not exists registered scene
            if (registeredScenes == 0)
            {
                int confirmation = EditorUtility.DisplayDialogComplex("Installation Wizard",
                    "Could not find a main scene registered in \"Build Settings\". You can install \"NAT Data Handler\" on the current scene, but communication with Android Native code will only start working when your game runs this scene.\n\nDo you want to proceed with the installation?",
                    "Install In This Scene",
                    "Open Build Settings",
                    "Cancel");
                switch (confirmation)
                {
                    case 0:
                        //Create data handler
                        EditorUtility.DisplayDialog("Installation Wizard",
                        "Data Handler was successfully installed on your scene!",
                        "Finish");
                        CreateDataHandler();
                        this.Close();
                        EditorUtility.DisplayDialog("Installation Wizard",
                        "The installation was completed successfully. You can always repeat installation by going to \"Tools > MT Assets > Native Android Toolkit > Open Setup\" tab.",
                        "Ok");
                        break;
                    case 1:
                        //Open build settings
                        EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
                        break;
                    case 2:
                        break;
                }
            }

            //If exists registered scene
            if (registeredScenes > 0)
            {
                EditorUtility.DisplayDialog("Installation Wizard",
                    "The main scene of your game was found in \"Build Settings\". Click next to proceed with the installation.",
                    "Next");

                //Load the first scene
                string firstScene = SceneUtility.GetScenePathByBuildIndex(0);
                EditorSceneManager.OpenScene(firstScene);

                EditorUtility.DisplayDialog("Installation Wizard",
                        "Data Handler was successfully installed on your scene!",
                        "Finish");

                CreateDataHandler();

                this.Close();

                EditorUtility.DisplayDialog("Installation Wizard",
                        "The installation was completed successfully. You can always repeat installation by going to \"Tools > MT Assets > Native Android Toolkit > Open Setup\" tab.",
                        "Ok");
            }
        }

        void CreateDataHandler()
        {
            //Save the prefs of install
            natPreferences.natInstalledInFirstScene = true;
            SaveThePreferences();

            //Create the data handler in the scene
            GameObject dataReceiverFind = GameObject.Find("NAT Data Handler");
            if(dataReceiverFind == null)
            {
                GameObject dataReceiver = new GameObject("NAT Data Handler");
                dataReceiver.transform.SetAsFirstSibling();
                dataReceiver.tag = "NAT DataHandler";
                dataReceiver.AddComponent<NativeAndroidDataHandler>();
                GameObjectUtility.SetStaticEditorFlags(dataReceiver, StaticEditorFlags.BatchingStatic);
                EditorGUIUtility.PingObject(dataReceiver);
                Selection.objects = new UnityEngine.Object[] { dataReceiver };
            }
            if(dataReceiverFind != null)
            {
                EditorGUIUtility.PingObject(dataReceiverFind);
                Selection.objects = new UnityEngine.Object[] { dataReceiverFind };
            }
        }
    }
}