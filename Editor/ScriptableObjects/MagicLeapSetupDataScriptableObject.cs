using System;
using System.IO;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MagicLeapSetupTool.Editor.ScriptableObjects
{
	/// <summary>
	///     Scriptable Object that tracks the state of the setup process
	/// </summary>
	public class MagicLeapSetupDataScriptableObject : SingletonScriptableObject<MagicLeapSetupDataScriptableObject>
	{
		
	
	
	
		
		private const string MAGIC_LEAP_PACKAGE_ID = "com.magicleap.unitysdk";                                                // Used to check if the build platform is installed
		private const string LUMIN_PACKAGE_ID = "com.unity.xr.magicleap";                                                     // Used to check if the build platform is installed
		


	


		public bool IsRestarting;
	
	
		
	
	

		
	
	
		public bool EmbeddedPackage;
	

		private static int _busyCounter;

		public static int BusyCounter
		{
			get => _busyCounter;
			set
			{
			//	Debug.Log($"set: {value} was {_busyCounter}");
				_busyCounter = Mathf.Clamp(value, 0, 100);
			} 
		}

		public bool Busy => BusyCounter > 0;


		private int _currentImportSdkStep;
		public int CurrentImportSdkStep
		{
			get => _currentImportSdkStep;
			set
			{
				if (_currentImportSdkStep != value)
				{
					_currentImportSdkStep = Mathf.Clamp(value, 0, 2);
					EditorUtility.SetDirty(this);
					AssetDatabase.SaveAssets();
				}
			}
		}



		/// <summary>
		///     Refreshes the variables and conditions
		/// </summary>
		public void RefreshVariables()
		{
	
			if (Busy)
			{
				return;
			}

			BusyCounter++;
			
			
	
		
		

			

		
		

		
			CheckSdkPackageState();

			BusyCounter--;
		}
		
		/// <summary>
		///     Checks if the Magic Leap SDK is installed from the package manager and if it is embedded into the project
		/// </summary>
		private void CheckSdkPackageState()
		{
			var versionLabel = MagicLeapLuminPackageUtility.GetSdkVersion();
			if (Version.TryParse(versionLabel, out var currentVersion))
			{
				if (currentVersion < new Version(0, 26, 0))
				{
				//	ImportMagicLeapPackageFromPackageManager = false;
				}
				else
				{
				//	ImportMagicLeapPackageFromPackageManager = true;
					var packagePath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Packages/")).Replace('\\', '/');
					var embedded = DefineSymbolsUtility.DirectoryPathExistsWildCard(packagePath, "com.magicleap.unitysdk");
					if (embedded)
					{
						CurrentImportSdkStep = 2;
					}
					else
					{
						BusyCounter++;
						CurrentImportSdkStep = 0;
						PackageUtility.HasPackage("com.magicleap.unitysdk", OnCheckedPackagedList);



						void OnCheckedPackagedList(bool exists)
						{
							CurrentImportSdkStep = exists ? 1 : 0;

							BusyCounter--;
						}
					}
				}
			}
		}



		/// <summary>
		///     Updates the compilation define symbols based on if the Lumin SDK is installed
		/// </summary>
	






	}
}
