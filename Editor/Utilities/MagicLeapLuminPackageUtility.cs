/* Copyright (C) 2021 Adrian Babilinski
* You may use, distribute and modify this code under the
* terms of the MIT License
*/

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
#if MAGICLEAP
using UnityEditor.XR.MagicLeap;
using UnityEditor.XR.Management;
using UnityEngine.XR.MagicLeap;
using UnityEngine.XR.Management;

#endif

namespace MagicLeapSetupTool.Editor.Utilities
{
    /// <summary>
    ///     Script responsible for giving access to the sdk calls using reflections.
    /// </summary>
    public static class MagicLeapLuminPackageUtility
	{
	#region LOG MESSAGES

		private const string WRONG_VERSION_FORMAT_ERROR = "Cannot convert Label: [{0}] to Version"; // 0 is version that failed to parse
		private const string ERROR_FAILED_TO_CREATE_WINDOW = "Failed to create a view for XR Manager Settings Instance";
		private const string REMOVED_PACKAGE_MANAGER_PACKAGE_SUCCESSFULLY = "Removed Package {0}: successfully";           //{0} is package name
		private const string REMOVED_PACKAGE_MANAGER_PACKAGE_UNSUCCESSFULLY = "Removed Package {0}: unsuccessfully";       //{0} is package name
		private const string ERROR_CANNOT_DISPLAY_PLUGIN_WINDOW_UI = "Error trying to display plug-in assignment UI: {0}"; //{0} is the error message from the exception
		private const string SETTINGS_NOT_FOUND = "Settings not Found";

	#endregion

		private const string LUMIN_PACKAGE_ID = "com.unity.xr.magicleap";                                                     // Used to check if the build platform is installed
		private const string MAGIC_LEAP_PACKAGE_ID = "com.magicleap.unitysdk";                                                // Used to check if the build platform is installed
		private const string MAGIC_LEAP_LOADER_ID = "MagicLeapLoader";                                                        // Used to test if the loader is installed and active
		private const string ASSET_RELATIVE_PATH_TO_OLD_SDK = "MagicLeap/APIs";                                               //used to check for  SDK<26
		private const string SDK_PATH_EDITOR_PREF_KEY = "LuminSDKRoot";                                                       //used to set and check the sdk path [key is an internal variable set by Unity]
		private const string SDK_PACKAGE_MANAGER_PATH_RELATIVE_TO_SDK_ROOT = "../../tools/unity/v{0}/com.magicleap.unitysdk"; //The path to the Package Manager folder relative to the SDK Root | {0} is the sdk version
		private const string OLD_ASSET_PACKAGE_PATH = "../../tools/unity/v{0}/MagicLeap.unitypackage";                        // {0} is the SDK version. Used for SDK<26
		public static Action<bool> EnableLuminXRFinished;

		private static Type _internalSDKUtilityType;

#if MAGICLEAP
		private static MagicLeapManifestSettings _magicLeapManifest;
#endif

		public static string GetUnityPackagePath => Path.Combine(GetSDKPath(), string.Format(OLD_ASSET_PACKAGE_PATH, GetSdkVersion()));

		public static string MagicLeapSdkPackageManagerPath => "file:"
															+ Path.GetFullPath(Path.Combine(EditorPrefs.GetString(SDK_PATH_EDITOR_PREF_KEY),
																							string.Format(SDK_PACKAGE_MANAGER_PATH_RELATIVE_TO_SDK_ROOT, GetSdkVersion()))); // Used to install the Magic Leap SDK for version <=0.26.0

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
					var assembly = Assembly.Load("UnityEditor.XR.MagicLeap");
					_internalSDKUtilityType = assembly.GetType("UnityEditor.XR.MagicLeap.SDKUtility");
				}

				return _internalSDKUtilityType;
			}
		}

#if MAGICLEAP
		private static Type _XRSettingsManager;
		public static XRGeneralSettingsPerBuildTarget currentSettings
		{
			get
			{
				if (_XRSettingsManager == null)
				{
					var assembly = Assembly.Load("UnityEditor.XR.Management");
					Debug.Log($"{_XRSettingsManager.Assembly.FullName} | {assembly.GetType("UnityEditor.XR.Management.XRSettingsManager")}");
					_XRSettingsManager = TypeUtility.FindTypeByPartialName("UnityEditor.XR.Management.XRSettingsManager");
				}

				var currentSettingsProperty = _XRSettingsManager.GetProperty("currentSettings", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
				var settings = (XRGeneralSettingsPerBuildTarget)currentSettingsProperty.GetValue(null);

				return settings;
			}
		}
#endif
        /// <summary>
        ///     Refreshes the BuildTargetGroup XR Loader
        /// </summary>
        /// <param name="buildTargetGroup"></param>
        private static void UpdateLoader(BuildTargetGroup buildTargetGroup)
		{
#if MAGICLEAP

			try
			{
				var settings = currentSettings.SettingsForBuildTarget(buildTargetGroup);

				if (settings == null)
				{
					settings = ScriptableObject.CreateInstance<XRGeneralSettings>();
					currentSettings.SetSettingsForBuildTarget(buildTargetGroup, settings);
					settings.name = $"{buildTargetGroup.ToString()} Settings";
					AssetDatabase.AddObjectToAsset(settings, AssetDatabase.GetAssetOrScenePath(currentSettings));
				}

				var serializedSettingsObject = new SerializedObject(settings);
				serializedSettingsObject.Update();
				EditorGUILayout.Space();

				var loaderProp = serializedSettingsObject.FindProperty("m_LoaderManagerInstance");

				if (loaderProp.objectReferenceValue == null)
				{
					var xrManagerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
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
						Debug.LogError(ERROR_FAILED_TO_CREATE_WINDOW);
					}
					else
					{
						var sdkAPILevelProperty = e.GetType().GetMethod("OnInspectorGUI", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
						sdkAPILevelProperty.Invoke(e, null);
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
				Debug.LogError(string.Format(ERROR_CANNOT_DISPLAY_PLUGIN_WINDOW_UI, ex.Message));
			}

#endif
		}

        /// <summary>
        ///     Enables Lumin XR on the available Build Target Groups
        /// </summary>
        public static void EnableLuminXRPlugin()
		{
			Debug.Log("Enable");
#if MAGICLEAP

			if (_XRSettingsManager == null)
			{
				var assembly = Assembly.Load("UnityEditor.XR.Management");
				Debug.Log($"{_XRSettingsManager.Assembly.FullName} | {assembly.GetType("UnityEditor.XR.Management.XRSettingsManager")}");
				_XRSettingsManager = TypeUtility.FindTypeByPartialName("UnityEditor.XR.Management.XRSettingsManager");
			}


			var method = _XRSettingsManager.GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
			method.Invoke(_XRSettingsManager, null);
			var info = _XRSettingsManager.GetMethod("CreateAllChildSettingsProviders", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
			info.Invoke(_XRSettingsManager, null);

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
				Debug.LogWarning(SETTINGS_NOT_FOUND);
			}
#endif
		}

        /// <summary>
        ///     Checks if Lumin XR is enabled on all available Build Target Groups
        /// </summary>
        /// <returns></returns>
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
				if (luminSettings != null && luminSettings.Manager != null)
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

        /// <summary>
        ///     Returns the SDK version based on the SDK path provided in the Preferences window
        /// </summary>
        /// <returns></returns>
        public static string GetSdkVersion()
		{
#if MAGICLEAP
			var sdkPath = EditorPrefs.GetString(SDK_PATH_EDITOR_PREF_KEY);
			if (string.IsNullOrWhiteSpace(sdkPath) || !Directory.Exists(sdkPath))
			{
				return "0.0.0";
			}

			var sdkAPILevelProperty = InternalSDKUtilityType.GetProperty("sdkVersion", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

			return ((Version)sdkAPILevelProperty.GetValue(InternalSDKUtilityType, null)).ToString();
#else
            return "0.0.0";
#endif
		}

        /// <summary>
        ///     Returns the SDK path provided in the Preferences window
        /// </summary>
        /// <returns></returns>
        public static string GetSDKPath()
		{
#if MAGICLEAP
			var sdkPathProperty = InternalSDKUtilityType.GetProperty("sdkPath", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

			return (string)sdkPathProperty.GetValue(InternalSDKUtilityType, null);
#else
            return "";
#endif
		}

        /// <summary>
        ///     Returns the SDK API level based on the SDK path provided in the Preferences window
        /// </summary>
        /// <returns></returns>
        public static int GetSdkApiLevel()
		{
#if MAGICLEAP
			var sdkPath = GetSDKPath();
			if (string.IsNullOrWhiteSpace(sdkPath) || !Directory.Exists(sdkPath))
			{
				return -1;
			}

			var sdkAPILevelProperty = InternalSDKUtilityType.GetProperty("sdkAPILevel", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

			return (int)sdkAPILevelProperty.GetValue(InternalSDKUtilityType, null);
#else
            return -1;
#endif
		}

        /// <summary>
        ///     Checks if the current Lumin SDK version is compatible with the Magic Leap SDK. [this checks compatibility between
        ///     sdk 0.24.0 and 0.26.0
        /// </summary>
        /// <returns></returns>
        public static bool HasCompatibleMagicLeapSdk()
		{
			var versionLabel = GetSdkVersion();
			if (Version.TryParse(versionLabel, out var currentVersion))
			{
				if (currentVersion == new Version(0, 0, 0))
				{
					return true;
				}

				if (currentVersion < new Version(0, 26, 0))
				{
					// return Directory.EnumerateDirectories(path, searchPattern).Any();
					var cachePath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Library"));
					var magicLeapDirectoryExists = Directory.EnumerateDirectories(cachePath, MAGIC_LEAP_PACKAGE_ID, SearchOption.AllDirectories).Any();
					return !magicLeapDirectoryExists;
				}

				if (Directory.Exists(Path.Combine(Application.dataPath, ASSET_RELATIVE_PATH_TO_OLD_SDK)))
				{
					return false;
				}
			}
			else
			{
				Debug.LogError($"Cannot convert Label: [{versionLabel}] to Version");
			}

			return true;
		}

        /// <summary>
        ///     Checks if the Magic Leap SDK unity asset package is imported with sdk version 0.26.0
        /// </summary>
        /// <returns></returns>
        public static bool HasIncompatibleUnityAssetPackage()
		{
			var versionLabel = GetSdkVersion();
			if (Version.TryParse(versionLabel, out var currentVersion))
			{
				if (currentVersion >= new Version(0, 26, 0) && Directory.Exists(Path.Combine(Application.dataPath, ASSET_RELATIVE_PATH_TO_OLD_SDK)))
				{
					return true;
				}
			}
			else
			{
				Debug.LogError(string.Format(WRONG_VERSION_FORMAT_ERROR, versionLabel));
			}

			return false;
		}

        /// <summary>
        ///     removes the Magic Leap SDK from the package manager (only for sdk 0.26.0)
        /// </summary>
        /// <param name="finished"></param>
        public static void RemoveMagicLeapPackageManagerSDK(Action finished)
		{
			var versionLabel = GetSdkVersion();

			if (Version.TryParse(versionLabel, out var currentVersion))
			{
				if (currentVersion < new Version(0, 26, 0))
				{
					PackageUtility.HasPackageInstalled(MAGIC_LEAP_PACKAGE_ID, (requestSuccess, hasPackage) =>
																			{
																				if (hasPackage)
																				{
																					PackageUtility.RemovePackage(MAGIC_LEAP_PACKAGE_ID, FinishedRemovingPackage);



																					void FinishedRemovingPackage(bool success)
																					{
																						if (success)
																						{
																							Debug.Log(string.Format(REMOVED_PACKAGE_MANAGER_PACKAGE_SUCCESSFULLY, MAGIC_LEAP_PACKAGE_ID));
																						}
																						else
																						{
																							Debug.LogError(string.Format(REMOVED_PACKAGE_MANAGER_PACKAGE_UNSUCCESSFULLY, MAGIC_LEAP_PACKAGE_ID));
																						}


																						finished?.Invoke();
																					}
																				}
																				else
																				{
																					finished?.Invoke();
																				}
																			});
				}
			}
			else
			{
				finished?.Invoke();
				Debug.LogError(string.Format(WRONG_VERSION_FORMAT_ERROR, versionLabel));
			}
		}

        /// <summary>
        ///     Adds the Lumin XR Package to the package manager
        /// </summary>
        /// <param name="success"></param>
        public static void AddLuminSdkPackage(Action<bool> success)
		{
			// Add a package to the project
			PackageUtility.AddPackage(LUMIN_PACKAGE_ID, success);
		}

		public static void CheckForMagicLeapSdkPackage(Action<bool> checkFinished)
		{
			PackageUtility.HasPackageInstalled(MAGIC_LEAP_PACKAGE_ID, (success, hasPackage) =>
																	{
																		if (success && hasPackage)
																		{
																			checkFinished.Invoke(true);
																		}
																		else
																		{
																			checkFinished.Invoke(false);
																		}
																	});
		}

        /// <summary>
        ///     Returns true if sdk version 0.26.0 is being used
        /// </summary>
        /// <returns></returns>
        public static bool UseSdkFromPackageManager()
		{
			var versionLabel = GetSdkVersion();

			if (Version.TryParse(versionLabel, out var currentVersion))
			{
				if (currentVersion >= new Version(0, 26, 0))
				{
					return true;
				}

				return false;
			}

			return true;
		}

        /// <summary>
        ///     Checks if the Lumin SDK package for 0.26.0 is installed
        /// </summary>
        /// <param name="successAndInstalled"></param>
        public static void CheckForLuminSdkPackage(Action<bool, bool> successAndInstalled)
		{
			PackageUtility.HasPackageInstalled(LUMIN_PACKAGE_ID, successAndInstalled);
		}
	}
}
