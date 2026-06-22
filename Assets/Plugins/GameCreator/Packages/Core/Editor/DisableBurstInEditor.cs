using UnityEditor;
using System;
using System.Reflection;

namespace GameCreator.Editor.Core
{
    [InitializeOnLoad]
    internal static class DisableBurstInEditor
    {
        static DisableBurstInEditor()
        {
            // Force-disable Burst in editor sessions to avoid entry-point resolution failures.
            TryDisableBurstCompilation();
        }

        private static void TryDisableBurstCompilation()
        {
            try
            {
                Type burstCompilerType = Type.GetType("Unity.Burst.BurstCompiler, Unity.Burst");
                if (burstCompilerType == null) return;

                PropertyInfo optionsProperty = burstCompilerType.GetProperty(
                    "Options",
                    BindingFlags.Public | BindingFlags.Static
                );
                object options = optionsProperty?.GetValue(null);
                if (options == null) return;

                PropertyInfo enableProperty = options.GetType().GetProperty(
                    "EnableBurstCompilation",
                    BindingFlags.Public | BindingFlags.Instance
                );
                if (enableProperty?.CanWrite == true)
                {
                    enableProperty.SetValue(options, false);
                }
            }
            catch
            {
                // Keep editor startup resilient if Burst API shape changes.
            }
        }
    }
}
