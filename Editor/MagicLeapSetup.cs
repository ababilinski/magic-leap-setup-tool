/* Copyright (C) 2021 Adrian Babilinski
* You may use, distribute and modify this code under the
* terms of the MIT License
*/

using System;
using System.IO;
using MagicLeapSetupTool.Editor.Templates;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MagicLeapSetupTool.Editor
{
    /// <summary>
    ///     <para>
    ///         Script responsible for communicating and managing values provided by
    ///         <see cref="MagicLeapLuminPackageUtility" /> as well as changing the project settings for lumin
    ///     </para>
    /// </summary>
    public static class MagicLeapSetup
    {


    #region GUI TEXT

        private const string CERTIFICATE_FILE_BROWSER_TITLE = "Locate developer certificate"; //Title text of certificate file path browser
        private const string CERTIFICATE_EXTENSTION = "cert";                                 //extension to look for while browsing


    #endregion


        private const string CERTIFICATE_PATH_KEY = "LuminCertificate"; //Editor Pref key to set/get previously used certificate
        private static string _certificatePath = "";

        private static int _busyCounter; 
        public static bool IsBusy => BusyCounter > 0;

        public static bool HasLuminInstalled
        {
            get
            {
#if MAGICLEAP
                return true;
#else
                return false;
#endif
            }
        }
        public static bool HasCompatibleMagicLeapSdk { get; private set; }
        public static bool HasMagicLeapSdkInstalled { get; private set; }
        public static bool HasCorrectGraphicConfiguration { get; private set; }
        public static bool LuminSettingEnabled { get; private set; }
        public static bool ValidCertificatePath { get; private set; }
        public static int SdkApiLevel { get; private set; }
        public static string PreviousCertificatePath { get; private set; }

        public static bool HasRootSDKPath { get; private set; }
        public static string CertificatePath
        {
            get
            {
                if (HasLuminInstalled && (string.IsNullOrEmpty(_certificatePath) || !File.Exists(_certificatePath)))
                {
                    _certificatePath = UnityProjectSettingsUtility.Lumin.GetInternalCertificatePath();
                }

                ValidCertificatePath = HasLuminInstalled && !string.IsNullOrEmpty(_certificatePath) && File.Exists(_certificatePath);
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
                ValidCertificatePath = HasLuminInstalled && !string.IsNullOrEmpty(_certificatePath) && File.Exists(_certificatePath);
            }
        }

        public static bool ManifestIsUpdated
        {
            get
            {
#if MAGICLEAP
                if (MagicLeapLuminPackageUtility.MagicLeapManifest == null)
                {
                    return false;
                }
            
                return MagicLeapLuminPackageUtility.MagicLeapManifest.minimumAPILevel == SdkApiLevel;
#else
                return false;
#endif
            }
        }

        public static int BusyCounter
        {
            get => _busyCounter;
            set
            {

                _busyCounter = Mathf.Clamp(value,0,100);
            }
        }

      


        public static void BrowseForCertificate()
        {
            var startDirectory = PreviousCertificatePath;
            if (!string.IsNullOrEmpty(startDirectory))
            {
                startDirectory = Path.GetDirectoryName(startDirectory);
            }

            var path = EditorUtility.OpenFilePanel(CERTIFICATE_FILE_BROWSER_TITLE, startDirectory, CERTIFICATE_EXTENSTION);
            if (path.Length != 0)
            {
                CertificatePath = path;
            }
        }



    }
}
