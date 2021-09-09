using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor.Setup
{
	/// <summary>
	///     Switches the Color Space to Linear
	/// </summary>
	public class ColorSpaceSetupStep : ISetupStep
	{
		private const string COLOR_SPACE_LABEL = "Set Color Space To Linear";
		private const string CONDITION_MET_LABEL = "Done";
		private const string FIX_SETTING_BUTTON_LABEL = "Fix Setting";

		/// <inheritdoc />
		public bool Draw(MagicLeapSetupDataScriptableObject data)
		{
			data.CorrectColorSpace = PlayerSettings.colorSpace == ColorSpace.Linear;
			if (CustomGuiContent.CustomButtons.DrawConditionButton(COLOR_SPACE_LABEL, data.CorrectColorSpace, CONDITION_MET_LABEL, FIX_SETTING_BUTTON_LABEL, Styles.FixButtonStyle))
			{
				Execute(data);
				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public void Execute(MagicLeapSetupDataScriptableObject data)
		{
			if (data.CorrectColorSpace)
			{
				return;
			}

			PlayerSettings.colorSpace = ColorSpace.Linear;
			data.CorrectColorSpace = PlayerSettings.colorSpace == ColorSpace.Linear;

			Debug.Log($"Set Color Space to: [{PlayerSettings.colorSpace}]");
		}
	}
}
