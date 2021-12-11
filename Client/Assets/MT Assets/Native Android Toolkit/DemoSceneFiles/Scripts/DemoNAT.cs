using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MTAssets.NativeAndroidToolkit;
using MTAssets.NativeAndroidToolkit.Time;
using MTAssets.NativeAndroidToolkit.Notify;
using MTAssets.NativeAndroidToolkit.Files;
using MTAssets.NativeAndroidToolkit.Events;
using MTAssets.NativeAndroidToolkit.Webview;
using System.Text;

public class DemoNAT : MonoBehaviour
{

    public GameObject texture2DShow;
    public Image texture2Dimage;
    public GameObject notification;
    public Text notificationText;
    public Texture2D textureToShare;

    private void Start()
    {
        //Set max fps
        Application.targetFrameRate = 60;

        //Register the callback for OnResumeAfterPause
        Time_RegisterOnResumeAfterPauseEvent();
    }

    IEnumerator DemoAnimation(string text)
    {
        notification.SetActive(false);
        notificationText.text = text;
        notification.SetActive(true);
        yield return null;
    }

    //Button for close texture2d shower
    public void CloseTextShower()
    {
        texture2DShow.SetActive(false);
    }

    //------------------------ Dialogs -----------------------------

    //Demo for Simple Alert Dialog
    public void Dialog_SimpleAlertDialog()
    {
        NativeAndroid.Dialogs.ShowSimpleAlertDialog("Title", "This is a simple alert dialog.");

        DialogsEvents.onSimpleAlertDialogOk += () =>
        {
            StartCoroutine(DemoAnimation("You hit the \"Ok\" button!"));
        };

        DialogsEvents.onSimpleAlertDialogCancel += () =>
        {
            StartCoroutine(DemoAnimation("You have canceled the simple alert dialog."));
        };
    }

    //Demo for Confirmation Alert Dialog
    public void Dialog_ConfirmationDialog()
    {
        NativeAndroid.Dialogs.ShowConfirmationDialog("Title", "This is a confirmation dialog. I'm cool?", "Yes!", "No :(");

        DialogsEvents.onConfirmationDialogYes += () =>
        {
            StartCoroutine(DemoAnimation("You hit the \"Yes\" button!"));
        };

        DialogsEvents.onConfirmationDialogNo += () =>
        {
            StartCoroutine(DemoAnimation("You hit the \"No\" button!"));
        };

        DialogsEvents.onConfirmationDialogCancel += () =>
        {
            StartCoroutine(DemoAnimation("You have canceled the confirmation dialog."));
        };
    }

    //Demo for Radial List Dialog
    public void Dialog_RadialListDialog()
    {
        NativeAndroid.Dialogs.ShowRadialListDialog("Title", "Done", new string[] { "Good!", "Awesome", "Cool" });

        DialogsEvents.onRadialListDialogDone += (int choosed) =>
        {
            StartCoroutine(DemoAnimation("Option \"" + choosed.ToString() + "\" selected!"));
        };

        DialogsEvents.onRadialListDialogCancel += () =>
        {
            StartCoroutine(DemoAnimation("You have canceled the radial list dialog."));
        };
    }

    //Demo for checkbox List Dialog
    public void Dialog_CheckboxListDialog()
    {
        NativeAndroid.Dialogs.ShowCheckboxListDialog("Title", "Done", new string[] { "Option 1", "Option 2", "Option 3" }, new bool[] { false, false, false });

        DialogsEvents.onCheckboxListDialogDone += (bool[] checks) =>
        {
            string str = checks[0].ToString();
            for (int i = 0; i < checks.Length; i++)
            {
                if (i == 0)
                {
                    continue;
                }
                str += "," + checks[i].ToString();
            }
            StartCoroutine(DemoAnimation("Options \"" + str + "\" selected!"));
        };

        DialogsEvents.onCheckboxListDialogCancel += () =>
        {
            StartCoroutine(DemoAnimation("You have canceled the checkbox list dialog."));
        };
    }

    //Demo for Neutral Dialog
    public void Dialog_NeutralDialog()
    {
        NativeAndroid.Dialogs.ShowNeutralDialog("Title", "This is a dialog with neutral action. I'm cool?", "Yes", "No", "Never");

        DialogsEvents.onNeutralYes += () =>
        {
            StartCoroutine(DemoAnimation("You hit the \"Yes\" button!"));
        };

        DialogsEvents.onNeutralNo += () =>
        {
            StartCoroutine(DemoAnimation("You hit the \"No\" button!"));
        };

        DialogsEvents.onNeutralNeutral += () =>
        {
            StartCoroutine(DemoAnimation("You hit the \"Never\" button!"));
        };

        DialogsEvents.onNeutralCancel += () =>
        {
            StartCoroutine(DemoAnimation("You have canceled the neutral dialog."));
        };
    }

    //-------------------- Notifications ---------------------------

    //Demo for Send Notification
    public void Notification_SendPushNotification()
    {
        StartCoroutine(DemoAnimation("The Push Notification was delivered."));

        NativeAndroid.Notifications.SendPushNotification("Notification title!", "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.");
    }

    //Demo for Show Toast
    public void Notification_ShowToast()
    {
        NativeAndroid.Notifications.ShowToast("This is a toast!", false, ToastPosition.Bottom);
    }

    //Demo for Schedule Inactivity Notification
    public void Notification_PrepareInactivityNotification()
    {
        NativeAndroid.Notifications.PrepareInactivityNotification("Hey!", "This is a Inactivity Notification of NAT.");

        StartCoroutine(DemoAnimation("The inactivity notification was successfully prepared. Get on your watch in 3 days to see it!"));
    }

    //Demo for Is Inactivity Notification Scheduled
    public void Notification_isInactivityNotificationPrepared()
    {
        StartCoroutine(DemoAnimation("Is inactivity notification prepared?\n" + NativeAndroid.Notifications.isInactivityNotificationPrepared().ToString()));
    }

    //Demo for Is Cancel Inactivity Notification
    public void Notification_CancelInactivityNotification()
    {
        NativeAndroid.Notifications.CancelInactivityNotification();

        StartCoroutine(DemoAnimation("Inactivity notification (if exists) has been canceled."));
    }

    //Demo for Schedule Notification
    public void Notification_ScheduleNotification(int channel)
    {
        StartCoroutine(DemoAnimation("A new Notification in channel " + channel.ToString() + " was scheduled, to 3 minutes in future."));

        new NativeAndroid.Notifications.ScheduledNotification(ChannelTools.IntToCh(channel),
            "Notification of channel " + channel.ToString(),
            "This is a scheduled notification of channel " + channel.ToString() + "!")
            .setMinutesInFuture(3)
            .ScheduleThisNotification();
    }

    //Demo for is Channel Scheduled
    public void Notification_isChannelScheduled(int channel)
    {
        StartCoroutine(DemoAnimation("The channel " + channel + " has a Notification scheduled?\n" + NativeAndroid.Notifications.isNotificationScheduledInChannel(ChannelTools.IntToCh(channel)).ToString()));
    }

    //Demo for Cancel Channel
    public void Notification_CancelChannel(int channel)
    {
        NativeAndroid.Notifications.CancelScheduledNotification(ChannelTools.IntToCh(channel));
        StartCoroutine(DemoAnimation("The Notification of channel " + channel.ToString() + " (if exists) has been canceled."));
    }

    //-------------------- Sharing ---------------------------

    //Demo for Share Texture2D
    public void Sharing_ShareTexture2D()
    {
        NativeAndroid.Sharing.ShareTexture2D(textureToShare);
    }

    //Demo for Share Text
    public void Sharing_SharePlainText()
    {
        NativeAndroid.Sharing.SharePlainText("Share Window", "Here is a text of Native Android Toolkit!");
    }

    //Demo for Take Screenshot And Save In Gallery
    public void Sharing_TakeScreenshotAndSaveInGallery()
    {
        string path = NativeAndroid.Sharing.TakeScreenshotAndSaveOnGallery();

        StartCoroutine(DemoAnimation("The screenshot was captured, and saved on the path\n" + path));
    }

    //Demo for Take Screenshot And Get Texture2D
    public void Sharing_TakeScreenshotAndGetTexture2D()
    {
        StartCoroutine(DemoAnimation("Screenshot captured, processing..."));

        NativeAndroid.Sharing.TakeScreenshotAndGetTexture2D();

        //Register callback to show texture 2D of screenshot, after processing complete
        SharingEvents.onCompleteTexture2Dprocessing += (Texture2D texture) =>
        {
            texture2DShow.SetActive(true);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, Screen.width, Screen.height), new Vector2(0.5f, 0.5f));
            texture2Dimage.sprite = sprite;
        };
    }

    //Demo for Copy To Clipboard
    public void Sharing_CopyToClipboard()
    {
        StartCoroutine(DemoAnimation("Something was copied to your Clipboard!"));

        NativeAndroid.Sharing.CopyTextToClipboard("Some text from Native Android Toolkit.");
    }

    //Demo for Get From Clipboard
    public void Sharing_GetFromClipboard()
    {
        StartCoroutine(DemoAnimation("Current text from your Clipboard\n" + NativeAndroid.Sharing.GetTextFromClipboard()));
    }

    //Open A App In Play Store
    public void Sharing_OpenAppInPlayStore()
    {
        NativeAndroid.Sharing.OpenInPlayStore("com.unity3d.genericremote");
    }

    //-------------------- Utils ---------------------------

    //Demo for Is Vibration Is Available
    public void Utils_IsVibrationAvailable()
    {
        StartCoroutine(DemoAnimation("Is vibration available?\n" + NativeAndroid.Utils.isVibrationAvailable().ToString()));
    }

    //Demo for Verify if bluetooth is enabled
    public void Utils_isBluetoothEnabled()
    {
        StartCoroutine(DemoAnimation("Is bluetooth enabled?\n" + NativeAndroid.Utils.isBluetoothEnabled().ToString()));
    }

    //Demo for Verify if is using headset
    public void Utils_isUsingHeadset()
    {
        StartCoroutine(DemoAnimation("Is using headset now?\n" + NativeAndroid.Utils.isUsingHeadset().ToString()));
    }

    //Demo for Verify if is using wireless headset
    public void Utils_isUsingWirelessHeadset()
    {
        StartCoroutine(DemoAnimation("Is using wireless headset now?\n" + NativeAndroid.Utils.isUsingWirelessHeadset().ToString()));
    }

    //Demo for Verify if internet is available
    public void Utils_isInternetConnectivityAvailable()
    {
        StartCoroutine(DemoAnimation("Is internet connectivity available?\n" + NativeAndroid.Utils.isInternetConnectivityAvailable().ToString()));
    }

    //Demo for Verify if is connected to wifi
    public void Utils_isConnectedToWifi()
    {
        StartCoroutine(DemoAnimation("Is connected to a wifi?\n" + NativeAndroid.Utils.isConnectedToWifi().ToString()));
    }

    //Demo for Verify permisson of File
    public void Utils_isFilePermissionGuaranteed()
    {
        StartCoroutine(DemoAnimation("Is file permission given?\n" + NativeAndroid.Utils.isFilePermissionGuaranteed().ToString()));
    }

    //Demo for Verify permisson of Camera
    public void Utils_isCameraPermissionGuaranteed()
    {
        StartCoroutine(DemoAnimation("Is camera permission given?\n" + NativeAndroid.Utils.isCameraPermissionGuaranteed().ToString()));
    }

    //Demo for Verify if FlashLight available
    public void Utils_IsFlashLightAvailable()
    {
        StartCoroutine(DemoAnimation("Is flashlight available?\n" + NativeAndroid.Utils.isFlashLightAvailable().ToString()));
    }

    //Demo for Vibrate
    public void Utils_Vibrate()
    {
        StartCoroutine(DemoAnimation("Vibrating..."));

        NativeAndroid.Utils.Vibrate(500);
    }

    //Demo for Vibrate With Pattern
    public void Utils_VibrateWithPattern()
    {
        StartCoroutine(DemoAnimation("Vibrating with pattern..."));

        NativeAndroid.Utils.VibrateWithPattern(new long[] { 1000, 250, 1000, 250 });
    }

    //Demo for Verify if FlashLight available
    public void Utils_SetFlashLightEnabled(bool enabled)
    {
        if (enabled == true)
        {
            StartCoroutine(DemoAnimation("Flashlight enabled! (If available)"));
        }
        if (enabled == false)
        {
            StartCoroutine(DemoAnimation("Flashlight disabled! (If available)"));
        }

        NativeAndroid.Utils.SetFlashLightEnabled(enabled);
    }

    //Demo for Request File Permissions
    public void Utils_RequestFilePermissions()
    {
        NativeAndroid.Utils.RequestFileMangementPermission();
    }

    //Demo for Request Camera Permissions
    public void Utils_RequestCameraPermissions()
    {
        NativeAndroid.Utils.RequestCameraPermission();
    }

    //Demo for Get Version Code
    public void Utils_GetVersionCode()
    {
        StartCoroutine(DemoAnimation("The Android version code is " + NativeAndroid.Utils.GetAndroidVersionCode().ToString()));
    }

    //Demo for Restart The Game
    public void Utils_RestartGame()
    {
        NativeAndroid.Utils.RestartUnityPlayer();
    }

    //Demo for Open Settings
    public void Utils_OpenSystemSettings()
    {
        NativeAndroid.Utils.OpenSettings();
    }

    //Demo for Open Wifi Settings
    public void Utils_OpenWifiSettings()
    {
        NativeAndroid.Utils.OpenWifiSettings();
    }

    //Demo for Open Bluetooth Settings
    public void Utils_OpenBluetoothSettings()
    {
        NativeAndroid.Utils.OpenBluetoothSettings();
    }

    //Demo for Open This App Settings
    public void Utils_OpenThisAppSettings()
    {
        NativeAndroid.Utils.OpenThisAppSettings();
    }

    //Demo for show Unique Android ID
    public void Utils_GetUniqueAndroidID()
    {
        StartCoroutine(DemoAnimation("Unique Android ID is\n" + NativeAndroid.Utils.GetThisDeviceUniqueID()));
    }

    //---------------------- Apps ----------------------------

    //Demo for Verify if app is installed
    public void Apps_isAppInstalled()
    {
        StartCoroutine(DemoAnimation("Is Gmail installed?\n" + NativeAndroid.Apps.isAppInstalled("com.google.android.gm").ToString()));
    }

    //Demo for Open App By Package Name
    public void Apps_OpenAppByPackageName()
    {
        NativeAndroid.Apps.OpenAppByPackage("com.google.android.gm");
    }

    //-------------------- Time ---------------------------

    //Demo for Hour Picker
    public void Time_OpenHourPicker()
    {
        NativeAndroid.Time.OpenHourPicker("Select time!");

        TimeEvents.onHourPicked += (Calendar time) =>
        {
            StartCoroutine(DemoAnimation("Hour picked is\n" + time.Hour + ":" + time.Minute));
        };
    }

    //Demo for Date Picker
    public void Time_OpenDatePicker()
    {
        NativeAndroid.Time.OpenDatePicker("Select date!");

        TimeEvents.onDatePicked += (Calendar date) =>
        {
            StartCoroutine(DemoAnimation("Date picked is\n" + date.Year + "/" + date.Month + "/" + date.Day));
        };
    }

    //Demo for Date Picker
    public void Time_GetCurrentTimeNtp()
    {
        NativeAndroid.Time.LoadCurrentTimeOfNtp();

        TimeEvents.onDoneNtpRequest += (bool sucess, Calendar time) =>
        {
            StartCoroutine(DemoAnimation("Sucess on load? " + sucess.ToString() + "\nTime: " + time.Year + "/" + time.Month + "/" + time.Day + " " + time.Hour + ":" + time.Minute + ":" + time.Second));
        };
    }

    //Demo for Get Elapsed Realtime Since Boot
    public void Time_GetElapsedRealtimeSinceBoot()
    {
        Calendar time = NativeAndroid.Time.GetElapsedRealtimeSinceBoot();

        StartCoroutine(DemoAnimation("Time elapsed since system boot\n" + time.Year + "y, " + time.Month + "m, " + time.Day + "d, " + time.Hour + "h, " + time.Minute + "m, " + time.Second + "s."));
    }

    //Demo for Get Time Of Last Closing
    public void Time_GetTimeOfLastClosing()
    {
        Calendar time = NativeAndroid.Time.GetDateTimeOfLastClosing();

        StartCoroutine(DemoAnimation("Time of last close of this app\n" + time.Year + "/" + time.Month + "/" + time.Day + " " + time.Hour + ":" + time.Minute + ":" + time.Second));
    }

    //Demo for Get Time Elapsed Since Last Close
    public void Time_GetTimeElapsedSinceLastClosing()
    {
        TimeElapsedInfo timeInfo = NativeAndroid.Time.GetElapsedTimeLastCloseUntilThisRun();

        StartCoroutine(DemoAnimation("Date changed while the app was closed? " + timeInfo.dateTimeWasChanged + "\n" +
            "Elapsed time since last close: " + timeInfo.elapsedTime.Year + "/" + timeInfo.elapsedTime.Month + "/" + timeInfo.elapsedTime.Day + " " + timeInfo.elapsedTime.Hour + ":" + timeInfo.elapsedTime.Minute + ":" + timeInfo.elapsedTime.Second));
    }

    //Demo for Register OnResumeAfterPause Event
    public void Time_RegisterOnResumeAfterPauseEvent()
    {
        StartCoroutine(DemoAnimation("The event \"OnResumeAfterPause\" has been logged. Minimize the app, wait a few moments and restore it again to see how much time has passed while it has been paused."));

        TimeEvents.onResumeAppAfterPause += (TimeElapsedInfo timeElapsedInfo) =>
        {
            StartCoroutine(DemoAnimation("Date changed while the app was paused? " + timeElapsedInfo.dateTimeWasChanged + "\n" +
            "Elapsed Time while paused (minimized)\n" + timeElapsedInfo.elapsedTime.Year + "/" + timeElapsedInfo.elapsedTime.Month + "/" + timeElapsedInfo.elapsedTime.Day + " " + timeElapsedInfo.elapsedTime.Hour + ":" + timeElapsedInfo.elapsedTime.Minute + ":" + timeElapsedInfo.elapsedTime.Second));
        };
    }

    //-------------------- Webview ---------------------------

    //Demo for Open Webview
    public void Webview_OpenWebview()
    {
        NativeAndroid.Webview.OpenWebview("Demo of NAT Webview", "https://google.com/", true, true);

        WebviewEvents.onWebviewClose += () =>
        {
            StartCoroutine(DemoAnimation("Webview closed!"));
        };
    }

    //Demo for Open Webview Fullscreen
    public void Webview_OpenFullscreenWebview()
    {
        NativeAndroid.Webview.OpenFullScreenWebview("Demo of NAT Webview Fullscreen", "https://google.com/", true, true, WebviewOrientation.Landscape);
    }

    //-------------------- Files ---------------------------

    //Demo for is internal memory available
    public void Files_isInternalMemoryAvailable()
    {
        StartCoroutine(DemoAnimation("The internal memory of this device, is available?\n" + NativeAndroid.File.isInternalMemoryAvailable()));
    }

    //Demo for Get Internal Memory Path
    public void Files_GetInternalMemoryPath()
    {
        StartCoroutine(DemoAnimation("The root path of internal memory is\n" + NativeAndroid.File.GetInternalMemoryPath()));
    }

    //Demo for Get Internal Memory Space
    public void Files_GetTotalDeviceMemory()
    {
        StartCoroutine(DemoAnimation("This device have a memory space of\n" + NativeAndroid.File.GetTotalDeviceMemory().Gigabytes.ToString("F1") + " GB"));
    }

    //Demo for Get Internal Memory Path
    public void Files_GetTotalFreeDeviceMemory()
    {
        StartCoroutine(DemoAnimation("This device have a free memory space of\n" + NativeAndroid.File.GetTotalFreeMemory().Gigabytes.ToString("F1") + " GB"));
    }

    //Demo for Create Directory
    public void Files_CreateDiretory()
    {
        NativeAndroid.File.CreateDirectory(Application.persistentDataPath + "/NAT/Test");
        NativeAndroid.File.CreateFile(Application.persistentDataPath + "/NAT/Test/bin1.bin", new byte[] { });
        NativeAndroid.File.CreateFile(Application.persistentDataPath + "/NAT/Test/bin2.bin", new byte[] { });

        StartCoroutine(DemoAnimation("The directory \"/Test\" was created in\n" + Application.persistentDataPath + "/NAT"));
    }

    //Demo for create file
    public void Files_CreateFile()
    {
        NativeAndroid.File.CreateFile(Application.persistentDataPath + "/NAT/file.bin", new byte[] { });

        StartCoroutine(DemoAnimation("The file \"file.bin\" was created in\n" + Application.persistentDataPath + "/NAT"));
    }

    //Demo for create text file
    public void Files_CreateTextFile()
    {
        NativeAndroid.File.CreateTextFile(Application.persistentDataPath + "/NAT/text.txt", "This is a text file of Native Android Toolkit! This is all!");

        StartCoroutine(DemoAnimation("The file \"text.txt\" was created in\n" + Application.persistentDataPath + "/NAT"));
    }

    //Demo for create image file
    public void Files_CreateImageFile()
    {
        NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);

        StartCoroutine(DemoAnimation("The file \"img.png\" was created in\n" + Application.persistentDataPath + "/NAT"));
    }

    //------------- FileRef Class (With FileRef class) -----------

    //Demo for FileRef updateReference
    public void FileRef_UpdateReference()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            StartCoroutine(DemoAnimation("The FileRef object reference of \"img.png\" was updated."));
        }
    }

    //Demo for FileRef deleteFile
    public void FileRef_DeleteFile()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            img.Delete();

            StartCoroutine(DemoAnimation("The file \"img.png\" was deleted and not exists more."));
        }
    }

    //Demo for FileRef deleteDir
    public void FileRef_DeleteDir()
    {
        FileRef dir = new FileRef(Application.persistentDataPath + "/NAT/Test");
        if (dir.Exists == false)
        {
            StartCoroutine(DemoAnimation("The directory \"/Test\" was not found. A new Example Directory \"/Test\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateDirectory(Application.persistentDataPath + "/NAT/Test");
            NativeAndroid.File.CreateFile(Application.persistentDataPath + "/NAT/Test/bin1.bin", new byte[] { });
            NativeAndroid.File.CreateFile(Application.persistentDataPath + "/NAT/Test/bin2.bin", new byte[] { });
        }
        if (dir.Exists == true)
        {
            dir.Delete();

            StartCoroutine(DemoAnimation("The diretory \"PersistentDataPath/NAT/Test\" was deleted and all sub-folders and files inside, are deleted too."));
        }
    }

    //Demo for FileRef getParent
    public void FileRef_GetParent()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            StartCoroutine(DemoAnimation("The path of parent folder of \"img.png\" is\n" + img.GetParentFolder().ThisFolder.Path));
        }
    }

    //Demo for FileRef exists
    public void FileRef_Exists()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" exists?\n" + img.Exists));
        }
    }

    //Demo for FileRef isDirectory
    public void FileRef_isDirectory()
    {
        FileRef dir = new FileRef(Application.persistentDataPath + "/NAT/Test");
        if (dir.Exists == false)
        {
            StartCoroutine(DemoAnimation("The directory \"/Test\" was not found. A new Example Directory \"/Test\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateDirectory(Application.persistentDataPath + "/NAT/Test");
            NativeAndroid.File.CreateFile(Application.persistentDataPath + "/NAT/Test/bin1.bin", new byte[] { });
            NativeAndroid.File.CreateFile(Application.persistentDataPath + "/NAT/Test/bin2.bin", new byte[] { });
        }
        if (dir.Exists == true)
        {
            StartCoroutine(DemoAnimation("Is \"/Test\" a directory?\n" + dir.IsDirectory));
        }
    }

    //Demo for FileRef isFile
    public void FileRef_isFile()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            StartCoroutine(DemoAnimation("Is \"img.png\" a file?\n" + img.IsFile));
        }
    }

    //Demo for FileRef isFile
    public void FileRef_isHidden()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            StartCoroutine(DemoAnimation("Is \"img.png\" hidden?\n" + img.IsHidden));
        }
    }

    //Demo for FileRef isFile
    public void FileRef_isWritable()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            StartCoroutine(DemoAnimation("Is \"img.png\" writable?\n" + img.IsWritable));
        }
    }

    //Demo for FileRef lastModify
    public void FileRef_lastModify()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            StartCoroutine(DemoAnimation("File \"img.png\" was modified last time in\n" + img.LastModify.Year + "/" + img.LastModify.Month + "/" + img.LastModify.Day + " " + img.LastModify.Hour + ":" + img.LastModify.Minute + ":" + img.LastModify.Second));
        }
    }

    //---------- FileRef Class (With ArchiveRef class) -----------

    //Demo for FileRef copyTo
    public void FileRef_Archive_CopyTo()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            img.ThisFile.CopyTo(NativeAndroid.File.GetInternalMemoryPath());

            StartCoroutine(DemoAnimation("The \"img.png\" was copied to root folder of your device memory."));
        }
    }

    //Demo for FileRef copyTo
    public void FileRef_Archive_MoveTo()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            img.ThisFile.MoveTo(NativeAndroid.File.GetInternalMemoryPath());

            StartCoroutine(DemoAnimation("The \"img.png\" was moved to root folder of your device memory. This file not exists more in old path."));
        }
    }

    //Demo for FileRef rename
    public void FileRef_Archive_Rename()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            img.ThisFile.Rename("rename.png");

            StartCoroutine(DemoAnimation("The \"img.png\" file was renamed to \"rename.png\"."));
        }
    }

    //Demo for FileRef setReadOnly
    public void FileRef_Archive_ReadOnly()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            img.ThisFile.SetReadOnly(false);

            StartCoroutine(DemoAnimation("The \"img.png\" was changed to ReadOnly false."));
        }
    }

    //Demo for FileRef open
    public void FileRef_Archive_Open()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            img.ThisFile.Open();
        }
    }

    //Demo for FileRef getAllBytes
    public void FileRef_Archive_GetAllBytes()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            StartCoroutine(DemoAnimation("The bytes of \"img.png\" was loaded. The lenght of byte array is\n" + img.ThisFile.GetAllBytes().Length));
        }
    }

    //Demo for FileRef getTexture
    public void FileRef_Archive_GetTexture()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            Texture2D texture = img.ThisFile.GetAsTexture2D();
            texture2DShow.SetActive(true);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            texture2Dimage.sprite = sprite;

            StartCoroutine(DemoAnimation("The Texture2D was loaded from \"img.png\". Showing the image..."));
        }
    }

    //Demo for FileRef getString
    public void FileRef_Archive_GetString()
    {
        FileRef txt = new FileRef(Application.persistentDataPath + "/NAT/text.txt");
        if (txt.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"text.txt\" was not found. A new Example Text File \"text.txt\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateTextFile(Application.persistentDataPath + "/NAT/text.txt", "This is a text file of Native Android Toolkit! This is all!");
        }
        if (txt.Exists == true)
        {
            StartCoroutine(DemoAnimation("The Text was loaded from \"text.txt\". Content is\n" + txt.ThisFile.GetAsTextString()));
        }
    }

    //Demo for FileRef path
    public void FileRef_Archive_Path()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            StartCoroutine(DemoAnimation("The path to \"img.png\" is\n" + img.ThisFile.Path));
        }
    }

    //Demo for FileRef size
    public void FileRef_Archive_Size()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            StartCoroutine(DemoAnimation("The size of \"img.png\" is " + img.ThisFile.Size.Kilobytes + " Kb."));
        }
    }

    //Demo for FileRef name
    public void FileRef_Archive_PureName()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            StartCoroutine(DemoAnimation("The pure name of \"img.png\" is \"" + img.ThisFile.PureName + "\"."));
        }
    }

    //Demo for FileRef extension
    public void FileRef_Archive_Extension()
    {
        FileRef img = new FileRef(Application.persistentDataPath + "/NAT/img.png");
        if (img.Exists == false)
        {
            StartCoroutine(DemoAnimation("The file \"img.png\" was not found. A new Example Image File \"img.png\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateImageFile(Application.persistentDataPath + "/NAT/img.png", textureToShare);
        }
        if (img.Exists == true)
        {
            StartCoroutine(DemoAnimation("The extension of \"img.png\" is \"" + img.ThisFile.Extension + "\"."));
        }
    }

    //---------- FileRef Class (With FolderRef class) -----------

    //Demo for FileRef getAllFiles
    public void FileRef_Folder_GetAllFiles()
    {
        FileRef dir = new FileRef(Application.persistentDataPath + "/NAT/Test");
        if (dir.Exists == false)
        {
            StartCoroutine(DemoAnimation("The directory \"/Test\" was not found. A new Example Directory \"/Test\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateDirectory(Application.persistentDataPath + "/NAT/Test");
            NativeAndroid.File.CreateFile(Application.persistentDataPath + "/NAT/Test/bin1.bin", new byte[] { });
            NativeAndroid.File.CreateFile(Application.persistentDataPath + "/NAT/Test/bin2.bin", new byte[] { });
        }
        if (dir.Exists == true)
        {
            FileRef[] filesInPath = dir.ThisFolder.GetAllFiles();
            StringBuilder list = new StringBuilder();
            for (int i = 0; i < filesInPath.Length; i++)
            {
                if (i != filesInPath.Length - 1)
                {
                    list.Append(filesInPath[i].ThisFile.PureName + "\n");
                }
                if (i == filesInPath.Length - 1)
                {
                    list.Append(filesInPath[i].ThisFile.PureName);
                }
            }

            StartCoroutine(DemoAnimation("The diretory \"PersistentDataPath/NAT/Test\" contains files\n" + list.ToString()));
        }
    }

    //Demo for FileRef path
    public void FileRef_Folder_Path()
    {
        FileRef dir = new FileRef(Application.persistentDataPath + "/NAT/Test");
        if (dir.Exists == false)
        {
            StartCoroutine(DemoAnimation("The directory \"/Test\" was not found. A new Example Directory \"/Test\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateDirectory(Application.persistentDataPath + "/NAT/Test");
            NativeAndroid.File.CreateFile(Application.persistentDataPath + "/NAT/Test/bin1.bin", new byte[] { });
            NativeAndroid.File.CreateFile(Application.persistentDataPath + "/NAT/Test/bin2.bin", new byte[] { });
        }
        if (dir.Exists == true)
        {
            StartCoroutine(DemoAnimation("The path to \"PersistentDataPath/NAT/Test\" is\n" + dir.ThisFolder.Path));
        }
    }

    //Demo for FileRef name
    public void FileRef_Folder_Name()
    {
        FileRef dir = new FileRef(Application.persistentDataPath + "/NAT/Test");
        if (dir.Exists == false)
        {
            StartCoroutine(DemoAnimation("The directory \"/Test\" was not found. A new Example Directory \"/Test\" was created. Please, try to execute this function again!"));
            NativeAndroid.File.CreateDirectory(Application.persistentDataPath + "/NAT/Test");
            NativeAndroid.File.CreateFile(Application.persistentDataPath + "/NAT/Test/bin1.bin", new byte[] { });
            NativeAndroid.File.CreateFile(Application.persistentDataPath + "/NAT/Test/bin2.bin", new byte[] { });
        }
        if (dir.Exists == true)
        {
            StartCoroutine(DemoAnimation("The name of \"PersistentDataPath/NAT/Test\" folder is \"" + dir.ThisFolder.Name + "\"."));
        }
    }
}