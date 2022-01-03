#region

using MagicLeapSetupTool.Editor.Setup;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

#endregion

namespace MagicLeapSetupTool.Editor
{
    [InitializeOnLoad]
    public static class AutoRunner
    {
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

        static AutoRunner()
        {
            if (Application.isBatchMode)
                return;

            EditorApplication.update += OnEditorApplicationUpdate;
            EditorApplication.quitting += OnQuit;
            Events.registeringPackages += SetupData.RegisteringPackagesEventHandler;
        }

        private static void OnQuit()
        {
            EditorApplication.quitting -= OnQuit;
            EditorPrefs.SetBool(EditorKeyUtility.WindowClosedEditorPrefKey, false);
            Events.registeringPackages -= SetupData.RegisteringPackagesEventHandler;
        }

       

        private static void OnEditorApplicationUpdate()
        {
            SetupData.UpdateDefineSymbols();
            if (AssetDatabase.IsAssetImportWorkerProcess() || EditorApplication.isCompiling ||
                EditorApplication.isUpdating) return;

            var autoShow = EditorPrefs.GetBool(EditorKeyUtility.AutoShowEditorPrefKey, true);
            if (!SetupData.HasRootSDKPathInEditorPrefs
                || !HasLuminInstalled
                || !BuildTargetSetupStep.CorrectBuildTarget
                || !ImportMagicLeapSdkSetupStep.HasCompatibleMagicLeapSdk)
            {
                autoShow = true;
                EditorPrefs.SetBool(EditorKeyUtility.AutoShowEditorPrefKey, true);
            }

            EditorApplication.update -= OnEditorApplicationUpdate;
            if (!autoShow) return;

            MagicLeapSetupWindow.ForceOpen();
        }
    }
}