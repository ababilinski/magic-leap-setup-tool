/* Copyright (C) 2021 Adrian Babilinski
* You may use, distribute and modify this code under the
* terms of the MIT License
*/

using System;
using System.Collections.Generic;
using System.IO;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MagicLeapSetupTool.Editor
{
    [InitializeOnLoad]
    public class MagicLeapSetupWindow : EditorWindow
    {
    #region EDITOR PREFS

        private const string PREVIOUS_CERTIFICATE_PROMPT_KEY = "PREVIOUS_CERTIFICATE_PROMPT_KEY";
        private const string MAGIC_LEAP_SETUP_POSTFIX_KEY = "MAGIC_LEAP_SETUP_KEY";

    #endregion

    #region TEXT AND LABELS

        private const string WINDOW_PATH = "Magic Leap/Project Setup Utility";
        private const string WINDOW_TITLE_LABEL = "Magic Leap Project Setup";
        private const string TITLE_LABEL = "MAGIC LEAP";
        private const string SUBTITLE_LABEL = "PROJECT SETUP";
        private const string HELP_BOX_TEXT = "Required settings For Lumin SDK";
        private const string LOADING_TEXT = "   Loading and Importing...";
        private const string CONDITION_MET_LABEL = "Done";
        private const string CONDITION_MET_CHANGE_LABEL = "Change";
        private const string FIX_SETTING_BUTTON_LABEL = "Fix Setting";

        private const string COLOR_SPACE_LABEL = "Set Color Space To Linear";

        private const string BUILD_SETTING_LABEL = "Set build target to Lumin";
        private const string INSTALL_PLUGIN_LABEL = "Install the Magic Leap XR plug-in";
        private const string INSTALL_PLUGIN_BUTTON_LABEL = "Install Package";

        private const string ENABLE_PLUGIN_SETTINGS_LABEL = "Enable the Magic Leap XR plug-in";
        private const string ENABLE_PLUGIN_LABEL = "Enable Plugin";
        private const string ENABLE_PLUGIN_FAILED_PLUGIN_NOT_INSTALLED_MESSAGE = "Magic Leap Pug-in is not installed.";

        private const string LOCATE_SDK_FOLDER_LABEL = "Set external Lumin SDK Folder";
        private const string LOCATE_SDK_FOLDER_BUTTON_LABEL = "Locate SDK";


        private const string UPDATE_MANIFEST_LABEL = "Update the manifest file";
        private const string UPDATE_MANIFEST_BUTTON_LABEL = "Update";
        private const string LINKS_TITLE = "Helpful Links:";
        private readonly string SET_CERTIFICATE_PATH_LABEL = "Locate developer certificate";
        private const string SET_CERTIFICATE_PATH_BUTTON_LABEL = "Locate";
        
        private const string SET_CERTIFICATE_HELP_TEXT = "Get a developer certificate";

        private const string IMPORT_LUMIN_SDK_UNITYPACKAGE = "Import the SDK Unity Package";
        private const string IMPORT_LUMIN_SDK_UNITYPACKAGE_BUTTON = "Import package";
        private const string FAILED_TO_IMPORT_TITLE = "Failed to import Unity Package.";
        private const string FAILED_TO_IMPORT_MESSAGE = "Failed to find the Lumin SDK Unity Package. Please make sure your development enviornment is setup correctly.";
        private const string FAILED_TO_IMPORT_OK = "Try Again";
        private const string FAILED_TO_IMPORT_CANCEL = "Cancel";
        private const string FAILED_TO_IMPORT_ALT = "Setup Developer Environment";
        private const string FAILED_TO_IMPORT_HELP_TEXT = "Setup the developer environment";

        private const string FOUND_PREVIOUS_CERTIFICATE_TITLE = "Found Previously Used Developer Certificate";
        private const string FOUND_PREVIOUS_CERTIFICATE_MESSAGE = "Magic Leap Setup has found a previously used developer certificate. Would you like to use it in this project?";
        private const string FOUND_PREVIOUS_CERTIFICATE_OK = "Yes";
        private const string FOUND_PREVIOUS_CERTIFICATE_CANCEL = "Cancel";
        private const string FOUND_PREVIOUS_CERTIFICATE_ALT = "Browse For Certificate";

        private const string SET_CORRECT_GRAPHICS_API_LABEL = "Add OpenGLCore to Graphics API";
        private const string SET_CORRECT_GRAPHICS_BUTTON_LABEL = "Update";
        private const string GETTING_STARTED_HELP_TEXT = "Read the getting started guide";

        private const string APPLY_ALL_PROMPT_TITLE = "Configure all settings";
        private const string APPLY_ALL_PROMPT_MESSAGE = "This will update the project to the recommended settings for Magic leap EXCEPT FOR SETTING A DEVELOPMENT CERTIFICATE. Would you like to continue?";
        private const string APPLY_ALL_PROMPT_OK = "Continue";
        private const string APPLY_ALL_PROMPT_CANCEL = "Cancel";
        private const string APPLY_ALL_PROMPT_ALT = "Setup Development Certificate";


        private const string APPLY_ALL_PROMPT_NOTHING_TO_DO_MESSAGE = "All settings are configured. There is no need to run utility";
        private const string APPLY_ALL_PROMPT_NOTHING_TO_DO_OK = "Close";

        private const string APPLY_ALL_PROMPT_MISSING_CERT_MESSAGE = "All settings are configured except the developer certificate. Would you like to set it now?";
        private const string APPLY_ALL_PROMPT_MISSING_CERT_OK = "Set Certificate";
        private const string APPLY_ALL_PROMPT_MISSING_CERT_CANCEL = "Cancel";

        private const string CHANGE_EDITOR_GRAPHICS_API_TITLE = "Changing editor graphics API";
        private const string CHANGE_EDITOR_GRAPHICS_API_MESSAGE = "You've changed the active graphics API. This requires a restart of the Editor. Do you want to save the Scene when restarting?";
        private const string CHANGE_EDITOR_GRAPHICS_API_SAVE_MESSAGE = "You've changed the active graphics API. This requires a restart of the Editor.";
        private const string CHANGE_EDITOR_GRAPHICS_API_OK = "Restart";
        private const string CHANGE_EDITOR_GRAPHICS_API_OK_SAVE = "Save and Restart";
        private const string CHANGE_EDITOR_GRAPHICS_API_DONTSAVE_CANCEL = "Discard Changes and Restart";
        private const string CHANGE_EDITOR_GRAPHICS_API_CANCEL = "Not Now";


    #endregion

    #region HELP URLS

        private const string Get_CERTIFICATE_URL = "https://developer.magicleap.com/en-us/learn/guides/developer-certificates";
        private const string SETUP_ENVIRONMENT_URL = "https://developer.magicleap.com/en-us/learn/guides/set-up-development-environment#installing-lumin-sdk-packages";
        private const string GETTING_STARTED_URL = "https://developer.magicleap.com/en-us/learn/guides/get-started-developing-in-unity";

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

    #region ENUMS

        private enum ApplyAllState
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

        private static MagicLeapSetupWindow _setupWindow;
        private static bool subscribedToUpdate;
        private static ApplyAllState _currentApplyAllState = ApplyAllState.Done;
        private static bool _loading;
        private static bool _showPreviousCertificatePrompt = true;
        private static bool _allAutoStepsComplete => MagicLeapSetup.HasCorrectGraphicConfiguration
                                                  && PlayerSettings.colorSpace == ColorSpace.Linear
                                                  && MagicLeapSetup.ExtendedUnityPackageImported
                                                  && MagicLeapSetup.ManifestIsUpdated
                                                  && MagicLeapSetup.HasRootSDKPath
                                                  && MagicLeapSetup.MagicLeapSettingEnabled
                                                  && MagicLeapSetup.HasLuminInstalled
                                                  && EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin;

        private static string AutoShowEditorPrefKey
        {
            get
            {
                var projectKey = UnityProjectSettingsUtility.GetProjectKey();
                var path = Path.GetFullPath(Application.dataPath);
                return $"{MAGIC_LEAP_SETUP_POSTFIX_KEY}_[{projectKey}]-[{path}]";
            }
        }


        static MagicLeapSetupWindow()
        {
           
                EditorApplication.update += OnEditorApplicationUpdate;
                subscribedToUpdate = true;
           
        }

        private static void OnEditorApplicationUpdate()
        {
            Open();
        }

        [MenuItem(WINDOW_PATH, priority = -1001)]
        public static void Open()
        {

            
            var autoShow = EditorPrefs.GetBool(AutoShowEditorPrefKey, true);
            if (!MagicLeapSetup.HasRootSDKPathInEditorPrefs || !MagicLeapSetup.HasLuminInstalled || EditorUserBuildSettings.activeBuildTarget != BuildTarget.Lumin)
            {
      
                autoShow = true;
                EditorPrefs.SetBool(AutoShowEditorPrefKey, true);
            }
            
            if (subscribedToUpdate && !autoShow)
            {
            
              EditorApplication.update -= OnEditorApplicationUpdate;
              subscribedToUpdate = false;
              return;
            }

            _showPreviousCertificatePrompt = EditorPrefs.GetBool(PREVIOUS_CERTIFICATE_PROMPT_KEY, true);
            _currentApplyAllState = ApplyAllState.Done;
            _setupWindow = GetWindow<MagicLeapSetupWindow>(false, WINDOW_TITLE_LABEL);
            _setupWindow.minSize = new Vector2(350, 380);
            _setupWindow.maxSize = new Vector2(350, 580);
            EditorApplication.projectChanged += FullRefresh;

            if (subscribedToUpdate)
            {
              

                EditorApplication.update -= OnEditorApplicationUpdate;
                subscribedToUpdate = false;
            }
        }

        private void RunApplyAll()
        {
            if (!_allAutoStepsComplete)
            {
                var dialogComplex = EditorUtility.DisplayDialogComplex(APPLY_ALL_PROMPT_TITLE, APPLY_ALL_PROMPT_MESSAGE,
                                                                       APPLY_ALL_PROMPT_OK, APPLY_ALL_PROMPT_CANCEL, APPLY_ALL_PROMPT_ALT);

                switch (dialogComplex)
                {
                    case 0: //Continue
                        _currentApplyAllState = ApplyAllState.SwitchBuildTarget;
                        break;
                    case 1: //Stop
                        _currentApplyAllState = ApplyAllState.Done;
                        break;
                    case 2: //Go to documentation
                        Help.BrowseURL(SETUP_ENVIRONMENT_URL);
                        _currentApplyAllState = ApplyAllState.Done;
                        break;
                }
            }
            else if (!MagicLeapSetup.ValidCertificatePath)
            {
                var dialogComplex = EditorUtility.DisplayDialogComplex(APPLY_ALL_PROMPT_TITLE, APPLY_ALL_PROMPT_MISSING_CERT_MESSAGE,
                                                                       APPLY_ALL_PROMPT_MISSING_CERT_OK, APPLY_ALL_PROMPT_MISSING_CERT_CANCEL, APPLY_ALL_PROMPT_ALT);

                switch (dialogComplex)
                {
                    case 0: //Continue
                        BrowseForCertificate();
                        break;
                    case 1: //Stop
                        _currentApplyAllState = ApplyAllState.Done;
                        break;
                    case 2: //Go to documentation
                        Help.BrowseURL(SETUP_ENVIRONMENT_URL);
                        _currentApplyAllState = ApplyAllState.Done;
                        break;
                }
            }
            else if (MagicLeapSetup.ValidCertificatePath)
            {
                EditorUtility.DisplayDialog(APPLY_ALL_PROMPT_TITLE, APPLY_ALL_PROMPT_NOTHING_TO_DO_MESSAGE,
                                            APPLY_ALL_PROMPT_NOTHING_TO_DO_OK);
            }
        }

        private void DrawHeader()
        {
            GUILayout.Space(5);
            GUILayout.BeginVertical();
            EditorGUILayout.LabelField(TITLE_LABEL, Styles.TitleStyle);
            EditorGUILayout.LabelField(SUBTITLE_LABEL, Styles.TitleStyle);
            GUILayout.EndVertical();
            CustomGuiContent.DrawUILine(Color.grey, 1, 5);
            GUI.backgroundColor = Color.white;
            GUILayout.Space(2);
        }

        private void DrawInfoBox()
        {
            var luminLogo = EditorGUIUtility.IconContent("BuildSettings.Lumin").image as Texture2D;

            GUILayout.Space(5);

            var content = new GUIContent(HELP_BOX_TEXT, luminLogo);
            EditorGUILayout.LabelField(content, Styles.InfoTitleStyle);

            GUILayout.Space(5);
            GUI.backgroundColor = Color.white;
        }

        public void EnableMagicLeapPlugin()
        {
            if (!MagicLeapSetup.HasLuminInstalled)
            {
                Debug.LogWarning(ENABLE_PLUGIN_FAILED_PLUGIN_NOT_INSTALLED_MESSAGE);
                return;
            }

            MagicLeapSetup.EnableLuminXRPluginAndRefresh();
            UnityProjectSettingsUtility.OpenXrManagementWindow();
            FullRefresh();

           
        }

        public void UpdateManifest()
        {
            if (!MagicLeapSetup.HasLuminInstalled)
            {
                Debug.LogWarning(ENABLE_PLUGIN_FAILED_PLUGIN_NOT_INSTALLED_MESSAGE);
                return;
            }

            MagicLeapSetup.UpdateManifest();
        }

        public void BrowseForCertificate()
        {
            var startDirectory = MagicLeapSetup.PreviousCertificatePath;
            if (!string.IsNullOrEmpty(startDirectory))
            {
                startDirectory= Path.GetDirectoryName(startDirectory);
            }
         
            var path = EditorUtility.OpenFilePanel(SET_CERTIFICATE_PATH_LABEL, startDirectory, "cert");
            if (path.Length != 0)
            {
                MagicLeapSetup.CertificatePath = path;
            }
        }

        public void BrowseForSDK()
        {
        
            var path = EditorUtility.OpenFolderPanel(LOCATE_SDK_FOLDER_LABEL, MagicLeapSetup.GetCurrentSDKLocation(), MagicLeapSetup.GetCurrentSDKFolderName());
            if (path.Length != 0)
            {
                MagicLeapSetup.SetRootSDK(path);
            }
        }
        private static void FullRefresh()
        {
            if (!MagicLeapSetup.CheckingAvailability)
            {
                MagicLeapSetup.CheckSDKAvailability();
            }
        }

        private void DrawHelpLinks()
        {
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(2);
            EditorGUILayout.LabelField(LINKS_TITLE, Styles.HelpTitleStyle);
            CustomGuiContent.DisplayLink(GETTING_STARTED_HELP_TEXT, GETTING_STARTED_URL, 3);
            CustomGuiContent.DisplayLink(SET_CERTIFICATE_HELP_TEXT, Get_CERTIFICATE_URL, 3);
            CustomGuiContent.DisplayLink(FAILED_TO_IMPORT_HELP_TEXT, SETUP_ENVIRONMENT_URL, 3);

            GUILayout.Space(2);
            GUILayout.Space(2);
            GUILayout.EndVertical();
        }

        private void FoundPreviousCertificateLocationPrompt()
        {
            var usePreviousCertificateOption = EditorUtility.DisplayDialogComplex(FOUND_PREVIOUS_CERTIFICATE_TITLE, FOUND_PREVIOUS_CERTIFICATE_MESSAGE,
                                                                           FOUND_PREVIOUS_CERTIFICATE_OK, FOUND_PREVIOUS_CERTIFICATE_CANCEL, FOUND_PREVIOUS_CERTIFICATE_ALT);
          
                switch (usePreviousCertificateOption)
                {
                    case 0: //Yes
                        MagicLeapSetup.CertificatePath = MagicLeapSetup.PreviousCertificatePath;
                        break;
                    case 1: //Cancel
                        EditorPrefs.SetBool(PREVIOUS_CERTIFICATE_PROMPT_KEY, false);
                        _showPreviousCertificatePrompt = false;
                        break;
                    case 2: //Browse
                        BrowseForCertificate();
                        break;
                }
          
        }

        private void ImportSdkPackage()
        {
            MagicLeapSetup.FailedToImportPackage += () =>
                                                    {
                                                        var failedToImportOptions = EditorUtility.DisplayDialogComplex(FAILED_TO_IMPORT_TITLE, FAILED_TO_IMPORT_MESSAGE,
                                                                                                                       FAILED_TO_IMPORT_OK, FAILED_TO_IMPORT_CANCEL, FAILED_TO_IMPORT_ALT);

                                                        switch (failedToImportOptions)
                                                        {
                                                            case 0: //Try again
                                                                ImportSdkPackage();
                                                                break;
                                                            case 1: //Stop
                                                                _currentApplyAllState = ApplyAllState.Done;
                                                                break;
                                                            case 2: //Go to documentation
                                                                Help.BrowseURL(SETUP_ENVIRONMENT_URL);
                                                                break;
                                                        }
                                                    };
            MagicLeapSetup.ImportPackageProcessFailed += () =>
                                                         {
                                                             _setupWindow.Focus();
                                                             _currentApplyAllState = ApplyAllState.Done;
                                                         };
            MagicLeapSetup.ImportPackageProcessCancelled += () =>
                                                            {
                                                                _setupWindow.Focus();
                                                                _currentApplyAllState = ApplyAllState.Done;
                                                            };
            MagicLeapSetup.ImportPackageProcessComplete += () =>
                                                           {
                                                               _setupWindow.Focus();
                                                           };

            MagicLeapSetup.ImportUnityPackage();
        }

        private void UpdateGraphicsApi()
        {
            MagicLeapSetup.UpdatedGraphicSettings += OnUpdateGraphicsApi;
            MagicLeapSetup.UpdateGraphicsSettings();

        }

        private void OnUpdateGraphicsApi(bool needsReset)
        {

            if (needsReset)
            {
                // If we have dirty scenes we need to save or discard changes before we restart editor.
                // Otherwise user will get a dialog later on where they can click cancel and put editor in a bad device state.
                var dirtyScenes = new List<Scene>();
                for (int i = 0; i < EditorSceneManager.sceneCount; ++i)
                {
                    var scene = EditorSceneManager.GetSceneAt(i);
                    if (scene.isDirty)
                        dirtyScenes.Add(scene);
                }

                bool restart = false;
                if (dirtyScenes.Count != 0)
                {
                    restart = ShowSaveAndQuitGraphicsApiDialogue(dirtyScenes);
                }
                else
                {
                    restart = ShowQuitGraphicsApiDialogue();
                }

                if (restart)
                {
                    UnityProjectSettingsUtility.RequestCloseAndRelaunchWithCurrentArguments();
                }
            }

            MagicLeapSetup.UpdatedGraphicSettings -= OnUpdateGraphicsApi;
        }

        private bool ShowSaveAndQuitGraphicsApiDialogue(List<Scene> dirtyScenes)
        {
            bool doRestart = false;
            var dialogComplex = EditorUtility.DisplayDialogComplex(CHANGE_EDITOR_GRAPHICS_API_TITLE, CHANGE_EDITOR_GRAPHICS_API_SAVE_MESSAGE,
                                                                   CHANGE_EDITOR_GRAPHICS_API_OK_SAVE, CHANGE_EDITOR_GRAPHICS_API_CANCEL, CHANGE_EDITOR_GRAPHICS_API_DONTSAVE_CANCEL);

            switch (dialogComplex)
            {
                case 0: //Save and Restart
                    doRestart = true;
                    for (int i = 0; i < dirtyScenes.Count; ++i)
                    {
                        var saved = EditorSceneManager.SaveScene(dirtyScenes[i]);
                        if (saved == false)
                        {
                            doRestart = false;
                        }
                    }
                    break;
                case 1: //Cancel
                    _currentApplyAllState = ApplyAllState.Done;
                    break;
                case 2: //Discard Changes and Restart
                    doRestart = true;
                    for (int i = 0; i < dirtyScenes.Count; ++i)
                        UnityProjectSettingsUtility.ClearSceneDirtiness(dirtyScenes[i]);

                    break;
            }

            return doRestart;
        }

        private bool ShowQuitGraphicsApiDialogue()
        {
            var dialogComplex = EditorUtility.DisplayDialog(CHANGE_EDITOR_GRAPHICS_API_TITLE, CHANGE_EDITOR_GRAPHICS_API_MESSAGE,
                                                                   CHANGE_EDITOR_GRAPHICS_API_OK, CHANGE_EDITOR_GRAPHICS_API_CANCEL);
           return dialogComplex;
        }
 
        public void DrawWaitingInfo()
        {
            var luminLogo = EditorGUIUtility.IconContent("BuildSettings.Lumin").image as Texture2D;

            GUILayout.Space(5);

            var content = new GUIContent(LOADING_TEXT, luminLogo);
            EditorGUILayout.LabelField(content, Styles.InfoTitleStyle);
            GUI.enabled = false;
            GUILayout.Space(5);
            GUI.backgroundColor = Color.white;
        }
    
        private void DrawFooter()
        {
            GUILayout.FlexibleSpace();

            if (_allAutoStepsComplete && MagicLeapSetup.ValidCertificatePath)
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Close", GUILayout.MinWidth(20)))
                {
                    Close();
                }
            }
            else
            {
                if (MagicLeapSetup.IsBusy)
                {
                    GUI.enabled = false;
                }
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("Apply All", GUILayout.MinWidth(20)))
                {
                    RunApplyAll();
                }

                GUI.enabled = true;
            }


            GUI.backgroundColor = Color.clear;
        }

        private void ApplyAll()
        {

            switch (_currentApplyAllState)
            {
                case ApplyAllState.SwitchBuildTarget:
                    if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Lumin)
                    {
                        Debug.Log(CHANGING_BUILD_PLATFORM_DEBUG);
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Lumin, BuildTarget.Lumin);
                    }

                    _currentApplyAllState = ApplyAllState.InstallLumin;
                    break;
                case ApplyAllState.InstallLumin:
                    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin)
                    {
                        if (!MagicLeapSetup.HasLuminInstalled)
                        {
                            Debug.Log(INSTALLING_LUMIN_SDK_DEBUG);
                            MagicLeapSetup.AddLuminSdkAndRefresh();
                        }

                        _currentApplyAllState = ApplyAllState.EnableXrPackage;
                    }

                    break;
                case ApplyAllState.EnableXrPackage:
                    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin && MagicLeapSetup.HasLuminInstalled)
                    {
                        if (!MagicLeapSetup.MagicLeapSettingEnabled)
                        {
                            Debug.Log(ENABLING_LUMIN_SDK_DEBUG);
                            EnableMagicLeapPlugin();
                        }

                        _currentApplyAllState = ApplyAllState.UpdateManifest;
                    }

                    break;
                case ApplyAllState.UpdateManifest:
                    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin && MagicLeapSetup.HasLuminInstalled && MagicLeapSetup.MagicLeapSettingEnabled)
                    {
                        if (!MagicLeapSetup.ManifestIsUpdated)
                        {
                            Debug.Log(UPDATING_MANIFEST_DEBUG);
                            UpdateManifest();
                        }

                        _currentApplyAllState = ApplyAllState.ChangeColorSpace;
                    }

                    break;

                case ApplyAllState.ChangeColorSpace:
                    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin && MagicLeapSetup.HasLuminInstalled && MagicLeapSetup.MagicLeapSettingEnabled && MagicLeapSetup.ManifestIsUpdated)
                    {
                        if (PlayerSettings.colorSpace != ColorSpace.Linear)
                        {
                            Debug.Log(UPDATING_COLORSPACE_DEBUG);
                            PlayerSettings.colorSpace = ColorSpace.Linear;
                        }

                        _currentApplyAllState = ApplyAllState.ImportSdkUnityPackage;
                    }

                    break;

                case ApplyAllState.ImportSdkUnityPackage:
                    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin && MagicLeapSetup.HasLuminInstalled && MagicLeapSetup.MagicLeapSettingEnabled && MagicLeapSetup.ManifestIsUpdated)
                    {
                        if (!MagicLeapSetup.ExtendedUnityPackageImported)
                        {
                            Debug.Log(IMPORTING_LUMIN_UNITYPACKAGE_DEBUG);
                            ImportSdkPackage();
                        }

                        _currentApplyAllState = ApplyAllState.ChangeGraphicsApi;
                    }

                    break;
               
                case ApplyAllState.ChangeGraphicsApi:
                    if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin && MagicLeapSetup.HasLuminInstalled && MagicLeapSetup.MagicLeapSettingEnabled && MagicLeapSetup.ManifestIsUpdated && MagicLeapSetup.ExtendedUnityPackageImported && PlayerSettings.colorSpace == ColorSpace.Linear)
                    {
                        if (!MagicLeapSetup.HasCorrectGraphicConfiguration)
                        {
                            Debug.Log(CHANGING_GRAPHICS_API_DEBUG);
                            MagicLeapSetup.UpdateGraphicsSettings();
                        }

                        _currentApplyAllState = ApplyAllState.Done;
                    }

                    break;
                case ApplyAllState.Done:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnEnable()
        {
            FullRefresh();
        }

        private void OnDisable()
        {
            EditorPrefs.SetBool(PREVIOUS_CERTIFICATE_PROMPT_KEY, true);
            EditorPrefs.SetBool(AutoShowEditorPrefKey, !MagicLeapSetup.ValidCertificatePath || !_allAutoStepsComplete);
        }

        private void OnFocus()
        {
            if (!MagicLeapSetup.CheckingAvailability)
            {
                MagicLeapSetup.RefreshVariables();
            }
        }

        public void OnGUI()
        {
            DrawHeader();
            _loading = (AssetDatabase.IsAssetImportWorkerProcess() || EditorApplication.isCompiling || MagicLeapSetup.IsBusy || EditorApplication.isUpdating);
       
            if (_loading)
            {
                DrawWaitingInfo();
            }
            else
            {
                DrawInfoBox();
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);
       
            if (CustomGuiContent.CustomButtons.DrawConditionButton(new GUIContent(LOCATE_SDK_FOLDER_LABEL), MagicLeapSetup.HasRootSDKPath, new GUIContent(CONDITION_MET_CHANGE_LABEL,MagicLeapSetup.SdkRoot), new GUIContent(LOCATE_SDK_FOLDER_BUTTON_LABEL), Styles.FixButtonStyle, false))
            {
                BrowseForSDK();
            }

            GUI.enabled = MagicLeapSetup.HasRootSDKPath && !_loading;

            if (CustomGuiContent.CustomButtons.DrawConditionButton(BUILD_SETTING_LABEL, EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin, CONDITION_MET_LABEL, FIX_SETTING_BUTTON_LABEL, Styles.FixButtonStyle))
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Lumin, BuildTarget.Lumin);
            }

            //Makes sure the user changes to the Lumin Build Target before being able to set the other options
            GUI.enabled = MagicLeapSetup.HasRootSDKPath && EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin && !_loading;

            if (CustomGuiContent.CustomButtons.DrawConditionButton(INSTALL_PLUGIN_LABEL, MagicLeapSetup.HasLuminInstalled, CONDITION_MET_LABEL, INSTALL_PLUGIN_BUTTON_LABEL, Styles.FixButtonStyle))
            {
                MagicLeapSetup.AddLuminSdkAndRefresh();
                Repaint();
              
            }

            //Check for Lumin SDK before allowing user to change sdk settings
            GUI.enabled = MagicLeapSetup.HasRootSDKPath && EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin && MagicLeapSetup.HasLuminInstalled && !_loading;


            if (CustomGuiContent.CustomButtons.DrawConditionButton(ENABLE_PLUGIN_SETTINGS_LABEL, MagicLeapSetup.MagicLeapSettingEnabled, CONDITION_MET_LABEL, ENABLE_PLUGIN_LABEL, Styles.FixButtonStyle))
            {
                EnableMagicLeapPlugin();
            }

            //Check that lumin is enabled before being able to import package and change color space
            GUI.enabled = MagicLeapSetup.MagicLeapSettingEnabled && !_loading;

            if (!_loading && CustomGuiContent.CustomButtons.DrawConditionButton(UPDATE_MANIFEST_LABEL, MagicLeapSetup.ManifestIsUpdated, CONDITION_MET_LABEL, UPDATE_MANIFEST_BUTTON_LABEL, Styles.FixButtonStyle))
            {
                UpdateManifest();
                Repaint();
            }

            if (CustomGuiContent.CustomButtons.DrawConditionButton(SET_CERTIFICATE_PATH_LABEL, MagicLeapSetup.ValidCertificatePath, new GUIContent(CONDITION_MET_CHANGE_LABEL,MagicLeapSetup.CertificatePath), SET_CERTIFICATE_PATH_BUTTON_LABEL, Styles.FixButtonStyle, SET_CERTIFICATE_HELP_TEXT, Get_CERTIFICATE_URL, false))
            {
                BrowseForCertificate();
            }

        

            if (CustomGuiContent.CustomButtons.DrawConditionButton(COLOR_SPACE_LABEL, PlayerSettings.colorSpace == ColorSpace.Linear, CONDITION_MET_LABEL, FIX_SETTING_BUTTON_LABEL, Styles.FixButtonStyle))
            {
                PlayerSettings.colorSpace = ColorSpace.Linear;
                Repaint();
            }

          
            if (CustomGuiContent.CustomButtons.DrawConditionButton(IMPORT_LUMIN_SDK_UNITYPACKAGE, MagicLeapSetup.ExtendedUnityPackageImported, CONDITION_MET_LABEL, IMPORT_LUMIN_SDK_UNITYPACKAGE_BUTTON, Styles.FixButtonStyle))
            {
                ImportSdkPackage();
                Repaint();
            }

    
            if (CustomGuiContent.CustomButtons.DrawConditionButton(SET_CORRECT_GRAPHICS_API_LABEL, MagicLeapSetup.HasCorrectGraphicConfiguration, CONDITION_MET_LABEL, SET_CORRECT_GRAPHICS_BUTTON_LABEL, Styles.FixButtonStyle))
            {
                UpdateGraphicsApi();
                Repaint();
            }

            GUI.backgroundColor = Color.clear;

            GUILayout.EndVertical();
            GUILayout.Space(30);
            DrawHelpLinks();
            DrawFooter();

            if (_currentApplyAllState != ApplyAllState.Done && !_loading)
            {
                ApplyAll();
               
            }
        }

        private void OnInspectorUpdate()
        {
        
            if (!_loading && MagicLeapSetup.MagicLeapSettingEnabled && !MagicLeapSetup.ValidCertificatePath && _showPreviousCertificatePrompt && !string.IsNullOrWhiteSpace(MagicLeapSetup.PreviousCertificatePath))
            {
                FoundPreviousCertificateLocationPrompt();
            }
        }
    }
}
