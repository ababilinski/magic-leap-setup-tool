using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.ScriptableObjects;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor.Setup
{
	public class InstallPluginSetupStep : ISetupStep
	{
		private const string FAILED_TO_EXECUTE_ERROR = "Failed to execute [{0}]"; //0 is method/action name
		private const string INSTALL_PLUGIN_LABEL = "Install the Lumin XR plug-in";
		private const string INSTALL_PLUGIN_BUTTON_LABEL = "Install Package";
		private const string CONDITION_MET_LABEL = "Done";
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
			//Makes sure the user changes to the Lumin Build Target before being able to set the other options
			GUI.enabled = data.HasRootSDKPath && data.CorrectBuildTarget && !data.Loading;
			 if (CustomGuiContent.CustomButtons.DrawConditionButton(INSTALL_PLUGIN_LABEL, data.HasLuminInstalled, CONDITION_MET_LABEL, INSTALL_PLUGIN_BUTTON_LABEL, Styles.FixButtonStyle))
			 {
				 Execute(data);
				 MagicLeapSetupWindow.RepaintUI();

			 }
		}

		/// <inheritdoc />
		public void Execute(MagicLeapSetupDataScriptableObject data)
		{
			BusyCounter++;
			MagicLeapLuminPackageUtility.AddLuminSdkPackage(OnAddLuminPackageRequestFinished);



			void OnAddLuminPackageRequestFinished(bool success)
			{
				if (success)
				{
					if (data.LuminSettingEnabled)
					{
						Debug.LogError("DONE");
						CheckSDKAvailability(data);
					}
				}
				else
				{
					Debug.LogError(string.Format(FAILED_TO_EXECUTE_ERROR, "Add Lumin Sdk Package"));
				}


				BusyCounter--;
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
				MagicLeapLuminPackageUtility.CheckForMagicLeapSdkPackage(OnCheckForMagicLeapPackageInPackageManager);

			}



			void OnCheckForMagicLeapPackageInPackageManager(bool hasPackage)
			{
				//Debug.Log($"OnCheckForMagicLeapPackageInPackageManager: hasPackage: {hasPackage}");
				data.RefreshVariables();
				BusyCounter--;
				data.HasMagicLeapSdkInPackageManager = hasPackage;
			}



			void OnCheckForLuminRequestFinished(bool success, bool hasLumin)
			{
				//Debug.Log($"OnCheckForLuminRequestFinished: success: {success} | hasLumin: {hasLumin}");
				if (success && hasLumin)
				{
					data.RefreshVariables();
				}

				BusyCounter--;
				data.CheckingAvailability = false;
			}
		}
	}
}
