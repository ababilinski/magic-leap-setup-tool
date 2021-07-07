using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

using UnityEngine;

#if MAGICLEAP
using UnityEditor.XR.Management.Metadata;
using UnityEditor.XR.MagicLeap;
using UnityEditor.XR.Management;
using UnityEngine.XR.MagicLeap;
using UnityEngine.XR.Management;
#endif

namespace MagicLeapSetupTool.Editor.Utilities
{
    public static class LuminPackageUtility
    {
        private const string LUMIN_PACKAGE_ID = "com.unity.xr.magicleap";                     // Used to check if the build platform is installed
        private const string MAGIC_LEAP_LOADER_ID = "MagicLeapLoader";                       // Used to test if the loader is installed and active


   
        public static Action<bool,bool> CheckForLuminSdkRequestFinished;
        public static Action<bool> AddLuminPackageRequestFinished;
        public static Action<bool> EnableLuminXRFinished;
        private static AddRequest _addPackageRequest;
        private static ListRequest _listInstalledPackagesRequest;
        private static Type _internalSDKUtilityType;
      
#if MAGICLEAP
        private static MagicLeapManifestSettings _magicLeapManifest;
#endif

#if MAGICLEAP
        public static MagicLeapManifestSettings MagicLeapManifest
        {
            get
            {
              

                if (_magicLeapManifest == null)
                {
                    _magicLeapManifest = MagicLeapManifestSettings.GetOrCreateSettings();
                }

                return _magicLeapManifest;
            }
        }
#endif

        public static Type InternalSDKUtilityType
        {
            get
            {
                if (_internalSDKUtilityType == null)
                {
                    _internalSDKUtilityType = TypeUtility.FindTypeByPartialName("UnityEditor.XR.MagicLeap.SDKUtility", "+");
                }

                return _internalSDKUtilityType;
            }
        }

        #if MAGICLEAP
        public static XRGeneralSettingsPerBuildTarget currentSettings
        {
            get
            {
                var s = TypeUtility.FindTypeByPartialName("UnityEditor.XR.Management.XRSettingsManager");
              
                //foreach (var info in s.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                //{
                //    Debug.Log($"field: {info.Name}");
                //}

                //foreach (var info in s.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                //{
                //    Debug.Log($"Property: {info.Name}");
                //}
                var sdkAPILevelProperty =s.GetProperty("currentSettings", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
               var settings = (XRGeneralSettingsPerBuildTarget) sdkAPILevelProperty.GetValue(null);
              
               return settings;
            }   
        }
        #endif

        private static void UpdateLoader(BuildTargetGroup buildTargetGroup)
            {
        #if MAGICLEAP

            try
            {

         
                XRGeneralSettings settings = currentSettings.SettingsForBuildTarget(buildTargetGroup);
              
                if (settings == null)
                {
                    settings = ScriptableObject.CreateInstance<XRGeneralSettings>() as XRGeneralSettings;
                    currentSettings.SetSettingsForBuildTarget(buildTargetGroup, settings);
                    settings.name = $"{buildTargetGroup.ToString()} Settings";
                    AssetDatabase.AddObjectToAsset(settings, AssetDatabase.GetAssetOrScenePath(currentSettings));
                }

                var serializedSettingsObject = new SerializedObject(settings);
                serializedSettingsObject.Update();
                EditorGUILayout.Space();

                SerializedProperty loaderProp = serializedSettingsObject.FindProperty("m_LoaderManagerInstance");

                if (loaderProp.objectReferenceValue == null)
                {
                    var xrManagerSettings = ScriptableObject.CreateInstance<XRManagerSettings>() as XRManagerSettings;
                    xrManagerSettings.name = $"{buildTargetGroup.ToString()} Providers";
                    AssetDatabase.AddObjectToAsset(xrManagerSettings, AssetDatabase.GetAssetOrScenePath(currentSettings));
                    loaderProp.objectReferenceValue = xrManagerSettings;
                    serializedSettingsObject.ApplyModifiedProperties();
                    var serializedManagerSettingsObject = new SerializedObject(xrManagerSettings);
                    xrManagerSettings.InitializeLoaderSync();
                    serializedManagerSettingsObject.ApplyModifiedProperties();
                    serializedManagerSettingsObject.Update();

                    
                }

                var obj = loaderProp.objectReferenceValue;

                if (obj != null)
                {
                    loaderProp.objectReferenceValue = obj;
                 
                    var e = UnityEditor.Editor.CreateEditor(obj);


                    if (e == null)
                    {
                        Debug.Log("Failed to create a view for XR Manager Settings Instance");
                    }
                    else
                    {
                        var sdkAPILevelProperty = e.GetType().GetMethod("OnInspectorGUI", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                        sdkAPILevelProperty.Invoke(e,null);
                    }

                }
                else if (obj == null)
                {
                    settings.AssignedSettings = null;
                    loaderProp.objectReferenceValue = null;
                }

                serializedSettingsObject.ApplyModifiedProperties();
                serializedSettingsObject.Update();
               // AssetDatabase.SaveAssets();
       
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error trying to display plug-in assingment UI : {ex.Message}");
            }

   #endif
        }
        public static void EnableLuminXRPlugin()
        {
#if MAGICLEAP
          

            var findTypeByPartialName = TypeUtility.FindTypeByPartialName("UnityEditor.XR.Management.XRSettingsManager");

            var method = findTypeByPartialName.GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            method.Invoke(findTypeByPartialName, null);
            var info = findTypeByPartialName.GetMethod("CreateAllChildSettingsProviders", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            info.Invoke(findTypeByPartialName, null);
              
                Debug.Log(currentSettings);
                UpdateLoader(BuildTargetGroup.Lumin);
                UpdateLoader(BuildTargetGroup.Standalone);


                EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget standaloneBuildSetting);
                EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget luminBuildSetting);

                if (standaloneBuildSetting && luminBuildSetting)
                {
                    var standaloneSettings = standaloneBuildSetting.SettingsForBuildTarget(BuildTargetGroup.Standalone);
                    var luminSettings = luminBuildSetting.SettingsForBuildTarget(BuildTargetGroup.Lumin);
                   

                    luminSettings.Manager.TryAddLoader(MagicLeapLoader.assetInstance);
                    standaloneSettings.Manager.TryAddLoader(MagicLeapLoader.assetInstance);

                    EnableLuminXRFinished.Invoke(true);
              
        
                }
                else
                {
                    EnableLuminXRFinished.Invoke(false);
                    Debug.LogWarning("Settings not Found");
                }
#endif
        }

        public static bool IsLuminXREnabled()
        {
#if MAGICLEAP
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget standaloneBuildSetting);
            EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out XRGeneralSettingsPerBuildTarget luminBuildSetting);
            var hasLuminLoader = false;
            var hasStandaloneLoader = false;

            if (standaloneBuildSetting == null || luminBuildSetting == null)
            {
                return false;
            }

      
          
            if (luminBuildSetting != null)
            {
                var luminSettings = luminBuildSetting.SettingsForBuildTarget(BuildTargetGroup.Lumin);
                if (luminSettings != null && luminSettings.Manager!= null)
                {
                    hasLuminLoader = luminSettings.Manager.activeLoaders.Any(e =>
                                                                             {
                                                                                 var fullName = e.GetType().FullName;
                                                                                 return fullName != null && fullName.Contains(MAGIC_LEAP_LOADER_ID);
                                                                             });
                }
            }

            if (standaloneBuildSetting != null)
            {
                var standaloneSettings = standaloneBuildSetting.SettingsForBuildTarget(BuildTargetGroup.Standalone);
                if (standaloneSettings != null && standaloneSettings.Manager != null)
                {
                    hasStandaloneLoader = standaloneSettings.Manager.activeLoaders.Any(e =>
                                                                                       {
                                                                                           var fullName = e.GetType().FullName;
                                                                                           return fullName != null && fullName.Contains(MAGIC_LEAP_LOADER_ID);
                                                                                       });
                }
            }

         

         
        


            if (hasStandaloneLoader && hasLuminLoader)
            {
                return true;
            }
#endif
            return false;
        }

        public static string GetSdkVersion()
        {
            #if MAGICLEAP
            var sdkAPILevelProperty = InternalSDKUtilityType.GetProperty("sdkVersion", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            return ((Version) sdkAPILevelProperty.GetValue(InternalSDKUtilityType, null)).ToString();
            #else
            return "0.0.0";
            #endif
        }

        public static string GetSDKPath()
        { 
        #if MAGICLEAP
            var sdkPathProperty = InternalSDKUtilityType.GetProperty("sdkPath", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            return (string) sdkPathProperty.GetValue(InternalSDKUtilityType, null);
        #else
            return "";
        #endif
        }

        public static int GetSdkApiLevel()
        {
#if MAGICLEAP
            var sdkAPILevelProperty = InternalSDKUtilityType.GetProperty("sdkAPILevel", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            return (int) sdkAPILevelProperty.GetValue(InternalSDKUtilityType, null);
#else
            return -1;
#endif
        }

        public static void AddLuminSdkPackage()
        {
            // Add a package to the project
            _addPackageRequest = Client.Add(LUMIN_PACKAGE_ID);
            EditorApplication.update += ClientAddProgress;

        }

        public static void CheckForLuminSdkPackage()
        {
            _listInstalledPackagesRequest = Client.List(); // List packages installed for the project
            EditorApplication.update += ClientListProgress;

        }

        private static void ClientListProgress()
        {
            if (_listInstalledPackagesRequest.IsCompleted)
            {
                if (_listInstalledPackagesRequest.Status == StatusCode.Success)
                {
                    if (_listInstalledPackagesRequest.Result.Any(e => e.packageId.Contains(LUMIN_PACKAGE_ID)))
                    {

                        CheckForLuminSdkRequestFinished?.Invoke(true,true);
                    }
                    else
                    {
                        CheckForLuminSdkRequestFinished?.Invoke(true, false);
                    }
                   
                   
                }
                else if (_listInstalledPackagesRequest.Status >= StatusCode.Failure)
                {
                    CheckForLuminSdkRequestFinished?.Invoke(false,false);
                    Debug.LogError(_listInstalledPackagesRequest.Error.message);
                }
              
                EditorApplication.update -= ClientListProgress;
            }
        }

        private static void ClientAddProgress()
        {
            if (_addPackageRequest.IsCompleted)
            {
                if (_addPackageRequest.Status == StatusCode.Success)
                {
                    AddLuminPackageRequestFinished.Invoke(true);
                }
                else if (_addPackageRequest.Status >= StatusCode.Failure)
                {
                    Debug.LogError(_addPackageRequest.Error.message);
                    AddLuminPackageRequestFinished.Invoke(false);
                }
                EditorApplication.update -= ClientAddProgress;
               
            }
        }
    }
}
