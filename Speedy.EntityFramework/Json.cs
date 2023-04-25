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
using Speedy.EntityFramework.Sql;
using Speedy.Extensions;
using Speedy.Serialization;

#endregion

namespace Speedy.EntityFramework;

/// <summary>
/// Json Database Functions for SQL / Sqlite translations.
/// </summary>
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

		_jsonGetValue = methods.First(x => x.Name == nameof(Value));
		_jsonMethods = methods.Where(x => inclusions.Contains(x.Name));
	}

	#endregion

	#region Methods

	/// <summary>
	/// Configure the functions for a memory database.
	/// </summary>
	public static void ConfigureForMemory()
	{
		_jsonStringFunction = (source, path) =>
		{
			var jParsed = JObject.Parse(source, new JsonLoadSettings
			{
				LineInfoHandling = LineInfoHandling.Ignore,
				DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace,
				CommentHandling = CommentHandling.Ignore
			});
			var data = jParsed.SelectToken(path);
			return data switch
			{
				JArray jArray => jArray.ToJson(),
				JObject jObject => jObject.ToJson(),
				_ => data?.ToString()
			};
		};
	}

	/// <summary>
	/// Configure the functions for a SQL database.
	/// </summary>
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

	/// <summary>
	/// Configure the functions for a Sqlite database.
	/// </summary>
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

	/// <summary>
	/// Read a boolean member from a JSON source.
	/// </summary>
	/// <param name="source"> The JSON source. </param>
	/// <param name="path"> The path to the data to read. </param>
	/// <returns> The read value. </returns>
	public static bool ToBoolean(string source, string path)
	{
		return JsonConvert<bool>(source, path);
	}

	/// <summary>
	/// Read a DateTime member from a JSON source.
	/// </summary>
	/// <param name="source"> The JSON source. </param>
	/// <param name="path"> The path to the data to read. </param>
	/// <returns> The read value. </returns>
	public static DateTime ToDateTime(string source, string path)
	{
		return JsonConvert<DateTime>(source, path);
	}

	/// <summary>
	/// Read a numeric member from a JSON source.
	/// </summary>
	/// <param name="source"> The JSON source. </param>
	/// <param name="path"> The path to the data to read. </param>
	/// <returns> The read value. </returns>
	public static short ToInt16(string source, string path)
	{
		return JsonConvert<short?>(source, path) ?? 0;
	}

	/// <summary>
	/// Read a numeric member from a JSON source.
	/// </summary>
	/// <param name="source"> The JSON source. </param>
	/// <param name="path"> The path to the data to read. </param>
	/// <returns> The read value. </returns>
	public static int ToInt32(string source, string path)
	{
		return JsonConvert<int?>(source, path) ?? 0;
	}

	/// <summary>
	/// Read a numeric member from a JSON source.
	/// </summary>
	/// <param name="source"> The JSON source. </param>
	/// <param name="path"> The path to the data to read. </param>
	/// <returns> The read value. </returns>
	public static long ToInt64(string source, string path)
	{
		return JsonConvert<long?>(source, path) ?? 0;
	}

	/// <summary>
	/// Read a boolean member from a JSON source.
	/// </summary>
	/// <param name="source"> The JSON source. </param>
	/// <param name="path"> The path to the data to read. </param>
	/// <returns> The read value. </returns>
	public static bool? ToNullableBoolean(string source, string path)
	{
		return JsonConvert<bool?>(source, path);
	}

	/// <summary>
	/// Read a DateTime member from a JSON source.
	/// </summary>
	/// <param name="source"> The JSON source. </param>
	/// <param name="path"> The path to the data to read. </param>
	/// <returns> The read value. </returns>
	public static DateTime? ToNullableDateTime(string source, string path)
	{
		return JsonConvert<DateTime?>(source, path);
	}

	/// <summary>
	/// Read a numeric member from a JSON source.
	/// </summary>
	/// <param name="source"> The JSON source. </param>
	/// <param name="path"> The path to the data to read. </param>
	/// <returns> The read value. </returns>
	public static short? ToNullableInt16(string source, string path)
	{
		return JsonConvert<short?>(source, path);
	}

	/// <summary>
	/// Read a numeric member from a JSON source.
	/// </summary>
	/// <param name="source"> The JSON source. </param>
	/// <param name="path"> The path to the data to read. </param>
	/// <returns> The read value. </returns>
	public static int? ToNullableInt32(string source, string path)
	{
		return JsonConvert<int?>(source, path);
	}

	/// <summary>
	/// Read a numeric member from a JSON source.
	/// </summary>
	/// <param name="source"> The JSON source. </param>
	/// <param name="path"> The path to the data to read. </param>
	/// <returns> The read value. </returns>
	public static long? ToNullableInt64(string source, string path)
	{
		return JsonConvert<long?>(source, path);
	}

	/// <summary>
	/// Read a numeric member from a JSON source.
	/// </summary>
	/// <param name="source"> The JSON source. </param>
	/// <param name="path"> The path to the data to read. </param>
	/// <returns> The read value. </returns>
	public static ushort? ToNullableUInt16(string source, string path)
	{
		return JsonConvert<ushort?>(source, path);
	}

	/// <summary>
	/// Read a numeric member from a JSON source.
	/// </summary>
	/// <param name="source"> The JSON source. </param>
	/// <param name="path"> The path to the data to read. </param>
	/// <returns> The read value. </returns>
	public static uint? ToNullableUInt32(string source, string path)
	{
		return JsonConvert<uint?>(source, path);
	}

	/// <summary>
	/// Read a numeric member from a JSON source.
	/// </summary>
	/// <param name="source"> The JSON source. </param>
	/// <param name="path"> The path to the data to read. </param>
	/// <returns> The read value. </returns>
	public static ulong? ToNullableUInt64(string source, string path)
	{
		return JsonConvert<ulong?>(source, path);
	}

	/// <summary>
	/// Read a numeric member from a JSON source.
	/// </summary>
	/// <param name="source"> The JSON source. </param>
	/// <param name="path"> The path to the data to read. </param>
	/// <returns> The read value. </returns>
	public static ushort ToUInt16(string source, string path)
	{
		return JsonConvert<ushort?>(source, path) ?? 0;
	}

	/// <summary>
	/// Read a numeric member from a JSON source.
	/// </summary>
	/// <param name="source"> The JSON source. </param>
	/// <param name="path"> The path to the data to read. </param>
	/// <returns> The read value. </returns>
	public static uint ToUInt32(string source, string path)
	{
		return JsonConvert<uint?>(source, path) ?? 0;
	}

	/// <summary>
	/// Read a numeric member from a JSON source.
	/// </summary>
	/// <param name="source"> The JSON source. </param>
	/// <param name="path"> The path to the data to read. </param>
	/// <returns> The read value. </returns>
	public static ulong ToUInt64(string source, string path)
	{
		return JsonConvert<ulong?>(source, path) ?? 0;
	}

	/// <summary>
	/// Gets a JSON value from a JSON field.
	/// </summary>
	/// <param name="source"> The JSON source. </param>
	/// <param name="path"> The path to the data to read. </param>
	/// <returns> The JSON member read. </returns>
	public static string Value(string source, string path)
	{
		var function = _jsonStringFunction;
		if (function != null)
		{
			// do not use Null propagation because null is a valid response.
			return function.Invoke(source, path);
		}

		throw new NotSupportedException();
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
				var sqlType = SqlStatement.GetSqlType(methodInfo.ReturnType).ToString();
				var ex = new SqlFunctionExpression("JSON_VALUE", args.Take(2), false, Array.Empty<bool>(), typeof(string), RelationalTypeMapping.NullMapping);
				return new SqlFunctionExpression("TRY_CONVERT",
					new SqlExpression[]
					{
						new SqlFragmentExpression(sqlType),
						ex
					},
					// This must be false even though we can and sometimes will return null...?
					false,
					Array.Empty<bool>(),
					methodInfo.ReturnType,
					null);
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
				return SqlFunctionExpression.Create("TRY_CONVERT",
					new SqlExpression[]
					{
						new SqlFragmentExpression(SqlStatement.GetSqlType(methodInfo.ReturnType).ToString()),
						ex
					},
					methodInfo.ReturnType,
					null
				);
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
				var ex = new SqlFunctionExpression("JSON_EXTRACT", args.Take(2), false, Array.Empty<bool>(), typeof(string), RelationalTypeMapping.NullMapping);
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
				var ex = SqlFunctionExpression.Create("JSON_EXTRACT", args.Take(2), typeof(string), RelationalTypeMapping.NullMapping);
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