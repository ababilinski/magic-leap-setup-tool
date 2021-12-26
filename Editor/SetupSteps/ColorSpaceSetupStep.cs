using MagicLeapSetupTool.Editor.Interfaces;
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
		private static bool _correctColorSpace;

		/// <inheritdoc />
		public void Refresh()
		{
			_correctColorSpace = PlayerSettings.colorSpace == ColorSpace.Linear;
		}
		
		/// <inheritdoc />
		public bool Draw()
		{
		
			if (CustomGuiContent.CustomButtons.DrawConditionButton(COLOR_SPACE_LABEL, _correctColorSpace, CONDITION_MET_LABEL, FIX_SETTING_BUTTON_LABEL, Styles.FixButtonStyle))
			{
				Execute();
				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public void Execute()
		{
			if (_correctColorSpace)
			{
				return;
			}

			PlayerSettings.colorSpace = ColorSpace.Linear;

			Debug.Log($"Set Color Space to: [{PlayerSettings.colorSpace}]");
		}
	}
}
