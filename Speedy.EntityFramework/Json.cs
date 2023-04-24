#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json.Linq;
using Speedy.Converters;
using Speedy.Extensions;
using Speedy.Serialization;

#endregion

namespace Speedy.EntityFramework;

public static class Json
{
	#region Fields

	private static readonly MethodInfo _jsonGetValue;
	private static readonly IEnumerable<MethodInfo> _jsonMethods;
	private static Func<string, string, string> _jsonStringFunction;

	#endregion

	#region Constructors

	static Json()
	{
		var methods = typeof(Json).GetCachedMethods(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public);
		var inclusions = new[]
		{
			nameof(ToBoolean),
			nameof(ToDateTime),
			nameof(ToInt16),
			nameof(ToInt32),
			nameof(ToInt64),
			nameof(ToUInt16),
			nameof(ToUInt32),
			nameof(ToUInt64),
			nameof(ToNullableBoolean),
			nameof(ToNullableDateTime),
			nameof(ToNullableInt16),
			nameof(ToNullableInt32),
			nameof(ToNullableInt64),
			nameof(ToNullableUInt16),
			nameof(ToNullableUInt32),
			nameof(ToNullableUInt64)
		};

		_jsonGetValue = methods.First(x => x.Name == nameof(GetValue));
		_jsonMethods = methods.Where(x => inclusions.Contains(x.Name));
	}

	#endregion

	#region Methods

	public static void ConfigureForMemory()
	{
		_jsonStringFunction = (source, path) =>
		{
			var jObject = JObject.Parse(source);
			var data = jObject.SelectToken(path);
			return data?.ToString();
		};
	}

	public static void ConfigureForSql(ModelBuilder builder)
	{
		// JSON value can be a string, number, bool, null, or array

		#if (NET6_0_OR_GREATER)
		builder
			.HasDbFunction(_jsonGetValue)
			.HasStoreType("nvarchar(max)")
			.HasName($"Speedy {_jsonGetValue.Name} Function")
			.HasTranslation(args => new SqlFunctionExpression("JSON_VALUE", args, false, Array.Empty<bool>(), typeof(string), null))
			.IsBuiltIn();
		#else
		builder
			.HasDbFunction(_jsonGetValue)
			.HasStoreType("nvarchar(max)")
			.HasName($"Speedy {_jsonGetValue.Name} Function")
			.HasTranslation(args => SqlFunctionExpression.Create("JSON_VALUE", args, typeof(string), null));
		#endif

		foreach (var method in _jsonMethods)
		{
			ConfigureForSql(builder, method);
		}
	}

	public static void ConfigureForSqlite(ModelBuilder builder)
	{
		// JSON value can be a string, number, bool, null, or array

		#if (NET6_0_OR_GREATER)
		builder
			.HasDbFunction(_jsonGetValue)
			.HasStoreType("nvarchar(max)")
			.HasName($"Speedy {_jsonGetValue.Name} Function")
			.HasTranslation(args => new SqlFunctionExpression("JSON_EXTRACT", args, false, Array.Empty<bool>(), typeof(string), null))
			.IsBuiltIn();
		#else
		builder
			.HasDbFunction(_jsonGetValue)
			.HasStoreType("nvarchar(max)")
			.HasName($"Speedy {_jsonGetValue.Name} Function")
			.HasTranslation(args => SqlFunctionExpression.Create("JSON_EXTRACT", args, typeof(string), null));
		#endif

		foreach (var method in _jsonMethods)
		{
			ConfigureForSqlite(builder, method);
		}
	}

	public static string GetValue(string source, string path)
	{
		var function = _jsonStringFunction;
		if (function != null)
		{
			// do not use Null propagation because null is a valid response.
			return function.Invoke(source, path);
		}

		throw new NotSupportedException();
	}

	public static bool ToBoolean(string source, string path)
	{
		return JsonConvert<bool>(source, path);
	}

	public static DateTime ToDateTime(string source, string path)
	{
		return JsonConvert<DateTime>(source, path);
	}

	public static short ToInt16(string source, string path)
	{
		return JsonConvert<short?>(source, path) ?? 0;
	}

	public static int ToInt32(string source, string path)
	{
		return JsonConvert<int?>(source, path) ?? 0;
	}

	public static long ToInt64(string source, string path)
	{
		return JsonConvert<long?>(source, path) ?? 0;
	}

	public static bool? ToNullableBoolean(string source, string path)
	{
		return JsonConvert<bool?>(source, path);
	}

	public static DateTime? ToNullableDateTime(string source, string path)
	{
		return JsonConvert<DateTime?>(source, path);
	}

	public static short? ToNullableInt16(string source, string path)
	{
		return JsonConvert<short?>(source, path);
	}

	public static int? ToNullableInt32(string source, string path)
	{
		return JsonConvert<int?>(source, path);
	}

	public static long? ToNullableInt64(string source, string path)
	{
		return JsonConvert<long?>(source, path);
	}

	public static ushort? ToNullableUInt16(string source, string path)
	{
		return JsonConvert<ushort?>(source, path);
	}

	public static uint? ToNullableUInt32(string source, string path)
	{
		return JsonConvert<uint?>(source, path);
	}

	public static ulong? ToNullableUInt64(string source, string path)
	{
		return JsonConvert<ulong?>(source, path);
	}

	public static ushort ToUInt16(string source, string path)
	{
		return JsonConvert<ushort?>(source, path) ?? 0;
	}

	public static uint ToUInt32(string source, string path)
	{
		return JsonConvert<uint?>(source, path) ?? 0;
	}

	public static ulong ToUInt64(string source, string path)
	{
		return JsonConvert<ulong?>(source, path) ?? 0;
	}

	private static void ConfigureForSql(ModelBuilder builder, MethodInfo methodInfo)
	{
		// JSON value can be a string, number, bool, null, or array

		#if (NET6_0_OR_GREATER)
		builder
			.HasDbFunction(methodInfo)
			.HasStoreType("nvarchar(max)")
			.HasName($"Speedy {methodInfo.Name} Function")
			.HasTranslation(args =>
			{
				var ex = new SqlFunctionExpression("JSON_VALUE", args.Take(2), false, Array.Empty<bool>(), typeof(string), RelationalTypeMapping.NullMapping);
				return new SqlUnaryExpression(ExpressionType.Convert, ex, methodInfo.ReturnType, null);
			})
			.IsBuiltIn();
		#else
		builder
			.HasDbFunction(methodInfo)
			.HasStoreType("nvarchar(max)")
			.HasName($"Speedy {methodInfo.Name} Function")
			.HasTranslation(args =>
			{
				var ex = SqlFunctionExpression.Create("JSON_VALUE", args.Take(2), typeof(string), RelationalTypeMapping.NullMapping);
				return new SqlUnaryExpression(ExpressionType.Convert, ex, methodInfo.ReturnType, null);
			});
		#endif
	}

	private static void ConfigureForSqlite(ModelBuilder builder, MethodInfo methodInfo)
	{
		// JSON value can be a string, number, bool, null, or array

		#if (NET6_0_OR_GREATER)
		builder
			.HasDbFunction(methodInfo)
			.HasStoreType("nvarchar(max)")
			.HasName($"Speedy {methodInfo.Name} Function")
			.HasTranslation(args =>
			{
				var ex = new SqlFunctionExpression("JSON_EXTRACT", args.Take(2), false, Array.Empty<bool>(), typeof(string), null);
				return new SqlUnaryExpression(ExpressionType.Convert, ex, methodInfo.ReturnType, null);
			})
			.IsBuiltIn();
		#else
		builder
			.HasDbFunction(methodInfo)
			.HasStoreType("nvarchar(max)")
			.HasName($"Speedy {methodInfo.Name} Function")
			.HasTranslation(args =>
			{
				var ex = SqlFunctionExpression.Create("JSON_EXTRACT", args.Take(2), typeof(string), null);
				return new SqlUnaryExpression(ExpressionType.Convert, ex, methodInfo.ReturnType, null);
			});
		#endif
	}

	private static T JsonConvert<T>(string source, string path)
	{
		return (T) JsonConvert(source, path, typeof(T).FullName);
	}

	private static object JsonConvert(string source, string path, string typeName)
	{
		var function = _jsonStringFunction;
		if (function != null)
		{
			// do not use Null propagation because null is a valid response.
			var data = function.Invoke(source, path);
			var type = Type.GetType(typeName);
			return StringConverter.TryParse(type, data, out var parseResult)
				? parseResult
				: data.TryFromJson(type, out var jResult)
					? jResult
					: type.GetDefaultValue();
		}

		throw new NotSupportedException();
	}

	#endregion
}