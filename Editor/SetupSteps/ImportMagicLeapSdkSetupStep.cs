using System;
using System.IO;
using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.ScriptableObjects;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor.Setup
{
	/// <summary>
	///     Imports the Magic Leap SDK
	/// </summary>
	public class ImportMagicLeapSdkSetupStep : ISetupStep
	{
		private const string IMPORT_MAGIC_LEAP_SDK = "Import the Magic Leap SDK";
		private const string IMPORT_MAGIC_LEAP_SDK_BUTTON = "Import package";
		private const string FAILED_TO_IMPORT_TITLE = "Failed to import Unity Package.";
		private const string FAILED_TO_IMPORT_MESSAGE = "Failed to find the Magic Leap SDK Package. Please make sure your development enviornment is setup correctly.";
		private const string FAILED_TO_IMPORT_OK = "Try Again";
		private const string FAILED_TO_IMPORT_CANCEL = "Cancel";
		private const string CONDITION_MET_LABEL = "Done";

		private const string SDK_NOT_INSTALLED_TEXT = "Cannot preform that action while the SDK is not installed in the project";
		private const string REMOVE_INCOMPATIBLE_MAGIC_LEAP_SDK_TITLE = "Found Incompatable Magic Leap SDK";
		private const string REMOVE_INCOMPATIBLE_MAGIC_LEAP_SDK_MESSAGE = "The Magic Leap SDK found in your project does not support the selected Lumin SDK Version. Would you like to remove it?";
		private const string REMOVE_INCOMPATIBLE_MAGIC_LEAP_SDK_OK = "Remove";
		private const string REMOVE_INCOMPATIBLE_MAGIC_LEAP_SDK_CANCEL = "Cancel";
		private const string REMOVE_INCOMPATIBLE_MAGIC_LEAP_SDK_ALT = "Remove And Update";
		private const string FAILED_TO_IMPORT_ALT = "Setup Developer Environment";
		private const string FAILED_TO_IMPORT_HELP_TEXT = "Setup the developer environment";
		private const string IMPORTING_PACKAGE_TEXT = "importing [{0}]"; // {0} is the path  to the unity package
		private const string CANNOT_FIND_PACKAGE_TEXT = "Could not find Unity Package at path [{0}].\n SDK Path: [{1}]\nSDK Version: [{2}]";
		private const string FAILED_TO_EXECUTE_ERROR = "Failed to execute [{0}]"; //0 is method/action name
		private const string CONFLICT_WHILE_INSTALLING_MAGIC_LEAP_PACKAGE_MANAGER_ASSET = "Cannot install Magic Leap SDK. an old version is currently installed. Please delete: [Assets/MagicLeap/APIs] and try again.";
		private const string ASSET_RELATIVE_PATH_TO_OLD_SDK = "MagicLeap/APIs";                          //used to check for  SDK<26
		private const string WRONG_VERSION_FORMAT_ERROR = "Cannot convert Label: [{0}] to Version"; // 0 is version that failed to parse
		private const string MAGIC_LEAP_PACKAGE_ID = "com.magicleap.unitysdk";                      // Used to check if the build platform is installed
		internal const string SETUP_ENVIRONMENT_URL = "https://developer.magicleap.com/en-us/learn/guides/set-up-development-environment#installing-lumin-sdk-packages";

		private const string EMBED_PACKAGE_OPTION_TITLE = "Embed Installed Package?";
		private const string EMBED_PACKAGE_OPTION_BODY = "Would you like to embed the Magic Leap SDK so that it is Read/Write enabled?";
		private const string EMBED_PACKAGE_OPTION_OK = "Embed Package";
		private const string EMBED_PACKAGE_OPTION_CANCEL = "Don't Embed";
		
		public static readonly Type CachedMagicLeapMLInputType = Type.GetType("UnityEngine.XR.MagicLeap.MLInput,LuminUnity"); // Test this assembly. If it does not exist. The package is not imported. 
		public static bool HasMagicLeapSdkInstalled = (object)CachedMagicLeapMLInputType != null;
		public static bool HasCompatibleMagicLeapSdk;
		public static bool HasMagicLeapSdkInPackageManager;
		public static bool GetSdkFromPackageManager;
		public static bool HasIncompatibleSDKAssetPackage;
		private bool _hasRootSDKPath;
		private static int _busyCounter;
		
		public static int BusyCounter
		{
			get => _busyCounter;
			set => _busyCounter = Mathf.Clamp(value, 0, 100);
		}

		private int _currentImportSdkStep;
		//TODO: FIX
		public int CurrentImportSdkStep
		{
			get => _currentImportSdkStep;
			set
			{
				if (_currentImportSdkStep != value)
				{
					_currentImportSdkStep = Mathf.Clamp(value, 0, 2);

				}
			}
		}
		public bool Busy => BusyCounter > 0;
		
		/// <inheritdoc />
		public void Refresh()
		{
			HasIncompatibleSDKAssetPackage = MagicLeapLuminPackageUtility.HasIncompatibleUnityAssetPackage();
			HasCompatibleMagicLeapSdk = MagicLeapLuminPackageUtility.HasCompatibleMagicLeapSdk();
			HasMagicLeapSdkInstalled = (object)CachedMagicLeapMLInputType != null;
			GetSdkFromPackageManager = MagicLeapLuminPackageUtility.UseSdkFromPackageManager();
		}

		/// <summary>
		///     Updates the variables based on if the the Magic Leap SDK are installed
		/// </summary>
		public static void CheckForMagicLeapSdkPackage()
		{
			if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin)
			{
				BusyCounter++;
				PackageUtility.HasPackageInstalled(MAGIC_LEAP_PACKAGE_ID, OnCheckForMagicLeapPackageInPackageManager);



				void OnCheckForMagicLeapPackageInPackageManager(bool success, bool hasPackage)
				{
					//RefreshVariables();
					BusyCounter--;
					HasMagicLeapSdkInPackageManager = hasPackage;
				}
			}
		}
		/// <inheritdoc />
		public bool Draw()
		{
			if (!HasCompatibleMagicLeapSdk)
			{
				if (CustomGuiContent.CustomButtons.DrawConditionButton(IMPORT_MAGIC_LEAP_SDK, HasCompatibleMagicLeapSdk, "....", "Incompatible", Styles.FixButtonStyle, conditionMissingColor: Color.red))
				{
					Execute();
					return true;
				}

				return false;
			}

			if (CurrentImportSdkStep == 1 && !Busy)
			{
				EmbedPackage();
			}

			if (CustomGuiContent.CustomButtons.DrawConditionButton(IMPORT_MAGIC_LEAP_SDK, HasMagicLeapSdkInstalled, CONDITION_MET_LABEL, IMPORT_MAGIC_LEAP_SDK_BUTTON, Styles.FixButtonStyle))
			{
				Execute();
				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public void Execute()
		{
			if (!HasCompatibleMagicLeapSdk)
			{
				UpgradePrompt();
			}
			else
			{
				if (GetSdkFromPackageManager)
				{
					ImportSdkFromUnityPackageManager();
				}
			}
		}

		/// <summary>
		///     Embeds Magic Leap SDK package into the Packages folder
		/// </summary>
		/// <param name="data"></param>
		private void EmbedPackage()
		{

			var embedPackage = EditorUtility.DisplayDialog(EMBED_PACKAGE_OPTION_TITLE, EMBED_PACKAGE_OPTION_BODY,
																				EMBED_PACKAGE_OPTION_OK, EMBED_PACKAGE_OPTION_CANCEL);
			if (embedPackage)
			{
				PackageUtility.EmbedPackage(MAGIC_LEAP_PACKAGE_ID, OnAddMagicLeapPackageRequestFinished);
				BusyCounter++;
			}



			void OnAddMagicLeapPackageRequestFinished(bool success)
			{
				if (success)
				{
					MagicLeapLuminPackageUtility.UpdateDefineSymbols();
					HasMagicLeapSdkInPackageManager = success;
					CurrentImportSdkStep = 2;
				}
				else
				{
					MagicLeapSetupAutoRun.Stop();
					Debug.LogError(string.Format(FAILED_TO_EXECUTE_ERROR, "Add Magic Leap Sdk Package"));
				}

				BusyCounter--;
			}
		}

		/// <summary>
		///     Shows an upgrade prompt. <see cref="Execute" /> calls this method if there is a Lumin SDK and Magic Leap SDK
		///     mismatch
		/// </summary>
		/// <param name="data"></param>
		private void UpgradePrompt()
		{
			var usePreviousCertificateOption =
				EditorUtility.DisplayDialogComplex(REMOVE_INCOMPATIBLE_MAGIC_LEAP_SDK_TITLE,
													REMOVE_INCOMPATIBLE_MAGIC_LEAP_SDK_MESSAGE,
													REMOVE_INCOMPATIBLE_MAGIC_LEAP_SDK_OK,
													REMOVE_INCOMPATIBLE_MAGIC_LEAP_SDK_CANCEL,
													REMOVE_INCOMPATIBLE_MAGIC_LEAP_SDK_ALT);

			switch (usePreviousCertificateOption)
			{
				case 0: //Remove
					if (HasIncompatibleSDKAssetPackage)
					{
						//data.IsRestarting = true;
						UnityProjectSettingsUtility.DeleteFolder(Path.Combine(Application.dataPath, "MagicLeap"), null,
																MagicLeapSetupWindow._setupWindow,
																$"{Application.dataPath}-DeletedFoldersReset");
					}
					else
					{
						BusyCounter++;
						MagicLeapLuminPackageUtility.RemoveMagicLeapPackageManagerSDK(() => { BusyCounter--; });
					}

					break;
				case 1: //Cancel
					break;
				case 2: //Remove and update
					if (HasIncompatibleSDKAssetPackage)
					{
						EditorPrefs.SetBool($"{Application.dataPath}-Install", true);
						UnityProjectSettingsUtility.DeleteFolder(Path.Combine(Application.dataPath, "MagicLeap"),
																() => { ImportSdkFromUnityPackageManager(); },
																MagicLeapSetupWindow._setupWindow,
																$"{Application.dataPath}-DeletedFoldersReset");
					}
					

					break;
			}
		}


		/// <summary>
		///     Imports Magic Leap SDK through the package manager
		/// </summary>
		/// <param name="data"></param>
		internal void ImportSdkFromUnityPackageManager()
		{
			EditorPrefs.SetBool($"{Application.dataPath}-Install", false);

			// _busyCounter++;
			AddMagicLeapSdkFromPackageManagerAndRefresh();
		}


		/// <summary>
		///     Adds the Magic Leap SDK and refreshes setup variables
		/// </summary>
		/// <param name="data"></param>
		private void AddMagicLeapSdkFromPackageManagerAndRefresh()
		{
			BusyCounter++;
			var versionLabel = MagicLeapLuminPackageUtility.GetSdkVersion();

			if (Version.TryParse(versionLabel, out var currentVersion))
			{
				if (currentVersion >= new Version(0, 26, 0))
				{
					if (Directory.Exists(Path.Combine(Application.dataPath, ASSET_RELATIVE_PATH_TO_OLD_SDK)))
					{
						Debug.LogError(CONFLICT_WHILE_INSTALLING_MAGIC_LEAP_PACKAGE_MANAGER_ASSET);
						MagicLeapSetupAutoRun.Stop();
						Debug.LogError(string.Format(FAILED_TO_EXECUTE_ERROR, "Add Magic Leap Sdk Package"));
					}
					else
					{
						PackageUtility.AddPackage(MagicLeapLuminPackageUtility.MagicLeapSdkPackageManagerPath, OnAddedPackage);



						void OnAddedPackage(bool success)
						{
							if (success)
							{
								EmbedPackage();
							}
						}
					}
				}
			}
			else
			{
				MagicLeapSetupAutoRun.Stop();
				Debug.LogError(string.Format(FAILED_TO_EXECUTE_ERROR, "Add Magic Leap Sdk Package"));
				Debug.LogError(string.Format(WRONG_VERSION_FORMAT_ERROR, versionLabel));
			}
		}

	}
}
