#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

#endregion

namespace Speedy.EntityFramework.Internal
{
	internal static class Extensions
	{
		#region Fields

		private static readonly ConcurrentDictionary<string, MethodInfo[]> _methodInfos;

		#endregion

		#region Constructors

		static Extensions()
		{
			_methodInfos = new ConcurrentDictionary<string, MethodInfo[]>();
		}

		#endregion

		#region Methods

		internal static IList<MethodInfo> GetCachedMethods(this Type type, BindingFlags flags)
		{
			MethodInfo[] response;

			if (_methodInfos.ContainsKey(type.FullName))
			{
				if (_methodInfos.TryGetValue(type.FullName, out response))
				{
					return response;
				}
			}

			response = type.GetMethods(flags);
			return _methodInfos.AddOrUpdate(type.FullName, response, (s, infos) => response);
		}

		#endregion
	}
}