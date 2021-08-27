

using System;
using System.IO;


namespace MagicLeapSetupTool.Editor.ScriptableObjects
{
using System.Linq;

using UnityEngine;

/// <summary>
/// Abstract class for making reload-proof singletons out of ScriptableObjects
/// Returns the asset created on editor, null if there is none
/// Based on https://www.youtube.com/watch?v=VBA1QCoEAX4
/// </summary>
/// <typeparam name="T">Type of the singleton</typeparam>

public abstract class SingletonScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
	static T _instance = null;
	public static T Instance
	{
		get
		{
			if (!_instance)
				_instance = FindAsset(_instance);
			
			return _instance;
		}
	}
	/// <summary>
	/// We have to find the asset through AssetDatabase in case the asset is not loaded.
	/// </summary>
	/// <typeparam name="TO"></typeparam>
	/// <param name="typeObject"></param>
	/// <returns></returns>
	public static TO FindAsset<TO>(TO typeObject) where TO : Object
	{
		var editorPrefKey =$"{typeof(TO)}_{Application.dataPath.Replace('/', '-').Replace('\\', '-')}";

#if UNITY_EDITOR
		if (UnityEditor.EditorPrefs.HasKey(editorPrefKey))
#else
				if (false)
#endif
		{
#if UNITY_EDITOR
			string objectPath = UnityEditor.EditorPrefs.GetString(editorPrefKey);
			var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(objectPath, typeof(TO)) as TO;

			if (asset == null)
			{
				var assets = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(TO));

				if (assets.Length > 0)
				{
					var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(assets[0]);
					var foundAsset = (TO)UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(TO));
					UnityEditor.EditorPrefs.SetString(editorPrefKey, assetPath);

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
			var assets = UnityEditor.AssetDatabase.FindAssets("t:" + typeof(TO));

			if (assets.Length > 0)
			{
				var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(assets[0]);
				var foundAsset = (TO)UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, typeof(TO));
				UnityEditor.EditorPrefs.SetString(editorPrefKey, assetPath);

				return foundAsset;

			}
#endif
			var asset = Resources.FindObjectsOfTypeAll<TO>().FirstOrDefault();

			if (asset != null)
			{
#if UNITY_EDITOR
				string relPath = UnityEditor.AssetDatabase.GetAssetPath(asset);
				UnityEditor.EditorPrefs.SetString(editorPrefKey, relPath);
#endif
				return asset;
			}





		}
#if UNITY_EDITOR
		var assetInstance = ScriptableObject.CreateInstance(typeof(TO));
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
			bool exists = Directory.Exists(scriptPath);

			if (!exists)
			{
				Directory.CreateDirectory(scriptPath);
			}

			var assetPath = scriptPath.Substring(projectPath.Length, scriptPath.Length- projectPath.Length);
			assetPath = Path.Combine(assetPath, $"{typeof(TO).Name}.asset");
			assetPath =assetPath.Replace("\\", "/");
			UnityEditor.AssetDatabase.CreateAsset(assetInstance, assetPath);
			UnityEditor.AssetDatabase.SaveAssets();
		}
							
#endif
		return default;
	}



	public static string GetScriptPath<TO>(TO typeObject) where TO : Object
	{
		var scriptName = typeof(TO).Name +".cs";
	
		string[] filePaths = Array.Empty<string>();
		filePaths = Directory.GetFiles(Application.dataPath, scriptName, SearchOption.AllDirectories);
		if (filePaths.Length == 0)
		{
			filePaths = Directory.GetFiles(Path.Combine(Application.dataPath,"../Packages"), scriptName, SearchOption.AllDirectories);
			
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

		string path = filePaths[0].Replace("\\", "/");
		return path;
	}
}
}
