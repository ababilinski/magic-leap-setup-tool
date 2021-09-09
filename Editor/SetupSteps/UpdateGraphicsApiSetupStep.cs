using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.ScriptableObjects;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MagicLeapSetupTool.Editor.Setup
{
	/// <summary>
	///     Changes the graphics APIs to work with Magic Leap and Zero Iteration
	/// </summary>
	public class UpdateGraphicsApiSetupStep : ISetupStep
	{
		private const string SET_CORRECT_GRAPHICS_API_LABEL = "Add OpenGLCore to Graphics API";
		private const string SET_CORRECT_GRAPHICS_BUTTON_LABEL = "Update";
		private const string CONDITION_MET_LABEL = "Done";
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
			if (CustomGuiContent.CustomButtons.DrawConditionButton(SET_CORRECT_GRAPHICS_API_LABEL, data.HasCorrectGraphicConfiguration, CONDITION_MET_LABEL, SET_CORRECT_GRAPHICS_BUTTON_LABEL, Styles.FixButtonStyle))
			{
				Execute(data);
				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public void Execute(MagicLeapSetupDataScriptableObject data)
		{
			if (data.HasCorrectGraphicConfiguration)
			{
				return;
			}

			UpdateGraphicsSettings(data);
		}

		/// <summary>
		///     Changes the graphics settings for all Lumin platforms
		/// </summary>
		/// <param name="data"></param>
		public static void UpdateGraphicsSettings(MagicLeapSetupDataScriptableObject data)
		{
			BusyCounter++;

			var standaloneWindowsResetRequired =
				UnityProjectSettingsUtility.SetGraphicsApi(BuildTarget.StandaloneWindows, GraphicsDeviceType.OpenGLCore,
															0);
			var standaloneWindows64ResetRequired =
				UnityProjectSettingsUtility.SetGraphicsApi(BuildTarget.StandaloneWindows64,
															GraphicsDeviceType.OpenGLCore, 0);
			var standaloneOSXResetRequired =
				UnityProjectSettingsUtility.SetGraphicsApi(BuildTarget.StandaloneOSX, GraphicsDeviceType.OpenGLCore, 0);
			var standaloneLinuxResetRequired =
				UnityProjectSettingsUtility.SetGraphicsApi(BuildTarget.StandaloneLinux64, GraphicsDeviceType.OpenGLCore,
															0);


			UnityProjectSettingsUtility.SetAutoGraphicsApi(BuildTarget.StandaloneWindows, false);
			UnityProjectSettingsUtility.SetAutoGraphicsApi(BuildTarget.StandaloneWindows64, false);
			UnityProjectSettingsUtility.SetAutoGraphicsApi(BuildTarget.StandaloneOSX, false);
			UnityProjectSettingsUtility.SetAutoGraphicsApi(BuildTarget.StandaloneLinux64, false);
			data.RefreshVariables();

			MagicLeapSetupAutoRun.Stop();

			if (standaloneWindowsResetRequired
			|| standaloneWindows64ResetRequired
			|| standaloneOSXResetRequired
			|| standaloneLinuxResetRequired)
			{
				UnityProjectSettingsUtility.UpdateGraphicsApi(true);
			}
			else
			{
				UnityProjectSettingsUtility.UpdateGraphicsApi(false);
			}


			BusyCounter--;
		}
	}
}
