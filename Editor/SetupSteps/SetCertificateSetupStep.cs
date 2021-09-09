using System.IO;
using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor.Setup
{
	/// <summary>
	///     Sets Developer Certificate
	/// </summary>
	public class SetCertificateSetupStep : ISetupStep
	{
		private readonly string SET_CERTIFICATE_PATH_LABEL = "Locate developer certificate";
		private const string SET_CERTIFICATE_PATH_BUTTON_LABEL = "Locate";
		private const string CONDITION_MET_CHANGE_LABEL = "Change";
		private const string SET_CERTIFICATE_HELP_TEXT = "Get a developer certificate";
		private const string CERTIFICATE_FILE_BROWSER_TITLE = "Locate developer certificate"; //Title text of certificate file path browser
		private const string CERTIFICATE_EXTENSTION = "cert";                                 //extension to look for while browsing
		internal const string Get_CERTIFICATE_URL = "https://developer.magicleap.com/en-us/learn/guides/developer-certificates";

		/// <inheritdoc />
		public bool Draw(MagicLeapSetupDataScriptableObject data)
		{
			if (CustomGuiContent.CustomButtons.DrawConditionButton(SET_CERTIFICATE_PATH_LABEL, data.ValidCertificatePath,
																	new GUIContent(CONDITION_MET_CHANGE_LABEL, data.CertificatePath), SET_CERTIFICATE_PATH_BUTTON_LABEL, Styles.FixButtonStyle, SET_CERTIFICATE_HELP_TEXT, Get_CERTIFICATE_URL, false))
			{
				Execute(data);
				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public void Execute(MagicLeapSetupDataScriptableObject data)
		{
			var startDirectory = data.PreviousCertificatePath;
			if (!string.IsNullOrEmpty(startDirectory))
			{
				startDirectory = Path.GetDirectoryName(startDirectory);
			}

			var path = EditorUtility.OpenFilePanel(CERTIFICATE_FILE_BROWSER_TITLE, startDirectory,
													CERTIFICATE_EXTENSTION);
			if (path.Length != 0)
			{
				data.CertificatePath = path;
			}
		}
	}
}
