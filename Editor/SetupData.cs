using System;
using System.IO;
using MagicLeapSetupTool.Editor.Setup;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MagicLeapSetupTool.Editor
{
	public class SetupData
	{
		
		private const string MAGIC_LEAP_DEFINES_SYMBOL = "MAGICLEAP";
		private const string TEST_FOR_PACKAGE_MANAGER_ML_SCRIPT_UPDATED = "com.magicleap.unitysdk"; // Test this assembly. The name switched after the initial release 
		private const string TEST_FOR_ML_SCRIPT = "UnityEngine.XR.MagicLeap.MLInput";               // Test this assembly. If it does not exist. The package is not imported. 
		private const string TEST_FOR_PACKAGE_MANAGER_ML_SCRIPT = "com.magicleap.unitysdk";         // Test this assembly. If it does not exist. The package is not imported. 
		private const string MAGIC_LEAP_PACKAGE_ID = "com.magicleap.unitysdk";                      // Used to check if the build platform is installed
		private const string LUMIN_PACKAGE_ID = "com.unity.xr.magicleap";                           // Used to check if the build platform is installed
	

			private const string LUMIN_SDK_PATH_KEY = "LuminSDKRoot";       //Editor Pref key to set/get the Lumin SDK
	


		public static void CheckForPackage()
		{
			
		}

		public bool Loading;

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


		public static readonly string SdkRoot = EditorPrefs.GetString(LUMIN_SDK_PATH_KEY, null);
		public static readonly bool HasRootSDKPath = !string.IsNullOrEmpty(SdkRoot) && Directory.Exists(SdkRoot);
		public static readonly bool HasRootSDKPathInEditorPrefs = !string.IsNullOrEmpty(EditorPrefs.GetString(LUMIN_SDK_PATH_KEY, null));
		public bool IsRestarting;
		private int _currentImportSdkStep;
		public bool CheckingAvailability;
	
		public bool HasMagicLeapSdkInstalled;
	
		public bool LuminSettingEnabled;
		public bool GetSdkFromPackageManager;
	
		public bool ImportMagicLeapPackageFromPackageManager;
		public bool HasIncompatibleSDKAssetPackage;
		public int SdkApiLevel;
		public bool CorrectColorSpace;
		public static bool HasMagicLeapSdkInPackageManager;
		public bool EmbeddedPackage;
		public bool ManifestIsUpdated { private set; get; }
		private static int _busyCounter;

		private static int BusyCounter
		{
			get => _busyCounter;
			set => _busyCounter = Mathf.Clamp(value, 0, 100);
		}

		public bool Busy => BusyCounter > 0;

	

		public int CurrentImportSdkStep
		{
			get => _currentImportSdkStep;
			set
			{
				if (_currentImportSdkStep != value)
				{
					_currentImportSdkStep = Mathf.Clamp(value, 0, 2);
				
					AssetDatabase.SaveAssets();
				}
			}
		}

		/// <summary>
		///     Sets the Lumin SDK path in the Preferences window
		/// </summary>
		/// <param name="path"></param>
		public void SetSdkRoot(string path)
		{
			
			EditorPrefs.SetString(LUMIN_SDK_PATH_KEY, SdkRoot);
			RefreshVariables();
		}



		/// <summary>
		///     Checks if the Magic Leap SDK is installed from the package manager and if it is embedded into the project
		/// </summary>
		private void CheckSdkPackageState()
		{
			var versionLabel = MagicLeapLuminPackageUtility.GetSdkVersion();
			if (Version.TryParse(versionLabel, out var currentVersion))
			{
				ImportMagicLeapPackageFromPackageManager = true;
				var packagePath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Packages/")).Replace('\\', '/');
				var embedded = DefineSymbolsUtility.DirectoryPathExistsWildCard(packagePath, "com.magicleap.unitysdk");
				if (embedded)
				{
					CurrentImportSdkStep = 2;
				}
				else
				{
					BusyCounter++;
					CurrentImportSdkStep = 0;
					PackageUtility.HasPackage("com.magicleap.unitysdk", OnCheckedPackagedList);



					void OnCheckedPackagedList(bool exists)
					{
						CurrentImportSdkStep = exists ? 1 : 0;

						BusyCounter--;
					}
				}
			}
		}



		/// <summary>
		///     Updates the compilation define symbols based on if the Lumin SDK is installed
		/// </summary>
	

		/// <summary>
		///     Updates the variables based on if the the Magic Leap SDK and Lumin SDK are installed
		///     <see cref="CheckForLuminSdkPackage" /> and <see cref="CheckForMagicLeapSdkPackage" />
		/// </summary>
		public void CheckSDKAvailability()
		{
		
			RefreshVariables();

			if (BuildTargetSetupStep.CorrectBuildTarget)
			{
				CheckingAvailability = true;
				CheckForLuminSdkPackage();
				CheckForMagicLeapSdkPackage();
			}
		}

		/// <summary>
		///     Updates the variables based on if the the  Lumin SDK are installed
		/// </summary>
		private void CheckForLuminSdkPackage()
		{
			BusyCounter++;
			PackageUtility.HasPackageInstalled(LUMIN_PACKAGE_ID, OnCheckForLuminRequestFinished);



			void OnCheckForLuminRequestFinished(bool success, bool hasLumin)
			{
				if (!success || !hasLumin)
				{
					MagicLeapSetupAutoRun.Stop();
				}

				BusyCounter--;
				CheckingAvailability = false;
			}
		}

		/// <summary>
		///     Updates the variables based on if the the Magic Leap SDK are installed
		/// </summary>
		public static void CheckForMagicLeapSdkPackage()
		{
			BusyCounter++;
			PackageUtility.HasPackageInstalled(MAGIC_LEAP_PACKAGE_ID, onCheckForMagicLeapPackageInPackageManager);



			void onCheckForMagicLeapPackageInPackageManager(bool success, bool hasPackage)
			{
				//RefreshVariables();
				BusyCounter--;
				HasMagicLeapSdkInPackageManager = hasPackage;
			}
		}
		public static void UpdateDefineSymbols()
		{
			//var sdkPath = EditorPrefs.GetString("LuminSDKRoot");


			EditorApplication.delayCall += () =>
											{
												if (!DefineSymbolsUtility.DirectoryPathExistsWildCard(Path.GetFullPath(Path.Combine(Application.dataPath, "../Library/PackageCache")), "com.unity.xr.magicleap"))
												{
													if (DefineSymbolsUtility.ContainsDefineSymbol(MAGIC_LEAP_DEFINES_SYMBOL))
													{
														DefineSymbolsUtility.RemoveDefineSymbol(MAGIC_LEAP_DEFINES_SYMBOL);
													}
												}
												else
												{
													if ( /*!string.IsNullOrWhiteSpace(sdkPath) && Directory.Exists(sdkPath) && */!DefineSymbolsUtility.ContainsDefineSymbol(MAGIC_LEAP_DEFINES_SYMBOL))
													{
														DefineSymbolsUtility.AddDefineSymbol(MAGIC_LEAP_DEFINES_SYMBOL);
													}
												}
											};
		}


		public void RefreshVariables()
		{
			if (Busy)
			{
				return;
			}

			BusyCounter++;
			
		
		
		
		
			SdkApiLevel = MagicLeapLuminPackageUtility.GetSdkApiLevel();
			GetSdkFromPackageManager = MagicLeapLuminPackageUtility.UseSdkFromPackageManager();
			LuminSettingEnabled = MagicLeapLuminPackageUtility.IsLuminXREnabled();
			
			HasIncompatibleSDKAssetPackage = MagicLeapLuminPackageUtility.HasIncompatibleUnityAssetPackage();
			var typeInfo = TypeUtility.FindTypeByPartialName(TEST_FOR_ML_SCRIPT);
			if (typeInfo != null)
				Debug.Log($"TYPE FOUND: {typeInfo.FullName} || Assembly: {typeInfo.Assembly.FullName}");
			HasMagicLeapSdkInstalled = typeInfo != null;
		
#if MAGICLEAP
			ManifestIsUpdated = MagicLeapLuminPackageUtility.MagicLeapManifest != null && MagicLeapLuminPackageUtility.MagicLeapManifest.minimumAPILevel == SdkApiLevel;
#else
			ManifestIsUpdated = false;
#endif
			CorrectColorSpace = PlayerSettings.colorSpace == ColorSpace.Linear;
			CheckSdkPackageState();

			BusyCounter--;
		}
	}
}
