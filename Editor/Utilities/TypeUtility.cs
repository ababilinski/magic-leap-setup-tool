/* Copyright (C) 2021 Adrian Babilinski
* You may use, distribute and modify this code under the
* terms of the MIT License
*/

using System;
using System.Collections.Generic;

namespace MagicLeapSetupTool.Editor.Utilities
{
	/// <summary>
	/// Utility that adds ability to find Types by partial names
	/// </summary>
	public static class TypeUtility
	{
		private static readonly List<string> _typeFullNames = new List<string>();
		private static readonly Dictionary<string, Type> _typesByFullname = new Dictionary<string, Type>();
		private static int _assembliesCount;
		public static Type FindTypeByPartialName(string contains, string doesNotContain = null, bool fullRefresh=false)
		{
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			if (_assembliesCount < assemblies.Length || fullRefresh)
			{
				foreach (var assembly in assemblies)
				{
					var types = assembly.GetTypes();

					foreach (var scriptType in types)
					{
						if (scriptType.FullName != null)
						{
							var key = scriptType.Assembly.GetName().FullName + scriptType.FullName;
							_typeFullNames.Add(key);
							_typesByFullname.Add(key, scriptType);
						}
					}
				}
				_assembliesCount = assemblies.Length;
			}
		
			string foundType="";
			foundType = !string.IsNullOrEmpty(doesNotContain) ? _typeFullNames.Find((e) => !e.Contains(doesNotContain) && 
			e.Contains(contains)) : _typeFullNames.Find((e) => e.Contains(contains));
			
			return !string.IsNullOrEmpty(foundType) ? _typesByFullname[foundType] : null;

		
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
