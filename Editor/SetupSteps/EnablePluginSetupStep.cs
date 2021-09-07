using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.ScriptableObjects;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor.Setup
{
	public class EnablePluginSetupStep: ISetupStep
	{
		private const string ENABLE_PLUGIN_FAILED_PLUGIN_NOT_INSTALLED_MESSAGE = "Magic Leap Pug-in is not installed.";
		private const string ENABLE_PLUGIN_LABEL = "Enable Plugin";
		private const string ENABLE_PLUGIN_SETTINGS_LABEL = "Enable the Lumin XR plug-in";
		private const string CONDITION_MET_LABEL = "Done";
		private const string ENABLE_LUMIN_FINISHED_UNSUCCESSFULLY_WARNING = "Unsuccessful call:[{0}]. action finished, but Lumin XR Settings are still not enabled."; //0 is method/action name
		private const string FAILED_TO_EXECUTE_ERROR = "Failed to execute [{0}]"; //0 is method/action name
		private static int _busyCounter;
		public static int BusyCounter
		{
			get => _busyCounter;
			set => _busyCounter = Mathf.Clamp(value, 0, 100);
		}

		public bool Busy => BusyCounter > 0;

		/// <inheritdoc />
		public bool Draw(MagicLeapSetupDataScriptableObject data)
		{
			GUI.enabled = data.HasRootSDKPath && data.CorrectBuildTarget && data.HasLuminInstalled && !data.Loading;
			 if (CustomGuiContent.CustomButtons.DrawConditionButton(ENABLE_PLUGIN_SETTINGS_LABEL, data.LuminSettingEnabled, CONDITION_MET_LABEL, ENABLE_PLUGIN_LABEL, Styles.FixButtonStyle))
			 {
				 Execute(data);
				return true;
			}
			return false;
		}

		/// <inheritdoc />
		public void Execute(MagicLeapSetupDataScriptableObject data)
		{
			if(data.LuminSettingEnabled)
			return;
			
			if (!data.HasLuminInstalled)
			{
				Debug.LogWarning(ENABLE_PLUGIN_FAILED_PLUGIN_NOT_INSTALLED_MESSAGE);
				return;
			}

			BusyCounter++;
			MagicLeapLuminPackageUtility.EnableLuminXRFinished += OnEnableMagicLeapPluginFinished;
			MagicLeapLuminPackageUtility.EnableLuminXRPlugin();



			void OnEnableMagicLeapPluginFinished(bool success)
			{
				if (success)
				{
					data.RefreshVariables();
					data.LuminSettingEnabled = MagicLeapLuminPackageUtility.IsLuminXREnabled();
					if (!data.LuminSettingEnabled)
					{
						Debug.LogWarning(string.Format(ENABLE_LUMIN_FINISHED_UNSUCCESSFULLY_WARNING,
													   "Enable Lumin XR action"));
					}
				}
				else
				{
					Debug.LogError(string.Format(FAILED_TO_EXECUTE_ERROR, "Enable Lumin XR Package"));
				}

				BusyCounter--;
				MagicLeapLuminPackageUtility.EnableLuminXRFinished -= OnEnableMagicLeapPluginFinished;
			}
			UnityProjectSettingsUtility.OpenXrManagementWindow();
			data.UpdateDefineSymbols();
		}


	
	}
}
