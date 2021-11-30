﻿using System;
using System.IO;
using MagicLeapSetupTool.Editor.Interfaces;
using MagicLeapSetupTool.Editor.ScriptableObjects;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace MagicLeapSetupTool.Editor.Setup
{
	/// <summary>
	///     Sets Developer Certificate
	/// </summary>
	public class SetCertificateSetupStep : ISetupStep
	{
		public static bool ValidCertificatePath;
		public static string PreviousCertificatePath;
		
		private readonly string SET_CERTIFICATE_PATH_LABEL = "Locate developer certificate";
		private const string FOUND_PREVIOUS_CERTIFICATE_TITLE = "Found Previously Used Developer Certificate";
		private const string FOUND_PREVIOUS_CERTIFICATE_MESSAGE = "Magic Leap Setup has found a previously used developer certificate. Would you like to use it in this project?";
		private const string FOUND_PREVIOUS_CERTIFICATE_OK = "Yes";
		private const string FOUND_PREVIOUS_CERTIFICATE_CANCEL = "Cancel";
		private const string FOUND_PREVIOUS_CERTIFICATE_ALT = "Browse For Certificate";
		private const string SET_CERTIFICATE_PATH_BUTTON_LABEL = "Locate";
		private const string CONDITION_MET_CHANGE_LABEL = "Change";
		private const string SET_CERTIFICATE_HELP_TEXT = "Get a developer certificate";
		private const string CERTIFICATE_FILE_BROWSER_TITLE = "Locate developer certificate"; //Title text of certificate file path browser
		private const string CERTIFICATE_EXTENSTION = "cert";                                 //extension to look for while browsing
		internal const string Get_CERTIFICATE_URL = "https://developer.magicleap.com/en-us/learn/guides/developer-certificates";
		private const string CERTIFICATE_PATH_KEY = "LuminCertificate"; //Editor Pref key to set/get previously used certificate
		private static string _certificatePath = "";

		private static readonly bool _hasLuminInstalled =
														#if MAGICLEAP
															true;
														#else
															false;
														#endif

		public static string CertificatePath
		{
			get
			{
				if (_hasLuminInstalled && (string.IsNullOrEmpty(_certificatePath) || !File.Exists(_certificatePath)))
				{
					_certificatePath = UnityProjectSettingsUtility.Lumin.GetInternalCertificatePath();
					EditorPrefs.SetString(CERTIFICATE_PATH_KEY, _certificatePath);
				}

				ValidCertificatePath = _hasLuminInstalled && !string.IsNullOrEmpty(_certificatePath) && File.Exists(_certificatePath);
				return _certificatePath;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					EditorPrefs.SetString(CERTIFICATE_PATH_KEY, value);
				}

				UnityProjectSettingsUtility.Lumin.SetInternalCertificatePath(value);
				_certificatePath = value;
				ValidCertificatePath = _hasLuminInstalled && !string.IsNullOrEmpty(_certificatePath) && File.Exists(_certificatePath);
			}
		}
		/// <inheritdoc />
		public void Refresh()
		{
			CertificatePath = UnityProjectSettingsUtility.Lumin.GetInternalCertificatePath();
			ValidCertificatePath = !string.IsNullOrEmpty(CertificatePath) && File.Exists(CertificatePath);
			PreviousCertificatePath = MagicLeapLuminPackageUtility.PreviousCertificatePath;

		}
		/// <inheritdoc />
		public bool Draw()
		{
			if (CustomGuiContent.CustomButtons.DrawConditionButton(SET_CERTIFICATE_PATH_LABEL, ValidCertificatePath,
																	new GUIContent(CONDITION_MET_CHANGE_LABEL, CertificatePath), SET_CERTIFICATE_PATH_BUTTON_LABEL, Styles.FixButtonStyle, SET_CERTIFICATE_HELP_TEXT, Get_CERTIFICATE_URL, false))
			{
				Execute();
				return true;
			}

			return false;
		}

		/// <inheritdoc />
		public void Execute()
		{
			var startDirectory = PreviousCertificatePath;
			if (!string.IsNullOrEmpty(startDirectory))
			{
				startDirectory = Path.GetDirectoryName(startDirectory);
			}

			var path = EditorUtility.OpenFilePanel(CERTIFICATE_FILE_BROWSER_TITLE, startDirectory,
													CERTIFICATE_EXTENSTION);
			if (path.Length != 0)
			{
				CertificatePath = path;
			}
		}

		public void FoundPreviousCertificateLocationPrompt(Action<bool> showAgain)
		{
			var usePreviousCertificateOption = EditorUtility.DisplayDialogComplex(FOUND_PREVIOUS_CERTIFICATE_TITLE, FOUND_PREVIOUS_CERTIFICATE_MESSAGE,
																				FOUND_PREVIOUS_CERTIFICATE_OK, FOUND_PREVIOUS_CERTIFICATE_CANCEL, FOUND_PREVIOUS_CERTIFICATE_ALT);

			switch (usePreviousCertificateOption)
			{
				case 0: //Yes
					SetCertificateSetupStep.CertificatePath = PreviousCertificatePath;
					AssetDatabase.SaveAssets();
					showAgain?.Invoke(true);
					break;
				case 1: //Cancel
					EditorPrefs.SetBool(EditorKeyUtility.PreviousCertificatePrompt, false);
					 showAgain?.Invoke(false);
					break;
				case 2: //Browse
					showAgain?.Invoke(true);
					Execute();
					break;
			}
		}
	}
}
