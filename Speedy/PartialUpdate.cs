#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Speedy.Converters;
using Speedy.Exceptions;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.Serialization.Converters;
using Speedy.Validation;

#endregion

namespace Speedy
{
	/// <summary>
	/// This class contains updates for an object. JSON is deserialized into this type.
	/// </summary>
	/// <typeparam name="T"> The type of object to be updated. </typeparam>
	[JsonConverter(typeof(PartialUpdateConverter))]
	public class PartialUpdate<T> : PartialUpdate
	{
		#region Constructors

		/// <summary>
		/// Instantiates a partial update.
		/// </summary>
		public PartialUpdate() : this(null)
		{
		}

		/// <summary>
		/// Instantiates a partial update.
		/// </summary>
		/// <param name="dispatcher"> An optional dispatcher. </param>
		public PartialUpdate(IDispatcher dispatcher) : base(dispatcher)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Applies the updates to the entity.
		/// </summary>
		/// <param name="entity"> Entity to be updated. </param>
		public void Apply(T entity)
		{
			Apply(entity, IncludedProperties, ExcludedProperties);
		}

		/// <summary>
		/// Get the property value.
		/// </summary>
		/// <typeparam name="TProperty"> The type to cast the value to. </typeparam>
		/// <param name="expression"> The expression of the member to set. </param>
		/// <returns> The value if it was found otherwise default(T). </returns>
		public TProperty Get<TProperty>(Expression<Func<T, TProperty>> expression)
		{
			var propertyExpression = (MemberExpression) expression.Body;
			return Get<TProperty>(propertyExpression.Member.Name);
		}

		/// <summary>
		/// Get the property value.
		/// </summary>
		/// <typeparam name="TProperty"> The type to cast the value to. </typeparam>
		/// <param name="expression"> The expression of the member to set. </param>
		/// <param name="defaultValue"> A default value if update not available. </param>
		/// <returns> The value if it was found otherwise default(T). </returns>
		public TProperty Get<TProperty>(Expression<Func<T, TProperty>> expression, TProperty defaultValue)
		{
			var propertyExpression = (MemberExpression) expression.Body;
			return Get(propertyExpression.Member.Name, defaultValue);
		}

		/// <summary>
		/// Creates an instance of the type and applies the partial update.
		/// </summary>
		/// <returns> </returns>
		public T GetInstance()
		{
			var response = Activator.CreateInstance<T>();
			Apply(response);
			return response;
		}

		/// <summary>
		/// Remove a property from the update.
		/// </summary>
		/// <param name="expression"> The expression of the member to set. </param>
		public void Remove<TProperty>(Expression<Func<T, TProperty>> expression)
		{
			var propertyExpression = (MemberExpression) expression.Body;
			Remove(propertyExpression.Member.Name);
		}

		/// <summary>
		/// Set a property for the update. The name must be available of the target value.
		/// </summary>
		/// <param name="name"> The name of the member to set. </param>
		/// <param name="value"> The value of the member. </param>
		public override void Set(string name, object value)
		{
			var properties = typeof(T).GetCachedPropertyDictionary();
			Set(name, value, properties);
		}

		/// <summary>
		/// Set a full set of updates.
		/// </summary>
		/// <param name="value"> The value that contains a full set of updates. </param>
		public void Set(T value)
		{
			var properties = GetCachedPropertyDictionary();

			foreach (var property in properties.Values)
			{
				var response = new PartialUpdateValue
				{
					Name = property.Name,
					Type = property.PropertyType,
					Value = property.GetValue(value)
				};

				Updates.AddOrUpdate(property.Name, response);
			}
		}

		/// <summary>
		/// Set a property for the update.
		/// </summary>
		/// <param name="expression"> The expression of the member to set. </param>
		/// <param name="value"> The value of the member. </param>
		public void Set<TProperty>(Expression<Func<T, TProperty>> expression, TProperty value)
		{
			var propertyExpression = (MemberExpression) expression.Body;
			Set(propertyExpression.Member.Name, value);
		}

		/// <summary>
		/// Runs the validator to check the parameter.
		/// </summary>
		public bool TryValidate(out IList<IValidation> failures)
		{
			failures = new List<IValidation>();
			ProcessValidator(this, failures);
			return failures.Count <= 0;
		}

		/// <summary>
		/// Runs the validator to check the partial update.
		/// </summary>
		public void Validate()
		{
			Validate(this);
		}

		/// <summary>
		/// Configure a validation for a property.
		/// </summary>
		/// <remarks>
		/// If this is updated, also update <seealso cref="Validator{T}.Property" />
		/// </remarks>
		public MemberValidator<TProperty> Validate<TProperty>(Expression<Func<T, TProperty>> expression)
		{
			var propertyExpression = (MemberExpression) expression.Body;
			var response = new MemberValidator<TProperty>(propertyExpression.Member);
			PropertyValidators.Add(response);
			return response;
		}

		/// <inheritdoc />
		internal override IDictionary<string, PropertyInfo> GetCachedPropertyDictionary()
		{
			return typeof(T).GetCachedPropertyDictionary();
		}

		#endregion
	}

	/// <summary>
	/// This class contains updates for an object. JSON is deserialized into this type.
	/// </summary>
	[JsonConverter(typeof(PartialUpdateConverter))]
	public class PartialUpdate : Validator
	{
		#region Constructors

		/// <summary>
		/// Instantiates a partial update.
		/// </summary>
		public PartialUpdate() : this(null)
		{
		}

		/// <summary>
		/// Instantiates a partial update.
		/// </summary>
		/// <param name="dispatcher"> An optional dispatcher. </param>
		public PartialUpdate(IDispatcher dispatcher) : base(dispatcher)
		{
			ExcludedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			IncludedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			Updates = new SortedDictionary<string, PartialUpdateValue>(StringComparer.OrdinalIgnoreCase);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Properties to be excluded.
		/// </summary>
		[JsonIgnore]
		public HashSet<string> ExcludedProperties { get; }

		/// <summary>
		/// Properties to be included.
		/// </summary>
		[JsonIgnore]
		public HashSet<string> IncludedProperties { get; }

		/// <summary>
		/// A list of updates for this partial update.
		/// </summary>
		[JsonIgnore]
		public IDictionary<string, PartialUpdateValue> Updates { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Add an update value
		/// </summary>
		/// <param name="name"> The name of the update to add. </param>
		/// <param name="value"> The value of the update. </param>
		public void AddOrUpdate(string name, object value)
		{
			var properties = GetType().GetCachedPropertyDictionary();
			var property = properties.Values.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
			var type = value == null ? property?.PropertyType ?? typeof(object) : value.GetType();

			if (Updates.ContainsKey(name))
			{
				var update = Updates[name];

				if (!string.Equals(update.Name, name, StringComparison.Ordinal))
				{
					// Rename the key...
					Updates.Remove(update.Name);
					update.Name = name;
					Updates.Add(update.Name, update);
				}

				update.Value = value;
				return;
			}

			Updates.Add(name, new PartialUpdateValue(name, type, value));
		}

		/// <summary>
		/// Applies the updates to the entity.
		/// </summary>
		/// <param name="entity"> Entity to be updated. </param>
		public void Apply(object entity)
		{
			Apply(entity, IncludedProperties, ExcludedProperties);
		}

		/// <summary>
		/// Create an instance of a partial update for a specific type.
		/// </summary>
		/// <param name="genericType"> The type the partial update is for. </param>
		/// <returns> The partial update. </returns>
		public static PartialUpdate CreateGeneric(Type genericType)
		{
			var isAlreadyPartialUpdate = typeof(PartialUpdate).IsAssignableFrom(genericType);
			var typeToCreate = isAlreadyPartialUpdate ? genericType : typeof(PartialUpdate<>).MakeGenericType(genericType);
			var response = (PartialUpdate) Activator.CreateInstance(typeToCreate);
			return response;
		}

		/// <summary>
		/// Gets a partial update from a JSON string.
		/// </summary>
		/// <typeparam name="T"> The type the partial update is for. </typeparam>
		/// <param name="json"> The JSON containing the partial update. </param>
		/// <returns> The partial update. </returns>
		public static PartialUpdate<T> FromJson<T>(string json)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				return new PartialUpdate<T>();
			}

			return (PartialUpdate<T>) FromJson(json, typeof(T));
		}

		/// <summary>
		/// Gets a partial update from a JSON string.
		/// </summary>
		/// <param name="json"> The JSON containing the partial update. </param>
		/// <param name="type"> The type the partial update is for. </param>
		/// <returns> The partial update. </returns>
		public static PartialUpdate FromJson(string json, Type type)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				var response = CreateGeneric(type);
				return response;
			}

			var serializer = new JsonSerializer();
			using var reader = new JsonTextReader(new StringReader(json));
			reader.Read();
			return FromJson(reader, serializer, type);
		}

		/// <summary>
		/// Gets a partial update from a JSON string.
		/// </summary>
		/// <param name="json"> The JSON containing the partial update. </param>
		/// <returns> The partial update. </returns>
		public static PartialUpdate FromJson(string json)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				var response = new PartialUpdate();
				return response;
			}

			using var reader = new JsonTextReader(new StringReader(json));
			reader.Read();
			return FromJson(reader);
		}

		/// <summary>
		/// Get the update for the provided name.
		/// </summary>
		/// <param name="name"> The name of the update. </param>
		/// <returns> The value if it was found otherwise null. </returns>
		public object Get(string name)
		{
			if (!Updates.ContainsKey(name))
			{
				throw new KeyNotFoundException($"{name} update was not found.");
			}

			return Updates[name].Value;
		}

		/// <summary>
		/// Get the update for the provided name.
		/// </summary>
		/// <typeparam name="T"> The type to cast the value to. </typeparam>
		/// <param name="name"> The name of the update. </param>
		/// <returns> The value if it was found otherwise default(T). </returns>
		public T Get<T>(string name)
		{
			if (!Updates.ContainsKey(name))
			{
				throw new KeyNotFoundException($"{name} update was not found.");
			}

			var update = Updates[name];
			return ConvertUpdate<T>(update);
		}

		/// <summary>
		/// Get the update for the provided name with a fallback default value if not found.
		/// </summary>
		/// <typeparam name="T"> The type to cast the value to. </typeparam>
		/// <param name="name"> The name of the update. </param>
		/// <param name="defaultValue"> A default value if update not available. </param>
		/// <returns> The value if it was found otherwise default(T). </returns>
		public T Get<T>(string name, T defaultValue)
		{
			if (!Updates.ContainsKey(name))
			{
				return defaultValue;
			}

			var update = Updates[name];
			return ConvertUpdate<T>(update);
		}

		/// <summary>
		/// Refresh the results partial updates.
		/// </summary>
		public virtual void Refresh()
		{
		}

		/// <summary>
		/// Remove a property from the update.
		/// </summary>
		/// <param name="name"> The name of the member to set. </param>
		public void Remove(string name)
		{
			if (Updates.ContainsKey(name))
			{
				Updates.Remove(name);
			}
		}

		/// <summary>
		/// Set a property for the update. The name must be available of the target value.
		/// </summary>
		/// <param name="name"> The name of the member to set. </param>
		/// <param name="value"> The value of the member. </param>
		public virtual void Set(string name, object value)
		{
			var properties = GetType().GetCachedPropertyDictionary();
			Set(name, value, properties);
		}

		/// <summary>
		/// Check to see if a property should be processed.
		/// </summary>
		/// <param name="propertyName"> The name of the property to test. </param>
		/// <returns> True if the property should be processed otherwise false. </returns>
		public bool ShouldProcessProperty(string propertyName)
		{
			if (IncludedProperties.Any()
				&& !IncludedProperties.Contains(propertyName))
			{
				// Ignore this property because we only want to include it
				return false;
			}

			if (ExcludedProperties.Contains(propertyName))
			{
				// Ignore this property because we only want to exclude it
				return false;
			}

			return true;
		}

		/// <summary>
		/// Get the JSON for the partial update.
		/// </summary>
		/// <returns> The JSON for the partial update. </returns>
		public virtual string ToJson(SerializerSettings settings = null)
		{
			var expando = GetExpandoObject();

			return settings != null
				? expando.ToJson(settings)
				: expando.ToRawJson();
		}

		/// <summary>
		/// Get the property value.
		/// </summary>
		/// <typeparam name="T"> The type to cast the value to. </typeparam>
		/// <param name="name"> The name of the update. </param>
		/// <param name="value"> The value that was retrieve or default value if not found. </param>
		/// <returns> True if the update was found otherwise false. </returns>
		public bool TryGet<T>(string name, out T value)
		{
			if (Updates.ContainsKey(name))
			{
				value = ConvertUpdate<T>(Updates[name]);
				return true;
			}

			value = default;
			return false;
		}

		/// <summary>
		/// Applies the updates to the entity.
		/// </summary>
		/// <param name="entity"> Entity to be updated. </param>
		/// <param name="including"> Properties to be included. </param>
		/// <param name="excluding"> Properties to be excluded. </param>
		protected void Apply(object entity, HashSet<string> including, HashSet<string> excluding)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			if (Updates == null)
			{
				return;
			}

			var propertyInfos = entity.GetType().GetCachedPropertyDictionary();

			foreach (var update in Updates)
			{
				if (including.Any() && !including.Contains(update.Key))
				{
					// Ignore this property because we only want to include it
					continue;
				}

				if (excluding.Contains(update.Key))
				{
					// Ignore this property because we only want to exclude it
					continue;
				}

				if (!propertyInfos.ContainsKey(update.Key))
				{
					continue;
				}

				var propertyInfo = propertyInfos[update.Key];
				if (!propertyInfo.CanWrite)
				{
					continue;
				}

				if ((update.Value == null) && propertyInfo.PropertyType.IsNullable())
				{
					propertyInfo.SetValue(entity, null);
					continue;
				}

				try
				{
					var value = Convert.ChangeType(update.Value?.Value, propertyInfo.PropertyType);
					propertyInfo.SetValue(entity, value);
					continue;
				}
				catch (Exception)
				{
					// Ignore changing of type
				}

				if ((update.Value?.Type != propertyInfo.PropertyType)
					|| (update.Value?.Value?.GetType() != propertyInfo.PropertyType))
				{
					continue;
				}

				if (update.Value != null)
				{
					propertyInfo.SetValue(entity, update.Value.Value);
				}
			}
		}

		/// <summary>
		/// Create a dynamic object of the partial update.
		/// </summary>
		/// <returns> The dynamic version of the partial update. </returns>
		protected virtual ExpandoObject GetExpandoObject()
		{
			var expando = new ExpandoObject();

			Refresh();

			foreach (var update in Updates)
			{
				if (!ShouldProcessProperty(update.Key))
				{
					continue;
				}

				expando.AddOrUpdate(update.Key, update.Value.Value);
			}

			return expando;
		}

		/// <summary>
		/// Set a property for the update. The name must be available of the target value.
		/// </summary>
		/// <param name="name"> The name of the member to set. </param>
		/// <param name="value"> The value of the member. </param>
		/// <param name="properties"> The properties for the set. </param>
		protected void Set(string name, object value, IDictionary<string, PropertyInfo> properties)
		{
			if (!properties.ContainsKey(name))
			{
				throw new SpeedyException("The property does not exist by the provided name.");
			}

			var property = properties[name];
			if (!property.CanWrite)
			{
				throw new SpeedyException("The property cannot be set because it's not writable.");
			}

			if (property.PropertyType.IsNullable() && (value == null))
			{
				Updates.AddOrUpdate(name, new PartialUpdateValue
				{
					Name = name,
					Type = property.PropertyType,
					Value = null
				});
				return;
			}

			var valueType = value.GetType();
			if (!property.PropertyType.IsAssignableFrom(valueType))
			{
				throw new SpeedyException("The property type does not match the values type.");
			}

			var response = new PartialUpdateValue
			{
				Name = name,
				Type = property.PropertyType,
				Value = value
			};

			Updates.AddOrUpdate(name, response);
		}

		internal static PartialUpdate FromJson(JsonReader reader)
		{
			bool TryGetValue(JToken token, Type type, out object value)
			{
				if (token is not JValue jValue)
				{
					value = null;
					return false;
				}

				if ((jValue.Type == JTokenType.Null) && (jValue.Value == null))
				{
					value = null;
					return true;
				}

				try
				{
					value = Convert.ChangeType(jValue, type);
					return true;
				}
				catch
				{
					value = null;
					return false;
				}
			}

			var response = new PartialUpdate();

			if (reader.TokenType == JsonToken.StartArray)
			{
				return response;
			}

			var jObject = JObject.Load(reader);
			var jProperties = jObject.Properties();

			foreach (var jProperty in jProperties)
			{
				if (!response.ShouldProcessProperty(jProperty.Name))
				{
					continue;
				}

				var propertyName = jProperty.Name;
				if (jProperty.Type == JTokenType.Null)
				{
					var nullValue = new PartialUpdateValue(propertyName, typeof(string), null);
					response.Updates.Add(propertyName, nullValue);
					continue;
				}

				// Property of array must be IEnumerable (ignoring some types like string)
				if (jProperty.Value is JArray jArray)
				{
					var genericList = new List<object>(jArray.Count);

					foreach (var jArrayValue in jArray.Values())
					{
						if (TryGetValue(jArrayValue, typeof(object), out var arrayValue))
						{
							genericList.Add(arrayValue);
						}
					}

					var update2 = new PartialUpdateValue(propertyName, typeof(object), genericList);
					response.Updates.Add(propertyName, update2);
					continue;
				}

				if (TryGetValue(jProperty.Value, typeof(object), out var value2))
				{
					var update = new PartialUpdateValue(propertyName, typeof(object), value2);
					response.Updates.Add(propertyName, update);
				}
			}

			return response;
		}

		internal static PartialUpdate FromJson(JsonReader reader, JsonSerializer serializer, Type objectType)
		{
			bool TryGetValue(JToken token, Type type, out object value)
			{
				if (token is not JValue jValue)
				{
					value = null;
					return false;
				}

				if ((jValue.Type == JTokenType.Null) && (jValue.Value == null))
				{
					value = null;
					return true;
				}

				try
				{
					value = Convert.ChangeType(jValue, type);
					return true;
				}
				catch
				{
					value = null;
					return false;
				}
			}

			var response = CreateGeneric(objectType);

			if (reader.TokenType == JsonToken.StartArray)
			{
				return response;
			}

			var jObject = JObject.Load(reader);
			var jProperties = jObject.Properties();
			var propertyDictionary = response.GetCachedPropertyDictionary();

			foreach (var jProperty in jProperties)
			{
				if (!response.ShouldProcessProperty(jProperty.Name))
				{
					continue;
				}

				var property = propertyDictionary.ContainsKey(jProperty.Name)
					? propertyDictionary[jProperty.Name]
					: null;

				if (property == null)
				{
					continue;
				}

				var propertyName = property.Name;
				if (jProperty.Type == JTokenType.Null)
				{
					var nullValue = new PartialUpdateValue(propertyName, property.PropertyType, null);
					response.Updates.Add(propertyName, nullValue);
					continue;
				}

				// Property of array must be IEnumerable (ignoring some types like string)
				if (jProperty.Value is JArray jArray && property.PropertyType.IsEnumerable())
				{
					var genericType = property.PropertyType.GenericTypeArguments.FirstOrDefault();
					var genericListType = typeof(List<>).MakeGenericType(genericType);
					var genericList = (IList) Activator.CreateInstance(genericListType);

					foreach (var jArrayValue in jArray.Values())
					{
						if (TryGetValue(jArrayValue, genericType, out var arrayValue))
						{
							genericList.Add(arrayValue);
						}
					}

					var update2 = new PartialUpdateValue(propertyName, property.PropertyType, genericList);
					response.Updates.Add(propertyName, update2);
					continue;
				}

				if (TryGetValue(jProperty.Value, property.PropertyType, out var value2))
				{
					var update = new PartialUpdateValue(propertyName, property.PropertyType, value2);
					response.Updates.Add(propertyName, update);
				}
			}

			return response;
		}

		internal virtual IDictionary<string, PropertyInfo> GetCachedPropertyDictionary()
		{
			return GetType().GetCachedPropertyDictionary();
		}

		internal void WriteJson(JsonWriter writer, JsonSerializer serializer)
		{
			var expando = GetExpandoObject();
			serializer.Serialize(writer, expando);
		}

		private static T ConvertUpdate<T>(PartialUpdateValue update)
		{
			var desiredType = typeof(T);

			if (update.Type == desiredType)
			{
				return (T) update.Value;
			}

			if (update.Value is string sValue)
			{
				if (StringConverter.TryParse<T>(sValue, out var value))
				{
					return value;
				}
			}

			return (T) Convert.ChangeType(update.Value, desiredType);
		}

		#endregion
	}
}