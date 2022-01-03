#region

using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

#endregion

namespace MagicLeapSetupTool.Editor.Setup
{
    /// <summary>
    /// Changes the graphics APIs to work with Magic Leap and Zero Iteration
    /// </summary>
    public class UpdateGraphicsApiSetupStep : ISetupStep
    {
        private const string SET_CORRECT_GRAPHICS_API_LABEL = "Add OpenGLCore to Graphics API";
        private const string SET_CORRECT_GRAPHICS_BUTTON_LABEL = "Update";
        private const string CONDITION_MET_LABEL = "Done";
        private static int _busyCounter;
        public static bool HasCorrectGraphicConfiguration;

        public static int BusyCounter
        {
            get => _busyCounter;
            set => _busyCounter = Mathf.Clamp(value, 0, 100);
        }

        public bool Busy => BusyCounter > 0;

        /// <inheritdoc />
        public void Refresh()
        {
            HasCorrectGraphicConfiguration = CorrectGraphicsConfiguration();
        }

        /// <inheritdoc />
        public bool Draw()
        {
            if (CustomGuiContent.CustomButtons.DrawConditionButton(SET_CORRECT_GRAPHICS_API_LABEL,
                HasCorrectGraphicConfiguration, CONDITION_MET_LABEL, SET_CORRECT_GRAPHICS_BUTTON_LABEL,
                Styles.FixButtonStyle))
            {
                Execute();
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void Execute()
        {
            if (HasCorrectGraphicConfiguration) return;

            UpdateGraphicsSettings();
        }

        /// <summary>
        /// checks if the graphics configuration supports Magic Leap and Zero Iteration
        /// </summary>
        /// <returns></returns>
        private static bool CorrectGraphicsConfiguration()
        {
            #region Windows

            var correctSetup = false;
            var hasGraphicsDevice =
                UnityProjectSettingsUtility.HasGraphicsDeviceTypeAtIndex(BuildTarget.StandaloneWindows,
                    GraphicsDeviceType.OpenGLCore, 0);
            correctSetup = hasGraphicsDevice &&
                           !UnityProjectSettingsUtility.GetAutoGraphicsApi(BuildTarget.StandaloneWindows);
            if (!correctSetup) return false;

            #endregion

            #region OSX

            hasGraphicsDevice =
                UnityProjectSettingsUtility.HasGraphicsDeviceTypeAtIndex(BuildTarget.StandaloneOSX,
                    GraphicsDeviceType.OpenGLCore, 0);
            correctSetup = hasGraphicsDevice &&
                           !UnityProjectSettingsUtility.GetAutoGraphicsApi(BuildTarget.StandaloneOSX);
            if (!correctSetup) return false;

            #endregion

            #region Linux

            hasGraphicsDevice = UnityProjectSettingsUtility.HasGraphicsDeviceTypeAtIndex(BuildTarget.StandaloneLinux64,
                GraphicsDeviceType.OpenGLCore, 0);
            correctSetup = hasGraphicsDevice &&
                           !UnityProjectSettingsUtility.GetAutoGraphicsApi(BuildTarget.StandaloneLinux64);
            if (!correctSetup) return false;

            #endregion

            return correctSetup;
        }

        /// <summary>
        /// Changes the graphics settings for all Lumin platforms
        /// </summary>
        /// <param name="data"></param>
        public static void UpdateGraphicsSettings()
        {
            BusyCounter++;

            var standaloneWindowsResetRequired =
                UnityProjectSettingsUtility.SetGraphicsApi(BuildTarget.StandaloneWindows, GraphicsDeviceType.OpenGLCore,
                    0);
            var standaloneWindows64ResetRequired =
                UnityProjectSettingsUtility.SetGraphicsApi(BuildTarget.StandaloneWindows64,
                    GraphicsDeviceType.OpenGLCore, 0);
            var standaloneOSXResetRequired =
                UnityProjectSettingsUtility.SetGraphicsApi(BuildTarget.StandaloneOSX, GraphicsDeviceType.OpenGLCore, 0);

            var standaloneLinuxResetRequired =
                UnityProjectSettingsUtility.SetGraphicsApi(BuildTarget.StandaloneLinux64, GraphicsDeviceType.OpenGLCore,
                    0);

            UnityProjectSettingsUtility.SetAutoGraphicsApi(BuildTarget.StandaloneWindows, false);
            UnityProjectSettingsUtility.SetAutoGraphicsApi(BuildTarget.StandaloneWindows64, false);
            UnityProjectSettingsUtility.SetAutoGraphicsApi(BuildTarget.StandaloneOSX, false);
            UnityProjectSettingsUtility.SetAutoGraphicsApi(BuildTarget.StandaloneLinux64, false);


            MagicLeapSetupAutoRun.Stop();

            if (standaloneWindowsResetRequired
                || standaloneWindows64ResetRequired
                || standaloneOSXResetRequired
                || standaloneLinuxResetRequired)
                UnityProjectSettingsUtility.UpdateGraphicsApi(true);
            else
                UnityProjectSettingsUtility.UpdateGraphicsApi(false);


            BusyCounter--;
        }
    }
}