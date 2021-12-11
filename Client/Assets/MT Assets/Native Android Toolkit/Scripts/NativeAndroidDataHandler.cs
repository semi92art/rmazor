#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MTAssets.NativeAndroidToolkit;
using MTAssets.NativeAndroidToolkit.Time;
using MTAssets.NativeAndroidToolkit.Events;

[AddComponentMenu("")] //Hide this script in component menu.
public class NativeAndroidDataHandler : MonoBehaviour
{
    /*
     * This class is responsible for communicating between your game and Android Native code.
     */

    //Data to save of Native Android Toolkit
    [System.Serializable]
    public class NativeAndroidToolkitData
    {
        public long lastDateTheAppIsRunning = 0;
    }

    //Loaded variables from /data.nat
    private static long lastDateTheAppIsRunning;

    //Private variables of this Data Handler
    private static WaitForSeconds waitTimeForGetTexture2dOfScreenshot = new WaitForSeconds(1.5f);
    private static float delayOfTimeMonitor = 5;
    private static Calendar timeOfLastCheckIfIsAppRunning;

#if UNITY_EDITOR
    //Public variables of Interface
    private bool gizmosOfThisComponentIsDisabled = false;

    //The UI of this component
    #region INTERFACE_CODE
    [UnityEditor.CustomEditor(typeof(NativeAndroidDataHandler))]
    public class CustomInspector : UnityEditor.Editor
    {
        public bool DisableGizmosInSceneView(string scriptClassNameToDisable, bool isGizmosDisabled)
        {
            /*
            *  This method disables Gizmos in scene view, for this component
            */

            if (isGizmosDisabled == true)
                return true;

            //Try to disable
            try
            {
                //Get all data of Unity Gizmos manager window
                var Annotation = System.Type.GetType("UnityEditor.Annotation, UnityEditor");
                var ClassId = Annotation.GetField("classID");
                var ScriptClass = Annotation.GetField("scriptClass");
                var Flags = Annotation.GetField("flags");
                var IconEnabled = Annotation.GetField("iconEnabled");

                System.Type AnnotationUtility = System.Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
                var GetAnnotations = AnnotationUtility.GetMethod("GetAnnotations", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                var SetIconEnabled = AnnotationUtility.GetMethod("SetIconEnabled", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

                //Scann all Gizmos of Unity, of this project
                System.Array annotations = (System.Array)GetAnnotations.Invoke(null, null);
                foreach (var a in annotations)
                {
                    int classId = (int)ClassId.GetValue(a);
                    string scriptClass = (string)ScriptClass.GetValue(a);
                    int flags = (int)Flags.GetValue(a);
                    int iconEnabled = (int)IconEnabled.GetValue(a);

                    // this is done to ignore any built in types
                    if (string.IsNullOrEmpty(scriptClass))
                    {
                        continue;
                    }

                    const int HasIcon = 1;
                    bool hasIconFlag = (flags & HasIcon) == HasIcon;

                    //If the current gizmo is of the class desired, disable the gizmo in scene
                    if (scriptClass == scriptClassNameToDisable)
                    {
                        if (hasIconFlag && (iconEnabled != 0))
                        {
                            /*UnityEngine.Debug.LogWarning(string.Format("Script:'{0}' is not ment to show its icon in the scene view and will auto hide now. " +
                                "Icon auto hide is checked on script recompile, if you'd like to change this please remove it from the config", scriptClass));*/
                            SetIconEnabled.Invoke(null, new object[] { classId, scriptClass, 0 });
                        }
                    }
                }

                return true;
            }
            //Catch any error
            catch (System.Exception exception)
            {
                string exceptionOcurred = "";
                exceptionOcurred = exception.Message;
                if (exceptionOcurred != null)
                    exceptionOcurred = "";
                return false;
            }
        }

        public Rect GetInspectorWindowSize()
        {
            //Returns the current size of inspector window
            return EditorGUILayout.GetControlRect(true, 0f);
        }

        public override void OnInspectorGUI()
        {
            //Start the undo event support, draw default inspector and monitor of changes
            NativeAndroidDataHandler script = (NativeAndroidDataHandler)target;
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(target, "Undo Event");
            script.gizmosOfThisComponentIsDisabled = DisableGizmosInSceneView("NativeAndroidDataHandler", script.gizmosOfThisComponentIsDisabled);

            var warnStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            EditorGUILayout.HelpBox("Remember to read the Native Android Toolkit documentation to understand how to use it.\nGet support at: mtassets@windsoft.xyz", MessageType.None);

            //Warning
            EditorGUILayout.LabelField("Warning", warnStyle, GUILayout.ExpandWidth(true));
            EditorGUILayout.HelpBox("This GameObject is responsible for receiving and transfer responses to Android system, and communicating between your game and the system, with the Active Native Android Toolkit. Please do not delete this. Keep this GameObject in the first scene of your game. It can only be added to the first scene, to avoid duplicates.", MessageType.Info);

            GUILayout.Space(10);

            if (GUILayout.Button("Change NAT Preferences", GUILayout.Height(22)))
            {
                System.Reflection.Assembly editorAssembly = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName.StartsWith("Assembly-CSharp-Editor,")); //',' included to ignore  Assembly-CSharp-Editor-FirstPass
                Type utilityType = editorAssembly.GetTypes().FirstOrDefault(t => t.FullName.Contains("MTAssets.NativeAndroidToolkit.Editor.Preferences"));
                utilityType.GetMethod("OpenWindow", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Invoke(obj: null, parameters: new object[] { false });
            }

            GUILayout.Space(10);

            //Apply changes on script, case is not playing in editor
            if (GUI.changed == true && Application.isPlaying == false)
            {
                EditorUtility.SetDirty(script);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(script.gameObject.scene);
            }
            if (EditorGUI.EndChangeCheck() == true)
            {

            }
        }
    }
    #endregion
#endif

    //--------------          Code of this Data Handler         ------------------

    private void LoadData()
    {
        //Load the data of asset
        if (File.Exists(Application.persistentDataPath + "/NAT/data.nat") == true)
        {
            BinaryFormatter binnary = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/NAT/data.nat", FileMode.Open);
            NativeAndroidToolkitData data = (NativeAndroidToolkitData)binnary.Deserialize(file);

            //Load the data
            lastDateTheAppIsRunning = data.lastDateTheAppIsRunning;

            file.Close();
        }
        if (File.Exists(Application.persistentDataPath + "/NAT/data.nat") == false)
        {
            SaveData();
        }
    }

    private void SaveData()
    {
        //Create the NAT directory, if not existis
        if (Application.platform == RuntimePlatform.Android == false)
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/NAT");
        }
        if (Application.platform == RuntimePlatform.Android == true)
        {
            NativeAndroid.File.CreateDirectory(Application.persistentDataPath + "/NAT");
        }

        //Save asset data in binnary file
        BinaryFormatter binnary = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/NAT/data.nat");
        NativeAndroidToolkitData data = new NativeAndroidToolkitData();

        //Dados de configurações
        data.lastDateTheAppIsRunning = timeOfLastCheckIfIsAppRunning.GetTimeUtcUnixMillis();

        binnary.Serialize(file, data);
        file.Close();
    }

    //Prepare this GameObject and Initialize Java side of Native Android Toolkit
    public void Awake()
    {
        //Delete others Data Handlers, to avoid duplicates
        GameObject[] dataHandlers = GameObject.FindGameObjectsWithTag("NAT DataHandler");
        if (dataHandlers != null && dataHandlers.Length >= 2)
        {
            for (int i = 0; i < dataHandlers.Length; i++)
            {
                if (i == 0)
                {
                    continue;
                }
                if (dataHandlers[i] != null)
                {
                    DestroyImmediate(dataHandlers[i], true);
                    Debug.Log("A duplicate of the NAT Data Handler was found in this scene. It has already been removed.");
                    return;
                }
            }
        }

        //Set correct name
        this.gameObject.name = "NAT Data Handler";

        //Configures to not destroy
        DontDestroyOnLoad(this.gameObject);

        //If is in Android, Initialize the Java Lib
        AndroidJavaObject activity = null;
        AndroidJavaObject context = null;
        if (Application.platform == RuntimePlatform.Android == true)
        {
            //Get current activity
            AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            activity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");

            //Get current context
            context = activity.Call<AndroidJavaObject>("getApplicationContext");

            //Start the plugin
            AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Global.NAT");
            javaClass.CallStatic("InitializeJavaLib", context);
        }

        //Initialize variables of this Data Handler
        timeOfLastCheckIfIsAppRunning = new Calendar();

        //Load the data, if exists
        LoadData();

        //Initialize the static side of asset (NativeAndroid)
        if (lastDateTheAppIsRunning == 0)
        {
            NativeAndroid.InitializeStaticClass(activity, context, this, new Calendar().DecreaseIn(10, TimeSpanValue.Seconds), new Calendar(), false);
        }
        if (lastDateTheAppIsRunning != 0)
        {
            NativeAndroid.InitializeStaticClass(activity, context, this, new Calendar(lastDateTheAppIsRunning), new Calendar(), isDateTimeChangedSinceLastCheck());
        }
    }

    //Run event of OnResumeAfterPause when pause or resume game (minimize or restore game)
    public void OnApplicationPause(bool isPaused)
    {
        //On Pause
        if (isPaused == true)
        {
            delayOfTimeMonitor = 0;

            timeOfLastCheckIfIsAppRunning.SetTimeToNow();
            SaveData();
        }

        //On Resume
        if (isPaused == false)
        {
            delayOfTimeMonitor = 0;

            //Call delegate to run all registered methods of event
            if (TimeEvents.onResumeAppAfterPause != null)
                TimeEvents.onResumeAppAfterPause(new TimeElapsedInfo(isDateTimeChangedSinceLastCheck(), new Calendar().DecreaseWithDate(timeOfLastCheckIfIsAppRunning)));

            timeOfLastCheckIfIsAppRunning.SetTimeToNow();
            SaveData();
        }
    }

    //Monitor of time for the app. Responsible for storing the time the game was last closed.
    public void Update()
    {
        //Increase time of monitor, with unscaled delta time
        delayOfTimeMonitor += Time.unscaledDeltaTime;

        //Save current DateTime every 5 seconds, to get a very approximate time from the time the app was closed
        if (delayOfTimeMonitor >= 5)
        {
            timeOfLastCheckIfIsAppRunning.SetTimeToNow();
            SaveData();

            delayOfTimeMonitor = 0;
        }
    }

    //Get information if the date/time has changed on device, since last check
    bool isDateTimeChangedSinceLastCheck()
    {
        if (Application.platform == RuntimePlatform.Android == true)
        {
            AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Time");
            return javaClass.CallStatic<bool>("isDateChangedSinceLastCheck");
        }
        return false;
    }

    //--------------          Alert Dialogs Events         ------------------

    //Results for Simple Alert Dialog. The event call all handlers and Reset the handlers list for save peformance
    private void SimpleAlertDialog_OkButton()
    {
        if (DialogsEvents.onSimpleAlertDialogOk != null)
            DialogsEvents.onSimpleAlertDialogOk();

        //Reset all events after interact with this
        DialogsEvents.onSimpleAlertDialogCancel = null;
        DialogsEvents.onSimpleAlertDialogOk = null;
    }
    private void SimpleAlertDialog_Cancel()
    {
        if (DialogsEvents.onSimpleAlertDialogCancel != null)
            DialogsEvents.onSimpleAlertDialogCancel();

        //Reset all events after interact with this
        DialogsEvents.onSimpleAlertDialogCancel = null;
        DialogsEvents.onSimpleAlertDialogOk = null;
    }

    //Results for Confirmation Dialog. The event call all handlers and Reset the handlers list for save peformance
    private void ConfirmationDialog_YesButton()
    {
        if (DialogsEvents.onConfirmationDialogYes != null)
            DialogsEvents.onConfirmationDialogYes();

        //Reset all events after interact with this
        DialogsEvents.onConfirmationDialogCancel = null;
        DialogsEvents.onConfirmationDialogYes = null;
        DialogsEvents.onConfirmationDialogNo = null;
    }
    private void ConfirmationDialog_NoButton()
    {
        if (DialogsEvents.onConfirmationDialogNo != null)
            DialogsEvents.onConfirmationDialogNo();

        //Reset all events after interact with this
        DialogsEvents.onConfirmationDialogCancel = null;
        DialogsEvents.onConfirmationDialogYes = null;
        DialogsEvents.onConfirmationDialogNo = null;
    }
    private void ConfirmationDialog_Cancel()
    {
        if (DialogsEvents.onConfirmationDialogCancel != null)
            DialogsEvents.onConfirmationDialogCancel();

        //Reset all events after interact with this
        DialogsEvents.onConfirmationDialogCancel = null;
        DialogsEvents.onConfirmationDialogYes = null;
        DialogsEvents.onConfirmationDialogNo = null;
    }

    //Results for Radial List Dialog. The event call all handlers and Reset the handlers list for save peformance
    private void RadialListDialog_Done(string result)
    {
        if (DialogsEvents.onRadialListDialogDone != null)
            DialogsEvents.onRadialListDialogDone(int.Parse(result));

        //Reset all events after interact with this
        DialogsEvents.onRadialListDialogDone = null;
        DialogsEvents.onRadialListDialogCancel = null;
    }
    private void RadialListDialog_Cancel()
    {
        if (DialogsEvents.onRadialListDialogCancel != null)
            DialogsEvents.onRadialListDialogCancel();

        //Reset all events after interact with this
        DialogsEvents.onRadialListDialogDone = null;
        DialogsEvents.onRadialListDialogCancel = null;
    }

    //Results for Checkbox List Dialog. The event call all handlers and Reset the handlers list for save peformance
    private void CheckboxListDialog_Done(string result)
    {
        string[] resultsStr = result.Split(',');
        bool[] resultsBool = new bool[resultsStr.Length];
        for (int i = 0; i < resultsBool.Length; i++)
        {
            resultsBool[i] = bool.Parse(resultsStr[i]);
        }

        if (DialogsEvents.onCheckboxListDialogDone != null)
            DialogsEvents.onCheckboxListDialogDone(resultsBool);

        //Reset all events after interact with this
        DialogsEvents.onCheckboxListDialogDone = null;
        DialogsEvents.onCheckboxListDialogCancel = null;
    }
    private void CheckboxListDialog_Cancel()
    {
        if (DialogsEvents.onCheckboxListDialogCancel != null)
            DialogsEvents.onCheckboxListDialogCancel();

        //Reset all events after interact with this
        DialogsEvents.onCheckboxListDialogDone = null;
        DialogsEvents.onCheckboxListDialogCancel = null;
    }

    //Results for Neutral Dialog. The event call all handlers and Reset the handlers list for save peformance
    private void NeutralDialog_YesButton()
    {
        if (DialogsEvents.onNeutralYes != null)
            DialogsEvents.onNeutralYes();

        //Reset all events after interact with this
        DialogsEvents.onNeutralYes = null;
        DialogsEvents.onNeutralNo = null;
        DialogsEvents.onNeutralNeutral = null;
        DialogsEvents.onNeutralCancel = null;
    }
    private void NeutralDialog_NoButton()
    {
        if (DialogsEvents.onNeutralNo != null)
            DialogsEvents.onNeutralNo();

        //Reset all events after interact with this
        DialogsEvents.onNeutralYes = null;
        DialogsEvents.onNeutralNo = null;
        DialogsEvents.onNeutralNeutral = null;
        DialogsEvents.onNeutralCancel = null;
    }
    private void NeutralDialog_NeutralButton()
    {
        if (DialogsEvents.onNeutralNeutral != null)
            DialogsEvents.onNeutralNeutral();

        //Reset all events after interact with this
        DialogsEvents.onNeutralYes = null;
        DialogsEvents.onNeutralNo = null;
        DialogsEvents.onNeutralNeutral = null;
        DialogsEvents.onNeutralCancel = null;
    }
    private void NeutralDialog_CancelButton()
    {
        if (DialogsEvents.onNeutralCancel != null)
            DialogsEvents.onNeutralCancel();

        //Reset all events after interact with this
        DialogsEvents.onNeutralYes = null;
        DialogsEvents.onNeutralNo = null;
        DialogsEvents.onNeutralNeutral = null;
        DialogsEvents.onNeutralCancel = null;
    }

    //--------------          Sharing         ------------------

    //Wait a time and call event to run callbacks of take screnshot and get texture 2D
    public static IEnumerator StartTimeCounterToCallEventOfCompleteScreenShotAndGetTexture2D(string pathToScreenshotFile)
    {
        yield return waitTimeForGetTexture2dOfScreenshot;

        //Read the texture from memory
        byte[] data = File.ReadAllBytes(Application.persistentDataPath + "/" + pathToScreenshotFile);
        Texture2D texture2D = new Texture2D(Screen.width, Screen.height);
        texture2D.LoadImage(data);

        //Call the event for return texture, and clear all registrations
        if (SharingEvents.onCompleteTexture2Dprocessing != null)
            SharingEvents.onCompleteTexture2Dprocessing(texture2D);
        SharingEvents.onCompleteTexture2Dprocessing = null;
    }

    //--------------          Time Events         ------------------

    //Results for Hour Picker. The event call all handlers and Reset the handlers list for save peformance
    private void HourPicker_OnPicked(string result)
    {
        string[] resultSplited = result.Split(',');

        if (TimeEvents.onHourPicked != null)
            TimeEvents.onHourPicked(new Calendar(0, 0, 0, int.Parse(resultSplited[0]), int.Parse(resultSplited[1]), 0));
        TimeEvents.onHourPicked = null;
    }
    private void DatePicker_OnPicked(string result)
    {
        string[] date = result.Split(',');

        if (TimeEvents.onDatePicked != null)
            TimeEvents.onDatePicked(new Calendar(int.Parse(date[0]), int.Parse(date[1]), int.Parse(date[2]), 0, 0, 0));
        TimeEvents.onDatePicked = null;
    }
    private void LoadCurrentTimeOfNtp_OnDoneNtpRequest(string result)
    {
        long resultTime = long.Parse(result);
        if (resultTime == -1)
        {
            if (TimeEvents.onDoneNtpRequest != null)
                TimeEvents.onDoneNtpRequest(false, new Calendar().SetTimeToZero());
            TimeEvents.onDoneNtpRequest = null;
        }
        if (resultTime != -1)
        {
            if (TimeEvents.onDoneNtpRequest != null)
                TimeEvents.onDoneNtpRequest(true, new Calendar(long.Parse(result)));
            TimeEvents.onDoneNtpRequest = null;
        }
    }

    //--------------          WebView Events         ------------------

    //Results for Webview. The event call all handlers and Reset the handlers list for save peformance
    private void Webview_OnClose()
    {
        if (WebviewEvents.onWebviewClose != null)
            WebviewEvents.onWebviewClose();
        WebviewEvents.onWebviewClose = null;
    }
}