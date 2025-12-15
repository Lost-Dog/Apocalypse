using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

[InitializeOnLoad]
public static class TDxLogSuppressor
{
    private static readonly Regex TDxPattern = new Regex(
        @"(TDxController\.cs:|RTTDxController\.cs:|~CameraController|~RTCameraController|CloseConnection|navigation3D|TDxController\(\))",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    static TDxLogSuppressor()
    {
        Application.logMessageReceived += OnLogMessageReceived;
    }

    private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Log)
        {
            if (TDxPattern.IsMatch(condition) || TDxPattern.IsMatch(stackTrace))
            {
                return;
            }
        }
    }
}
