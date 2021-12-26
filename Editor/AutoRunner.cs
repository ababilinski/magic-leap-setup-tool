using System.IO;
using MagicLeapSetupTool.Editor.Setup;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor
{
	[InitializeOnLoad]
	public static class AutoRunner
	{
		
		private static readonly bool _hasLuminInstalled =
														#if MAGICLEAP
															true;
														#else
															false;
														#endif
		static AutoRunner()
		{
			EditorApplication.update += OnEditorApplicationUpdate;
			EditorApplication.quitting += OnQuit;
		}

		private static void OnQuit()
		{
			EditorApplication.quitting -= OnQuit;
			EditorPrefs.SetBool(EditorKeyUtility.WindowClosedEditorPrefKey, false);
		}
		

		private static void OnEditorApplicationUpdate()
		{

			SetupData.UpdateDefineSymbols();

			var autoShow = EditorPrefs.GetBool(EditorKeyUtility.AutoShowEditorPrefKey, true);
			if (!SetupData.HasRootSDKPathInEditorPrefs
			 || !_hasLuminInstalled
			 || BuildTargetSetupStep.CorrectBuildTarget
			 || !ImportMagicLeapSdkSetupStep.HasCompatibleMagicLeapSdk)
			{
				autoShow = true;
				EditorPrefs.SetBool(EditorKeyUtility.AutoShowEditorPrefKey, true);
			}

			EditorApplication.update -= OnEditorApplicationUpdate;
			if (!autoShow)
			{
				return;
			}

			MagicLeapSetupWindow.ForceOpen();
			
            
		}
	}
}
