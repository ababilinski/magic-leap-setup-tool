#region

using System;
using System.IO;
using MagicLeapSetupTool.Editor.Setup;
using MagicLeapSetupTool.Editor.Utilities;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

#endregion

namespace MagicLeapSetupTool.Editor
{
    public class SetupData
    {
        private const string MAGIC_LEAP_DEFINES_SYMBOL = "MAGICLEAP";

        // Test this assembly. The name switched after the initial release 
        private const string TEST_FOR_PACKAGE_MANAGER_ML_SCRIPT_UPDATED = "com.magicleap.unitysdk";

        // Test this assembly. If it does not exist. The package is not imported. 
        private const string TEST_FOR_ML_SCRIPT = "UnityEngine.XR.MagicLeap.MLInput";

        // Test this assembly. If it does not exist. The package is not imported. 
        private const string TEST_FOR_PACKAGE_MANAGER_ML_SCRIPT = "com.magicleap.unitysdk";

        // Used to check if the build platform is installed
        private const string  MAGIC_LEAP_PACKAGE_ID = "com.magicleap.unitysdk";

        // Used to check if the build platform is installed
        private const string LUMIN_PACKAGE_ID = "com.unity.xr.magicleap";

        //Editor Pref key to set/get the Lumin SDK
        private const string LUMIN_SDK_PATH_KEY = "LuminSDKRoot";


        public static readonly string SdkRoot = EditorPrefs.GetString(LUMIN_SDK_PATH_KEY, null);
        public static readonly bool HasRootSDKPath = !string.IsNullOrEmpty(SdkRoot) && Directory.Exists(SdkRoot);

        public static readonly bool HasRootSDKPathInEditorPrefs =
            !string.IsNullOrEmpty(EditorPrefs.GetString(LUMIN_SDK_PATH_KEY, null));

        public static bool HasMagicLeapSdkInPackageManager;
        private static int _busyCounter;
        private int _currentImportSdkStep;
        public bool CheckingAvailability;
        public bool CorrectColorSpace;
        public bool EmbeddedPackage;
        public bool GetSdkFromPackageManager;
        public bool HasIncompatibleSDKAssetPackage;

        public bool HasMagicLeapSdkInstalled;

        public bool ImportMagicLeapPackageFromPackageManager;
        public bool IsRestarting;

        public bool Loading;

        public bool LuminSettingEnabled;
        public int SdkApiLevel;

        public static bool HasLuminInstalled
        {
            get
            {
#if MAGICLEAP
                return true;
#else
                return  false;
#endif
            }
        }

        public bool ManifestIsUpdated { private set; get; }

        private static int BusyCounter
        {
            get => _busyCounter;
            set => _busyCounter = Mathf.Clamp(value, 0, 100);
        }

        public bool Busy => BusyCounter > 0;


        public int CurrentImportSdkStep
        {
            get => _currentImportSdkStep;
            set
            {
                if (_currentImportSdkStep != value)
                {
                    _currentImportSdkStep = Mathf.Clamp(value, 0, 2);

                    AssetDatabase.SaveAssets();
                }
            }
        }

        /// <summary>
        /// Sets the Lumin SDK path in the Preferences window
        /// </summary>
        /// <param name="path"></param>
        public void SetSdkRoot(string path)
        {
            EditorPrefs.SetString(LUMIN_SDK_PATH_KEY, SdkRoot);
            RefreshVariables();
        }


        /// <summary>
        /// Checks if the Magic Leap SDK is installed from the package manager and if it is embedded into the project
        /// </summary>
        private void CheckSdkPackageState()
        {
            var versionLabel = MagicLeapLuminPackageUtility.GetSdkVersion();
            if (Version.TryParse(versionLabel, out var currentVersion))
            {
                ImportMagicLeapPackageFromPackageManager = true;
                var packagePath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Packages/"))
                    .Replace('\\', '/');
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


        /// <summary>
        /// Updates the variables based on if the the  Lumin SDK are installed
        /// </summary>
        private void CheckForLuminSdkPackage()
        {
            BusyCounter++;
            PackageUtility.HasPackageInstalled(LUMIN_PACKAGE_ID, OnCheckForLuminRequestFinished);


            void OnCheckForLuminRequestFinished(bool success, bool hasLumin)
            {
                if (!success || !hasLumin) MagicLeapSetupAutoRun.Stop();

                BusyCounter--;
                CheckingAvailability = false;
            }
        }

        /// <summary>
        /// Updates the variables based on if the the Magic Leap SDK are installed
        /// </summary>
        public static void CheckForMagicLeapSdkPackage()
        {
            BusyCounter++;
            PackageUtility.HasPackageInstalled(MAGIC_LEAP_PACKAGE_ID, onCheckForMagicLeapPackageInPackageManager);


            void onCheckForMagicLeapPackageInPackageManager(bool success, bool hasPackage)
            {
                BusyCounter--;
                HasMagicLeapSdkInPackageManager = hasPackage;
            }
        }

        // The method is expected to receive a PackageRegistrationEventArgs event argument to check if the Magic Leap SDK is being installed or uninstalled.
        public static void RegisteringPackagesEventHandler(PackageRegistrationEventArgs packageRegistrationEventArgs)
        {
            foreach (var addedPackage in packageRegistrationEventArgs.added)
            {

                if (addedPackage.name == LUMIN_PACKAGE_ID)
                {
                    if (!DefineSymbolsUtility.ContainsDefineSymbol(MAGIC_LEAP_DEFINES_SYMBOL))
                        DefineSymbolsUtility.AddDefineSymbol(MAGIC_LEAP_DEFINES_SYMBOL);
                }

            }

            foreach (var removedPackage in packageRegistrationEventArgs.removed)
            {

                if (removedPackage.name == LUMIN_PACKAGE_ID)
                {
                    if (DefineSymbolsUtility.ContainsDefineSymbol(MAGIC_LEAP_DEFINES_SYMBOL))
                        DefineSymbolsUtility.RemoveDefineSymbol(MAGIC_LEAP_DEFINES_SYMBOL);
                }

            }

        }

        public static void UpdateDefineSymbols()
        {
            EditorApplication.delayCall += () =>
            {
                if (!DefineSymbolsUtility.DirectoryPathExistsWildCard(
                    Path.GetFullPath(Path.Combine(Application.dataPath, "../Library/PackageCache")),
                    LUMIN_PACKAGE_ID))
                {
                    if (DefineSymbolsUtility.ContainsDefineSymbol(MAGIC_LEAP_DEFINES_SYMBOL))
                        DefineSymbolsUtility.RemoveDefineSymbol(MAGIC_LEAP_DEFINES_SYMBOL);
                }
                else
                {
                    if (!DefineSymbolsUtility.ContainsDefineSymbol(MAGIC_LEAP_DEFINES_SYMBOL))
                        DefineSymbolsUtility.AddDefineSymbol(MAGIC_LEAP_DEFINES_SYMBOL);
                }
            };
        }


        public void RefreshVariables()
        {
            if (Busy) return;

            BusyCounter++;


            SdkApiLevel = MagicLeapLuminPackageUtility.GetSdkApiLevel();
            GetSdkFromPackageManager = MagicLeapLuminPackageUtility.UseSdkFromPackageManager();
            LuminSettingEnabled = MagicLeapLuminPackageUtility.IsLuminXREnabled();

            HasIncompatibleSDKAssetPackage = MagicLeapLuminPackageUtility.HasIncompatibleUnityAssetPackage();
            var typeInfo = TypeUtility.FindTypeByPartialName(TEST_FOR_ML_SCRIPT);
            if (typeInfo != null)
                Debug.Log($"TYPE FOUND: {typeInfo.FullName} || Assembly: {typeInfo.Assembly.FullName}");
            HasMagicLeapSdkInstalled = typeInfo != null;

#if MAGICLEAP
            ManifestIsUpdated = MagicLeapLuminPackageUtility.MagicLeapManifest != null &&
                                MagicLeapLuminPackageUtility.MagicLeapManifest.minimumAPILevel == SdkApiLevel;
#else
			ManifestIsUpdated = false;
#endif
            CorrectColorSpace = PlayerSettings.colorSpace == ColorSpace.Linear;
            CheckSdkPackageState();

            BusyCounter--;
        }
    }
}