using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor.Setup
{
	public class ColorSpaceSetupStep : ISetupStep
	{
		private const string COLOR_SPACE_LABEL = "Set Color Space To Linear";
		private const string CONDITION_MET_LABEL = "Done";
		private const string FIX_SETTING_BUTTON_LABEL = "Fix Setting";
		/// <inheritdoc />
		public void Draw(MagicLeapSetupDataScriptableObject data)
		{
			if (CustomGuiContent.CustomButtons.DrawConditionButton(COLOR_SPACE_LABEL, PlayerSettings.colorSpace == ColorSpace.Linear, CONDITION_MET_LABEL, FIX_SETTING_BUTTON_LABEL, Styles.FixButtonStyle))
			{
				Execute(data);
				MagicLeapSetupWindow.RepaintUI();

			}
		}

		/// <inheritdoc />
		public void Execute(MagicLeapSetupDataScriptableObject data)
		{
			PlayerSettings.colorSpace = ColorSpace.Linear;
			data.CorrectColorSpace = PlayerSettings.colorSpace == ColorSpace.Linear;
			Debug.Log($"Set Color Space to: [{PlayerSettings.colorSpace}]");
		}
	}
}
