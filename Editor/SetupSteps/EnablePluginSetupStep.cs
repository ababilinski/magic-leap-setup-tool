#region

using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;

#endregion

namespace MagicLeapSetupTool.Editor.Setup
{
    /// <summary>
    /// Enables the Lumin XR plugin
    /// </summary>
    public class EnablePluginSetupStep : ISetupStep
    {
        private const string ENABLE_PLUGIN_FAILED_PLUGIN_NOT_INSTALLED_MESSAGE = "Magic Leap Pug-in is not installed.";
        private const string ENABLE_PLUGIN_LABEL = "Enable Plugin";
        private const string ENABLE_PLUGIN_SETTINGS_LABEL = "Enable the Lumin XR plug-in";
        private const string CONDITION_MET_LABEL = "Done";

        //0 is method/action name
        private const string ENABLE_LUMIN_FINISHED_UNSUCCESSFULLY_WARNING =
            "Unsuccessful call:[{0}]. action finished, but Lumin XR Settings are still not enabled.";

        //0 is method/action name
        private const string FAILED_TO_EXECUTE_ERROR = "Failed to execute [{0}]"; 
        private static int _busyCounter;
        private static bool _correctBuildTarget;

        public static int BusyCounter
        {
            get => _busyCounter;
            set => _busyCounter = Mathf.Clamp(value, 0, 100);
        }

        private bool _hasRootSDKPath;
        public static bool LuminSettingEnabled;
        public static bool HasLuminInstalled
        {
            get
            {
#if MAGICLEAP
                return true;
#else
                return  false;
#endif
            }
        }

        public bool Busy => BusyCounter > 0;

        /// <inheritdoc />
        public void Refresh()
        {
            _hasRootSDKPath = MagicLeapLuminPackageUtility.HasRootSDKPath;
            _correctBuildTarget = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin;
            LuminSettingEnabled = MagicLeapLuminPackageUtility.IsLuminXREnabled();
        }

        /// <inheritdoc />
        public bool Draw()
        {
            GUI.enabled = _hasRootSDKPath && _correctBuildTarget && HasLuminInstalled;
            if (CustomGuiContent.CustomButtons.DrawConditionButton(ENABLE_PLUGIN_SETTINGS_LABEL, LuminSettingEnabled,
                CONDITION_MET_LABEL, ENABLE_PLUGIN_LABEL, Styles.FixButtonStyle))
            {
                Execute();
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void Execute()
        {
            if (LuminSettingEnabled) return;

            if (!HasLuminInstalled)
            {
                Debug.LogWarning(ENABLE_PLUGIN_FAILED_PLUGIN_NOT_INSTALLED_MESSAGE);
                return;
            }

            BusyCounter++;
            MagicLeapLuminPackageUtility.EnableLuminXRFinished += OnEnableMagicLeapPluginFinished;
            MagicLeapLuminPackageUtility.EnableLuminXRPlugin();


            void OnEnableMagicLeapPluginFinished(bool success)
            {
                if (success)
                {
                    LuminSettingEnabled = MagicLeapLuminPackageUtility.IsLuminXREnabled();
                    if (!LuminSettingEnabled)
                        Debug.LogWarning(string.Format(ENABLE_LUMIN_FINISHED_UNSUCCESSFULLY_WARNING,
                            "Enable Lumin XR action"));
                }
                else
                {
                    Debug.LogError(string.Format(FAILED_TO_EXECUTE_ERROR, "Enable Lumin XR Package"));
                }

                BusyCounter--;
                MagicLeapLuminPackageUtility.EnableLuminXRFinished -= OnEnableMagicLeapPluginFinished;
            }


            UnityProjectSettingsUtility.OpenXrManagementWindow();
            MagicLeapLuminPackageUtility.UpdateDefineSymbols();
        }
    }
}