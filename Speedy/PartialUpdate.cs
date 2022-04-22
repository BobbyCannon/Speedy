#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Speedy.Exceptions;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.Serialization.Converters;

#endregion

namespace Speedy
{
	/// <summary>
	/// This class contains updates for an entity. JSON is deserialized into this type.
	/// </summary>
	/// <typeparam name="T"> The type of entity to be updated. </typeparam>
	[JsonConverter(typeof(PartialUpdateConverter))]
	public class PartialUpdate<T> : PartialUpdate
	{
		#region Constructors

		/// <summary>
		/// Instantiates a partial update.
		/// </summary>
		public PartialUpdate() : this(new PartialUpdateOptions<T>())
		{
		}

		/// <summary>
		/// Instantiates a paged request.
		/// </summary>
		public PartialUpdate(PartialUpdateOptions<T> options)
		{
			Options = options ?? new PartialUpdateOptions<T>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// The options for the partial update.
		/// </summary>
		public PartialUpdateOptions<T> Options { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Applies the updates to the entity.
		/// </summary>
		/// <param name="entity"> Entity to be updated. </param>
		public void Apply(T entity)
		{
			Apply(entity, Options);
		}

		/// <summary>
		/// Applies the updates to the entity.
		/// </summary>
		/// <param name="entity"> Entity to be updated. </param>
		public override void Apply(object entity)
		{
			Apply(entity, Options);
		}

		/// <summary>
		/// Applies the updates to the entity.
		/// </summary>
		/// <param name="entity"> Entity to be updated. </param>
		/// <param name="options"> The optional set of options for apply the updates. </param>
		public override void Apply(object entity, PartialUpdateOptions options)
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
				if (options.IncludedProperties.Any()
					&& !options.IncludedProperties.Contains(update.Key))
				{
					// Ignore this property because we only want to include it
					continue;
				}

				if (options.ExcludedProperties.Contains(update.Key))
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
		/// Gets a partial update from a JSON string.
		/// </summary>
		/// <param name="json"> The JSON containing the partial update. </param>
		/// <param name="options"> An optional set of options to use during parsing. </param>
		/// <returns> The partial update. </returns>
		public static PartialUpdate<T> FromJson(string json, PartialUpdateOptions options = null)
		{
			return (PartialUpdate<T>) FromJson(json, typeof(T), options);
		}

		/// <summary>
		/// Creates an instance of the type and applies the partial update.
		/// </summary>
		/// <returns> </returns>
		public override object GetInstance()
		{
			var response = Activator.CreateInstance<T>();
			Apply(response);
			return response;
		}

		/// <inheritdoc />
		public override void Set(string name, object value)
		{
			var properties = typeof(T).GetCachedPropertyDictionary();
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

			if (property.PropertyType != value.GetType())
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

		/// <summary>
		/// Validates the partial update is good.
		/// </summary>
		public override void Validate()
		{
			Options.Validator?.Validate(GetInstance());
		}

		internal override PartialUpdateOptions GetOptions()
		{
			return Options;
		}

		internal override string ToAssemblyName()
		{
			return typeof(T).ToAssemblyName();
		}

		#endregion
	}

	/// <summary>
	/// This class contains updates for an entity. JSON is deserialized into this type.
	/// </summary>
	public abstract class PartialUpdate
	{
		#region Constructors

		/// <summary>
		/// Instantiate an instance of the partial update.
		/// </summary>
		protected PartialUpdate()
		{
			Updates = new Dictionary<string, PartialUpdateValue>(StringComparer.InvariantCultureIgnoreCase);
		}

		#endregion

		#region Properties

		/// <summary>
		/// A list of updates for this partial update.
		/// </summary>
		public Dictionary<string, PartialUpdateValue> Updates { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Applies the updates to the entity.
		/// </summary>
		/// <param name="entity"> Entity to be updated. </param>
		public abstract void Apply(object entity);

		/// <summary>
		/// Applies the updates to the entity.
		/// </summary>
		/// <param name="entity"> Entity to be updated. </param>
		/// <param name="options"> The options for the apply. </param>
		public abstract void Apply(object entity, PartialUpdateOptions options);

		/// <summary>
		/// Create an instance of a partial update for a specific type.
		/// </summary>
		/// <param name="genericType"> The type the partial update is for. </param>
		/// <param name="options"> An optional set of options to use during parsing. </param>
		/// <returns> The partial update. </returns>
		public static PartialUpdate Create(Type genericType, PartialUpdateOptions options = null)
		{
			var isAlreadyPartialUpdate = typeof(PartialUpdate).IsAssignableFrom(genericType);
			var typeToCreate = isAlreadyPartialUpdate ? genericType : typeof(PartialUpdate<>).MakeGenericType(genericType);
			var response = isAlreadyPartialUpdate
				? (PartialUpdate) Activator.CreateInstance(typeToCreate)
				: (PartialUpdate) Activator.CreateInstance(typeToCreate, options);

			return response;
		}

		/// <summary>
		/// Gets a partial update from a JSON string.
		/// </summary>
		/// <typeparam name="T"> The type the partial update is for. </typeparam>
		/// <param name="json"> The JSON containing the partial update. </param>
		/// <param name="options"> An optional set of options to use during parsing. </param>
		/// <returns> The partial update. </returns>
		public static PartialUpdate<T> FromJson<T>(string json, PartialUpdateOptions<T> options = null)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				return new PartialUpdate<T>();
			}

			return (PartialUpdate<T>) FromJson(json, typeof(T), options);
		}

		/// <summary>
		/// Gets a partial update from a JSON string.
		/// </summary>
		/// <param name="json"> The JSON containing the partial update. </param>
		/// <param name="type"> The type the partial update is for. </param>
		/// <param name="options"> An optional set of options to use during parsing. </param>
		/// <returns> The partial update. </returns>
		public static PartialUpdate FromJson(string json, Type type, PartialUpdateOptions options = null)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				var response = Create(type, options);
				return response;
			}

			var serializer = new JsonSerializer();
			using var reader = new JsonTextReader(new StringReader(json));
			reader.Read();
			return FromJson(reader, serializer, type, options);
		}

		/// <summary>
		/// Get the property value.
		/// </summary>
		/// <param name="name"> The name of the update. </param>
		/// <returns> The value if it was found otherwise null. </returns>
		public object Get(string name)
		{
			var update = Updates.ContainsKey(name) ? Updates[name] : null;
			return update?.Value;
		}

		/// <summary>
		/// Get the property value.
		/// </summary>
		/// <typeparam name="T"> The type to cast the value to. </typeparam>
		/// <param name="name"> The name of the update. </param>
		/// <returns> The value if it was found otherwise default(T). </returns>
		public T Get<T>(string name)
		{
			var update = Updates.ContainsKey(name) ? Updates[name] : null;
			return (T) update?.Value ?? default;
		}

		/// <summary>
		/// Creates an instance of the type and applies the partial update.
		/// </summary>
		/// <returns> </returns>
		public abstract object GetInstance();

		/// <summary>
		/// Set a property for the update.
		/// </summary>
		/// <param name="name"> The name of the member to set. </param>
		/// <param name="value"> The value of the member. </param>
		public abstract void Set(string name, object value);

		/// <summary>
		/// Get the JSON for the partial update.
		/// </summary>
		/// <returns> The JSON for the partial update. </returns>
		public string ToJson(SerializerSettings settings = null)
		{
			var expando = new ExpandoObject();

			foreach (var updates in Updates)
			{
				expando.AddOrUpdate(updates.Key, updates.Value.Value);
			}

			return settings != null
				? expando.ToJson(settings)
				: expando.ToRawJson();
		}

		/// <summary>
		/// Validate an update.
		/// </summary>
		public abstract void Validate();

		internal static PartialUpdate FromJson(JsonReader reader, JsonSerializer serializer, Type objectType, PartialUpdateOptions options = null)
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

			var response = Create(objectType, options);

			if (reader.TokenType == JsonToken.StartArray)
			{
				return response;
			}

			var jObject = JObject.Load(reader);
			var jProperties = jObject.Properties();
			var propertyDictionary = objectType.GetCachedPropertyDictionary();
			var responseOptions = response.GetOptions();

			foreach (var jProperty in jProperties)
			{
				if (responseOptions.IncludedProperties.Any()
					&& !responseOptions.IncludedProperties.Contains(jProperty.Name))
				{
					// Ignore this property because we only want to include it
					continue;
				}

				if (responseOptions.ExcludedProperties.Contains(jProperty.Name))
				{
					// Ignore this property because we only want to exclude it
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

		internal abstract PartialUpdateOptions GetOptions();

		internal abstract string ToAssemblyName();

		#endregion
	}
}