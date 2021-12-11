using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using MTAssets.NativeAndroidToolkit.Time;
using MTAssets.NativeAndroidToolkit.Notify;
using MTAssets.NativeAndroidToolkit.Files;
using MTAssets.NativeAndroidToolkit.Webview;
using System.Linq;

namespace MTAssets.NativeAndroidToolkit
{
    /*
      This class is responsible for the functioning of the "Native Android Toolkit"
    */
    /*
     * The Native Android Toolkit was developed by Marcos Tomaz in 2019.
     * Need help? Contact me (mtassets@windsoft.xyz)
     */

    public class NativeAndroid
    {
        //Classes organized by function
        //Note that the functions only work when run on the Android and Editor platform. If you try to run them on another platform, the functions not works

        //Global variables
        private static AndroidJavaObject currentUnityActivity;
        private static AndroidJavaObject currentUnityContext;
        private static NativeAndroidDataHandler nativeAndroidDataHandler;
        private static Calendar timeOfLastCloseOfApp;
        private static Calendar timeOfLastOpenOfApp;
        private static bool wasDateChangedWhenAppIsClosed;

        //Methods of Native Android Toolkit asset
        public static void InitializeStaticClass(AndroidJavaObject activity, AndroidJavaObject context, NativeAndroidDataHandler dataHandler, Calendar timeLastClose, Calendar timeLastOpen, bool dateChangedSinceLastClose)
        {
            currentUnityActivity = activity;
            currentUnityContext = context;
            nativeAndroidDataHandler = dataHandler;
            timeOfLastCloseOfApp = timeLastClose;
            timeOfLastOpenOfApp = timeLastOpen;
            wasDateChangedWhenAppIsClosed = dateChangedSinceLastClose;

            NativeAndroid.LaunchDebugLog("Native Android Toolkit is Initilized!");
        }

        private static void LaunchDebugLog(string message)
        {
#if UNITY_EDITOR
            //Get info if "Launch logs option is enabled"
            System.Reflection.Assembly editorAssembly = System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName.StartsWith("Assembly-CSharp-Editor,")); //',' included to ignore  Assembly-CSharp-Editor-FirstPass
            Type utilityType = editorAssembly.GetTypes().FirstOrDefault(t => t.FullName.Contains("MTAssets.NativeAndroidToolkit.Editor.Preferences"));
            System.Reflection.MethodInfo info = utilityType.GetMethod("isLaunchLogsEnabled", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            var isLaunchLogsEnabled = info.Invoke(obj: null, parameters: null);

            if ((bool)isLaunchLogsEnabled == true)
            {
                Debug.Log(message);
            }
#endif
        }

        //Getters for Native Android Toolkit
        public static AndroidJavaObject CurrentUnityActivity
        {
            get { return currentUnityActivity; }
        }

        public static AndroidJavaObject CurrentUnityContext
        {
            get { return currentUnityContext; }
        }

        public static NativeAndroidDataHandler NativeAndroidDataHandler
        {
            get { return nativeAndroidDataHandler; }
        }

        public static Calendar TimeOfLastCloseOfApp
        {
            get { return timeOfLastCloseOfApp; }
        }

        public static Calendar TimeOfLastOpenOfApp
        {
            get { return timeOfLastOpenOfApp; }
        }

        public static bool WasDateChangedWhenAppIsClosed
        {
            get { return wasDateChangedWhenAppIsClosed; }
        }

        //Classes for functions
        public class Dialogs
        {
            //Calls native code to display a simple dialog box
            public static void ShowSimpleAlertDialog(string title, string text)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Displaying a Simple Alert Dialog...\nTitle: " + title + "\nText: " + text);
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Dialogs");
                    javaClass.CallStatic("ShowSimpleAlertDialog", currentUnityActivity, title, text);
                }
            }

            //Calls native code to display a confirmation dialog
            public static void ShowConfirmationDialog(string title, string text, string yesButton, string noButton)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Displaying a Confirmation Dialog...\nTitle: " + title + "\nText: " + text + "\nYes Button: " + yesButton + "\nNo Button: " + noButton);
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Dialogs");
                    javaClass.CallStatic("ShowConfirmationDialog", currentUnityActivity, title, text, yesButton, noButton);
                }
            }

            //Calls native code to display a radial list dialog
            public static void ShowRadialListDialog(string title, string doneButton, string[] options)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    StringBuilder ops = new StringBuilder();
                    foreach (string str in options)
                    {
                        ops.Append(str + ".");
                    }
                    LaunchDebugLog("Displaying a Radial List Dialog...\nTitle: " + title + "\nDone Button: " + doneButton + "\nOptions: " + ops.ToString());
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Dialogs");
                    javaClass.CallStatic("ShowRadialListDialog", currentUnityActivity, title, doneButton, options);
                }
            }

            //Calls native code to display a checkbox list dialog
            public static void ShowCheckboxListDialog(string title, string doneButton, string[] options, bool[] checkedOptions)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    StringBuilder ops = new StringBuilder();
                    StringBuilder checks = new StringBuilder();
                    foreach (string str in options)
                    {
                        ops.Append(str + ".");
                    }
                    foreach (bool boolean in checkedOptions)
                    {
                        checks.Append(boolean.ToString() + ".");
                    }
                    LaunchDebugLog("Displaying a Checkbox List Dialog...\nTitle: " + title + "\nDone Button: " + doneButton + "\nOptions: " + ops.ToString() + "\nDefault Checks: " + checkedOptions);
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Dialogs");
                    javaClass.CallStatic("ShowCheckboxListDialog", currentUnityActivity, title, doneButton, options, checkedOptions);
                }
            }

            //Calls native code to display a neutral dialog
            public static void ShowNeutralDialog(string title, string text, string yesButton, string noButton, string neutralButton)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Displaying a Neutral Dialog...\nTitle: " + title + "\nText: " + text + "\nYes Button: " + yesButton + "\nNo Button: " + noButton + "\nNeutral Button: " + neutralButton);
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Dialogs");
                    javaClass.CallStatic("ShowNeutralDialog", currentUnityActivity, title, text, yesButton, noButton, neutralButton);
                }
            }
        }

        public class Notifications
        {
            //Calls native code to send a notification
            public static void SendPushNotification(string title, string text)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Push Notification delivered\nTitle: " + title + "\nMessage: " + text);
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Notifications");
                    javaClass.CallStatic("SendPushNotification", currentUnityContext, title, text);
                }
            }

            //Calls native code to show toast
            public static void ShowToast(string text, bool longDuration, ToastPosition position)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Showing Toast\nText: " + text + "\nLong Duration: " + longDuration.ToString() + "\nPosition: " + position.ToString());
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //Get position
                    int positionInt = -1;
                    switch (position)
                    {
                        case ToastPosition.Top:
                            positionInt = 0;
                            break;
                        case ToastPosition.Center:
                            positionInt = 1;
                            break;
                        case ToastPosition.Bottom:
                            positionInt = 2;
                            break;
                    }

                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Notifications");
                    javaClass.CallStatic("ShowToast", currentUnityContext, text, longDuration, positionInt);
                }
            }

            //Calls native code to get response if Inactivity Notification is Prepared
            public static bool isInactivityNotificationPrepared()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Is Inactivity Notification Scheduled?\nResponse: False");
                    return false;
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Notifications");
                    return javaClass.CallStatic<bool>("isInactivityNotificationPrepared", currentUnityContext);
                }
                return false;
            }

            //Calls native code to Prepare Inactivity Notification
            public static void PrepareInactivityNotification(string title, string text)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("The Inactivity Notification was scheduled to 3 days in future. (Simulation)");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Notifications");
                    javaClass.CallStatic("PrepareInactivityNotification", currentUnityContext, title, text);
                }
            }

            //Calls native code to Cancel Inactivity Notification
            public static void CancelInactivityNotification()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("The Inactivity Notification was canceled.");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Notifications");
                    javaClass.CallStatic("CancelInactivityNotification", currentUnityContext);
                }
            }

            //Calls native code to verify if a channel have a scheduled notification
            public static bool isNotificationScheduledInChannel(Channel channel)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Exists a Notification Scheduled in channel " + ChannelTools.ChToInt(channel) + "?\nResponse: False");
                    return false;
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Notifications");
                    return javaClass.CallStatic<bool>("isNotificationScheduledInChannel", currentUnityContext, ChannelTools.ChToInt(channel));
                }
                return false;
            }

            //Builder for construct the new scheduled notification
            public class ScheduledNotification
            {
                private Channel channel;
                private string title;
                private string text;
                private int years;
                private int months;
                private int days;
                private int hours;
                private int minutes;

                public ScheduledNotification(Channel channel, string title, string text)
                {
                    this.channel = channel;
                    this.title = title;
                    this.text = text;
                    this.years = 0;
                    this.months = 0;
                    this.days = 0;
                    this.hours = 0;
                    this.minutes = 0;
                }

                public ScheduledNotification setYearsInFuture(int years)
                {
                    this.years = years;
                    return this;
                }

                public ScheduledNotification setMonthsInFuture(int months)
                {
                    this.months = months;
                    return this;
                }

                public ScheduledNotification setDaysInFuture(int days)
                {
                    this.days = days;
                    return this;
                }

                public ScheduledNotification setHoursInFuture(int hours)
                {
                    this.hours = hours;
                    return this;
                }

                public ScheduledNotification setMinutesInFuture(int minutes)
                {
                    this.minutes = minutes;
                    return this;
                }

                //Calls native code to schedule notification in future
                public void ScheduleThisNotification()
                {
                    //If is not in Android
                    if (Application.platform == RuntimePlatform.Android == false)
                    {
                        LaunchDebugLog("The Push Notification was scheduled in channel " + ChannelTools.ChToInt(channel).ToString() + ".\nTime in future\n" + years.ToString() + " Years\n" + months.ToString() + " Months\n" + days.ToString() + " Days\n" + hours.ToString() + " Hours\n" + minutes.ToString() + " Minutes\nIn future...");
                    }
                    //If is in Android
                    if (Application.platform == RuntimePlatform.Android == true)
                    {
                        AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Notifications");
                        javaClass.CallStatic("ScheduleNotificationToFuture", currentUnityContext, ChannelTools.ChToInt(channel), title, text, years, months, days, hours, minutes);
                    }
                }
            }

            //Calls native code to cancel a scheduled notification of a channel
            public static void CancelScheduledNotification(Channel channel)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("The Push Notification scheduled in channel " + ChannelTools.ChToInt(channel).ToString() + " was canceled.");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Notifications");
                    javaClass.CallStatic("CancelScheduledNotification", currentUnityContext, ChannelTools.ChToInt(channel));
                }
            }
        }

        public class Sharing
        {
            //Calls native code to share a texture 2D
            public static void ShareTexture2D(Texture2D texture)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Sharing a Texture 2D...");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Sharing");
                    javaClass.CallStatic("ShareTexture2D", currentUnityActivity, Application.persistentDataPath, texture.EncodeToPNG());
                }
            }

            //Calls native code to share a text
            public static void SharePlainText(string titleOfShareWindow, string textToShare)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Sharing a Plain Text...\nShare Window: " + titleOfShareWindow + "\nText To Share: " + textToShare);
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Sharing");
                    javaClass.CallStatic("SharePlainText", currentUnityActivity, titleOfShareWindow, textToShare);
                }
            }

            //Calls native code to Take and Save Screenshot
            public static string TakeScreenshotAndSaveOnGallery()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Taking a Screenshot and saving on Gallery...");
                    return "";
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Sharing");

                    string nameOfScreenshot = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond).ToString() + ".png";
                    string pathToScreenshot = Application.persistentDataPath + "/" + nameOfScreenshot;
                    ScreenCapture.CaptureScreenshot(nameOfScreenshot);

                    return javaClass.CallStatic<string>("SaveScreenshotOnGallery", currentUnityContext, pathToScreenshot);
                }
                return "";
            }

            //Calls the code for take screenshow, wait unity and response with a texture2D
            public static void TakeScreenshotAndGetTexture2D()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Taking Screenshot, and returning Texture2D...");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    //Create the NAT directory, if not exists
                    NativeAndroid.File.CreateDirectory(Application.persistentDataPath + "/NAT");

                    string nameOfScreenshot = "NAT/scr.png";
                    ScreenCapture.CaptureScreenshot(nameOfScreenshot);

                    nativeAndroidDataHandler.StartCoroutine(NativeAndroidDataHandler.StartTimeCounterToCallEventOfCompleteScreenShotAndGetTexture2D(nameOfScreenshot));
                }
            }

            //Calls the native code for copy string to clipboard
            public static void CopyTextToClipboard(string content)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Copying the text to Clipboard!\nText To Copy: " + content);
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Sharing");
                    javaClass.CallStatic("CopyTextToClipBoard", currentUnityContext, content);
                }
            }

            //Calls the native code for get text from clipboard
            public static string GetTextFromClipboard()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Getting text from Clipboard\nText Returned: Some Text.");
                    return "Some text.";
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Sharing");
                    return javaClass.CallStatic<string>("GetTextFromClipBoard", currentUnityContext);
                }
                return "";
            }

            //Calls the native code for open a app in play store
            public static void OpenInPlayStore(string packageName)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Opening \"" + packageName + "\" in Play Store...");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Sharing");
                    javaClass.CallStatic("OpenInPlayStore", currentUnityActivity, packageName);
                }
            }
        }

        public class Utils
        {
            //Calls native code to verify if vibration is available
            public static bool isVibrationAvailable()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Testing if Vibration is available...\nResponse: True");
                    return true;
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isVibrationAvailable", currentUnityContext);
                }
                return false;
            }

            //Calls native code to vibrate device
            public static void Vibrate(long milliseconds)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Vibrating the phone...\nDuration: " + milliseconds + "ms");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    javaClass.CallStatic("Vibrate", currentUnityContext, milliseconds);
                }
            }

            //Calls native code to vibrate device with pattern
            public static void VibrateWithPattern(long[] patternOfMilliseconds)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    StringBuilder patther = new StringBuilder();
                    foreach (long millis in patternOfMilliseconds)
                    {
                        patther.Append(millis.ToString() + "ms.");
                    }
                    LaunchDebugLog("Vibrating with pattern...\nPattern: " + patther.ToString());
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    javaClass.CallStatic("VibrateWithPattern", currentUnityContext, patternOfMilliseconds);
                }
            }

            //Calls native code to verify if flash light is available
            public static bool isFlashLightAvailable()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Testing if Flash Light is available...\nResponse: True");
                    return true;
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isFlashLightAvailable", currentUnityContext);
                }
                return false;
            }

            //Calls native code to enable/disable flashlight
            public static void SetFlashLightEnabled(bool enabled)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Setting Flashlight to enabled? " + enabled.ToString());
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    javaClass.CallStatic("SetFlashLightEnabled", currentUnityContext, enabled);
                }
            }

            //Calls native code to request file permission
            public static void RequestFileMangementPermission()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Requesting File Management permissions...");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    javaClass.CallStatic("RequestFileManagementPermission", currentUnityActivity);
                }
            }

            //Calls native code to request camera permission
            public static void RequestCameraPermission()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Requesting Camera permissions...");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    javaClass.CallStatic("RequestCameraPermission", currentUnityActivity);
                }
            }

            //Calls native code to verify if file permission is verifyied
            public static bool isFilePermissionGuaranteed()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Testing permission of Files...\nResponse: True");
                    return true;
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isFilePermissionGuaranteed", currentUnityContext);
                }
                return false;
            }

            //Calls native code to verify if camera permission is verifyied
            public static bool isCameraPermissionGuaranteed()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Testing permission of Camera...\nResponse: True");
                    return true;
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isCameraPermissionGuaranteed", currentUnityContext);
                }
                return false;
            }

            //Calls native code to return version code of device
            public static int GetAndroidVersionCode()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Getting Android version code...\nResponse: 16");
                    return 16;
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<int>("GetAndroidVersionCode");
                }
                return 0;
            }

            //Calls native code to verify if is connected to wifi
            public static bool isConnectedToWifi()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Testing if is connected to Wi-Fi...\nResponse: True");
                    return true;
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isConnectedToWifi", currentUnityContext);
                }
                return false;
            }

            //Calls native code to verify if is using headset
            public static bool isUsingHeadset()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Testing if is using Headset...\nResponse: True");
                    return true;
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isUsingHeadset", currentUnityContext);
                }
                return false;
            }

            //Calls native code to verify if is using wireless headset
            public static bool isUsingWirelessHeadset()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Testing if is using Wireless Headset...\nResponse: True");
                    return true;
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isUsingWirelessHeadset", currentUnityContext);
                }
                return false;
            }

            //Calls native code to verify if network is available
            public static bool isInternetConnectivityAvailable()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Testing if Internet is available...\nResponse: True");
                    return true;
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isInternetConnectivityAvailable", currentUnityContext);
                }
                return false;
            }

            //Calls native code to verify if bluetooth is enabled
            public static bool isBluetoothEnabled()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Testing if Bluetooth is enabled...\nResponse: True");
                    return true;
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<bool>("isBluetoothEnabled");
                }
                return false;
            }

            //Calls native code to restart game
            public static void RestartUnityPlayer()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Restarting the Unity Play Activity...");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    javaClass.CallStatic("RestartUnityPlayer", currentUnityActivity);
                }
            }

            //Calls native open settings
            public static void OpenSettings()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Opening System Settings...");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    javaClass.CallStatic("OpenSettings", currentUnityActivity);
                }
            }

            //Calls native open settings wifi
            public static void OpenWifiSettings()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Opening System Wifi Settings...");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    javaClass.CallStatic("OpenWifiSettings", currentUnityActivity);
                }
            }

            //Calls native open settings of bluetooth
            public static void OpenBluetoothSettings()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Opening System Bluetooth Settings...");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    javaClass.CallStatic("OpenBluetoothSettings", currentUnityActivity);
                }
            }

            //Calls native open settings for this app
            public static void OpenThisAppSettings()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Opening This App System Settings...");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    javaClass.CallStatic("OpenThisAppSettings", currentUnityActivity);
                }
            }

            //Calls native code to get Android Unique ID
            public static string GetThisDeviceUniqueID()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Getting Android Unique ID...\nResponse: demoID-xxxxxabcdefghijklmnopqrstuvwxyz1234567890");
                    return "demoID-xxxxxabcdefghijklmnopqrstuvwxyz1234567890";
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Utils");
                    return javaClass.CallStatic<string>("GetThisDeviceUniqueID", currentUnityActivity);
                }
                return "";
            }
        }

        public class Apps
        {
            //Calls native code to verify if a app is installed
            public static bool isAppInstalled(string packageName)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Verifying if \"" + packageName + "\" is installed...\nResponse: True");
                    return true;
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Apps");
                    return javaClass.CallStatic<bool>("isAppInstalled", currentUnityContext, packageName);
                }
                return false;
            }

            //Open app by package
            public static void OpenAppByPackage(string packageName)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Openning app \"" + packageName + "\"...");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Apps");
                    javaClass.CallStatic("OpenAppByPackage", currentUnityContext, packageName);
                }
            }
        }

        public class Time
        {
            //----------------------- Public Native Methods -----------------

            //Calls native code to return hour picked
            public static void OpenHourPicker(string title)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Opening Sytem Hour Picker pop-up...");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Time");
                    javaClass.CallStatic("OpenHourPicker", currentUnityActivity, title);
                }
            }

            //Calls native code to return date picked
            public static void OpenDatePicker(string title)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Opening System Date Picker pop-up...");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Time");
                    javaClass.CallStatic("OpenDatePicker", currentUnityActivity, title);
                }
            }

            //Calls native code to access an NTP server and get the current time/date
            public static void LoadCurrentTimeOfNtp()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Loading Current Time Of Network.");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Time");
                    javaClass.CallStatic("LoadCurrentTimeOfNtp");
                }
            }

            //Calls native code to return real time since boot
            public static Calendar GetElapsedRealtimeSinceBoot()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Getting Elapsed Realtime Since Boot...");
                    return new Calendar();
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Time");
                    Calendar elapsedTime = new Calendar(javaClass.CallStatic<long>("GetElapsedRealtimeSinceBoot"));
                    elapsedTime.DecreaseWithDate(new Calendar(1970, 1, 1, 0, 0, 0));
                    return elapsedTime;
                }
                return new Calendar();
            }

            //---------------- Derived of Public Native Methods ------------

            //Calls native code to return time of last closing
            public static Calendar GetDateTimeOfLastClosing()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Getting Date Time of last closing.");
                    return new Calendar();
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    return timeOfLastCloseOfApp;
                }
                return new Calendar();
            }

            //Calls native code to return time elapsed since last time app closed
            public static TimeElapsedInfo GetElapsedTimeLastCloseUntilThisRun()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Getting Time Elapse Since Last Close Of App, to start of app.");
                    return new TimeElapsedInfo(false, new Calendar());
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    return new TimeElapsedInfo(wasDateChangedWhenAppIsClosed, new Calendar(timeOfLastOpenOfApp).DecreaseWithDate(timeOfLastCloseOfApp));
                }
                return new TimeElapsedInfo(false, new Calendar());
            }

            #region OBSOLETE_METHODS
            public static void ConfigOnChangeTimeNotification(bool enabled, string title, string text) {}
            #endregion
        }

        public class Webview
        {
            //Calls native code to open webview
            public static void OpenWebview(string title, string url, bool showToolbar, bool showControls)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Opening Webview with URL \"" + url + "\".");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Webview");
                    javaClass.CallStatic("OpenWebview", currentUnityActivity, url, title, showToolbar, showControls);
                }
            }
        
            //Calls native code to open webview in a new activity
            public static void OpenFullScreenWebview(string title, string url, bool showToolbar, bool showControls, WebviewOrientation screenOrientation)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Opening Webview (Fullscreen) with URL \"" + url + "\".");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Webview");
                    switch (screenOrientation)
                    { 
                        case WebviewOrientation.Portrait:
                            javaClass.CallStatic("OpenFullScreenWebview", currentUnityActivity, url, title, showToolbar, showControls, 1);
                            break;
                        case WebviewOrientation.Landscape:
                            javaClass.CallStatic("OpenFullScreenWebview", currentUnityActivity, url, title, showToolbar, showControls, 0);
                            break;
                    }
                }
            }
        }

        public class File
        {
            //----------------------- Public Native Methods -----------------

            //Calls native code to verify if internal memory is available
            public static bool isInternalMemoryAvailable()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Testing if Internal Memory is available...\nResponse: True");
                    return true;
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Files");
                    return javaClass.CallStatic<bool>("isInternalMemoryAvailable");
                }
                return false;
            }

            //Calls native code to get internal memory path
            public static string GetInternalMemoryPath()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Returning path of Internal Memory...\nPath: " + Application.persistentDataPath);
                    return Application.persistentDataPath;
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Files");
                    return javaClass.CallStatic<string>("GetInternalMemoryPath");
                }
                return "";
            }

            //Calls native code to return total memory
            public static FileSize GetTotalDeviceMemory()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Returning Total Memory of device...");
                    return new FileSize(1000000);
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Files");
                    return new FileSize(javaClass.CallStatic<long>("GetTotalDeviceMemory"));
                }
                return new FileSize(0);
            }

            //Calls native code to return free memory
            public static FileSize GetTotalFreeMemory()
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Returning Free Memory of device...");
                    return new FileSize(1024000);
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Files");
                    return new FileSize(javaClass.CallStatic<long>("GetTotalFreeMemory"));
                }
                return new FileSize(0);
            }

            //Calls native code to create directory if not exists
            public static void CreateDirectory(string path)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Creating Directory \"" + path + "\" if not exists...");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Files");
                    javaClass.CallStatic("CreateDirectory", path);
                }
            }

            //Calls native code to create a new file, with a byte array
            public static void CreateFile(string pathToNewFile, byte[] bytesToSave)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Creating new File with byte array...");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    AndroidJavaClass javaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Files");
                    javaClass.CallStatic("CreateFile", pathToNewFile, bytesToSave);
                }
            }

            //---------------- Derived of Public Native Methods ------------

            //Save text in a new text file
            public static void CreateTextFile(string pathToNewFile, string textToSave)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Saving text to a File...");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    CreateFile(pathToNewFile, Encoding.UTF8.GetBytes(textToSave));
                }
            }

            //Save a texture in a new image file
            public static void CreateImageFile(string pathToNewFile, Texture2D textureToSave)
            {
                //If is not in Android
                if (Application.platform == RuntimePlatform.Android == false)
                {
                    LaunchDebugLog("Saving Texture to File...");
                }
                //If is in Android
                if (Application.platform == RuntimePlatform.Android == true)
                {
                    CreateFile(pathToNewFile, textureToSave.EncodeToPNG());
                }
            }
        }
    }

    #region Add-Ons Classes
    namespace Webview
    {
        //Enum for screen rotation
        public enum WebviewOrientation
        {
            Portrait,
            Landscape
        }
    }

    namespace Events
    {
        //Events for Alert Dialogs
        public class DialogsEvents
        {
            //Simple Alert Dialog
            public delegate void OnSimpleAlertDialogOk();
            public static OnSimpleAlertDialogOk onSimpleAlertDialogOk;
            public delegate void OnSimpleAlertDialogCancel();
            public static OnSimpleAlertDialogCancel onSimpleAlertDialogCancel;

            //Confirmation Dialog
            public delegate void OnConfirmationDialogYes();
            public static OnConfirmationDialogYes onConfirmationDialogYes;
            public delegate void OnConfirmationDialogNo();
            public static OnConfirmationDialogNo onConfirmationDialogNo;
            public delegate void OnConfirmationDialogCancel();
            public static OnConfirmationDialogCancel onConfirmationDialogCancel;

            //Radial List Dialog
            public delegate void OnRadialListDialogDone(int result);
            public static OnRadialListDialogDone onRadialListDialogDone;
            public delegate void OnRadialListDialogCancel();
            public static OnRadialListDialogCancel onRadialListDialogCancel;

            //Checkbox list dialog
            public delegate void OnCheckboxListDialogDone(bool[] result);
            public static OnCheckboxListDialogDone onCheckboxListDialogDone;
            public delegate void OnCheckboxListDialogCancel();
            public static OnCheckboxListDialogCancel onCheckboxListDialogCancel;

            //Neutral dialog
            public delegate void OnNeutralYes();
            public static OnNeutralYes onNeutralYes;
            public delegate void OnNeutralNo();
            public static OnNeutralNo onNeutralNo;
            public delegate void OnNeutralNeutral();
            public static OnNeutralNeutral onNeutralNeutral;
            public delegate void OnNeutralCancel();
            public static OnNeutralCancel onNeutralCancel;
        }
    
        //Events for Sharing
        public class SharingEvents
        {
            //Take screenshor and get texture 2D
            public delegate void OnCompleteTexture2Dprocessing(Texture2D texture);
            public static OnCompleteTexture2Dprocessing onCompleteTexture2Dprocessing;
        }
    
        //Events for Time
        public class TimeEvents
        {
            //On pick hour in HourPicker
            public delegate void OnHourPicked(Calendar hourPicked);
            public static OnHourPicked onHourPicked;
            
            //On pick date in DatePicker
            public delegate void OnDatePicked(Calendar datePicked);
            public static OnDatePicked onDatePicked;

            //On done NTP request
            public delegate void OnDoneNtpRequest(bool sucess, Calendar currentTime);
            public static OnDoneNtpRequest onDoneNtpRequest;

            //Event called whenever the game is resumed after a pause (restored after being minimized)
            public delegate void OnResumeAppAfterPause(TimeElapsedInfo timeElapsedInfo);
            public static OnResumeAppAfterPause onResumeAppAfterPause;
        }
    
        //Events for Webview
        public class WebviewEvents
        {
            //On close th webview
            public delegate void OnWebviewClose();
            public static OnWebviewClose onWebviewClose;
        }
    }

    namespace Notify
    {
        //Enum for channel of notification
        public enum Channel
        {
            Ch_1,
            Ch_2,
            Ch_3,
            Ch_4,
            Ch_5,
            Ch_6,
            Ch_7,
            Ch_8,
            Ch_9,
            Ch_10,
            Ch_11,
            Ch_12,
            Ch_13,
            Ch_14,
            Ch_15,
            Ch_16,
            Ch_17,
            Ch_18,
            Ch_19,
            Ch_20
        }

        //Class to handler channel enum
        public class ChannelTools
        {
            //Convert a Channel enum value to int ID
            public static int ChToInt(Channel channel)
            {
                switch (channel)
                {
                    case Channel.Ch_1:
                        return 1;
                    case Channel.Ch_2:
                        return 2;
                    case Channel.Ch_3:
                        return 3;
                    case Channel.Ch_4:
                        return 4;
                    case Channel.Ch_5:
                        return 5;
                    case Channel.Ch_6:
                        return 6;
                    case Channel.Ch_7:
                        return 7;
                    case Channel.Ch_8:
                        return 8;
                    case Channel.Ch_9:
                        return 9;
                    case Channel.Ch_10:
                        return 10;
                    case Channel.Ch_11:
                        return 11;
                    case Channel.Ch_12:
                        return 12;
                    case Channel.Ch_13:
                        return 13;
                    case Channel.Ch_14:
                        return 14;
                    case Channel.Ch_15:
                        return 15;
                    case Channel.Ch_16:
                        return 16;
                    case Channel.Ch_17:
                        return 17;
                    case Channel.Ch_18:
                        return 18;
                    case Channel.Ch_19:
                        return 19;
                    case Channel.Ch_20:
                        return 20;
                    default: return 0;
                }
            }

            //Convert a int ID to Channel enum value
            public static Channel IntToCh(int intChannelID)
            {
                switch (intChannelID)
                {
                    case 1:
                        return Channel.Ch_1;
                    case 2:
                        return Channel.Ch_2;
                    case 3:
                        return Channel.Ch_3;
                    case 4:
                        return Channel.Ch_4;
                    case 5:
                        return Channel.Ch_5;
                    case 6:
                        return Channel.Ch_6;
                    case 7:
                        return Channel.Ch_7;
                    case 8:
                        return Channel.Ch_8;
                    case 9:
                        return Channel.Ch_9;
                    case 10:
                        return Channel.Ch_10;
                    case 11:
                        return Channel.Ch_11;
                    case 12:
                        return Channel.Ch_12;
                    case 13:
                        return Channel.Ch_13;
                    case 14:
                        return Channel.Ch_14;
                    case 15:
                        return Channel.Ch_15;
                    case 16:
                        return Channel.Ch_16;
                    case 17:
                        return Channel.Ch_17;
                    case 18:
                        return Channel.Ch_18;
                    case 19:
                        return Channel.Ch_19;
                    case 20:
                        return Channel.Ch_20;
                    default: return Channel.Ch_1;
                }
            }
        }

        //Enum of ToastPosition
        public enum ToastPosition
        {
            Top,
            Center,
            Bottom
        }
    }

    namespace Time
    {
        //Enum of TimeSpanValue
        public enum TimeSpanValue
        {
            Days,
            Hours,
            Minutes,
            Seconds
        }

        //Enum of TimeMode
        public enum TimeMode
        {
            UtcTime,
            LocalTime
        }

        //This class can storage a date time, and make another functions, like increase, decrease, compare datetimes and etc.
        public class Calendar
        {
            //Private variables of this class
            private int year = 0;
            private int month = 0;
            private int day = 0;
            private int hour = 0;
            private int minute = 0;
            private int second = 0;

            //Return value of this class on use Debug.Log();
            public override string ToString()
            {
                return "Time of this Calendar (M/D/Y H:M:S)\n" + month.ToString() + "/" + day.ToString() + "/" + year.ToString() + " " + hour.ToString() + ":" + minute.ToString() + ":" + second.ToString();
            }

            //Instantiate this class with the current local time of system
            public Calendar()
            {
                //Get current local time and store
                DateTime currentTimeLocal = DateTime.Now;
                year = currentTimeLocal.Year;
                month = currentTimeLocal.Month;
                day = currentTimeLocal.Day;
                hour = currentTimeLocal.Hour;
                minute = currentTimeLocal.Minute;
                second = currentTimeLocal.Second;
            }

            //Instantiate this class with the current UTC Unix time
            public Calendar(long utcUnixMillisTime)
            {
                //Convert unix timestamp to local datetime
                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                DateTime dateTime = epoch.AddMilliseconds(utcUnixMillisTime).ToLocalTime();

                //Stores the datetime
                year = dateTime.Year;
                month = dateTime.Month;
                day = dateTime.Day;
                hour = dateTime.Hour;
                minute = dateTime.Minute;
                second = dateTime.Second;
            }

            //Instantiate this class with a predefined date and time local
            public Calendar(int year, int month, int day, int hour, int minute, int second)
            {
                //Validate the time
                if (hour < 0)
                {
                    hour = 0;
                }
                if (hour > 23)
                {
                    hour = 23;
                }
                if (minute < 0)
                {
                    minute = 0;
                }
                if (minute > 59)
                {
                    minute = 59;
                }
                if (second < 0)
                {
                    second = 0;
                }
                if (second > 59)
                {
                    second = 59;
                }

                //Store the time now, local format
                this.year = year;
                this.month = month;
                this.day = day;
                this.hour = hour;
                this.minute = minute;
                this.second = second;
            }

            //Instantiate this class, from another calendar class
            public Calendar(Calendar anotherCalendarObject)
            {
                //Copy the data from another calendar object
                this.year = anotherCalendarObject.Year;
                this.month = anotherCalendarObject.Month;
                this.day = anotherCalendarObject.Day;
                this.hour = anotherCalendarObject.Hour;
                this.minute = anotherCalendarObject.Minute;
                this.second = anotherCalendarObject.Second;
            }

            //Return the correspondent value of this Calendar, converted to Unix TimeStamp
            public long GetTimeUtcUnixMillis()
            {
                //Convert the content of this, to DateTime
                int originalYear = year;
                int originalMonth = month;
                int originalDay = day;
                int rYear = year == 0 ? year + 1 : year;
                int rMonth = month == 0 ? month + 1 : month;
                int rDay = day == 0 ? day + 1 : day;
                DateTime timeOfThis = new DateTime(rYear, rMonth, rDay, hour, minute, second, DateTimeKind.Local);

                //Show warning if detect a zero date
                if(originalYear == 0 || originalMonth == 0 || originalDay == 0)
                {
                    Debug.LogWarning("Warning, a value of zero was found for Year, Month, or Day when converting this calendar to Unix time. The Unix time returned to you may have been corrected to 1/1/0001 to prevent errors in operations.");
                }

                //Convert DateTime to unix time stamp
                DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                long unixTimeStampInTicks = (timeOfThis.ToUniversalTime() - unixStart).Ticks;
                return unixTimeStampInTicks / TimeSpan.TicksPerMillisecond;
            }

            //Return the correspondent value of this Calendar, converted to DateTime
            public DateTime GetDateTime(TimeMode timeMode)
            {
                //Convert the content of this, to DateTime
                int originalYear = year;
                int originalMonth = month;
                int originalDay = day;
                int rYear = year == 0 ? year + 1 : year;
                int rMonth = month == 0 ? month + 1 : month;
                int rDay = day == 0 ? day + 1 : day;

                //Show warning if detect a zero date
                if (originalYear == 0 || originalMonth == 0 || originalDay == 0)
                {
                    Debug.LogWarning("Warning, a value of zero was found for Year, Month, or Day when converting this calendar to Unix time. The Unix time returned to you may have been corrected to 1/1/0001 to prevent errors in operations.");
                }

                if (timeMode == TimeMode.LocalTime)
                {
                    return new DateTime(rYear, rMonth, rDay, hour, minute, second, DateTimeKind.Local).ToLocalTime();
                }
                if (timeMode == TimeMode.UtcTime)
                {
                    return new DateTime(rYear, rMonth, rDay, hour, minute, second, DateTimeKind.Local).ToUniversalTime();
                }
                return DateTime.Now;
            }
            
            //Return the correspondent value of This Calendar, converted to TimeSpan
            public TimeSpan GetTimeSpan()
            {
                int totalDays = (year * 365) + (month * 30) + day;
                return new TimeSpan(totalDays, hour, minute, second);
            }

            //Return the correspondent value of Calendar Ticks of this Calendar
            public long GetCalendarTicks()
            {
                long cSharpTicks = GetTimeSpan().Ticks;
                return cSharpTicks / 1000000000;
            }

            //Serialize this class with the curent time, to string, to load later
            public string SerializeToString()
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("Calendar[");
                builder.Append(year.ToString());
                builder.Append(",");
                builder.Append(month.ToString());
                builder.Append(",");
                builder.Append(day.ToString());
                builder.Append(",");
                builder.Append(hour.ToString());
                builder.Append(",");
                builder.Append(minute.ToString());
                builder.Append(",");
                builder.Append(second.ToString());
                builder.Append("]");
                return builder.ToString();
            }

            //Deserialize the content of string, and load to this class
            public void DeserializeFromString(string stringFromSerialization)
            {
                //Error message
                string errorMessage = "Could not deserialize a Calendar from a string because the entered string is invalid.";

                //Verify if contains Calendar word
                if (stringFromSerialization.Contains("Calendar") == true)
                {
                    string stringWithoutCalendar = stringFromSerialization.Replace("Calendar", "");

                    //Verify if contains the []
                    if (stringWithoutCalendar.Contains("[") == true && stringWithoutCalendar.Contains("]") == true)
                    {
                        string stringWithoutKeys = stringWithoutCalendar.Replace("[", "").Replace("]", "");
                        string[] arrayOfValues = stringWithoutKeys.Split(',');

                        this.year = int.Parse(arrayOfValues[0]);
                        this.month = int.Parse(arrayOfValues[1]);
                        this.day = int.Parse(arrayOfValues[2]);
                        this.hour = int.Parse(arrayOfValues[3]);
                        this.minute = int.Parse(arrayOfValues[4]);
                        this.second = int.Parse(arrayOfValues[5]);
                    }
                    if (stringWithoutCalendar.Contains("[") == false || stringWithoutCalendar.Contains("]") == false)
                    {
                        Debug.LogError(errorMessage);
                    }
                }
                if (stringFromSerialization.Contains("Calendar") == false)
                {
                    Debug.LogError(errorMessage);
                }
            }

            //Increase the time of this calendar with desired value
            public Calendar IncreaseIn(int valueToIncrease, TimeSpanValue timeSpanValue)
            {
                //Create the TimeSpan
                TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, 0);
                switch (timeSpanValue)
                {
                    case TimeSpanValue.Days:
                        timeSpan = new TimeSpan(valueToIncrease, 0, 0, 0, 0);
                        break;
                    case TimeSpanValue.Hours:
                        timeSpan = new TimeSpan(0, valueToIncrease, 0, 0, 0);
                        break;
                    case TimeSpanValue.Minutes:
                        timeSpan = new TimeSpan(0, 0, valueToIncrease, 0, 0);
                        break;
                    case TimeSpanValue.Seconds:
                        timeSpan = new TimeSpan(0, 0, 0, valueToIncrease, 0);
                        break;
                }

                //Store the original value of date
                int originalYear = year;
                int originalMonth = month;
                int originalDay = day;

                //Increase the date in 1 where is zero
                year = year == 0 ? year + 1 : year;
                month = month == 0 ? month + 1 : month;
                day = day == 0 ? day + 1 : day;

                //Creat the DateTime of this
                DateTime dateTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
                DateTime dateTimeIncreased = dateTime.Add(timeSpan);

                //Store the result
                int rYear = dateTimeIncreased.Year;
                int rMonth = dateTimeIncreased.Month;
                int rDay = dateTimeIncreased.Day;
                int rHour = dateTimeIncreased.Hour;
                int rMinute = dateTimeIncreased.Minute;
                int rSecond = dateTimeIncreased.Second;

                //Decreaze the date in 1 where the default is zero
                rYear = originalYear == 0 ? rYear - 1 : rYear;
                rMonth = originalMonth == 0 ? rMonth - 1 : rMonth;
                rDay = originalDay == 0 ? rDay - 1 : rDay;

                //Stores the values
                this.year = rYear;
                this.month = rMonth;
                this.day = rDay;
                this.hour = rHour;
                this.minute = rMinute;
                this.second = rSecond;

                return this;
            }

            //Decrease the time of this calendar with desired value
            public Calendar DecreaseIn(int valueToDecrease, TimeSpanValue timeSpanValue)
            {
                //Create the TimeSpan
                TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, 0);
                switch (timeSpanValue)
                {
                    case TimeSpanValue.Days:
                        timeSpan = new TimeSpan(valueToDecrease, 0, 0, 0, 0);
                        break;
                    case TimeSpanValue.Hours:
                        timeSpan = new TimeSpan(0, valueToDecrease, 0, 0, 0);
                        break;
                    case TimeSpanValue.Minutes:
                        timeSpan = new TimeSpan(0, 0, valueToDecrease, 0, 0);
                        break;
                    case TimeSpanValue.Seconds:
                        timeSpan = new TimeSpan(0, 0, 0, valueToDecrease, 0);
                        break;
                }

                //Store the original value of date
                int originalYear = year;
                int originalMonth = month;
                int originalDay = day;

                //Increase the date in 1 where is zero
                year = year == 0 ? year + 1 : year;
                month = month == 0 ? month + 1 : month;
                day = day == 0 ? day + 1 : day;

                //Creat the DateTime of this
                DateTime thisDateTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);

                //Decrease the DateTime from timespan desired
                if (thisDateTime.Ticks > timeSpan.Ticks)
                {
                    DateTime dateTimeDecreased = thisDateTime.Subtract(timeSpan);

                    //Store the result
                    int rYear = dateTimeDecreased.Year;
                    int rMonth = dateTimeDecreased.Month;
                    int rDay = dateTimeDecreased.Day;
                    int rHour = dateTimeDecreased.Hour;
                    int rMinute = dateTimeDecreased.Minute;
                    int rSecond = dateTimeDecreased.Second;

                    //Decreaze the date in 1 where the default is zero
                    rYear = originalYear == 0 ? rYear - 1 : rYear;
                    rMonth = originalMonth == 0 ? rMonth - 1 : rMonth;
                    rDay = originalDay == 0 ? rDay - 1 : rDay;

                    //Stores the values
                    this.year = rYear;
                    this.month = rMonth;
                    this.day = rDay;
                    this.hour = rHour;
                    this.minute = rMinute;
                    this.second = rSecond;
                }
                if (thisDateTime.Ticks <= timeSpan.Ticks)
                {
                    //Stores the zero value
                    this.year = 0;
                    this.month = 0;
                    this.day = 0;
                    this.hour = 0;
                    this.minute = 0;
                    this.second = 0;
                }

                return this;
            }
            
            //Decrease the time of this calendar with a another Calendar datetime
            public Calendar DecreaseWithDate(Calendar dateTime)
            {
                //Validate the DateTime
                if (dateTime.Year == 0 || dateTime.Month == 0 || dateTime.Day == 0)
                {
                    Debug.LogError("It is not possible to decrease the time of a Calendar if the date to decrease is equal to 00/00/0000. Please enter a valid date equal to or greater than 01/01/0001.");
                    return this;
                }
                if (year == 0 || month == 0 || day == 0)
                {
                    Debug.LogError("You cannot decrease the Calendar date if the Calendar to be decreased has a date of 00/00/0000. Please enter a valid date greater than or equal to 01/01/0001.");
                    return this;
                }

                //Create datetime representation of this calendar and another
                DateTime thisDateTime = new DateTime(year, month, day, hour, minute, second);
                TimeSpan thisTimeSpan = new TimeSpan(thisDateTime.Ticks);
                DateTime dateTimeToSubtract = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
                TimeSpan timeSpanToSubtract = new TimeSpan(dateTimeToSubtract.Ticks);

                //If the datetime to subtract is less than this timespan
                if (timeSpanToSubtract.Ticks < thisTimeSpan.Ticks)
                {
                    TimeSpan resultTimeSpan = thisTimeSpan - timeSpanToSubtract;

                    int days = resultTimeSpan.Days;
                    int months = 0;
                    int years = 0;

                    //Calculate the number of monts, with the days
                    while (days >= 30)
                    {
                        days -= 30;
                        months += 1;
                    }

                    //Calculate the number of years, with the months
                    while (months >= 12)
                    {
                        months -= 12;
                        years += 1;
                    }

                    this.year = years;
                    this.month = months;
                    this.day = days;
                    this.hour = resultTimeSpan.Hours;
                    this.minute = resultTimeSpan.Minutes;
                    this.second = resultTimeSpan.Seconds;
                }
                //If the datetime to subtract is more than this timespan
                if (timeSpanToSubtract.Ticks >= thisTimeSpan.Ticks)
                {
                    this.year = 0;
                    this.month = 0;
                    this.day = 0;
                    this.hour = 0;
                    this.minute = 0;
                    this.second = 0;
                }

                return this;
            }

            //Verify if this time is equal a zero
            public bool isEqualsZero()
            {
                if (year == 0 && month == 0 && day == 0 && hour == 0 && minute == 0 && second == 0)
                {
                    return true;
                }
                if (year != 0 || month != 0 || day != 0 || hour != 0 || minute != 0 || second != 0)
                {
                    return false;
                }
                return false;
            }
        
            //Verify if this time is equal to another Calendar class
            public bool isEqualTo(Calendar calendarToCompare)
            {
                int cYear = calendarToCompare.Year;
                int cMonth = calendarToCompare.Month;
                int cDay = calendarToCompare.Day;
                int cHour = calendarToCompare.Hour;
                int cMinute = calendarToCompare.Minute;
                int cSecond = calendarToCompare.Second;

                if (year == cYear && month == cMonth && day == cDay && hour == cHour && minute == cMinute && second == cSecond)
                {
                    return true;
                }
                if (year != cYear || month != cMonth || day != cDay || hour != cHour || minute != cMinute || second != cSecond)
                {
                    return false;
                }

                return false;
            }

            //Verify if this time is not equal to another Calendar class
            public bool isNotEqualTo(Calendar calendarToCompare)
            {
                int cYear = calendarToCompare.Year;
                int cMonth = calendarToCompare.Month;
                int cDay = calendarToCompare.Day;
                int cHour = calendarToCompare.Hour;
                int cMinute = calendarToCompare.Minute;
                int cSecond = calendarToCompare.Second;

                if (year == cYear && month == cMonth && day == cDay && hour == cHour && minute == cMinute && second == cSecond)
                {
                    return false;
                }
                if (year != cYear || month != cMonth || day != cDay || hour != cHour || minute != cMinute || second != cSecond)
                {
                    return true;
                }

                return false;
            }
        
            //Verify if this time is greater than another Calendar class
            public bool isGreaterThan(Calendar calendarToCompare)
            {
                //Calculate this Calendar TimeSpan
                int totalDaysOfThisCalendar = (year * 365) + (month * 30) + day;
                TimeSpan thisCalendarSpan = new TimeSpan(totalDaysOfThisCalendar, hour, minute, second);

                //Calculate another Calendar TimeSpan
                int totalDaysOfOtherCalendar = (calendarToCompare.Year * 365) + (calendarToCompare.Month * 30) + calendarToCompare.Day;
                TimeSpan anotherCalendarSpan = new TimeSpan(totalDaysOfOtherCalendar, calendarToCompare.Hour, calendarToCompare.Minute, calendarToCompare.Second);

                //Return the result
                if (thisCalendarSpan.Ticks > anotherCalendarSpan.Ticks)
                {
                    return true;
                }
                if (thisCalendarSpan.Ticks < anotherCalendarSpan.Ticks)
                {
                    return false;
                }

                return false;
            }

            //Verify if this time is less than another Calendar class
            public bool isLessThan(Calendar calendarToCompare)
            {
                //Calculate this Calendar TimeSpan
                int totalDaysOfThisCalendar = (year * 365) + (month * 30) + day;
                TimeSpan thisCalendarSpan = new TimeSpan(totalDaysOfThisCalendar, hour, minute, second);

                //Calculate another Calendar TimeSpan
                int totalDaysOfOtherCalendar = (calendarToCompare.Year * 365) + (calendarToCompare.Month * 30) + calendarToCompare.Day;
                TimeSpan anotherCalendarSpan = new TimeSpan(totalDaysOfOtherCalendar, calendarToCompare.Hour, calendarToCompare.Minute, calendarToCompare.Second);

                //Return the result
                if (thisCalendarSpan.Ticks > anotherCalendarSpan.Ticks)
                {
                    return false;
                }
                if (thisCalendarSpan.Ticks < anotherCalendarSpan.Ticks)
                {
                    return true;
                }

                return false;
            }

            //Verify if this time is less than another Calendar class
            public bool isGreaterThanOrEqualTo(Calendar calendarToCompare)
            {
                //Calculate this Calendar TimeSpan
                int totalDaysOfThisCalendar = (year * 365) + (month * 30) + day;
                TimeSpan thisCalendarSpan = new TimeSpan(totalDaysOfThisCalendar, hour, minute, second);

                //Calculate another Calendar TimeSpan
                int totalDaysOfOtherCalendar = (calendarToCompare.Year * 365) + (calendarToCompare.Month * 30) + calendarToCompare.Day;
                TimeSpan anotherCalendarSpan = new TimeSpan(totalDaysOfOtherCalendar, calendarToCompare.Hour, calendarToCompare.Minute, calendarToCompare.Second);

                //Return the result
                if (thisCalendarSpan.Ticks >= anotherCalendarSpan.Ticks)
                {
                    return true;
                }
                if (thisCalendarSpan.Ticks < anotherCalendarSpan.Ticks)
                {
                    return false;
                }

                return false;
            }

            //Verify if this time is less than another Calendar class
            public bool isLessThanOrEqualTo(Calendar calendarToCompare)
            {
                //Calculate this Calendar TimeSpan
                int totalDaysOfThisCalendar = (year * 365) + (month * 30) + day;
                TimeSpan thisCalendarSpan = new TimeSpan(totalDaysOfThisCalendar, hour, minute, second);

                //Calculate another Calendar TimeSpan
                int totalDaysOfOtherCalendar = (calendarToCompare.Year * 365) + (calendarToCompare.Month * 30) + calendarToCompare.Day;
                TimeSpan anotherCalendarSpan = new TimeSpan(totalDaysOfOtherCalendar, calendarToCompare.Hour, calendarToCompare.Minute, calendarToCompare.Second);

                //Return the result
                if (thisCalendarSpan.Ticks > anotherCalendarSpan.Ticks)
                {
                    return false;
                }
                if (thisCalendarSpan.Ticks <= anotherCalendarSpan.Ticks)
                {
                    return true;
                }

                return false;
            }
        
            //Set the date time of this calendar, to zero
            public Calendar SetTimeToZero()
            {
                this.year = 0;
                this.month = 0;
                this.day = 0;
                this.hour = 0;
                this.minute = 0;
                this.second = 0;
                return this;
            }

            //Set the date time of this calendar, to current time of system
            public Calendar SetTimeToNow()
            {
                DateTime dateTime = DateTime.Now;

                this.year = dateTime.Year;
                this.month = dateTime.Month;
                this.day = dateTime.Day;
                this.hour = dateTime.Hour;
                this.minute = dateTime.Minute;
                this.second = dateTime.Second;
                return this;
            }
        
            //Get for Year value
            public int Year
            {
                get { return year; }
            }

            //Get for Month value
            public int Month
            {
                get { return month; }
            }

            //Get for Day value
            public int Day
            {
                get { return day; }
            }

            //Get for Hour value
            public int Hour
            {
                get { return hour; }
            }

            //Get for Minute value
            public int Minute
            {
                get { return minute; }
            }

            //Get for Second value
            public int Second
            {
                get { return second; }
            }

            //Get for Year String value
            public string YearStr
            {
                get { return year.ToString(); }
            }

            //Get for Month String value
            public string MonthStr
            {
                get { return (month < 10) ? ("0" + month) : month.ToString(); }
            }

            //Get for Day String value
            public string DayStr
            {
                get { return (day < 10) ? ("0" + day) : day.ToString(); }
            }

            //Get for Hour String value
            public string HourStr
            {
                get { return (hour < 10) ? ("0" + hour) : hour.ToString(); }
            }

            //Get for Minute String value
            public string MinuteStr
            {
                get { return (minute < 10) ? ("0" + minute) : minute.ToString(); }
            }

            //Get for Second String value
            public string SecondStr
            {
                get { return (second < 10) ? ("0" + second) : second.ToString(); }
            }
        }

        //A class that stores elapsed time information
        public class TimeElapsedInfo
        {
            public bool dateTimeWasChanged;
            public Calendar elapsedTime;

            public TimeElapsedInfo(bool dateTimeWasChanged, Calendar elapsedTime)
            {
                this.dateTimeWasChanged = dateTimeWasChanged;
                this.elapsedTime = elapsedTime;
            }
        }
    }

    namespace Files
    {
        //FileSize class
        public class FileSize
        {
            //Private variables
            private long bytes;

            //Instantiate this class
            public FileSize(long sizeInBytes)
            {
                this.bytes = sizeInBytes;
            }

            //Get bytes
            public long Bytes
            {
                get { return bytes; }
            }

            //Get bytes converted to kilobytes
            public float Kilobytes
            {
                get { return (float)((Double)bytes / (Double)1000); }
            }

            //Get bytes converted to megabytes
            public float Megabytes
            {
                get { return (float)((Double)bytes / (Double)1000000); }
            }

            //Get bytes converted to megabytes
            public float Mebibytes
            {
                get { return (float)((Double)bytes / (Double)1048576); }
            }

            //Get bytes converted to gigabytes
            public float Gigabytes
            {
                get { return (float)((Double)bytes / (Double)1000000000); }
            }
        }

        //FileRef class
        public class FileRef
        {
            /*
             A class that references a native Android file/folder and provides methods for retrieving information from it, and manage it
            */

            //Reference class to deserialize json data of java
            public class ReferenceResponse
            {
                public long size;
                public long lastModify;
                public bool isDirectory;
                public bool isFile;
                public bool isHidden;
                public bool isWritable;
                public string name;
                public string pathOfParentFolder;
            }

            //Class to represent a folder (This class is here to stay hidden from AutoComplete of visual studio, and return informations embed in FileRef)
            public class FolderRef
            {
                //Private variables
                private AndroidJavaObject fileJavaClass;
                private string path;
                private string name;

                //Instantiate this class
                public FolderRef(AndroidJavaClass fileJavaClass, string path, string name)
                {
                    this.fileJavaClass = fileJavaClass;
                    this.path = path;
                    this.name = name;
                }
                
                //Get a array of all files in this folder
                public FileRef[] GetAllFiles()
                {
                    //Return the list of reference for files in this folder
                    string[] filesInThisFolder = fileJavaClass.CallStatic<string[]>("GetAllFiles", path);
                    List<FileRef> fileList = new List<FileRef>();
                    foreach (string filePath in filesInThisFolder)
                    {
                        fileList.Add(new FileRef(filePath));
                    }
                    return fileList.ToArray();
                }

                //Get for Path
                public string Path
                {
                    get { return path; }
                }

                //Get for path of this Folder
                public string Name
                {
                    get { return name; }
                }
            }

            //Class to represent a file (This class is here to stay hidden from AutoComplete of visual studio, and return informations embed in FileRef)
            public class ArchiveRef
            {
                //Private variables
                private AndroidJavaObject activityJavaClass;
                private AndroidJavaObject fileJavaClass;
                private FileRef thisfileRef;
                private FileSize size;
                private string pureName;
                private string extension;

                //Instantiate this class
                public ArchiveRef(AndroidJavaObject activityJavaClass, AndroidJavaObject fileJavaClass, FileRef thisfileRef, long size)
                {
                    this.activityJavaClass = activityJavaClass;
                    this.fileJavaClass = fileJavaClass;
                    this.thisfileRef = thisfileRef;
                    this.size = new FileSize(size);
                    this.pureName = System.IO.Path.GetFileNameWithoutExtension(thisfileRef.path);
                    this.extension = System.IO.Path.GetExtension(thisfileRef.path).Replace(".", "");
                }

                //Copy this file to a new path, and update all this file info
                public void CopyTo(string pathOfTargetDirectory)
                {
                    if (thisfileRef.isFile == false)
                    {
                        return;
                    }

                    string newPathOfFile = pathOfTargetDirectory + "/" + pureName + "." + extension;
                    fileJavaClass.CallStatic("CopyTo", thisfileRef.path, newPathOfFile);
                    thisfileRef.UpdateReference();
                }

                //Move this file to a new path, and update all this file info
                public void MoveTo(string pathOfTargetDirectory)
                {
                    if (thisfileRef.isFile == false)
                    {
                        return;
                    }

                    string newPathOfFile = pathOfTargetDirectory + "/" + pureName + "." + extension;
                    fileJavaClass.CallStatic("MoveTo", thisfileRef.path, newPathOfFile);
                    thisfileRef.UpdateReference();
                }

                //Rename this file to a new name, and update all this file info
                public void Rename(string newName)
                {
                    if (thisfileRef.isFile == false)
                    {
                        return;
                    }

                    fileJavaClass.CallStatic("Rename", thisfileRef.path, thisfileRef.ThisFolder.Path + "/" + newName);
                    thisfileRef.UpdateReference();
                }

                //Open this file with default app of system
                public void Open()
                {
                    if (thisfileRef.isFile == false)
                    {
                        return;
                    }

                    fileJavaClass.CallStatic("Open", activityJavaClass, thisfileRef.path);
                }

                //Set ReadOnly for file, and update all this file info
                public void SetReadOnly(bool enabled)
                {
                    if (thisfileRef.isFile == false)
                    {
                        return;
                    }

                    fileJavaClass.CallStatic("SetReadOnly", thisfileRef.path, enabled);
                    thisfileRef.UpdateReference();
                }

                //Get all bytes for file
                public byte[] GetAllBytes()
                {
                    if (thisfileRef.isFile == false)
                    {
                        return new byte[] { };
                    }

                    return fileJavaClass.CallStatic<byte[]>("GetAllBytes", thisfileRef.path);
                }

                //Get texture from file
                public Texture2D GetAsTexture2D()
                {
                    //Cancel if is not a file
                    if (thisfileRef.isFile == false)
                    {
                        return null;
                    }

                    Texture2D texture = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                    texture.LoadImage(GetAllBytes(), false);
                    texture.Apply();
                    return texture;
                }

                //Get text from file
                public string GetAsTextString()
                {
                    //Cancel if is not a file
                    if (thisfileRef.isFile == false)
                    {
                        return null;
                    }

                    return Encoding.UTF8.GetString(GetAllBytes());
                }

                //Get for Path
                public string Path
                {
                    get { return thisfileRef.path; }
                }

                //Get for FileSize
                public FileSize Size
                {
                    get { return size; }
                }

                //Get for PureName
                public string PureName
                {
                    get { return pureName; }
                }

                //Get for Extension
                public string Extension
                {
                    get { return extension; }
                }
            }

            //Base private variables
            private AndroidJavaClass fileJavaClass;
            private AndroidJavaObject activityJavaClass;
            private string path;
            private bool exists;

            //Variables if file/folder exists
            private bool isDirectory;
            private bool isFile;
            private bool isHidden;
            private bool isWritable;
            private Calendar lastModify;
            private FolderRef folderRef;
            private ArchiveRef fileRef;

            //Instantiate this class with the file/folder path
            public FileRef(string pathToFileOrFolder)
            {
                //Create base data of file
                this.fileJavaClass = new AndroidJavaClass("xyz.windsoft.mtassets.nat.Files");
                this.activityJavaClass = NativeAndroid.CurrentUnityActivity;
                this.path = pathToFileOrFolder;
                this.exists = fileJavaClass.CallStatic<bool>("Exists", path);

                //Get all data of this reference of this file
                UpdateReference();
            }

            //Update all file/folder info
            public void UpdateReference()
            {
                //If file/folder not extistis, clear all data of this
                if (exists == false)
                {
                    //Clear all data of this file/folder reference
                    this.isDirectory = false;
                    this.isFile = false;
                    this.isHidden = false;
                    this.isWritable = false;
                    this.lastModify = null;
                    this.folderRef = null;
                    this.fileRef = null;
                }
                //If file/folder exists, get all data of this
                if (exists == true)
                {
                    //Get all file/folder data in a json string, and deserialize
                    string jsonReferenceResponse = fileJavaClass.CallStatic<string>("UpdateReference", path);
                    ReferenceResponse reference = JsonUtility.FromJson<ReferenceResponse>(jsonReferenceResponse);

                    //Store the data of this file/folder
                    this.isDirectory = reference.isDirectory;
                    this.isFile = reference.isFile;
                    this.isHidden = reference.isHidden;
                    this.isWritable = reference.isWritable;
                    this.lastModify = new Calendar(reference.lastModify);

                    //If this is a directory
                    if (isDirectory == true)
                    {
                        //Get this file information
                        fileRef = null;

                        //Get this directory information
                        folderRef = new FolderRef(fileJavaClass, path, reference.name);
                    }

                    //If this is a file
                    if (isFile == true)
                    {
                        //Get this file information
                        fileRef = new ArchiveRef(activityJavaClass, fileJavaClass, this, reference.size);

                        //Get this directory information
                        folderRef = new FolderRef(fileJavaClass, reference.pathOfParentFolder, new FileRef(reference.pathOfParentFolder).ThisFolder.Name);
                    }
                }
            }

            //Delete the file/folder, and update all this file info
            public void Delete()
            {
                if (exists == false)
                {
                    return;
                }

                fileJavaClass.CallStatic("Delete", path);
                UpdateReference();
            }

            //Return the parent folder of this FileRef
            public FileRef GetParentFolder()
            {
                if (exists == false)
                {
                    return null;
                }

                return new FileRef(fileJavaClass.CallStatic<string>("GetParentFolder", path));
            }

            //Get for Exists
            public bool Exists
            {
                get { return exists; }
            }

            //Get for IsDirectory
            public bool IsDirectory
            {
                get { return isDirectory; }
            }

            //Get for IsFile
            public bool IsFile
            {
                get { return isFile; }
            }

            //Get for IsHidden
            public bool IsHidden
            {
                get { return isHidden; }
            }

            //Get for IsHidden
            public bool IsWritable
            {
                get { return isWritable; }
            }

            //Get for LastModify
            public Calendar LastModify
            {
                get { return lastModify; }
            }

            //Get for File
            public ArchiveRef ThisFile
            {
                get { return fileRef; }
            }

            //Get for Folder
            public FolderRef ThisFolder
            {
                get { return folderRef; }
            }
        }
    }
    #endregion
}