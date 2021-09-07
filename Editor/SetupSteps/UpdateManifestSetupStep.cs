using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.ScriptableObjects;
using MagicLeapSetupTool.Editor.Templates;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor.Setup
{
	public class UpdateManifestSetupStep:ISetupStep
	{
		private const string UPDATE_MANIFEST_LABEL = "Update the manifest file";
		private const string UPDATE_MANIFEST_BUTTON_LABEL = "Update";
		private const string CONDITION_MET_LABEL = "Done";
		private const string ENABLE_PLUGIN_FAILED_PLUGIN_NOT_INSTALLED_MESSAGE = "Magic Leap Pug-in is not installed.";
		private static int _busyCounter;

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
		public bool Draw(MagicLeapSetupDataScriptableObject data)
		{
			GUI.enabled = data.LuminSettingEnabled && !data.Loading;
			 if (CustomGuiContent.CustomButtons.DrawConditionButton(UPDATE_MANIFEST_LABEL, data.ManifestIsUpdated, CONDITION_MET_LABEL, UPDATE_MANIFEST_BUTTON_LABEL, Styles.FixButtonStyle))
			 {
				 Execute(data);
				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public void Execute(MagicLeapSetupDataScriptableObject data)
		{
			
			if (!data.HasLuminInstalled)
			{
				Debug.LogWarning(ENABLE_PLUGIN_FAILED_PLUGIN_NOT_INSTALLED_MESSAGE);
				return;
			}

			BusyCounter++;
#if MAGICLEAP
			Debug.Log($"Setting SDK Version To: {data.SdkApiLevel}");
			data.RefreshVariables();
			MagicLeapLuminPackageUtility.MagicLeapManifest.minimumAPILevel = data.SdkApiLevel;
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
