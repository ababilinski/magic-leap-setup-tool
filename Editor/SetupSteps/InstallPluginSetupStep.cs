using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor.Setup
{
	/// <summary>
	///     Installs Lumin XR plugin
	/// </summary>
	public class InstallPluginSetupStep : ISetupStep
	{
		private const string FAILED_TO_EXECUTE_ERROR = "Failed to execute [{0}]"; //0 is method/action name
		private const string INSTALL_PLUGIN_LABEL = "Install the Lumin XR plug-in";
		private const string INSTALL_PLUGIN_BUTTON_LABEL = "Install Package";
		private const string CONDITION_MET_LABEL = "Done";
		private static int _busyCounter;
		private static bool _correctBuildTarget;
		private bool _hasRootSDKPath;
		public static readonly bool HasLuminInstalled =
														#if MAGICLEAP
															true;
														#else
															false;
														#endif
		
		public static int BusyCounter
		{
			get => _busyCounter;
			set => _busyCounter = Mathf.Clamp(value, 0, 100);
		}

		public static bool LuminSettingEnabled;
		public bool Busy => BusyCounter > 0;

		/// <inheritdoc />
		public void Refresh()
		{
			LuminSettingEnabled = MagicLeapLuminPackageUtility.IsLuminXREnabled();
			_correctBuildTarget = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin;
			_hasRootSDKPath = MagicLeapLuminPackageUtility.HasRootSDKPath;
		}
		/// <inheritdoc />
		public bool Draw()
		{
			//Makes sure the user changes to the Lumin Build Target before being able to set the other options
			GUI.enabled = _hasRootSDKPath && _correctBuildTarget;
			if (CustomGuiContent.CustomButtons.DrawConditionButton(INSTALL_PLUGIN_LABEL, HasLuminInstalled, CONDITION_MET_LABEL, INSTALL_PLUGIN_BUTTON_LABEL, Styles.FixButtonStyle))
			{
				Execute();
				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public void Execute()
		{
			BusyCounter++;
			MagicLeapLuminPackageUtility.AddLuminSdkPackage(OnAddLuminPackageRequestFinished);



			void OnAddLuminPackageRequestFinished(bool success)
			{
				if (success)
				{
					if (LuminSettingEnabled)
					{
						Debug.LogError("DONE");
						CheckSDKAvailability();
					}
				}
				else
				{
					Debug.LogError(string.Format(FAILED_TO_EXECUTE_ERROR, "Add Lumin Sdk Package"));
				}


				BusyCounter--;
			}
		}

		/// <summary>
		///     Checks for the Lumin SDK availability and refreshes the Scriptable Object variables
		///     <see cref="MagicLeapSetupTool.Editor.ScriptableObjects.MagicLeapSetupDataScriptableObject.RefreshVariables" />
		/// </summary>
		/// <param name="data"></param>
		public void CheckSDKAvailability()
		{
			MagicLeapLuminPackageUtility.UpdateDefineSymbols();
			

			if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin)
			{
			
				BusyCounter++;
				MagicLeapLuminPackageUtility.CheckForMagicLeapSdkPackage(OnCheckForMagicLeapPackageInPackageManager);
			}



			void OnCheckForMagicLeapPackageInPackageManager(bool hasPackage)
			{
				//Debug.Log($"OnCheckForMagicLeapPackageInPackageManager: hasPackage: {hasPackage}");
			
				BusyCounter--;
		
			}




		}
	}
}
