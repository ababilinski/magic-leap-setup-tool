using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.Templates;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor.Setup
{
	/// <summary>
	/// Updates the SDK manifest file based on <see cref="MagicLeapSetupTool.Editor.Templates.DefaultPackageTemplate" />
	/// </summary>
	public class UpdateManifestSetupStep:ISetupStep
	{
		private const string UPDATE_MANIFEST_LABEL = "Update the manifest file";
		private const string UPDATE_MANIFEST_BUTTON_LABEL = "Update";
		private const string CONDITION_MET_LABEL = "Done";
		private const string ENABLE_PLUGIN_FAILED_PLUGIN_NOT_INSTALLED_MESSAGE = "Magic Leap Pug-in is not installed.";
		private static int _busyCounter;
		public int SdkApiLevel;
		public static bool ManifestIsUpdated { private set; get; }
		public bool LuminSettingEnabled;
		private static readonly bool _hasLuminInstalled =
														#if MAGICLEAP
															true;
														#else
															false;
														#endif
		public static int BusyCounter
		{
			get => _busyCounter;
			set
			{

				_busyCounter = Mathf.Clamp(value, 0, 100);
			}
		}

		public bool Busy => BusyCounter > 0;


		/// <inheritdoc />
		public void Refresh() 
		{
			SdkApiLevel = MagicLeapLuminPackageUtility.GetSdkApiLevel();

#if MAGICLEAP
			ManifestIsUpdated = MagicLeapLuminPackageUtility.MagicLeapManifest != null && MagicLeapLuminPackageUtility.MagicLeapManifest.minimumAPILevel == SdkApiLevel;
#else
			ManifestIsUpdated = false;
#endif
			LuminSettingEnabled = MagicLeapLuminPackageUtility.IsLuminXREnabled();
		}
		
		/// <inheritdoc />
		public bool Draw()
		{
			GUI.enabled = LuminSettingEnabled;
			 if (CustomGuiContent.CustomButtons.DrawConditionButton(UPDATE_MANIFEST_LABEL, ManifestIsUpdated, CONDITION_MET_LABEL, UPDATE_MANIFEST_BUTTON_LABEL, Styles.FixButtonStyle))
			 {
				 Execute();
				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public void Execute()
		{
			
			if (_hasLuminInstalled)
			{
				Debug.LogWarning(ENABLE_PLUGIN_FAILED_PLUGIN_NOT_INSTALLED_MESSAGE);
				return;
			}

			BusyCounter++;
#if MAGICLEAP
			Debug.Log($"Setting SDK Version To: {SdkApiLevel}");
			MagicLeapLuminPackageUtility.MagicLeapManifest.minimumAPILevel = SdkApiLevel;
			var serializedObject = new SerializedObject(MagicLeapLuminPackageUtility.MagicLeapManifest);
			var priv_groups = serializedObject.FindProperty("m_PrivilegeGroups");

			for (var i = 0; i < priv_groups.arraySize; i++)
			{
				var group = priv_groups.GetArrayElementAtIndex(i);


				var privs = group.FindPropertyRelative("m_Privileges");
				for (var j = 0; j < privs.arraySize; j++)
				{
					var priv = privs.GetArrayElementAtIndex(j);
					var enabled = priv.FindPropertyRelative("m_Enabled");
					var name = priv.FindPropertyRelative("m_Name").stringValue;
					if (DefaultPackageTemplate.DEFAULT_PRIVILEGES.Contains(name))
					{
						enabled.boolValue = true;
					}
				}
			}

			Debug.Log("Updated Privileges!");

			serializedObject.ApplyModifiedProperties();


			serializedObject.Update();
#endif
			
			BusyCounter--;
		}

	
	}
}
