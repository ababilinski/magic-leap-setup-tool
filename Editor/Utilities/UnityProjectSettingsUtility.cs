/* Copyright (C) 2021 Adrian Babilinski
* You may use, distribute and modify this code under the
* terms of the MIT License
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MagicLeapSetupTool.Editor.Utilities
{
    public static class UnityProjectSettingsUtility
    {
        private static readonly Dictionary<BuildTarget, bool> _supportedPlatformByBuildTarget = new Dictionary<BuildTarget, bool>(); //memo to avoid requesting the same value multiple times

        public static bool IsPlatformSupported(BuildTarget buildTargetToTest)
        {
            if (_supportedPlatformByBuildTarget.TryGetValue(buildTargetToTest, out var supported))
            {
                return supported;
            }

            var moduleManager = Type.GetType("UnityEditor.Modules.ModuleManager,UnityEditor.dll");
            var isPlatformSupportLoaded = moduleManager.GetMethod("IsPlatformSupportLoaded", BindingFlags.Static | BindingFlags.NonPublic);
            var getTargetStringFromBuildTarget = moduleManager.GetMethod("GetTargetStringFromBuildTarget", BindingFlags.Static | BindingFlags.NonPublic);
            var buildTargetSupported = (bool) isPlatformSupportLoaded.Invoke(null, new object[] {(string) getTargetStringFromBuildTarget.Invoke(null, new object[] {buildTargetToTest})});
            _supportedPlatformByBuildTarget.Add(buildTargetToTest, buildTargetSupported);


            return buildTargetSupported;
        }

        public static void OpenProjectSettingsWindow(string providerName)
        {
            var window = EditorWindow.GetWindow(System.Type.GetType("UnityEditor.SettingsWindow,UnityEditor"));

            window.Show();
            var info = window.GetType().GetMethod("SelectProviderByName", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            info.Invoke(window, new[] {providerName});
        }

        public static void OpenXrManagementWindow()
        {
            OpenProjectSettingsWindow("Project/XR Plug-in Management");
        }
        public static void SetAutoGraphicsApi(BuildTarget buildTarget, bool automatic)
        {
            if (GetAutoGraphicsApi(buildTarget) != automatic)
            {
                var playerSettings = Type.GetType("UnityEditor.PlayerSettings,UnityEditor.dll");


                var methodInfo = playerSettings.GetMethod("SetUseDefaultGraphicsAPIs", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                methodInfo.Invoke(playerSettings, new object[] {buildTarget, automatic});
            }
        }

        public static bool GetAutoGraphicsApi(BuildTarget buildTarget)
        {
            var moduleManager = Type.GetType("UnityEditor.PlayerSettings,UnityEditor.dll");

            var isPlatformSupportLoaded = moduleManager.GetMethod("GetUseDefaultGraphicsAPIs", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var buildTargetSupported = (bool) isPlatformSupportLoaded.Invoke(moduleManager, new object[] {buildTarget});
            return buildTargetSupported;
        }

        public static void ClearSceneDirtiness(Scene scene)
        {
            var moduleManager = TypeUtility.FindTypeByPartialName("UnityEditor.SceneManagement.EditorSceneManager","+");
            var methodInfo = moduleManager.GetMethod("ClearSceneDirtiness", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            methodInfo.Invoke(null, new object[] {scene});
        }
        public static void RequestCloseAndRelaunchWithCurrentArguments()
        {
            var editorApplicationDll = Type.GetType("UnityEditor.EditorApplication,UnityEditor.dll");
            var requestClose = editorApplicationDll.GetMethod("RequestCloseAndRelaunchWithCurrentArguments", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            requestClose.Invoke(null, null);
       
        }
        public static bool HasGraphicsDeviceType(BuildTarget buildTarget, GraphicsDeviceType graphicsDeviceType)
        {
            if (IsPlatformSupported(BuildTarget.StandaloneWindows))
            {
                var graphics = PlayerSettings.GetGraphicsAPIs(buildTarget).ToList();
                return graphics.Contains(graphicsDeviceType);
            }

            return true;
        }

        public static bool HasGraphicsDeviceTypeAtIndex(BuildTarget buildTarget, GraphicsDeviceType graphicsDeviceType, int index)
        {
            //if (IsPlatformSupported(BuildTarget.StandaloneWindows))
            //{
                var graphics = PlayerSettings.GetGraphicsAPIs(buildTarget).ToList();
                return graphics[index] == graphicsDeviceType;
            //}

            //return true;
        }

        public static bool SetGraphicsApi(BuildTarget buildTarget, GraphicsDeviceType graphicsDeviceType, int index = 9999)
        {
            //if (IsPlatformSupported(buildTarget))
            //{
                var graphicsDeviceTypes = PlayerSettings.GetGraphicsAPIs(buildTarget).ToList();
                index = Mathf.Clamp(index, 0, graphicsDeviceTypes.Count);
                if (graphicsDeviceTypes.Contains(graphicsDeviceType))
                {
                    if (index == graphicsDeviceTypes.Count)
                    {
                        //already added and we don't care about the index
                        return false;
                    }

                    if (graphicsDeviceTypes[index] == graphicsDeviceType)
                    {
                        //already setup to the desired index
                        return false;
                    }

                    graphicsDeviceTypes.Remove(graphicsDeviceType);
                }

                if (index == graphicsDeviceTypes.Count)
                {

                    graphicsDeviceTypes.Add(graphicsDeviceType);
                }
                else
                {
                    graphicsDeviceTypes.Insert(index, graphicsDeviceType);
                }

                PlayerSettings.SetGraphicsAPIs(buildTarget, graphicsDeviceTypes.ToArray());
                return index == 0 && WillEditorUseFirstGraphicsAPI(buildTarget);
            //}
            //else return false;
        }

        private static bool WillEditorUseFirstGraphicsAPI(BuildTarget targetPlatform)
        {
            return
                Application.platform == RuntimePlatform.WindowsEditor && targetPlatform == BuildTarget.StandaloneWindows || Application.platform == RuntimePlatform.LinuxEditor && targetPlatform == BuildTarget.StandaloneLinux64 || Application.platform == RuntimePlatform.OSXEditor && targetPlatform == BuildTarget.StandaloneOSX;
        }

        public static string GetProjectKey()
        {
            return PlayerSettings.companyName + "." + PlayerSettings.productName;
        }


        public static class Lumin
        {
            public static string GetInternalCertificatePath()
            {
                var foundScript = TypeUtility.FindTypeByPartialName("UnityEditor.PlayerSettings+Lumin");


                if (foundScript != null)
                {
                    var sdkAPILevelProperty = foundScript.GetProperty("certificatePath", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                    if (sdkAPILevelProperty != null)
                    {
                        return (string) sdkAPILevelProperty.GetValue(foundScript, null);
                    }

                    Debug.LogError("Could not find Property: [certificatePath] in script [UnityEditor.PlayerSettings+Lumin]");
                }
                else
                {
                    Debug.LogError("Could not find Type: [UnityEditor.PlayerSettings+Lumin]");
                }

                return "NULL";
            }

            public static void SetInternalCertificatePath(string certPath)
            {
                var foundScript = TypeUtility.FindTypeByPartialName("UnityEditor.PlayerSettings+Lumin");


                if (foundScript != null)
                {
                    var sdkAPILevelProperty = foundScript.GetProperty("certificatePath", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                    if (sdkAPILevelProperty != null)
                    {
                        sdkAPILevelProperty.SetValue(foundScript, certPath);
                    }
                    else
                    {
                        Debug.LogError("Could not find Property: [certificatePath] in script [UnityEditor.PlayerSettings+Lumin]");
                    }
                }
            }
        }
    }
}
