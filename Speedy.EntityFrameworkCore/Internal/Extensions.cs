#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

#endregion

namespace Speedy.EntityFrameworkCore.Internal
{
	internal static class Extensions
	{
		#region Fields

		private static readonly ConcurrentDictionary<string, FieldInfo[]> _fieldInfos;
		private static readonly ConcurrentDictionary<string, MethodInfo[]> _methodInfos;
		private static readonly ConcurrentDictionary<string, PropertyInfo[]> _propertyInfos;

		#endregion

		#region Constructors

		static Extensions()
		{
			_fieldInfos = new ConcurrentDictionary<string, FieldInfo[]>();
			_methodInfos = new ConcurrentDictionary<string, MethodInfo[]>();
			_propertyInfos = new ConcurrentDictionary<string, PropertyInfo[]>();
		}

		#endregion

		#region Methods

		internal static IEnumerable<FieldInfo> GetCachedFields(this Type type, BindingFlags flags)
		{
			FieldInfo[] response;

			if (_fieldInfos.ContainsKey(type.FullName))
			{
				if (_fieldInfos.TryGetValue(type.FullName, out response))
				{
					return response;
				}
			}

			response = type.GetFields(flags);
			return _fieldInfos.AddOrUpdate(type.FullName, response, (s, infos) => response);
		}

		internal static IEnumerable<MethodInfo> GetCachedMethods(this Type type, BindingFlags flags)
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

		internal static IEnumerable<PropertyInfo> GetCachedProperties(this Type type)
		{
			PropertyInfo[] response;

			if (_propertyInfos.ContainsKey(type.FullName))
			{
				if (_propertyInfos.TryGetValue(type.FullName, out response))
				{
					return response;
				}
			}

			response = type.GetProperties();
			return _propertyInfos.AddOrUpdate(type.FullName, response, (s, infos) => response);
		}

		#endregion
	}
}