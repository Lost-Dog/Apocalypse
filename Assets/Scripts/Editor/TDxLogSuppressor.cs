using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Reflection;

[InitializeOnLoad]
public static class TDxLogSuppressor
{
    private static readonly Regex TDxPattern = new Regex(
        @"(TDxController\.cs:|RTTDxController\.cs:|~CameraController|~RTCameraController|CloseConnection|navigation3D|TDxController\(\)|3Dconnexion|SpaceMouse|TDx\.TDxInput)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    static TDxLogSuppressor()
    {
        Application.logMessageReceived += OnLogMessageReceived;
        Application.logMessageReceivedThreaded += OnLogMessageReceived;
    }

    private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        // Filter out TDxController logs completely - they won't appear in console
        if (TDxPattern.IsMatch(condition) || TDxPattern.IsMatch(stackTrace))
        {
            // Suppress the log by not forwarding it
            // This prevents it from appearing in the Unity Console
        }
    }
}
