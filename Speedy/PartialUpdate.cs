#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Speedy.Converters;
using Speedy.Exceptions;
using Speedy.Extensions;
using Speedy.Serialization;
using Speedy.Serialization.Converters;
using Speedy.Validation;

#endregion

// ReSharper disable IntroduceOptionalParameters.Global

namespace Speedy
{
	/// <summary>
	/// This class contains updates for another object. JSON is deserialized into the
	/// provided type. Meaning, if you create an "AccountUpdate" that inherits
	/// "PartialUpdate[Account]" the updates are for the "Account" itself.
	/// </summary>
	/// <typeparam name="T"> The type of object to be updated. </typeparam>
	[JsonConverter(typeof(PartialUpdateConverter))]
	public class PartialUpdate<T> : PartialUpdate
	{
		#region Fields

		private readonly Validator<PartialUpdate<T>> _validator;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a partial update.
		/// </summary>
		public PartialUpdate() : this(null, null)
		{
		}

		/// <summary>
		/// Instantiates a partial update.
		/// </summary>
		/// <param name="options"> An optional set of options for the update. </param>
		public PartialUpdate(PartialUpdateOptions options) : this(options, null)
		{
		}

		/// <summary>
		/// Instantiates a partial update.
		/// </summary>
		/// <param name="dispatcher"> An optional dispatcher. </param>
		public PartialUpdate(IDispatcher dispatcher) : this(null, dispatcher)
		{
		}

		/// <summary>
		/// Instantiates a partial update.
		/// </summary>
		/// <param name="options"> An optional set of options for the update. </param>
		/// <param name="dispatcher"> An optional dispatcher. </param>
		public PartialUpdate(PartialUpdateOptions options, IDispatcher dispatcher) : base(options, dispatcher)
		{
			_validator = new Validator<PartialUpdate<T>>(dispatcher);
		}

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
			var properties = GetTargetProperties();
			Set(name, value, properties);
		}

		/// <summary>
		/// Set a full set of updates.
		/// </summary>
		/// <param name="value"> The value that contains a full set of updates. </param>
		public void Set(T value)
		{
			var properties = GetTargetProperties();

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
		public sealed override bool TryValidate(out IList<IValidation> failures)
		{
			_validator.TryValidate(this, out failures);
			return failures.Count <= 0;
		}

		/// <summary>
		/// Runs the validator to check the partial update.
		/// </summary>
		public sealed override void Validate()
		{
			_validator.Validate(this);
		}

		/// <summary>
		/// Configure a validation for an update.
		/// </summary>
		public void Validate(Func<T, bool> expression, string message)
		{
			_validator.IsTrue(expression, message);
		}

		/// <summary>
		/// Configure a validation for an update value.
		/// </summary>
		public PropertyValidator<TProperty> Validate<TProperty>(Expression<Func<T, TProperty>> expression)
		{
			return _validator.Property(expression);
		}

		/// <summary>
		/// Gets a list of property information for provided type.
		/// The results are cached so the next query is much faster.
		/// </summary>
		/// <returns> The list of properties for the provided type. </returns>
		protected override IDictionary<string, PropertyInfo> GetTargetProperties()
		{
			return typeof(T).GetCachedPropertyDictionary();
		}

		internal override Validator GetValidator()
		{
			return _validator;
		}

		internal override string ToAssemblyName()
		{
			return typeof(T).ToAssemblyName();
		}

		#endregion
	}

	/// <summary>
	/// This class contains updates for itself. JSON is deserialized into the declaring type.
	/// Meaning, if you create an "AccountUpdate" that inherits from "PartialUpdate" then the
	/// updates are for the "AccountUpdate" itself.
	/// </summary>
	[JsonConverter(typeof(PartialUpdateConverter))]
	public class PartialUpdate : Bindable
	{
		#region Fields

		private readonly Validator<PartialUpdate> _validator;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a partial update.
		/// </summary>
		public PartialUpdate() : this(null, null)
		{
		}

		/// <summary>
		/// Instantiates a partial update.
		/// </summary>
		/// <param name="options"> The options for the partial update. </param>
		public PartialUpdate(PartialUpdateOptions options) : this(options, null)
		{
		}

		/// <summary>
		/// Instantiates a partial update.
		/// </summary>
		/// <param name="dispatcher"> An optional dispatcher. </param>
		public PartialUpdate(IDispatcher dispatcher) : this(null, dispatcher)
		{
		}

		/// <summary>
		/// Instantiates a partial update.
		/// </summary>
		/// <param name="options"> The options for the partial update. </param>
		/// <param name="dispatcher"> An optional dispatcher. </param>
		public PartialUpdate(PartialUpdateOptions options, IDispatcher dispatcher) : base(dispatcher)
		{
			_validator = new Validator<PartialUpdate>();

			Options = options ?? new PartialUpdateOptions();
			Updates = new SortedDictionary<string, PartialUpdateValue>(StringComparer.OrdinalIgnoreCase);
		}

		#endregion

		#region Properties

		/// <summary>
		/// The options for the partial update.
		/// </summary>
		[JsonIgnore]
		public PartialUpdateOptions Options { get; }

		/// <summary>
		/// A list of updates for this partial update.
		/// </summary>
		[JsonIgnore]
		public IDictionary<string, PartialUpdateValue> Updates { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Add an update a
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
			Apply(entity, Options);
		}

		/// <summary>
		/// Applies the updates to the entity.
		/// </summary>
		/// <param name="entity"> Entity to be updated. </param>
		/// <param name="options"> Options for the partial update. </param>
		public void Apply(object entity, PartialUpdateOptions options)
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
				if (options.IncludedProperties.Any() && !options.IncludedProperties.Contains(update.Key))
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
		/// Create an instance of a partial update for a specific type.
		/// </summary>
		/// <param name="genericType"> The type the partial update is for. </param>
		/// <param name="options"> An optional set of options for the update. </param>
		/// <returns> The partial update. </returns>
		public static PartialUpdate CreateGeneric(Type genericType, PartialUpdateOptions options)
		{
			return CreateGeneric(genericType, options, out _);
		}

		/// <summary>
		/// Create an instance of a partial update for a specific type.
		/// </summary>
		/// <param name="genericType"> The type the partial update is for. </param>
		/// <param name="options"> An optional set of options for the update. </param>
		/// <param name="isPartialObject"> True if the generic type is already a partial update. </param>
		/// <returns> The partial update. </returns>
		public static PartialUpdate CreateGeneric(Type genericType, PartialUpdateOptions options, out bool isPartialObject)
		{
			isPartialObject = typeof(PartialUpdate).IsAssignableFrom(genericType);
			var typeToCreate = isPartialObject ? genericType : typeof(PartialUpdate<>).MakeGenericType(genericType);
			var constructor = typeToCreate.GetConstructor(new[] { typeof(PartialUpdateOptions) });
			if (constructor != null)
			{
				return (PartialUpdate) constructor.Invoke(new[] { options });
			}

			constructor = typeToCreate.GetConstructor(Array.Empty<Type>());
			var response = (PartialUpdate) constructor?.Invoke(Array.Empty<object>()) ?? new PartialUpdate();
			return response;
		}

		/// <summary>
		/// Gets a partial update from a JSON string.
		/// </summary>
		/// <param name="json"> The JSON containing the partial update. </param>
		/// <param name="options"> The options for the partial update. </param>
		/// <returns> The partial update. </returns>
		public static PartialUpdate FromJson(string json, PartialUpdateOptions options = null)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				return new PartialUpdate();
			}

			using var reader = new JsonTextReader(new StringReader(json));
			reader.Read();
			return FromJson(reader, options);
		}

		/// <summary>
		/// Gets a partial update from a JSON string.
		/// </summary>
		/// <typeparam name="T"> The type the partial update is for. </typeparam>
		/// <param name="json"> The JSON containing the partial update. </param>
		/// <param name="options"> The options for the partial update. </param>
		/// <returns> The partial update. </returns>
		public static PartialUpdate<T> FromJson<T>(string json, PartialUpdateOptions options = null)
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
		/// <param name="options"> The options for the partial update. </param>
		/// <returns> The partial update. </returns>
		public static PartialUpdate FromJson(string json, Type type, PartialUpdateOptions options = null)
		{
			if (string.IsNullOrWhiteSpace(json))
			{
				var response = CreateGeneric(type, options);
				return response;
			}

			using var reader = new JsonTextReader(new StringReader(json));
			reader.Read();
			return FromJson(reader, type, options);
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
		/// Get the update for the provided name.
		/// </summary>
		/// <param name="name"> The name of the update. </param>
		/// <param name="type"> The type to cast the value to. </param>
		/// <returns> The value if it was found otherwise default(T). </returns>
		public object Get(string name, Type type)
		{
			if (!Updates.ContainsKey(name))
			{
				throw new KeyNotFoundException($"{name} update was not found.");
			}

			var update = Updates[name];
			return ConvertUpdate(update, type);
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
		/// Creates an instance of the type and applies the partial update.
		/// </summary>
		/// <returns> </returns>
		public T GetInstance<T>()
		{
			return (T) GetInstance(typeof(T));
		}

		/// <summary>
		/// Creates an instance of the type and applies the partial update.
		/// </summary>
		/// <returns> </returns>
		public object GetInstance(Type type)
		{
			var response = Activator.CreateInstance(type);
			Apply(response);
			return response;
		}

		/// <summary>
		/// Explicit converter from string to PartialUpdate.
		/// </summary>
		/// <param name="value"> The string value to parse. </param>
		public static explicit operator PartialUpdate(string value)
		{
			if (value.IsQueryString())
			{
				var partialUpdate = new PartialUpdate();
				partialUpdate.ParseQueryString(value);
				return partialUpdate;
			}

			return FromJson(value);
		}

		/// <summary>
		/// Parse the paged request values from the query string.
		/// </summary>
		/// <param name="queryString"> The query string to process. </param>
		/// <remarks>
		/// see https://www.ietf.org/rfc/rfc2396.txt for details on url decoding
		/// </remarks>
		public void ParseQueryString(string queryString)
		{
			var collection = HttpUtility.ParseQueryString(queryString);
			var properties = GetType().GetCachedPropertyDictionary();

			foreach (var key in collection.AllKeys)
			{
				if (properties.ContainsKey(key))
				{
					var property = properties[key];

					if (StringConverter.TryParse(property.PropertyType, collection.Get(key), out var result))
					{
						Updates.AddOrUpdate(property.Name, new PartialUpdateValue(property.Name, property.PropertyType, result));
						continue;
					}
				}

				if (key.EndsWith("[]"))
				{
					var newKey = key.Substring(0, key.Length - 2);
					var newValue = collection.Get(key).Split(',');
					Updates.AddOrUpdate(newKey, new PartialUpdateValue(newKey, typeof(string[]), newValue));
					continue;
				}

				var value = collection.Get(key);
				Updates.AddOrUpdate(key, new PartialUpdateValue(key, typeof(string), value));
			}
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
			if (Options.IncludedProperties.Any()
				&& !Options.IncludedProperties.Contains(propertyName))
			{
				// Ignore this property because we only want to include it
				return false;
			}

			if (Options.ExcludedProperties.Contains(propertyName))
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
		public string ToJson(SerializerSettings settings = null)
		{
			var expando = GetExpandoObject();

			return settings != null
				? expando.ToJson(settings)
				: expando.ToJson();
		}

		/// <summary>
		/// Convert the request to the query string values.
		/// </summary>
		/// <returns> The request in a query string format. </returns>
		/// <remarks>
		/// see https://www.ietf.org/rfc/rfc2396.txt for details on url encoding
		/// </remarks>
		public string ToQueryString()
		{
			var builder = new StringBuilder();

			foreach (var update in Updates)
			{
				// https://www.ietf.org/rfc/rfc2396.txt
				var name = HttpUtility.UrlEncode(update.Key);

				if (update.Value.Value is not string
					&& update.Value.Value is IEnumerable e)
				{
					foreach (var item in e)
					{
						builder.Append($"&{name}[]={HttpUtility.UrlEncode(item.ToString())}");
					}
					continue;
				}

				var value = HttpUtility.UrlEncode(update.Value.Value.ToString());
				builder.Append($"&{name}={value}");
			}

			if ((builder.Length > 0) && (builder[0] == '&'))
			{
				builder.Remove(0, 1);
			}

			return builder.ToString();
		}

		/// <summary>
		/// Get the JSON for the partial update.
		/// </summary>
		/// <returns> The JSON for the partial update. </returns>
		public string ToRawJson()
		{
			var expando = GetExpandoObject();
			return expando.ToRawJson();
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
		/// Get the property value.
		/// </summary>
		/// <param name="type"> The type to cast the value to. </param>
		/// <param name="name"> The name of the update. </param>
		/// <param name="value"> The value that was retrieve or default value if not found. </param>
		/// <returns> True if the update was found otherwise false. </returns>
		public bool TryGet(Type type, string name, out object value)
		{
			if (Updates.ContainsKey(name))
			{
				value = ConvertUpdate(Updates[name], type);
				return true;
			}

			value = default;
			return false;
		}

		/// <summary>
		/// Runs the validator to check the parameter.
		/// </summary>
		public virtual bool TryValidate(out IList<IValidation> failures)
		{
			_validator.TryValidate(this, out failures);
			return failures.Count <= 0;
		}

		/// <summary>
		/// Runs the validator to check the partial update.
		/// </summary>
		public virtual void Validate()
		{
			_validator.Validate(this);
		}

		/// <summary>
		/// Configure a validation for an update.
		/// </summary>
		public void Validate(Func<PartialUpdate, bool> expression, string message)
		{
			_validator.IsTrue<PartialUpdate>(expression, message);
		}

		/// <summary>
		/// Configure a validation for an update value.
		/// </summary>
		public PropertyValidator<TProperty> Validate<T, TProperty>(Expression<Func<T, TProperty>> expression)
		{
			return _validator.Property(expression);
		}

		/// <summary>
		/// Create a dynamic object of the partial update.
		/// </summary>
		/// <returns> The dynamic version of the partial update. </returns>
		protected internal virtual ExpandoObject GetExpandoObject()
		{
			var expando = new ExpandoObject();

			RefreshUpdates();

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
		/// Refresh the update collection for this partial update.
		/// </summary>
		protected internal virtual void RefreshUpdates()
		{
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
		/// Gets a list of property information for this type.
		/// The results are cached so the next query is much faster.
		/// </summary>
		/// <returns> The list of properties for this type. </returns>
		protected virtual IDictionary<string, PropertyInfo> GetTargetProperties()
		{
			return GetType().GetCachedPropertyDictionary();
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

		internal static PartialUpdate FromJson(JsonReader reader, Type objectType, PartialUpdateOptions options)
		{
			if ((reader.TokenType == JsonToken.Null) && (reader.Value == null))
			{
				return null;
			}

			if (objectType == typeof(PartialUpdate))
			{
				return FromJson(reader, options);
			}

			options ??= new PartialUpdateOptions();

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

			var response = CreateGeneric(objectType, options, out var isPartialObject);

			if (reader.TokenType == JsonToken.StartArray)
			{
				return response;
			}

			var jObject = JObject.Load(reader);
			var jProperties = jObject.Properties();

			var propertyDictionary = isPartialObject
				? response.GetType().GetCachedPropertyDictionary()
				: response.GetTargetProperties();

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

				void ProcessForPartialUpdate()
				{
					if (jProperty.Type == JTokenType.Null)
					{
						var nullValue = new PartialUpdateValue(property.Name, property.PropertyType, null);
						response.Updates.Add(property.Name, nullValue);
						return;
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

						var update2 = new PartialUpdateValue(property.Name, property.PropertyType, genericList);
						response.Updates.Add(property.Name, update2);
						return;
					}

					if (TryGetValue(jProperty.Value, property.PropertyType, out var value2))
					{
						var update = new PartialUpdateValue(property.Name, property.PropertyType, value2);
						response.Updates.Add(property.Name, update);
					}
				}

				void ProcessForObject()
				{
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

						property.SetValue(response, genericList);
						return;
					}

					if (TryGetValue(jProperty.Value, property.PropertyType, out var value2)
						&& property.CanWrite)
					{
						property.SetValue(response, value2);
					}
				}

				if (isPartialObject)
				{
					ProcessForObject();
				}
				else
				{
					ProcessForPartialUpdate();
				}
			}

			if (!isPartialObject)
			{
				// Ensure the updates are there
				response.RefreshUpdates();
			}

			return response;
		}

		internal static PartialUpdate FromJson(JsonReader reader, PartialUpdateOptions options)
		{
			options ??= new PartialUpdateOptions();

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

			var response = new PartialUpdate(options);

			if (reader.TokenType == JsonToken.StartArray)
			{
				return response;
			}

			var jObject = JObject.Load(reader);
			var jProperties = jObject.Properties();

			foreach (var property in jProperties)
			{
				if (!response.ShouldProcessProperty(property.Name))
				{
					continue;
				}

				var type = PartialUpdateConverter.ConvertType(property.Value.Type);

				if (property.Type == JTokenType.Null)
				{
					var nullValue = new PartialUpdateValue(property.Name, type, null);
					response.Updates.Add(property.Name, nullValue);
					continue;
				}

				// Property of array must be IEnumerable (ignoring some types like string)
				if (property.Value is JArray jArray && type.IsEnumerable())
				{
					var genericType = type.GenericTypeArguments.FirstOrDefault();
					var genericListType = typeof(List<>).MakeGenericType(genericType);
					var genericList = (IList) Activator.CreateInstance(genericListType);

					foreach (var jArrayValue in jArray.Values())
					{
						if (TryGetValue(jArrayValue, genericType, out var arrayValue))
						{
							genericList.Add(arrayValue);
						}
					}

					var update2 = new PartialUpdateValue(property.Name, type, genericList);
					response.Updates.Add(property.Name, update2);
					continue;
				}

				if (TryGetValue(property.Value, type, out var value2))
				{
					var update = new PartialUpdateValue(property.Name, type, value2);
					response.Updates.Add(property.Name, update);
				}
			}

			return response;
		}

		internal virtual Validator GetValidator()
		{
			return _validator;
		}

		internal virtual string ToAssemblyName()
		{
			return GetType().ToAssemblyName();
		}

		private static object ConvertUpdate(PartialUpdateValue update, Type type)
		{
			if (update.Type == type)
			{
				return update.Value;
			}

			if (update.Value is string sValue)
			{
				if (StringConverter.TryParse(type, sValue, out var value))
				{
					return value;
				}
			}

			if (update.Value is JValue { Type: JTokenType.String, Value: string jsValue })
			{
				if (StringConverter.TryParse(type, jsValue, out var value))
				{
					return value;
				}
			}

			return Convert.ChangeType(update.Value, type);
		}

		private static T ConvertUpdate<T>(PartialUpdateValue update)
		{
			return (T) ConvertUpdate(update, typeof(T));
		}

		#endregion
	}
}