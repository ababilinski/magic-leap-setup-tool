#region

using System.IO;
using UnityEditor;
using UnityEngine;

#endregion

namespace MagicLeapSetupTool.Editor.Utilities
{
    public static class EditorKeyUtility
    {
        internal const string MAGIC_LEAP_SETUP_POSTFIX_KEY = "MAGIC_LEAP_SETUP_KEY";
        internal const string PREVIOUS_CERTIFICATE_PROMPT_KEY = "PREVIOUS_CERTIFICATE_PROMPT_KEY";
        internal const string MAGIC_LEAP_SETUP_CLOSED_WINDOW_KEY = "MAGIC_LEAP_SETUP_CLOSED_WINDOW_KEY";

        private static string ProjectKeyAndPath
        {
            get
            {
                var projectKey = GetProjectKey();
                var path = Path.GetFullPath(Application.dataPath);
                return $"[{projectKey}]-[{path}]";
            }
        }

        public static string PreviousCertificatePrompt => $"{PREVIOUS_CERTIFICATE_PROMPT_KEY}_{ProjectKeyAndPath}";
        public static string WindowClosedEditorPrefKey => $"{MAGIC_LEAP_SETUP_CLOSED_WINDOW_KEY}_{ProjectKeyAndPath}";
        public static string AutoShowEditorPrefKey => $"{MAGIC_LEAP_SETUP_POSTFIX_KEY}_{ProjectKeyAndPath}";

        public static string GetProjectKey()
        {
            return PlayerSettings.companyName + "." + PlayerSettings.productName;
        }
    }
}