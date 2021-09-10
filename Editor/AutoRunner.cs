using System.IO;
using MagicLeapSetupTool.Editor.ScriptableObjects;
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
			var data = MagicLeapSetupDataScriptableObject.Instance;
			if (!data)
			{
				return;
			}

			if (UnityConsoleUtility.GetErrorCount() > 0)
			{
				return;
			}
			MagicLeapSetupDataScriptableObject.Instance.UpdateDefineSymbols();

			var autoShow = EditorPrefs.GetBool(AutoShowEditorPrefKey, true);
			if (!data.HasRootSDKPathInEditorPrefs
			 || !data.HasLuminInstalled
			 || EditorUserBuildSettings.activeBuildTarget != BuildTarget.Lumin
			 || !data.HasCompatibleMagicLeapSdk)
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
