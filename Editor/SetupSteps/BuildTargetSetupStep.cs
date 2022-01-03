#region

using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;

#endregion

namespace MagicLeapSetupTool.Editor.Setup
{
    /// <summary>
    /// Switches the build platform to Lumin
    /// </summary>
    public class BuildTargetSetupStep : ISetupStep
    {
        private const string FIX_SETTING_BUTTON_LABEL = "Fix Setting";
        private const string CONDITION_MET_LABEL = "Done";
        private const string BUILD_SETTING_LABEL = "Set build target to Lumin";
        public static bool CorrectBuildTarget;
        private bool _hasRootSDKPath;

        /// <inheritdoc />
        public void Refresh()
        {
            CorrectBuildTarget = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin;
            _hasRootSDKPath = MagicLeapLuminPackageUtility.HasRootSDKPath;
        }


        /// <inheritdoc />
        public bool Draw()
        {
            GUI.enabled = _hasRootSDKPath;


            if (CustomGuiContent.CustomButtons.DrawConditionButton(BUILD_SETTING_LABEL, CorrectBuildTarget,
                CONDITION_MET_LABEL,
                FIX_SETTING_BUTTON_LABEL, Styles.FixButtonStyle))
            {
                Execute();
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void Execute()
        {
            if (CorrectBuildTarget) return;

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Lumin, BuildTarget.Lumin);
        }
    }
}