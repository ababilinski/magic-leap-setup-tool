/* Copyright (C) 2021 Adrian Babilinski
* You may use, distribute and modify this code under the
* terms of the MIT License
*/

using System;

namespace MagicLeapSetupTool.Editor.Utilities
{
   
    public static class TypeUtility 
    {
        public static Type FindTypeByPartialName(string contains, string doesNotContain = null)
        {
            System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            Type returnType = null;
            foreach (var assembly in assemblies)
            {
                System.Type[] types = assembly.GetTypes();

                foreach (var scriptType in types)
                {
                    if (scriptType.FullName != null)
                    {
                        if (!string.IsNullOrEmpty(doesNotContain) && scriptType.FullName.Contains(doesNotContain))
                        {
                            continue;
                        }

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

    }
}
