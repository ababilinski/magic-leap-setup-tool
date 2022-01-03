#region

using System;
using System.Reflection;
using UnityEditor;

#endregion

public static class UnityConsoleUtility
{
    private static Type _actualType { get; set; }

    public static Type UnityConsoleType
    {
        get
        {
            if (_actualType == null) _actualType = typeof(Editor).Assembly.GetType("UnityEditor.LogEntries", true);

            return _actualType;
        }
    }

    public static int GetErrorCount()
    {
        object[] parameters = {0, 0, 0};

        var methodInfo = UnityConsoleType.GetMethod("GetCountsByType",
            BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
            BindingFlags.Static);

        methodInfo.Invoke(null, parameters);

        return (int) parameters[0];
    }
}