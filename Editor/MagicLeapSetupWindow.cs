/* Copyright (C) 2021 Adrian Babilinski
* You may use, distribute and modify this code under the
* terms of the MIT License
*/


using System.IO;
using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.Setup;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor
{
   
    public class MagicLeapSetupWindow : EditorWindow
    {
    #region EDITOR PREFS

        private const string MAGIC_LEAP_DEFINES_SYMBOL = "MAGICLEAP";

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

     

        private const string GETTING_STARTED_HELP_TEXT = "Read the getting started guide";

    #endregion

    #region HELP URLS

        internal const string Get_CERTIFICATE_URL = "https://developer.magicleap.com/en-us/learn/guides/developer-certificates";
        internal const string SETUP_ENVIRONMENT_URL = "https://developer.magicleap.com/en-us/learn/guides/set-up-development-environment#installing-lumin-sdk-packages";
        internal const string GETTING_STARTED_URL = "https://developer.magicleap.com/en-us/learn/guides/get-started-developing-in-unity";

    #endregion

   
        internal static MagicLeapSetupWindow _setupWindow;

        private static bool _loading;
        private static bool _showPreviousCertificatePrompt;

        private static readonly SetSdkFolderSetupStep _setSdkFolderSetupStep = new SetSdkFolderSetupStep();
        private static readonly BuildTargetSetupStep _buildTargetSetupStep = new BuildTargetSetupStep();
        private static readonly EnablePluginSetupStep _enablePluginSetupStep = new EnablePluginSetupStep();
        private static readonly UpdateManifestSetupStep _updateManifestSetupStep = new UpdateManifestSetupStep();
        private static readonly SetCertificateSetupStep _setCertificateSetupStep = new SetCertificateSetupStep();
        private static readonly ImportMagicLeapSdkSetupStep _importMagicLeapSdkSetupStep = new ImportMagicLeapSdkSetupStep();
        private static readonly ColorSpaceSetupStep _colorSpaceSetupStep = new ColorSpaceSetupStep();
        private static readonly UpdateGraphicsApiSetupStep _updateGraphicsApiSetupStep = new UpdateGraphicsApiSetupStep();





        private void Awake()
        {
            EditorApplication.UnlockReloadAssemblies();
            _showPreviousCertificatePrompt = true;
        }

        private void OnEnable()
        {
            var temp = new SetupData();
            FullRefresh();
            RefreshSteps();
            EditorPrefs.SetBool(EditorKeyUtility.WindowClosedEditorPrefKey, false);
            if (EditorPrefs.GetBool($"{Application.dataPath}-DeletedFoldersReset", false) && EditorPrefs.GetBool($"{Application.dataPath}-Install", false))
            {

                _importMagicLeapSdkSetupStep.ImportSdkFromUnityPackageManager();
                EditorPrefs.SetBool($"{Application.dataPath}-DeletedFoldersReset", false);
                EditorPrefs.SetBool($"{Application.dataPath}-Install", false);
            }
        }


        private void OnDisable()
        {
            EditorPrefs.SetBool(EditorKeyUtility.PreviousCertificatePrompt, true);
            EditorPrefs.SetBool(EditorKeyUtility.AutoShowEditorPrefKey, !SetCertificateSetupStep.ValidCertificatePath || !MagicLeapSetupAutoRun._allAutoStepsComplete || !ImportMagicLeapSdkSetupStep.HasCompatibleMagicLeapSdk);
        }

        private void OnDestroy()
        {
            EditorPrefs.SetBool(EditorKeyUtility.WindowClosedEditorPrefKey, true);
   
            FullRefresh();
            MagicLeapSetupAutoRun.Stop();
        }
        
        public void OnGUI()
        {
           
            DrawHeader();
            _loading = AssetDatabase.IsAssetImportWorkerProcess() || EditorApplication.isCompiling || EditorApplication.isUpdating;
            // if (!_magicLeapSetupData.Busy && !_loading)
            // {
            //    
            //     _magicLeapSetupData.RefreshVariables();
            // }
            
            /*if (_magicLeapSetupData.Loading)
            {*/
                //DrawWaitingInfo();
            /*}
            else
            {*/
                DrawInfoBox();
            /*}*/

            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                GUILayout.Space(5);
                if (_setSdkFolderSetupStep.Draw())
                {
                    Repaint();
                }

                if (_buildTargetSetupStep.Draw())
                {
                    Repaint();
                }

                if (_importMagicLeapSdkSetupStep.Draw())
                {
                    Repaint();
                }

                if (_enablePluginSetupStep.Draw())
                {
                    Repaint();
                }
                
                if (_updateManifestSetupStep.Draw())
                {
                    Repaint();
                }

                if (_setCertificateSetupStep.Draw())
                {
                    Repaint();
                }

                if (_colorSpaceSetupStep.Draw())
                {
                    Repaint();
                }

              
                
                if (_updateGraphicsApiSetupStep.Draw())
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
            RefreshSteps();
            ImportMagicLeapSdkSetupStep.CheckForMagicLeapSdkPackage();
               
        }

     
        private void OnInspectorUpdate()
        {
        
            if (!_loading && EnablePluginSetupStep.LuminSettingEnabled && !SetCertificateSetupStep.ValidCertificatePath && _showPreviousCertificatePrompt && !string.IsNullOrWhiteSpace(SetCertificateSetupStep.PreviousCertificatePath))
            {
               _setCertificateSetupStep.FoundPreviousCertificateLocationPrompt(OnChoseSelected);



               void OnChoseSelected(bool showAgain)
               {
                   _showPreviousCertificatePrompt = showAgain;
               }
            }

            Repaint();
        }


        private static void RefreshSteps()
        {
            _setSdkFolderSetupStep.Refresh();
            _buildTargetSetupStep.Refresh();
            _enablePluginSetupStep.Refresh();
            _updateManifestSetupStep.Refresh();
            _setCertificateSetupStep.Refresh();
            _importMagicLeapSdkSetupStep.Refresh();
            _colorSpaceSetupStep.Refresh();
            _updateGraphicsApiSetupStep.Refresh();
        }

        private static void Open()
        {
        
            EditorApplication.projectChanged += SetupData.UpdateDefineSymbols;
            MagicLeapSetupAutoRun.CheckLastAutoSetupState();
            _showPreviousCertificatePrompt = EditorPrefs.GetBool(EditorKeyUtility.PreviousCertificatePrompt, true);


                _setupWindow = GetWindow<MagicLeapSetupWindow>(false, WINDOW_TITLE_LABEL);
                _setupWindow.minSize = new Vector2(350, 520);
                _setupWindow.maxSize = new Vector2(350, 580);
                EditorApplication.projectChanged += FullRefresh;
           

        }

        public static void ForceOpen()
        {
            if (EditorPrefs.GetBool(EditorKeyUtility.WindowClosedEditorPrefKey, false))
            {
                return;
            }
            Open();
        }

        [MenuItem(WINDOW_PATH)]
        public static void MenuOpen()
        {
            EditorPrefs.SetBool(EditorKeyUtility.WindowClosedEditorPrefKey, false);
            Open();
        }

 
      

        internal static void FullRefresh()
        {
            ImportMagicLeapSdkSetupStep.CheckForMagicLeapSdkPackage();
            SetupData.UpdateDefineSymbols();
            RefreshSteps();

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
            if (MagicLeapSetupAutoRun._allAutoStepsComplete && SetCertificateSetupStep.ValidCertificatePath)
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
    



    #endregion
    }
}
