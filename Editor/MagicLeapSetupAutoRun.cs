/* Copyright (C) 2021 Adrian Babilinski
* You may use, distribute and modify this code under the
* terms of the MIT License
*/

using System;
using System.Threading;
using System.Threading.Tasks;
using MagicLeapSetupTool.Editor.ScriptableObjects;
using MagicLeapSetupTool.Editor.Setup;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor
{
    public static class MagicLeapSetupAutoRun
    {

    #region EDITOR PREFS

        private const string MAGICLEAP_AUTO_SETUP_PREF = "MAGICLEAP-AUTO-SETUP";

    #endregion
    #region DEBUG TEXT

        private const string CHANGING_BUILD_PLATFORM_DEBUG = "Setting Build Platform To Lumin...";
        private const string INSTALLING_LUMIN_SDK_DEBUG = "Installing Magic Leap Plug-in...";
        private const string ENABLING_LUMIN_SDK_DEBUG = "Enabling Magic Leap Plug-in...";
        private const string UPDATING_MANIFEST_DEBUG = "Updating Magic Leap Manifest...";
        private const string IMPORTING_LUMIN_UNITYPACKAGE_DEBUG = "Importing Magic Leap UnityPackage...";
        private const string UPDATING_COLORSPACE_DEBUG = "Changing Color Space to Recommended Setting [Linear]...";
        private const string CHANGING_GRAPHICS_API_DEBUG = "Updating Graphics API To Include [OpenGLCore] (Auto Api = false)...";

    #endregion

    #region TEXT AND LABELS

        internal const string APPLY_ALL_PROMPT_TITLE = "Configure all settings";
        internal const string APPLY_ALL_PROMPT_MESSAGE = "This will update the project to the recommended settings for Magic leap EXCEPT FOR SETTING A DEVELOPMENT CERTIFICATE. Would you like to continue?";
        internal const string APPLY_ALL_PROMPT_OK = "Continue";
        internal const string APPLY_ALL_PROMPT_CANCEL = "Cancel";
        internal const string APPLY_ALL_PROMPT_ALT = "Setup Development Certificate";
        internal const string APPLY_ALL_PROMPT_NOTHING_TO_DO_MESSAGE = "All settings are configured. There is no need to run utility";
        internal const string APPLY_ALL_PROMPT_NOTHING_TO_DO_OK = "Close";
        internal const string APPLY_ALL_PROMPT_MISSING_CERT_MESSAGE = "All settings are configured except the developer certificate. Would you like to set it now?";
        internal const string APPLY_ALL_PROMPT_MISSING_CERT_OK = "Set Certificate";
        internal const string APPLY_ALL_PROMPT_MISSING_CERT_CANCEL = "Cancel";

    #endregion

        private static ApplyAllState _currentApplyAllState = ApplyAllState.Done;

        internal static bool _allAutoStepsComplete
        {
            get
            {
            
                if (_magicLeapSetupData == null)
                {
                    _magicLeapSetupData = MagicLeapSetupDataScriptableObject.Instance;
                }
                return UpdateGraphicsApiSetupStep.HasCorrectGraphicConfiguration
                   && PlayerSettings.colorSpace == ColorSpace.Linear
                   && ImportMagicLeapSdkSetupStep.HasMagicLeapSdkInstalled
                   && UpdateManifestSetupStep.ManifestIsUpdated
                   && SetupData.HasRootSDKPath
                   && EnablePluginSetupStep.LuminSettingEnabled
                   && _hasLuminInstalled
                   && ImportMagicLeapSdkSetupStep.HasCompatibleMagicLeapSdk
                   && BuildTargetSetupStep.CorrectBuildTarget;
            }
        }

        private static MagicLeapSetupDataScriptableObject _magicLeapSetupData;
        private static SetSdkFolderSetupStep _setSdkFolderSetupStep = new SetSdkFolderSetupStep();
        private static BuildTargetSetupStep _buildTargetSetupStep = new BuildTargetSetupStep();
        private static EnablePluginSetupStep _enablePluginSetupStep = new EnablePluginSetupStep();
        private static UpdateManifestSetupStep _updateManifestSetupStep = new UpdateManifestSetupStep();
        private static SetCertificateSetupStep _setCertificateSetupStep = new SetCertificateSetupStep();
        private static ImportMagicLeapSdkSetupStep _importMagicLeapSdkSetupStep = new ImportMagicLeapSdkSetupStep();
        private static ColorSpaceSetupStep _colorSpaceSetupStep = new ColorSpaceSetupStep();
        private static UpdateGraphicsApiSetupStep _updateGraphicsApiSetupStep = new UpdateGraphicsApiSetupStep();
		private static readonly bool _hasLuminInstalled =
														#if MAGICLEAP
															true;
														#else
															false;
														#endif
        internal static ApplyAllState CurrentApplyAllState
        {
            get => _currentApplyAllState;
            set
            {
                EditorPrefs.SetString(MAGICLEAP_AUTO_SETUP_PREF, value.ToString());
                _currentApplyAllState = value;
            }
        }

        public static void CheckLastAutoSetupState()
        {
           
           if(Enum.TryParse(EditorPrefs.GetString(MAGICLEAP_AUTO_SETUP_PREF),true, out ApplyAllState value))
           {
        
               CurrentApplyAllState = value;
           }
           else
           {
               _currentApplyAllState = ApplyAllState.Done;
           }
         
        }

        internal static void Stop()
        {
           

            CurrentApplyAllState = ApplyAllState.Done;
        }
     
        internal static void RunApplyAll()
        {
            _magicLeapSetupData = MagicLeapSetupDataScriptableObject.Instance;
            if (!_allAutoStepsComplete)
            {
                var dialogComplex = EditorUtility.DisplayDialogComplex(APPLY_ALL_PROMPT_TITLE, APPLY_ALL_PROMPT_MESSAGE,
                                                                       APPLY_ALL_PROMPT_OK, APPLY_ALL_PROMPT_CANCEL, APPLY_ALL_PROMPT_ALT);

                switch (dialogComplex)
                {
                    case 0: //Continue
                        CurrentApplyAllState = ApplyAllState.SwitchBuildTarget;
                        break;
                    case 1: //Stop
                        CurrentApplyAllState = ApplyAllState.Done;
                        break;
                    case 2: //Go to documentation
                        Help.BrowseURL(MagicLeapSetupWindow.SETUP_ENVIRONMENT_URL);
                        CurrentApplyAllState = ApplyAllState.Done;
                        break;
                }

               
               
             
            }
            else if (!SetCertificateSetupStep.ValidCertificatePath)
            {
                var dialogComplex = EditorUtility.DisplayDialogComplex(APPLY_ALL_PROMPT_TITLE, APPLY_ALL_PROMPT_MISSING_CERT_MESSAGE,
                                                                       APPLY_ALL_PROMPT_MISSING_CERT_OK, APPLY_ALL_PROMPT_MISSING_CERT_CANCEL, APPLY_ALL_PROMPT_ALT);

                switch (dialogComplex)
                {
                    case 0: //Continue
                        _setCertificateSetupStep.Execute();
                        break;
                    case 1: //Stop
                        CurrentApplyAllState = ApplyAllState.Done;
                        break;
                    case 2: //Go to documentation
                        Help.BrowseURL(MagicLeapSetupWindow.SETUP_ENVIRONMENT_URL);
                        CurrentApplyAllState = ApplyAllState.Done;
                        break;
                }
            }
            else if (SetCertificateSetupStep.ValidCertificatePath)
            {
                EditorUtility.DisplayDialog(APPLY_ALL_PROMPT_TITLE, APPLY_ALL_PROMPT_NOTHING_TO_DO_MESSAGE,
                                            APPLY_ALL_PROMPT_NOTHING_TO_DO_OK);
            }
        }


        internal static void Tick()
        {
            if (_magicLeapSetupData == null)
            {
                _magicLeapSetupData = MagicLeapSetupDataScriptableObject.Instance;
                return;
            }
            var _loading = AssetDatabase.IsAssetImportWorkerProcess() || EditorApplication.isCompiling || _magicLeapSetupData.Busy || EditorApplication.isUpdating;
            if (CurrentApplyAllState != ApplyAllState.Done && !_loading)
            {
                ApplyAll();
            }

           
        }

        private static void ApplyAll()
        {
          var correctBuildTarget = BuildTargetSetupStep.CorrectBuildTarget;
            switch (CurrentApplyAllState)
            {
                case ApplyAllState.SwitchBuildTarget:
                    if (!correctBuildTarget)
                    {
                        Debug.Log(CHANGING_BUILD_PLATFORM_DEBUG);
                       _buildTargetSetupStep.Execute();
                    }

                    CurrentApplyAllState = ApplyAllState.InstallLumin;
                    break;
                case ApplyAllState.InstallLumin:
                    if (correctBuildTarget)
                    {
                       
                        

                        CurrentApplyAllState = ApplyAllState.EnableXrPackage;
                    }

                    break;
                case ApplyAllState.EnableXrPackage:
                    if (correctBuildTarget && _hasLuminInstalled)
                    {
                        if (!EnablePluginSetupStep.LuminSettingEnabled)
                        {
                            Debug.Log(ENABLING_LUMIN_SDK_DEBUG);
                            _enablePluginSetupStep.Execute();
                            ImportMagicLeapSdkSetupStep.CheckForMagicLeapSdkPackage();
                        }

                        CurrentApplyAllState = ApplyAllState.UpdateManifest;
                    }

                    break;
                case ApplyAllState.UpdateManifest:
                    if (correctBuildTarget && _hasLuminInstalled && EnablePluginSetupStep.LuminSettingEnabled)
                    {
                        if (!UpdateManifestSetupStep.ManifestIsUpdated)
                        {
                            Debug.Log(UPDATING_MANIFEST_DEBUG);
                            _updateManifestSetupStep.Execute();
                        }

                        CurrentApplyAllState = ApplyAllState.ChangeColorSpace;
                    }

                    break;

                case ApplyAllState.ChangeColorSpace:
                    if (correctBuildTarget && _hasLuminInstalled && EnablePluginSetupStep.LuminSettingEnabled && UpdateManifestSetupStep.ManifestIsUpdated)
                    {
                        if (PlayerSettings.colorSpace != ColorSpace.Linear)
                        {
                            Debug.Log(UPDATING_COLORSPACE_DEBUG);
                           _colorSpaceSetupStep.Execute();
                        }

                        CurrentApplyAllState = ApplyAllState.ImportSdkUnityPackage;
                    }

                    break;

                case ApplyAllState.ImportSdkUnityPackage:
                    if (correctBuildTarget && _hasLuminInstalled && EnablePluginSetupStep.LuminSettingEnabled && UpdateManifestSetupStep.ManifestIsUpdated)
                    {
                        if (!ImportMagicLeapSdkSetupStep.HasMagicLeapSdkInstalled)
                        {
                            if (ImportMagicLeapSdkSetupStep.HasCompatibleMagicLeapSdk)
                            {
                               _importMagicLeapSdkSetupStep.Execute();

                                Debug.Log(IMPORTING_LUMIN_UNITYPACKAGE_DEBUG);
                                CurrentApplyAllState = ApplyAllState.ChangeGraphicsApi;
                            }
                            else
                            {
                                //TODO: Automate
                                Debug.LogError("Magic Leap SDK Conflict. Cannot resolve automatically.");
                                Stop();
                            }
                        }
                    }

                    break;

                case ApplyAllState.ChangeGraphicsApi:
                    if (correctBuildTarget && _hasLuminInstalled && EnablePluginSetupStep.LuminSettingEnabled && UpdateManifestSetupStep.ManifestIsUpdated && ImportMagicLeapSdkSetupStep.HasMagicLeapSdkInstalled && PlayerSettings.colorSpace == ColorSpace.Linear)
                    {
                        if (!UpdateGraphicsApiSetupStep.HasCorrectGraphicConfiguration)
                        {
                            Debug.Log(CHANGING_GRAPHICS_API_DEBUG);
                            _updateGraphicsApiSetupStep.Execute();
                        }

                        CurrentApplyAllState = ApplyAllState.Done;
                    }

                    break;
                case ApplyAllState.Done:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    #region ENUMS

        internal enum ApplyAllState
        {
            SwitchBuildTarget,
            InstallLumin,
            EnableXrPackage,
            UpdateManifest,
            ChangeColorSpace,
            ImportSdkUnityPackage,
            ChangeGraphicsApi,
            Done
        }

    #endregion

    }
}
