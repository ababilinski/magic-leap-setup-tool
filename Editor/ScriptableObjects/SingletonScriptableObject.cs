using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MagicLeapSetupTool.Editor.ScriptableObjects
{
	/// <summary>
	///     Abstract class for making reload-proof singletons out of ScriptableObjects
	///     Returns the asset created on editor, null if there is none
	///     Based on https://www.youtube.com/watch?v=VBA1QCoEAX4
	/// </summary>
	/// <typeparam name="T">Type of the singleton</typeparam>
	public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
	{
		private static T _instance;

		public static T Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = FindAsset(_instance);
				}

				return _instance;
			}
		}

		/// <summary>
		///     We have to find the asset through AssetDatabase in case the asset is not loaded.
		/// </summary>
		/// <typeparam name="TO"></typeparam>
		/// <param name="typeObject"></param>
		/// <returns></returns>
		public static TO FindAsset<TO>(TO typeObject) where TO : Object
		{
			var editorPrefKey = $"{typeof(TO)}_{Application.dataPath.Replace('/', '-').Replace('\\', '-')}";

#if UNITY_EDITOR
			if (EditorPrefs.HasKey(editorPrefKey))
#else
				if (false)
#endif
			{
#if UNITY_EDITOR
				var objectPath = EditorPrefs.GetString(editorPrefKey);
				var asset = AssetDatabase.LoadAssetAtPath(objectPath, typeof(TO)) as TO;

				if (asset == null)
				{
					var assets = AssetDatabase.FindAssets("t:" + typeof(TO));

					if (assets.Length > 0)
					{
						var assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);
						var foundAsset = (TO)AssetDatabase.LoadAssetAtPath(assetPath, typeof(TO));
						EditorPrefs.SetString(editorPrefKey, assetPath);

						return foundAsset;
					}
				}
				else
				{
					return asset;
				}
#endif
			}
			else
			{
#if UNITY_EDITOR
				var assets = AssetDatabase.FindAssets("t:" + typeof(TO));

				if (assets.Length > 0)
				{
					var assetPath = AssetDatabase.GUIDToAssetPath(assets[0]);
					var foundAsset = (TO)AssetDatabase.LoadAssetAtPath(assetPath, typeof(TO));
					EditorPrefs.SetString(editorPrefKey, assetPath);

					return foundAsset;
				}
#endif
				var asset = Resources.FindObjectsOfTypeAll<TO>().FirstOrDefault();

				if (asset != null)
				{
#if UNITY_EDITOR
					var relPath = AssetDatabase.GetAssetPath(asset);
					EditorPrefs.SetString(editorPrefKey, relPath);
#endif
					return asset;
				}
			}
#if UNITY_EDITOR
			var assetInstance = CreateInstance(typeof(TO));
			var scriptPath = GetScriptPath(typeObject);
			scriptPath = Path.GetDirectoryName(scriptPath);
			if (!string.IsNullOrEmpty(scriptPath))
			{
				var scriptsFolder = scriptPath.IndexOf("Scripts");
				if (scriptsFolder > -1)
				{
					scriptPath = scriptPath.Substring(0, scriptsFolder);
					Debug.Log($"Looking at: {scriptPath}");
				}
				else
				{
					scriptPath = Path.Combine(scriptPath, "../");
				}

				var projectPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
				scriptPath = Path.GetFullPath(Path.Combine(scriptPath, "Assets"));
				var exists = Directory.Exists(scriptPath);

				if (!exists)
				{
					Directory.CreateDirectory(scriptPath);
				}

				var assetPath = scriptPath.Substring(projectPath.Length, scriptPath.Length - projectPath.Length);
				assetPath = Path.Combine(assetPath, $"{typeof(TO).Name}.asset");
				assetPath = assetPath.Replace("\\", "/");
				AssetDatabase.CreateAsset(assetInstance, assetPath);
				AssetDatabase.SaveAssets();
			}

#endif
			return default;
		}


		public static string GetScriptPath<TO>(TO typeObject) where TO : Object
		{
			var scriptName = typeof(TO).Name + ".cs";

			var filePaths = Array.Empty<string>();
			filePaths = Directory.GetFiles(Application.dataPath, scriptName, SearchOption.AllDirectories);
			if (filePaths.Length == 0)
			{
				filePaths = Directory.GetFiles(Path.Combine(Application.dataPath, "../Packages"), scriptName, SearchOption.AllDirectories);

				if (filePaths.Length == 0)
				{
					filePaths = Directory.GetFiles(Path.Combine(Application.dataPath, "../Library/PackageCache"),
													scriptName, SearchOption.AllDirectories);
					if (filePaths.Length == 0)
					{
						Debug.LogError($"Could not find path for {scriptName}. | Searched:\n[{Application.dataPath}]\n[{Path.GetFullPath(Path.Combine(Application.dataPath, "../Packages"))}]\n[{Path.GetFullPath(Path.Combine(Application.dataPath, "../Library/PackageCache"))}]");
						return null;
					}
				}
			}

			var path = filePaths[0].Replace("\\", "/");
			return path;
		}
	}
}
