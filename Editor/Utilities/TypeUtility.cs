/* Copyright (C) 2021 Adrian Babilinski
* You may use, distribute and modify this code under the
* terms of the MIT License
*/

using System;
using System.Linq;
using System.Reflection;
using NUnit.Compatibility;
using UnityEngine;

namespace MagicLeapSetupTool.Editor.Utilities
{
    public static class TypeUtility
    {
        public static Type FindTypeByPartialName(string contains, string doesNotContain = null)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type returnType = null;
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();

                foreach (var scriptType in types)
                {
                    if (scriptType.FullName != null)
                    {
                        if (!string.IsNullOrEmpty(doesNotContain) && scriptType.FullName.Contains(doesNotContain))
                        {
                            continue;
                        }

                        //if (scriptType.FullName.ToUpper().Contains("LUMIN") || scriptType.FullName.ToUpper().Contains("MAGICLEAP") || scriptType.FullName.ToUpper().Contains("UNITYSDK"))
                        //{
                         //  Debug.Log($"TYPE: {scriptType.FullName} | {scriptType.Namespace} |  {scriptType.Assembly.GetName()} ::  {scriptType.Assembly.GetName(true)}");
                        //}

                        if (scriptType.FullName.Contains(contains))
                        {
                            returnType = scriptType;
                            break;
                        }
                    }
                }
            }


            return returnType;
        }

        /// <summary>
        ///     Checks if an assembly exists in the project by name.
        /// </summary>
        /// <param name="contains"> full or partial name</param>
        /// <param name="doesNotContain">filter</param>
        /// <returns></returns>
        public static bool AssemblyExists(string contains, string doesNotContain = null)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
       
              //  Debug.Log($"ASSEMBLY: {assembly.FullName}\nNAME:{assembly.GetName()}\nLOCATION: {assembly.Location}\nModule:{string.Join<Module>(",", assembly.GetModules())}");
      
                if (!string.IsNullOrEmpty(doesNotContain) && assembly.FullName.Contains(doesNotContain))
                {
                    continue;
                }

                if (assembly.FullName.Contains(contains))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
