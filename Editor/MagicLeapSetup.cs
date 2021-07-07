﻿/* Copyright (C) 2021 Adrian Babilinski
* You may use, distribute and modify this code under the
* terms of the MIT License
*/

using System;
using System.IO;

using MagicLeapSetupTool.Editor.Templates;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MagicLeapSetupTool.Editor
{
    public static class MagicLeapSetup
    {
        private const string TEST_FOR_ML_SCRIPT = "UnityEngine.XR.MagicLeap.MLInput"; // Test this assembly. If it does not exist. The package is not imported. 
        private const string MAGIC_LEAP_DEFINES_SYMBOL = "MAGICLEAP";
        private const string DEFINES_SYMBOL_SEARCH_TARGET = "UnityEngine.XR.MagicLeap"; //Type to search for to enable MAGICLEAP defines symbol
        private const string MAGIC_LEAP_UNITYPACKAGE = "MAGICLEAP";
        private const string PACKAGE_PATH = "../../tools/unity/v{0}/MagicLeap.unitypackage"; // {0} is the SDK version


        private const string IMPORTING_PACKAGE_TEXT = "importing [{0}]";                     // {0} is the path  to the unity package
        private const string CANNOT_FIND_PACKAGE_TEXT = "Could not find Unity Package at path [{0}].\n SDK Path: [{1}]\nSDK Version: [{2}]";
        private const string SDK_NOT_INSTALLED_TEXT = "Cannot preform that action while the SDK is not installed in the project";
        private const string FAILED_TO_EXECUTE_ERROR = "Failed to execute [{0}]";                                                                                     //0 is method/action name
        private const string ENABLE_LUMIN_FINISHED_UNSUCCESSFULLY_WARNING = "Unsuccessful call:[{0}]. action finished, but Lumin XR Settings are still not enabled."; //0 is method/action name
        private const string FAILED_TO_IMPORT_PACKAGE_ERROR = "Failed To Import [{0}.unitypackage] : {1}"; //[0] is package name | [1] is Unity error message
     
        private static string _certificatePath="";

        private static int _busyCounter; //Add value when task starts and remove it when finished
        public static Action FailedToImportPackage;
        public static Action ImportPackageProcessComplete;
        public static Action ImportPackageProcessCancelled;
        public static Action ImportPackageProcessFailed;
        public static Action<bool> UpdatedGraphicSettings;
        public static bool IsBusy => _busyCounter > 0;

        public static bool HasLuminInstalled
        {
            get
            {
                #if MAGICLEAP
                return true;
                #else
                return false;
                #endif
            }
        }
        public static bool CheckingAvailability { get; private set; }
        public static bool ExtendedUnityPackageImported { get; private set; }
        public static bool HasCorrectGraphicConfiguration { get; private set; }
        public static bool MagicLeapSettingEnabled { get; private set; }
        public static bool ValidCertificatePath { get; private set; }
        public static int SdkApiLevel { get; private set; }
        

        // "../../tools/unity/v{0}/MagicLeap.unitypackage"
        public static string GetUnityPackagePath => Path.Combine(LuminPackageUtility.GetSDKPath(), string.Format(PACKAGE_PATH, LuminPackageUtility.GetSdkVersion()));

        public static string CertificatePath
        {
            get
            {
                if (!HasLuminInstalled || (!string.IsNullOrEmpty(_certificatePath) && File.Exists(_certificatePath)))
                {
                    ValidCertificatePath = HasLuminInstalled && !string.IsNullOrEmpty(_certificatePath) && File.Exists(_certificatePath);
                    return _certificatePath;
             
                }
                else
                {
                    _certificatePath = UnityProjectSettingsUtility.Lumin.GetInternalCertificatePath();
                    ValidCertificatePath = HasLuminInstalled && !string.IsNullOrEmpty(_certificatePath) && File.Exists(_certificatePath);
                    return _certificatePath;
                }

            }
            set
            {
                UnityProjectSettingsUtility.Lumin.SetInternalCertificatePath(value);
                _certificatePath = value;
                ValidCertificatePath = HasLuminInstalled && !string.IsNullOrEmpty(_certificatePath) && File.Exists(_certificatePath);
            }
        }

        public static bool ManifestIsUpdated
        {
            get
            {
#if MAGICLEAP
                if (LuminPackageUtility.MagicLeapManifest == null)
                {
                    return false;
                }

                return LuminPackageUtility.MagicLeapManifest.minimumAPILevel == SdkApiLevel;
#else
                return false;
#endif
            }
        }

        public static void UpdateManifest()
        {
            _busyCounter++;
#if MAGICLEAP
            Debug.Log($"Setting SDK Version To: {SdkApiLevel}");
            RefreshVariables();
            LuminPackageUtility.MagicLeapManifest.minimumAPILevel = SdkApiLevel;
            var serializedObject = new SerializedObject(LuminPackageUtility.MagicLeapManifest);
            var priv_groups = serializedObject.FindProperty("m_PrivilegeGroups");
        
            for (var i = 0; i < priv_groups.arraySize; i++)
            {
                var group = priv_groups.GetArrayElementAtIndex(i);
               

                var privs = group.FindPropertyRelative("m_Privileges");
                for (var j = 0; j < privs.arraySize; j++)
                {
                    var priv = privs.GetArrayElementAtIndex(j);
                    var enabled = priv.FindPropertyRelative("m_Enabled");
                    var name = priv.FindPropertyRelative("m_Name").stringValue;
                    if (DefaultPackageTemplate.DEFAULT_PRIVILEGES.Contains(name))
                    {
                        enabled.boolValue = true;
                    }
                }
            }
            Debug.Log("Updated Privileges!");

            serializedObject.ApplyModifiedProperties();
          

            serializedObject.Update();
#endif
            _busyCounter--;
        }

        public static void RefreshVariables()
        {
            _busyCounter++;
#if MAGICLEAP
            CertificatePath = UnityProjectSettingsUtility.Lumin.GetInternalCertificatePath();
         
            SdkApiLevel = LuminPackageUtility.GetSdkApiLevel();
            MagicLeapSettingEnabled = LuminPackageUtility.IsLuminXREnabled();
            ExtendedUnityPackageImported = TypeUtility.FindTypeByPartialName(TEST_FOR_ML_SCRIPT) != null;
            ValidCertificatePath = !string.IsNullOrEmpty(CertificatePath) && File.Exists(CertificatePath);

#endif
           
            HasCorrectGraphicConfiguration = CorrectGraphicsConfiguration();
            _busyCounter--;
          
        }

        public static void CheckSDKAvailability()
        {

            UpdateDefineSymbols();
            LuminPackageUtility.CheckForLuminSdkRequestFinished = null;
            RefreshVariables();
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin)
            {
                CheckingAvailability = true;
                _busyCounter++;
                LuminPackageUtility.CheckForLuminSdkRequestFinished += OnCheckForLuminRequestFinished;
                LuminPackageUtility.CheckForLuminSdkPackage();
            }

       
        }

        public static void AddLuminSdkAndRefresh()
        {
            _busyCounter++;
            LuminPackageUtility.AddLuminPackageRequestFinished += OnAddLuminPackageRequestFinished;
            LuminPackageUtility.AddLuminSdkPackage();
        }

        private static void UpdateDefineSymbols()
        {
            if (DefineSymbolsUtility.TypeExists(DEFINES_SYMBOL_SEARCH_TARGET))
            {

                if (!DefineSymbolsUtility.ContainsDefineSymbol(MAGIC_LEAP_DEFINES_SYMBOL))
                {

                    DefineSymbolsUtility.AddDefineSymbol(MAGIC_LEAP_DEFINES_SYMBOL);

                }

            }
            else
            {
                if (!DefineSymbolsUtility.ContainsDefineSymbol(MAGIC_LEAP_DEFINES_SYMBOL))
                {

                    DefineSymbolsUtility.RemoveDefineSymbol(MAGIC_LEAP_DEFINES_SYMBOL);
                }
            }
        }
        private static void OnAddLuminPackageRequestFinished(bool success)
        {
            if (success)
            {
                if (!MagicLeapSettingEnabled)
                {
                    CheckSDKAvailability();
                }
            }
            else
            {
                Debug.LogError(string.Format(FAILED_TO_EXECUTE_ERROR,"Add Lumin Sdk Package"));
            }

            _busyCounter--;
        }

        public static void EnableLuminXRPluginAndRefresh()
        {
            _busyCounter++;
            LuminPackageUtility.EnableLuminXRFinished += OnEnableMagicLeapPluginFinished;
            LuminPackageUtility.EnableLuminXRPlugin();
        }

        private static void OnEnableMagicLeapPluginFinished(bool success)
        {
            if (success)
            {
                MagicLeapSettingEnabled = LuminPackageUtility.IsLuminXREnabled();
                if (!MagicLeapSettingEnabled)
                {
                    Debug.LogWarning(string.Format(ENABLE_LUMIN_FINISHED_UNSUCCESSFULLY_WARNING, "Enable Lumin XR action"));
                }
            }
            else
            {
                Debug.LogError(string.Format(FAILED_TO_EXECUTE_ERROR, "Enable Lumin XR Package"));
                
            }

            _busyCounter--;
            LuminPackageUtility.EnableLuminXRFinished -= OnEnableMagicLeapPluginFinished;
        }

        public static void ImportUnityPackage()
        {
       
            if (HasLuminInstalled)
            {
                _busyCounter++;
                var unityPackagePath = GetUnityPackagePath;
                if (File.Exists(unityPackagePath))
                {
                    // "importing [{0}]"
                    Debug.Log(string.Format(IMPORTING_PACKAGE_TEXT, Path.GetFullPath(unityPackagePath)));
                    AssetDatabase.importPackageCompleted += ImportPackageCompleted;
                    AssetDatabase.importPackageCancelled += ImportPackageCancelled;
                    AssetDatabase.importPackageFailed += ImportPackageFailed;
                    AssetDatabase.ImportPackage(Path.GetFullPath(unityPackagePath), true);
                
                }
                else
                {
                    FailedToImportPackage?.Invoke();
                    // "Could not find Unity Package at path [{0}].\n SDK Path: [{1}]\nSDK Version: [{2}]"
                    Debug.LogError(string.Format(CANNOT_FIND_PACKAGE_TEXT, Path.GetFullPath(unityPackagePath), LuminPackageUtility.GetSDKPath(), LuminPackageUtility.GetSdkVersion()));
                    FailedToImportPackage = null;
                }
            }
            else
            {
                Debug.LogError(SDK_NOT_INSTALLED_TEXT);
            }
        }

        private static void ImportPackageFailed(string packageName, string errorMessage)
        {
            if (packageName.ToUpper().Contains(MAGIC_LEAP_UNITYPACKAGE))
            {
                AssetDatabase.importPackageCompleted -= ImportPackageCompleted;
                AssetDatabase.importPackageCancelled -= ImportPackageCancelled;
                AssetDatabase.importPackageFailed -= ImportPackageFailed;
                Debug.LogError(string.Format(FAILED_TO_IMPORT_PACKAGE_ERROR, packageName, errorMessage));
                ImportPackageProcessFailed?.Invoke();
                ImportPackageProcessCancelled = null;
                ImportPackageProcessComplete = null;
                ImportPackageProcessFailed = null;
                _busyCounter--;
            
            }

          
        }

        private static void ImportPackageCancelled(string packageName)
        {
            if (packageName.ToUpper().Contains(MAGIC_LEAP_UNITYPACKAGE))
            {
                AssetDatabase.importPackageCompleted -= ImportPackageCompleted;
                AssetDatabase.importPackageCancelled -= ImportPackageCancelled;
                AssetDatabase.importPackageFailed -= ImportPackageFailed;
                ImportPackageProcessCancelled?.Invoke();
                ImportPackageProcessCancelled = null;
                ImportPackageProcessComplete = null;
                ImportPackageProcessFailed = null;
                _busyCounter--;
            }

            
        }

        private static void ImportPackageCompleted(string packageName)
        {
            if(packageName.ToUpper().Contains(MAGIC_LEAP_UNITYPACKAGE))
            {
                AssetDatabase.importPackageCompleted -= ImportPackageCompleted;
                AssetDatabase.importPackageCancelled -= ImportPackageCancelled;
                AssetDatabase.importPackageFailed -= ImportPackageFailed;
                ImportPackageProcessComplete?.Invoke();
                ImportPackageProcessCancelled = null;
                ImportPackageProcessComplete = null;
                ImportPackageProcessFailed = null;
                _busyCounter--;

            }
     
         
        }

        public static void UpdateGraphicsSettings()
        {
            _busyCounter++;
          
           bool standaloneWindowsResetRequired = UnityProjectSettingsUtility.SetGraphicsApi(BuildTarget.StandaloneWindows, GraphicsDeviceType.OpenGLCore, 0);
           bool standaloneWindows64ResetRequired = UnityProjectSettingsUtility.SetGraphicsApi(BuildTarget.StandaloneWindows64, GraphicsDeviceType.OpenGLCore, 0);
           bool standaloneOSXResetRequired = UnityProjectSettingsUtility.SetGraphicsApi(BuildTarget.StandaloneOSX, GraphicsDeviceType.OpenGLCore, 0);
           bool standaloneLinuxResetRequired = UnityProjectSettingsUtility.SetGraphicsApi(BuildTarget.StandaloneLinux64, GraphicsDeviceType.OpenGLCore, 0);


            UnityProjectSettingsUtility.SetAutoGraphicsApi(BuildTarget.StandaloneWindows, false);
            UnityProjectSettingsUtility.SetAutoGraphicsApi(BuildTarget.StandaloneWindows64, false);
            UnityProjectSettingsUtility.SetAutoGraphicsApi(BuildTarget.StandaloneOSX, false);
            UnityProjectSettingsUtility.SetAutoGraphicsApi(BuildTarget.StandaloneLinux64, false);
            RefreshVariables();

            if (standaloneWindowsResetRequired || standaloneWindows64ResetRequired || standaloneOSXResetRequired || standaloneLinuxResetRequired)
            {
                UpdatedGraphicSettings?.Invoke(true);
            }
            else
            {
                UpdatedGraphicSettings?.Invoke(false);
            }

   

            
            _busyCounter--;
        }

        private static bool CorrectGraphicsConfiguration()
        {
            _busyCounter++;
        #region Windows

            var correctSetup = false;
            var hasGraphicsDevice = UnityProjectSettingsUtility.HasGraphicsDeviceTypeAtIndex(BuildTarget.StandaloneWindows, GraphicsDeviceType.OpenGLCore, 0);
            correctSetup = hasGraphicsDevice && !UnityProjectSettingsUtility.GetAutoGraphicsApi(BuildTarget.StandaloneWindows);
            if (!correctSetup)
            {
                _busyCounter--;
                return false;
            }

           

        #endregion

        #region OSX

            hasGraphicsDevice = UnityProjectSettingsUtility.HasGraphicsDeviceTypeAtIndex(BuildTarget.StandaloneOSX, GraphicsDeviceType.OpenGLCore, 0);
            correctSetup = hasGraphicsDevice && !UnityProjectSettingsUtility.GetAutoGraphicsApi(BuildTarget.StandaloneOSX);
            if (!correctSetup)
            {
                _busyCounter--;
                return false;
            }

        #endregion

        #region Linux

            hasGraphicsDevice = UnityProjectSettingsUtility.HasGraphicsDeviceTypeAtIndex(BuildTarget.StandaloneLinux64, GraphicsDeviceType.OpenGLCore, 0);
            correctSetup = hasGraphicsDevice && !UnityProjectSettingsUtility.GetAutoGraphicsApi(BuildTarget.StandaloneLinux64);
            if (!correctSetup)
            {
                _busyCounter--;
                return false;
            }

        #endregion

            _busyCounter--;
            return correctSetup;
        }

        private static void OnCheckForLuminRequestFinished(bool success,bool hasLumin)
        {
            if (success && hasLumin)
            {
              
                MagicLeapSettingEnabled = LuminPackageUtility.IsLuminXREnabled();
                CertificatePath = UnityProjectSettingsUtility.Lumin.GetInternalCertificatePath();
                ExtendedUnityPackageImported = TypeUtility.FindTypeByPartialName(TEST_FOR_ML_SCRIPT) != null;
            }

            _busyCounter--;
            CheckingAvailability = false;
        }
    }
}
