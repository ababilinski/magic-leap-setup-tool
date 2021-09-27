/* Copyright (C) 2021 Adrian Babilinski
* You may use, distribute and modify this code under the
* terms of the MIT License
*/


using System.IO;
using MagicLeapSetupTool.Editor.ScriptableObjects;
using MagicLeapSetupTool.Editor.Setup;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor
{
   
    public class MagicLeapSetupWindow : EditorWindow
    {
    #region EDITOR PREFS

        internal const string PREVIOUS_CERTIFICATE_PROMPT_KEY = "PREVIOUS_CERTIFICATE_PROMPT_KEY";
        internal const string MAGIC_LEAP_SETUP_POSTFIX_KEY = "MAGIC_LEAP_SETUP_KEY";
        internal const string MAGIC_LEAP_SETUP_CLOSED_WINDOW_KEY = "MAGIC_LEAP_SETUP_CLOSED_WINDOW_KEY";

    #endregion

    #region TEXT AND LABELS

        private const string WINDOW_PATH = "Magic Leap/Project Setup Utility";
        private const string WINDOW_TITLE_LABEL = "Magic Leap Project Setup";
        private const string TITLE_LABEL = "MAGIC LEAP";
        private const string SUBTITLE_LABEL = "PROJECT SETUP";
        private const string HELP_BOX_TEXT = "Required settings For Lumin SDK";
        private const string LOADING_TEXT = "   Loading and Importing...";

        private const string LINKS_TITLE = "Helpful Links:";


        private const string SET_CERTIFICATE_HELP_TEXT = "Get a developer certificate";


        private const string FAILED_TO_IMPORT_HELP_TEXT = "Setup the developer environment";

        private const string FOUND_PREVIOUS_CERTIFICATE_TITLE = "Found Previously Used Developer Certificate";
        private const string FOUND_PREVIOUS_CERTIFICATE_MESSAGE = "Magic Leap Setup has found a previously used developer certificate. Would you like to use it in this project?";
        private const string FOUND_PREVIOUS_CERTIFICATE_OK = "Yes";
        private const string FOUND_PREVIOUS_CERTIFICATE_CANCEL = "Cancel";
        private const string FOUND_PREVIOUS_CERTIFICATE_ALT = "Browse For Certificate";

        private const string GETTING_STARTED_HELP_TEXT = "Read the getting started guide";

    #endregion

    #region HELP URLS

        internal const string Get_CERTIFICATE_URL = "https://developer.magicleap.com/en-us/learn/guides/developer-certificates";
        internal const string SETUP_ENVIRONMENT_URL = "https://developer.magicleap.com/en-us/learn/guides/set-up-development-environment#installing-lumin-sdk-packages";
        internal const string GETTING_STARTED_URL = "https://developer.magicleap.com/en-us/learn/guides/get-started-developing-in-unity";

    #endregion

        internal static MagicLeapSetupWindow _setupWindow;
        private static bool _showing;
        private static bool _loading;
        private static bool _showPreviousCertificatePrompt;
        private static MagicLeapSetupDataScriptableObject _magicLeapSetupData;
        private SetSdkFolderSetupStep _setSdkFolderSetupStep = new SetSdkFolderSetupStep();
        private BuildTargetSetupStep _buildTargetSetupStep = new BuildTargetSetupStep();
        private EnablePluginSetupStep _enablePluginSetupStep = new EnablePluginSetupStep();
        private UpdateManifestSetupStep _updateManifestSetupStep = new UpdateManifestSetupStep();
        private SetCertificateSetupStep _setCertificateSetupStep = new SetCertificateSetupStep();
        private ImportMagicLeapSdkSetupStep _importMagicLeapSdkSetupStep = new ImportMagicLeapSdkSetupStep();
        private ColorSpaceSetupStep _colorSpaceSetupStep = new ColorSpaceSetupStep();
        private UpdateGraphicsApiSetupStep _updateGraphicsApiSetupStep = new UpdateGraphicsApiSetupStep();



        private static string WindowClosedEditorPrefKey
        {
            get
            {
                var projectKey = UnityProjectSettingsUtility.GetProjectKey();
                var path = Path.GetFullPath(Application.dataPath);
                return $"{MAGIC_LEAP_SETUP_CLOSED_WINDOW_KEY}_[{projectKey}]-[{path}]";
            }
        }
        private static string AutoShowEditorPrefKey
        {
            get
            {
                var projectKey = UnityProjectSettingsUtility.GetProjectKey();
                var path = Path.GetFullPath(Application.dataPath);
                return $"{MAGIC_LEAP_SETUP_POSTFIX_KEY}_[{projectKey}]-[{path}]";
            }
        }

        private void Awake()
        {
            _magicLeapSetupData = MagicLeapSetupDataScriptableObject.Instance;
            EditorApplication.UnlockReloadAssemblies();
            _showPreviousCertificatePrompt = true;
        }

        private void OnEnable()
        {
           
            FullRefresh();
            _showing = true;
            EditorPrefs.SetBool(WindowClosedEditorPrefKey, false);
            if (EditorPrefs.GetBool($"{Application.dataPath}-DeletedFoldersReset", false) && EditorPrefs.GetBool($"{Application.dataPath}-Install", false))
            {

                _magicLeapSetupData = MagicLeapSetupDataScriptableObject.Instance;
                _importMagicLeapSdkSetupStep.ImportSdkFromUnityPackageManager(_magicLeapSetupData);
                EditorPrefs.SetBool($"{Application.dataPath}-DeletedFoldersReset", false);
                EditorPrefs.SetBool($"{Application.dataPath}-Install", false);
                _magicLeapSetupData.IsRestarting = false;
            }
        }


        private void OnDisable()
        {
            EditorPrefs.SetBool(PREVIOUS_CERTIFICATE_PROMPT_KEY, true);
            EditorPrefs.SetBool(AutoShowEditorPrefKey, !_magicLeapSetupData.ValidCertificatePath || !MagicLeapSetupAutoRun._allAutoStepsComplete || !_magicLeapSetupData.HasCompatibleMagicLeapSdk);
        }

        private void OnDestroy()
        {
            EditorPrefs.SetBool(WindowClosedEditorPrefKey, true);

            EditorApplication.quitting -= OnQuit;
            _showing = false;
            FullRefresh();
            MagicLeapSetupAutoRun.Stop();
        }
        
        public void OnGUI()
        {
           
            DrawHeader();
            _loading = AssetDatabase.IsAssetImportWorkerProcess() || EditorApplication.isCompiling || _magicLeapSetupData.Busy || EditorApplication.isUpdating;
            _magicLeapSetupData.Loading = _loading;
            if (_magicLeapSetupData.Loading)
            {
                DrawWaitingInfo();
            }
            else
            {
                DrawInfoBox();
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Space(5);
                if (_setSdkFolderSetupStep.Draw(_magicLeapSetupData))
                {
                    Repaint();
                }

                if (_buildTargetSetupStep.Draw(_magicLeapSetupData))
                {
                    Repaint();
                }

                if (_importMagicLeapSdkSetupStep.Draw(_magicLeapSetupData))
                {
                    Repaint();
                }

                if (_enablePluginSetupStep.Draw(_magicLeapSetupData))
                {
                    Repaint();
                }
                
                if (_updateManifestSetupStep.Draw(_magicLeapSetupData))
                {
                    Repaint();
                }

                if (_setCertificateSetupStep.Draw(_magicLeapSetupData))
                {
                    Repaint();
                }

                if (_colorSpaceSetupStep.Draw(_magicLeapSetupData))
                {
                    Repaint();
                }

              
                
                if (_updateGraphicsApiSetupStep.Draw(_magicLeapSetupData))
                {
                    Repaint();
                }

                GUI.backgroundColor = Color.clear;
            }
            GUILayout.EndVertical();
            GUILayout.Space(30);
            DrawHelpLinks();
            DrawFooter();
            MagicLeapSetupAutoRun.Tick();
        }

        private void OnFocus()
        {
                _magicLeapSetupData = MagicLeapSetupDataScriptableObject.Instance;
                if(!_magicLeapSetupData.Loading && !_magicLeapSetupData.Busy)
                {
                    _magicLeapSetupData.RefreshVariables();
                }
        }

     
        private void OnInspectorUpdate()
        {
        
            if (!_loading && _magicLeapSetupData.LuminSettingEnabled && !_magicLeapSetupData.ValidCertificatePath && _showPreviousCertificatePrompt && !string.IsNullOrWhiteSpace(_magicLeapSetupData.PreviousCertificatePath))
            {
                FoundPreviousCertificateLocationPrompt();
            }

            Repaint();
        }

     

      
        private static void Open()
        {
        
            EditorApplication.quitting += OnQuit;
       
          

            _magicLeapSetupData = MagicLeapSetupDataScriptableObject.Instance;
        
            if (_magicLeapSetupData == null)
            {
                return;
            }

            EditorApplication.projectChanged += _magicLeapSetupData.UpdateDefineSymbols;
            MagicLeapSetupAutoRun.CheckLastAutoSetupState();
            _showPreviousCertificatePrompt = EditorPrefs.GetBool(PREVIOUS_CERTIFICATE_PROMPT_KEY, true);
            if (!_showing)
            {
              
                _setupWindow = GetWindow<MagicLeapSetupWindow>(false, WINDOW_TITLE_LABEL);
                _setupWindow.minSize = new Vector2(350, 520);
                _setupWindow.maxSize = new Vector2(350, 580);
                EditorApplication.projectChanged += FullRefresh;
            }

        }

        public static void ForceOpen()
        {
            if (EditorPrefs.GetBool(WindowClosedEditorPrefKey, false))
            {
                return;
            }
            Open();
        }

        [MenuItem(WINDOW_PATH)]
        public static void MenuOpen()
        {
            EditorPrefs.SetBool(WindowClosedEditorPrefKey, false);
            Open();
        }

        private static void OnQuit()
        {
            EditorPrefs.SetBool(WindowClosedEditorPrefKey, false);
        }

      

        internal static void FullRefresh()
        {
            _magicLeapSetupData = MagicLeapSetupDataScriptableObject.Instance;
            if (!_magicLeapSetupData.Loading && !_magicLeapSetupData.Busy)
            {
               // _magicLeapSetupData.RefreshVariables();
                _magicLeapSetupData.CheckSDKAvailability();
            }
          
        }


    #region Draw Window Controls

        private void DrawHeader()
        {
            GUILayout.Space(5);
            GUILayout.BeginVertical();
            {
                EditorGUILayout.LabelField(TITLE_LABEL, Styles.TitleStyle);
                EditorGUILayout.LabelField(SUBTITLE_LABEL, Styles.TitleStyle);
                GUILayout.EndVertical();
            }
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

        private void DrawHelpLinks()
        {
            var currentGUIEnabledStatus = GUI.enabled;
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Space(2);
                EditorGUILayout.LabelField(LINKS_TITLE, Styles.HelpTitleStyle);
                CustomGuiContent.DisplayLink(GETTING_STARTED_HELP_TEXT, GETTING_STARTED_URL, 3);
                CustomGuiContent.DisplayLink(SET_CERTIFICATE_HELP_TEXT, Get_CERTIFICATE_URL, 3);
                CustomGuiContent.DisplayLink(FAILED_TO_IMPORT_HELP_TEXT, SETUP_ENVIRONMENT_URL, 3);

                GUILayout.Space(2);
                GUILayout.Space(2);
            }
            GUILayout.EndVertical();
            GUI.enabled = currentGUIEnabledStatus;
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
            var currentGUIEnabledStatus = GUI.enabled;
            GUI.enabled = !_loading;
            if (MagicLeapSetupAutoRun._allAutoStepsComplete && _magicLeapSetupData.ValidCertificatePath)
            {
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Close", GUILayout.MinWidth(20)))
                {
                    Close();
                }
            }
            else
            {
                GUI.backgroundColor = Color.yellow;
                if (GUILayout.Button("Apply All", GUILayout.MinWidth(20)))
                {
                    MagicLeapSetupAutoRun.RunApplyAll();
                }
            }

            GUI.enabled = currentGUIEnabledStatus;
            GUI.backgroundColor = Color.clear;
        }

    #endregion


    #region Prompts
    

        private void FoundPreviousCertificateLocationPrompt()
        {
            var usePreviousCertificateOption = EditorUtility.DisplayDialogComplex(FOUND_PREVIOUS_CERTIFICATE_TITLE, FOUND_PREVIOUS_CERTIFICATE_MESSAGE,
                                                                                  FOUND_PREVIOUS_CERTIFICATE_OK, FOUND_PREVIOUS_CERTIFICATE_CANCEL, FOUND_PREVIOUS_CERTIFICATE_ALT);

            switch (usePreviousCertificateOption)
            {
                case 0: //Yes
                    _magicLeapSetupData.CertificatePath = _magicLeapSetupData.PreviousCertificatePath;
                    AssetDatabase.SaveAssets();
                
                    break;
                case 1: //Cancel
                    EditorPrefs.SetBool(PREVIOUS_CERTIFICATE_PROMPT_KEY, false);
                    _showPreviousCertificatePrompt = false;
                    break;
                case 2: //Browse
                    _setCertificateSetupStep.Execute(_magicLeapSetupData);
                    break;
            }
        }

    #endregion
    }
}
