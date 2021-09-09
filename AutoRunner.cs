using System.IO;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor
{
	[InitializeOnLoad]
	public static class AutoRunner
	{
		
		internal const string MAGIC_LEAP_SETUP_POSTFIX_KEY = "MAGIC_LEAP_SETUP_KEY";
		private static string AutoShowEditorPrefKey
		{
			get
			{
				var projectKey = UnityProjectSettingsUtility.GetProjectKey();
				var path = Path.GetFullPath(Application.dataPath);
				return $"{MAGIC_LEAP_SETUP_POSTFIX_KEY}_[{projectKey}]-[{path}]";
			}
		}
		static AutoRunner()
		{
			EditorApplication.update += OnEditorApplicationUpdate;
			
		}
		

		private static void OnEditorApplicationUpdate()
		{
			if (!MagicLeapSetupDataScriptableObject.Instance)
			{
				return;
			}

			MagicLeapSetupDataScriptableObject.Instance.UpdateDefineSymbols();

			var autoShow = EditorPrefs.GetBool(AutoShowEditorPrefKey, true);
			if (!MagicLeapSetup.HasRootSDKPathInEditorPrefs
			 || !MagicLeapSetup.HasLuminInstalled
			 || EditorUserBuildSettings.activeBuildTarget != BuildTarget.Lumin
			 || !MagicLeapSetup.HasCompatibleMagicLeapSdk)
			{
				autoShow = true;
				EditorPrefs.SetBool(AutoShowEditorPrefKey, true);
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
