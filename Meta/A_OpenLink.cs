using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class A_OpenLink : MonoBehaviour
{
    public void OpenLink(string url)
    {
        UnityEngine.Debug.Log("Trying to open URL: " + url);
#if UNITY_WEBGL
        OpenTab(url);
#else
        OpenURL(url);
#endif
    }

    [DllImport("__Internal")]
    private static extern void OpenTab(string url);

    private void OpenURL(string url)
    {
        // Determine the platform and open the URL accordingly
#if UNITY_EDITOR
        // In the Unity Editor, simply print the URL for testing
        UnityEngine.Debug.Log("Opening URL: " + url);

#elif UNITY_STANDALONE_WIN
            // On Windows, open the URL using the default web browser
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });

#elif UNITY_STANDALONE_OSX
            // On macOS, open the URL using the default web browser
            Process.Start("open", url);

#elif UNITY_ANDROID
            // On Android, open the URL using an intent
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_VIEW"));
            intentObject.Call<AndroidJavaObject>("setData", AndroidJavaObject.CallStatic<AndroidJavaObject>("parse", url));
            AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
            currentActivity.Call("startActivity", intentObject);

#elif UNITY_IOS
            // On iOS, open the URL using the openURL method of the UIApplication class
            Application.OpenURL(url);

#elif UNITY_STANDALONE_LINUX
            // On Linux, open the URL using the default web browser
            Process.Start(new ProcessStartInfo
            {
                FileName = "xdg-open",
                Arguments = url,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

#else
            // For other platforms, print a message indicating that the operation is not supported
            Debug.LogWarning("Opening URLs is not supported on this platform: " + Application.platform);

#endif
    }
}
