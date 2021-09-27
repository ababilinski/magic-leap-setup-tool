using System;
using System.IO;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MagicLeapSetupTool.Editor.ScriptableObjects
{
	/// <summary>
	///     Scriptable Object that tracks the state of the setup process
	/// </summary>
	public class MagicLeapSetupDataScriptableObject : SingletonScriptableObject<MagicLeapSetupDataScriptableObject>
	{
		private const string LUMIN_SDK_PATH_KEY = "LuminSDKRoot";       //Editor Pref key to set/get the Lumin SDK
		private const string CERTIFICATE_PATH_KEY = "LuminCertificate"; //Editor Pref key to set/get previously used certificate
		private const string MAGIC_LEAP_DEFINES_SYMBOL = "MAGICLEAP";
		private const string TEST_FOR_PACKAGE_MANAGER_ML_SCRIPT_UPDATED = "com.magicleap.unitysdk"; // Test this assembly. The name switched after the initial release 
		private const string TEST_FOR_ML_SCRIPT = "UnityEngine.XR.MagicLeap.MLInput";               // Test this assembly. If it does not exist. The package is not imported. 
		private const string TEST_FOR_PACKAGE_MANAGER_ML_SCRIPT = "com.magicleap.unitysdk";         // Test this assembly. If it does not exist. The package is not imported. 
		private const string MAGIC_LEAP_PACKAGE_ID = "com.magicleap.unitysdk";                      // Used to check if the build platform is installed
		private const string LUMIN_PACKAGE_ID = "com.unity.xr.magicleap";                           // Used to check if the build platform is installed
		private static string _certificatePath = "";


		public bool Loading;
		public bool CorrectBuildTarget;

		public bool HasLuminInstalled
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

		public bool IsRestarting;
		private int _currentImportSdkStep;
		public bool CheckingAvailability;
		public bool HasCompatibleMagicLeapSdk;
		public bool HasMagicLeapSdkInstalled;
		public bool HasCorrectGraphicConfiguration;
		public bool LuminSettingEnabled;
		public bool GetSdkFromPackageManager;
		public bool ValidCertificatePath;
		public bool ImportMagicLeapPackageFromPackageManager;
		public bool HasIncompatibleSDKAssetPackage;
		public int SdkApiLevel;
		public bool CorrectColorSpace;
		public string PreviousCertificatePath;
		public string SdkRoot { private set; get; }
		public bool HasRootSDKPath;
		public bool HasMagicLeapSdkInPackageManager;
		public bool EmbeddedPackage;
		public bool ManifestIsUpdated { private set; get; }
		public bool HasRootSDKPathInEditorPrefs;
		private static int _busyCounter;

		private static int BusyCounter
		{
			get => _busyCounter;
			set => _busyCounter = Mathf.Clamp(value, 0, 100);
		}

		public bool Busy => BusyCounter > 0;

		public string CertificatePath
		{
			get
			{
				if (HasLuminInstalled && (string.IsNullOrEmpty(_certificatePath) || !File.Exists(_certificatePath)))
				{
					_certificatePath = UnityProjectSettingsUtility.Lumin.GetInternalCertificatePath();
				}

				ValidCertificatePath = HasLuminInstalled && !string.IsNullOrEmpty(_certificatePath) && File.Exists(_certificatePath);
				return _certificatePath;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					EditorPrefs.SetString(CERTIFICATE_PATH_KEY, value);
				}

				UnityProjectSettingsUtility.Lumin.SetInternalCertificatePath(value);
				_certificatePath = value;
				ValidCertificatePath = HasLuminInstalled && !string.IsNullOrEmpty(_certificatePath) && File.Exists(_certificatePath);
			}
		}

		public int CurrentImportSdkStep
		{
			get => _currentImportSdkStep;
			set
			{
				if (_currentImportSdkStep != value)
				{
					_currentImportSdkStep = Mathf.Clamp(value, 0, 2);
					EditorUtility.SetDirty(this);
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
			SdkRoot = path;
			EditorPrefs.SetString(LUMIN_SDK_PATH_KEY, SdkRoot);
			RefreshVariables();
		}

		/// <summary>
		///     Refreshes the variables and conditions
		/// </summary>
		public void RefreshVariables()
		{
			if (Busy)
			{
				return;
			}

			BusyCounter++;
			SdkRoot = EditorPrefs.GetString(LUMIN_SDK_PATH_KEY, null);
			HasRootSDKPath = !string.IsNullOrEmpty(SdkRoot) && Directory.Exists(SdkRoot);
			CertificatePath = UnityProjectSettingsUtility.Lumin.GetInternalCertificatePath();
			PreviousCertificatePath = EditorPrefs.GetString(CERTIFICATE_PATH_KEY, "");
			HasRootSDKPathInEditorPrefs = !string.IsNullOrEmpty(EditorPrefs.GetString(LUMIN_SDK_PATH_KEY, null));
			HasCompatibleMagicLeapSdk = MagicLeapLuminPackageUtility.HasCompatibleMagicLeapSdk();
			SdkApiLevel = MagicLeapLuminPackageUtility.GetSdkApiLevel();
			GetSdkFromPackageManager = MagicLeapLuminPackageUtility.UseSdkFromPackageManager();
			LuminSettingEnabled = MagicLeapLuminPackageUtility.IsLuminXREnabled();
			ValidCertificatePath = !string.IsNullOrEmpty(CertificatePath) && File.Exists(CertificatePath);
			HasIncompatibleSDKAssetPackage = MagicLeapLuminPackageUtility.HasIncompatibleUnityAssetPackage();
			HasMagicLeapSdkInstalled = TypeUtility.FindTypeByPartialName(TEST_FOR_ML_SCRIPT) != null;
			HasCorrectGraphicConfiguration = CorrectGraphicsConfiguration();
#if MAGICLEAP
			ManifestIsUpdated = MagicLeapLuminPackageUtility.MagicLeapManifest != null && MagicLeapLuminPackageUtility.MagicLeapManifest.minimumAPILevel == SdkApiLevel;
#else
			ManifestIsUpdated = false;
#endif
			CorrectColorSpace = PlayerSettings.colorSpace == ColorSpace.Linear;
			CheckSdkPackageState();

			BusyCounter--;
		}

		/// <summary>
		///     Checks if the Magic Leap SDK is installed from the package manager and if it is embedded into the project
		/// </summary>
		private void CheckSdkPackageState()
		{
			var versionLabel = MagicLeapLuminPackageUtility.GetSdkVersion();
			if (Version.TryParse(versionLabel, out var currentVersion))
			{
				if (currentVersion < new Version(0, 26, 0))
				{
					ImportMagicLeapPackageFromPackageManager = false;
				}
				else
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
		}

		/// <summary>
		///     checks if the graphics configuration supports Magic Leap and Zero Iteration
		/// </summary>
		/// <returns></returns>
		private bool CorrectGraphicsConfiguration()
		{
		#region Windows

			var correctSetup = false;
			var hasGraphicsDevice =
				UnityProjectSettingsUtility.HasGraphicsDeviceTypeAtIndex(BuildTarget.StandaloneWindows,
																		GraphicsDeviceType.OpenGLCore, 0);
			correctSetup = hasGraphicsDevice
						&& !UnityProjectSettingsUtility.GetAutoGraphicsApi(BuildTarget.StandaloneWindows);
			if (!correctSetup)
			{
				return false;
			}

		#endregion

		#region OSX

			hasGraphicsDevice =
				UnityProjectSettingsUtility.HasGraphicsDeviceTypeAtIndex(BuildTarget.StandaloneOSX,
																		GraphicsDeviceType.OpenGLCore, 0);
			correctSetup = hasGraphicsDevice
						&& !UnityProjectSettingsUtility.GetAutoGraphicsApi(BuildTarget.StandaloneOSX);
			if (!correctSetup)
			{
				return false;
			}

		#endregion

		#region Linux

			hasGraphicsDevice =
				UnityProjectSettingsUtility.HasGraphicsDeviceTypeAtIndex(BuildTarget.StandaloneLinux64,
																		GraphicsDeviceType.OpenGLCore, 0);
			correctSetup = hasGraphicsDevice
						&& !UnityProjectSettingsUtility.GetAutoGraphicsApi(BuildTarget.StandaloneLinux64);
			if (!correctSetup)
			{
				return false;
			}

		#endregion

			return correctSetup;
		}

		/// <summary>
		///     Updates the compilation define symbols based on if the Lumin SDK is installed
		/// </summary>
		public void UpdateDefineSymbols()
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

		/// <summary>
		///     Updates the variables based on if the the Magic Leap SDK and Lumin SDK are installed
		///     <see cref="CheckForLuminSdkPackage" /> and <see cref="CheckForMagicLeapSdkPackage" />
		/// </summary>
		public void CheckSDKAvailability()
		{
			UpdateDefineSymbols();
			RefreshVariables();

			if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin)
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
				if (success && hasLumin)
				{
					//RefreshVariables();
				}
				else
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
		private void CheckForMagicLeapSdkPackage()
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
	}
}
