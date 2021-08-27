﻿using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor.Setup
{
	public class BuildTargetSetupStep: ISetupStep
	{
		private const string FIX_SETTING_BUTTON_LABEL = "Fix Setting";
		private const string CONDITION_MET_LABEL = "Done";
		private const string BUILD_SETTING_LABEL = "Set build target to Lumin";
		/// <inheritdoc />
		public void Draw(MagicLeapSetupDataScriptableObject data)
		{
			
			GUI.enabled = data.HasRootSDKPath && !data.Loading;

			data.CorrectBuildTarget = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Lumin;
			if (CustomGuiContent.CustomButtons.DrawConditionButton(BUILD_SETTING_LABEL, data.CorrectBuildTarget, CONDITION_MET_LABEL,
																   FIX_SETTING_BUTTON_LABEL, Styles.FixButtonStyle))
			{
				Execute(data);
				MagicLeapSetupWindow.RepaintUI();
			}
		}

		/// <inheritdoc />
		public void Execute(MagicLeapSetupDataScriptableObject data)
		{
			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Lumin, BuildTarget.Lumin);
			data.CorrectBuildTarget = true;
		}
	}
}