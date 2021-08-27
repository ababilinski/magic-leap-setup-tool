using System.IO;
using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.ScriptableObjects;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor.Setup
{
	public class ImportMagicLeapSdkSetupStep: ISetupStep
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


        internal const string SETUP_ENVIRONMENT_URL = "https://developer.magicleap.com/en-us/learn/guides/set-up-development-environment#installing-lumin-sdk-packages";
		private static int _busyCounter;

		public static int BusyCounter
		{
			get => _busyCounter;
			set
			{

				_busyCounter = Mathf.Clamp(value, 0, 100);
			}
		}

		public bool Busy => BusyCounter > 0;
		/// <inheritdoc />
		public void Draw(MagicLeapSetupDataScriptableObject data)
		{
		 if (!data.HasCompatibleMagicLeapSdk)
		 {
			 if (CustomGuiContent.CustomButtons.DrawConditionButton(IMPORT_MAGIC_LEAP_SDK, data.HasCompatibleMagicLeapSdk, "....", "Incompatible", Styles.FixButtonStyle, conditionMissingColor: Color.red))
			 {
				 Execute(data);
				 MagicLeapSetupWindow.RepaintUI();
			 }
		 }
		 else
		 {
			 if (CustomGuiContent.CustomButtons.DrawConditionButton(IMPORT_MAGIC_LEAP_SDK, data.HasMagicLeapSdkInstalled, CONDITION_MET_LABEL, IMPORT_MAGIC_LEAP_SDK_BUTTON, Styles.FixButtonStyle))
			 {
				 Execute(data);
				 MagicLeapSetupWindow.RepaintUI();
			 }
		 }
		}

		/// <inheritdoc />
		public void Execute(MagicLeapSetupDataScriptableObject data)
		{
			if (!data.HasCompatibleMagicLeapSdk)
			{
				UpgradePrompt(data);
			}
			else
			{
				if (data.GetSdkFromPackageManager)
				{
					ImportSdkFromUnityPackageManagerPackage(data);
				}
				else
				{
					ImportSdkFromUnityAssetPackage(data);
				}
			}
		}

	
		private void UpgradePrompt(MagicLeapSetupDataScriptableObject data)
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
					if (data.HasIncompatibleSDKAssetPackage)
					{
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
					if (data.HasIncompatibleSDKAssetPackage)
					{
						EditorPrefs.SetBool($"{Application.dataPath}-Install", true);
						UnityProjectSettingsUtility.DeleteFolder(Path.Combine(Application.dataPath, "MagicLeap"),
																 () => { ImportSdkFromUnityPackageManagerPackage(data); },
																 MagicLeapSetupWindow._setupWindow,
																 $"{Application.dataPath}-DeletedFoldersReset");
					}
					else
					{
						BusyCounter++;
						MagicLeapLuminPackageUtility.RemoveMagicLeapPackageManagerSDK(() =>
						{
							ImportSdkFromUnityAssetPackage(data);
							BusyCounter--;
						});
						
					}

					break;
			}
		}
        public static void ImportOldUnityAssetPackage(MagicLeapSetupDataScriptableObject data)
        {
            if (data.HasLuminInstalled)
            {
                MagicLeapLuminPackageUtility.RemoveMagicLeapPackageManagerSDK(() =>
                                                                              {
                                                                                  BusyCounter++;
                                                                                  var unityPackagePath = MagicLeapLuminPackageUtility.GetUnityPackagePath;
                                                                                  if (File.Exists(unityPackagePath))
                                                                                  {
                                                                                      // "importing [{0}]"
                                                                                      Debug.Log(string.Format(IMPORTING_PACKAGE_TEXT, Path.GetFullPath(unityPackagePath)));
                                                                             
                                                                                      AssetDatabase.ImportPackage(Path.GetFullPath(unityPackagePath), true);
                                                                                  }
                                                                                  else
                                                                                  {
                                                                                      
                                                                                      // "Could not find Unity Package at path [{0}].\n SDK Path: [{1}]\nSDK Version: [{2}]"
                                                                                      Debug.LogError(string.Format(CANNOT_FIND_PACKAGE_TEXT, Path.GetFullPath(unityPackagePath), MagicLeapLuminPackageUtility.GetSDKPath(), MagicLeapLuminPackageUtility.GetSdkVersion()));
                                                                                   
                                                                                  }
                                                                              });
            }
            else
            {
                Debug.LogError(SDK_NOT_INSTALLED_TEXT);
            }
        }
		internal  void ImportSdkFromUnityAssetPackage(MagicLeapSetupDataScriptableObject data)
        {
            

           ImportOldUnityAssetPackage(data);
			AssetDatabase.importPackageFailed += OnFailedToImport;
			AssetDatabase.importPackageCancelled += AssetDatabase_importPackageCancelled;

            void OnFailedToImport(string packageName, string errorMessage)
            {
				
               Debug.LogError(errorMessage);
                var failedToImportOptions = EditorUtility.DisplayDialogComplex(FAILED_TO_IMPORT_TITLE, FAILED_TO_IMPORT_MESSAGE,
                                                                               FAILED_TO_IMPORT_OK, FAILED_TO_IMPORT_CANCEL, FAILED_TO_IMPORT_ALT);

                switch (failedToImportOptions)
                {
                    case 0: //Try again
                        ImportSdkFromUnityAssetPackage(data);
                        break;
                    case 1: //Stop
                        MagicLeapSetupAutoRun.Stop();
                        break;
                    case 2: //Go to documentation
                        Help.BrowseURL(SETUP_ENVIRONMENT_URL);
                        break;
                }
            }



			void AssetDatabase_importPackageCancelled(string packageName)
			{
				MagicLeapSetupAutoRun.Stop();
			}




        }

	

		internal void ImportSdkFromUnityPackageManagerPackage(MagicLeapSetupDataScriptableObject data)
        {
            EditorPrefs.SetBool($"{Application.dataPath}-Install", false);
          
            ImportSdkFromPackageManager(data);
        }

		public  void ImportSdkFromPackageManager(MagicLeapSetupDataScriptableObject data)
		{
			// _busyCounter++;
			if (data.HasLuminInstalled)
			{
				AddMagicLeapSdkFromPackageManagerAndRefresh(data);
			}
			else
			{
				Debug.LogError(SDK_NOT_INSTALLED_TEXT);
			}
		}

		public void CheckSDKAvailability(MagicLeapSetupDataScriptableObject data)
		{
			data.UpdateDefineSymbols();
			data.RefreshVariables();

			if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin)
			{
				data.CheckingAvailability = true;
				BusyCounter++;
				BusyCounter++;
				MagicLeapLuminPackageUtility.CheckForLuminSdkPackage(OnCheckForLuminRequestFinished);
				MagicLeapLuminPackageUtility.CheckForMagicLeapSdkPackage(onCheckForMagicLeapPackageInPackageManager);

			}



			void onCheckForMagicLeapPackageInPackageManager(bool hasPackage)
			{

				data.RefreshVariables();
				BusyCounter--;
				data.HasMagicLeapSdkInPackageManager = hasPackage;
			}



			void OnCheckForLuminRequestFinished(bool success, bool hasLumin)
			{
				if (success && hasLumin)
				{
					data.RefreshVariables();
				}
				else
				{
					MagicLeapSetupAutoRun.Stop();
				}
				BusyCounter--;
				data.CheckingAvailability = false;
			}
		}
		private void AddMagicLeapSdkFromPackageManagerAndRefresh(MagicLeapSetupDataScriptableObject data)
		{
			BusyCounter++;

			MagicLeapLuminPackageUtility.AddMagicLeapSdkPackage(OnAddMagicLeapPackageRequestFinished);



			void OnAddMagicLeapPackageRequestFinished(bool success)
			{
				if (success)
				{
					data.RefreshVariables();
					CheckSDKAvailability(data);
					
				}
				else
				{
					MagicLeapSetupAutoRun.Stop();
					Debug.LogError(string.Format(FAILED_TO_EXECUTE_ERROR, "Add Magic Leap Sdk Package"));
				}

				BusyCounter--;
			}
		}
	}
}
