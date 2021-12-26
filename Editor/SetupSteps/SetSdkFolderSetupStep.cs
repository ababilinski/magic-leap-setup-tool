using System;
using System.IO;
using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor.Setup
{
	/// <summary>
	///     Sets the Lumin SDK folder in the Preferences window
	/// </summary>
	public class SetSdkFolderSetupStep : ISetupStep
	{
	#region GUI TEXT

		private const string LOCATE_SDK_FOLDER_LABEL = "Set external Lumin SDK Folder";
		private const string CONDITION_MET_CHANGE_LABEL = "Change";
		private const string LOCATE_SDK_FOLDER_BUTTON_LABEL = "Locate SDK";
		private const string SDK_FILE_BROWSER_TITLE = "Set external Lumin SDK Folder"; //Title text of SDK path browser

	#endregion

	#region DEBUG LOGS

		private const string SET_MAGIC_LEAP_DIR_MESSAGE = "Updated Magic Leap SDK path to [{0}]."; //[0] folder path

	#endregion

		private const string LUMIN_SDK_PATH_KEY = "LuminSDKRoot"; //Editor Pref key to set/get the Lumin SDK
		private static string _sdkRoot;
		private static bool _hasRootSDKPath;
		/// <inheritdoc />
		public void Refresh()
		{
		
			_hasRootSDKPath = MagicLeapLuminPackageUtility.HasRootSDKPath;
			_sdkRoot = MagicLeapLuminPackageUtility.SdkRoot;
		}
		/// <inheritdoc />
		public bool Draw()
		{
			if (CustomGuiContent.CustomButtons.DrawConditionButton(new GUIContent(LOCATE_SDK_FOLDER_LABEL), _hasRootSDKPath,
																	new GUIContent(CONDITION_MET_CHANGE_LABEL, _sdkRoot),
																	new GUIContent(LOCATE_SDK_FOLDER_BUTTON_LABEL),Styles.FixButtonStyle, false))
			{
				Execute();
				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public void Execute()
		{
			BrowseForSDK();
		}

		/// <summary>
		///     Gets the current SDK location. If none is found. returns the mlsdk folder
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string GetCurrentSDKLocation()
		{
			var currentPath = _sdkRoot;
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

		/// <summary>
		///     Opens dialogue to select SDK folder
		/// </summary>
		/// <param name="data"></param>
		public static void BrowseForSDK()
		{
			var path = EditorUtility.OpenFolderPanel(SDK_FILE_BROWSER_TITLE, GetCurrentSDKLocation(),
													GetCurrentSDKFolderName());
			if (path.Length != 0)
			{
				SetRootSDK(path);
			}
		}

		/// <summary>
		///     Gets current SDK folder name based on the SDK path
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string GetCurrentSDKFolderName()
		{
			var currentPath = _sdkRoot;
			if (string.IsNullOrEmpty(currentPath) || !Directory.Exists(currentPath))
			{
				currentPath = FindSDKPath();
			}

			//version folder i.e: v[x].[x].[x]
			if (currentPath.Contains("v"))
			{
				var dirName = new DirectoryInfo(currentPath).Name;
				return dirName;
			}

			return "";
		}

		/// <summary>
		///     Returns the default Magic Leap install path [HOME/MagicLeap/mlsdk/]
		/// </summary>
		/// <returns></returns>
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

		/// <summary>
		///     Sets the SDK path in the Unity Editor
		/// </summary>
		/// <param name="data"></param>
		/// <param name="path"></param>
		public static void SetRootSDK(string path)
		{
			EditorPrefs.SetString(LUMIN_SDK_PATH_KEY, path);

			Debug.Log(string.Format(SET_MAGIC_LEAP_DIR_MESSAGE, path));
		}


		/// <summary>
		///     Finds the SDK path based on the default install location and newest added folder
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		public static string FindSDKPath()
		{
			var editorSdkPath = _sdkRoot;
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
