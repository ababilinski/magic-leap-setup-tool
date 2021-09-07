using System;
using System.IO;
using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor.Setup
{
	public class SetSdkFolderSetupStep: ISetupStep
	{

	#region GUI TEXT

		private const string LOCATE_SDK_FOLDER_LABEL = "Set external Lumin SDK Folder";
		private const string CONDITION_MET_CHANGE_LABEL = "Change";
		private const string LOCATE_SDK_FOLDER_BUTTON_LABEL = "Locate SDK";
        private const string SDK_FILE_BROWSER_TITLE = "Set external Lumin SDK Folder";        //Title text of SDK path browser

    #endregion

	#region DEBUG LOGS
        private const string SET_MAGIC_LEAP_DIR_MESSAGE = "Updated Magic Leap SDK path to [{0}].";  //[0] folder path
    #endregion
		/// <inheritdoc />
		public bool Draw(MagicLeapSetupDataScriptableObject data)
		{

			if (CustomGuiContent.CustomButtons.DrawConditionButton(new GUIContent(LOCATE_SDK_FOLDER_LABEL),
																   data.HasRootSDKPath,
																   new GUIContent(CONDITION_MET_CHANGE_LABEL,
																	   data.SdkRoot),
																   new GUIContent(LOCATE_SDK_FOLDER_BUTTON_LABEL),
																   Styles.FixButtonStyle, false))
			{
				Execute(data);
				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public void Execute(MagicLeapSetupDataScriptableObject data)
		{
			BrowseForSDK(data);
		}

		public static string GetCurrentSDKLocation(MagicLeapSetupDataScriptableObject data)
		{
			var currentPath = data.SdkRoot;
			if (string.IsNullOrEmpty(currentPath) || !Directory.Exists(currentPath))
			{
				currentPath = DefaultSDKPath();
			}

			//select folder just outside of the version folder i.e: PATH/v[x].[x].[x]
			if (currentPath.Contains("v"))
			{
				return Path.GetFullPath(Path.Combine(currentPath, "../"));
			}

			return currentPath;
		}

		public static void BrowseForSDK(MagicLeapSetupDataScriptableObject data)
		{
			var path = EditorUtility.OpenFolderPanel(SDK_FILE_BROWSER_TITLE, GetCurrentSDKLocation(data),
													 GetCurrentSDKFolderName(data));
			if (path.Length != 0)
			{
				SetRootSDK(data,path);
			}
		}

		public static string GetCurrentSDKFolderName(MagicLeapSetupDataScriptableObject data)
		{
			var currentPath = data.SdkRoot;
			if (string.IsNullOrEmpty(currentPath) || !Directory.Exists(currentPath))
			{
				currentPath = FindSDKPath(data);
			}

			//version folder i.e: v[x].[x].[x]
			if (currentPath.Contains("v"))
			{
				var dirName = new DirectoryInfo(currentPath).Name;
				return dirName;
			}

			return "";
		}

		public static string DefaultSDKPath()
		{
			var root = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			if (string.IsNullOrEmpty(root))
			{
				root = Environment.GetEnvironmentVariable("HOME");
			}

			if (!string.IsNullOrEmpty(root))
			{
				var sdkRoot = Path.Combine(root, "MagicLeap/mlsdk/");
				return sdkRoot;
			}

			return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		}


		public static void SetRootSDK(MagicLeapSetupDataScriptableObject data, string path)
		{
			data.SetSdkRoot(path);
		
			Debug.Log(string.Format(SET_MAGIC_LEAP_DIR_MESSAGE, path));

		}

		public static string FindSDKPath(MagicLeapSetupDataScriptableObject data)
		{
			var editorSdkPath = data.SdkRoot;
			if (string.IsNullOrEmpty(editorSdkPath)
			 || !Directory.Exists(editorSdkPath) /* && File.Exists(Path.Combine(editorSdkPath, MANIFEST_PATH))*/)
			{
				var root = Environment.GetEnvironmentVariable("USERPROFILE")
						?? Environment.GetEnvironmentVariable("HOME");


				if (!string.IsNullOrEmpty(root))
				{
					var sdkRoot = Path.Combine(root, "MagicLeap/mlsdk/");
					if (!string.IsNullOrEmpty(sdkRoot))
					{
						var getVersionDirectories = Directory.EnumerateDirectories(sdkRoot, "v*");
						var newestVersion = new Version(0, 0, 0);
						var newestFolder = "";

						foreach (var versionDirectory in getVersionDirectories)
						{
							var dirName = new DirectoryInfo(versionDirectory).Name;
							var versionOfFolder = new Version(dirName.Replace("v", ""));
							var result = versionOfFolder.CompareTo(newestVersion);
							if (result > 0)
							{
								newestVersion = versionOfFolder;
								newestFolder = versionDirectory;
							}
						}

						if (!string.IsNullOrEmpty(newestFolder))
						{
							return editorSdkPath;
						}
					}
				}
			}
			else
			{
				return editorSdkPath;
			}

			return null;
		}
	}
}
