#region References

using System;
using System.Collections;
using System.Collections.Concurrent;
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
using Speedy.Sync;
using Speedy.Validation;

#endregion

// ReSharper disable IntroduceOptionalParameters.Global

namespace Speedy;

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

	private readonly ConcurrentDictionary<string, Validator<PartialUpdate<T>>> _validators;

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
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public PartialUpdate(IDispatcher dispatcher) : this(null, dispatcher)
	{
	}

	/// <summary>
	/// Instantiates a partial update.
	/// </summary>
	/// <param name="options"> An optional set of options for the update. </param>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public PartialUpdate(PartialUpdateOptions options, IDispatcher dispatcher) : base(options, dispatcher)
	{
		_validators = new ConcurrentDictionary<string, Validator<PartialUpdate<T>>>();
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
	/// Applies the updates to the entity including only properties from the default validation group.
	/// </summary>
	/// <param name="entity"> Entity to be updated. </param>
	public void ApplyValidationGroup(T entity)
	{
		base.ApplyValidationGroup(entity);
	}

	/// <summary>
	/// Applies the updates to the entity including only properties from a validation group.
	/// </summary>
	/// <param name="entity"> Entity to be updated. </param>
	/// <param name="groupName"> The name of the validation group to apply. </param>
	public void ApplyValidationGroup(T entity, string groupName)
	{
		base.ApplyValidationGroup(entity, groupName);
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
		InternalSet(name, value);
	}

	/// <summary>
	/// Set a full set of updates.
	/// </summary>
	/// <param name="value"> The value that contains a full set of updates. </param>
	public void Set(T value)
	{
		var properties = GetTargetReadableWritableProperties();

		foreach (var property in properties)
		{
			InternalSet(property.Name, property.GetValue(value), property);
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
		return GetValidator(DefaultGroupName).TryValidate(this, out failures);
	}

	/// <summary>
	/// Runs the validator to check the parameter.
	/// </summary>
	public sealed override bool TryValidate(string name, out IList<IValidation> failures)
	{
		return GetValidator(name).TryValidate(this, out failures);
	}

	/// <summary>
	/// Runs the validator to check the partial update.
	/// </summary>
	public sealed override void Validate()
	{
		Validate(DefaultGroupName);
	}

	/// <summary>
	/// Runs the validator to check the partial update.
	/// </summary>
	public sealed override void Validate(string groupName)
	{
		GetValidator(groupName).Validate(this);
	}

	/// <summary>
	/// Configure a validation for an update value.
	/// </summary>
	public PropertyValidator<TProperty> ValidateProperty<TProperty>(Expression<Func<T, TProperty>> expression)
	{
		var propertyExpression = (MemberExpression) expression.Body;
		var propertyName = (PropertyInfo) propertyExpression.Member;
		var groupNames = GetGroupNames(propertyName.Name);
		return InternalValidateProperty(expression, groupNames);
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

	/// <summary>
	/// Gets a list of property information for provided type that are readable and writable.
	/// The results are cached so the next query is much faster.
	/// </summary>
	/// <returns> The list of properties for the provided type. </returns>
	protected override IEnumerable<PropertyInfo> GetTargetReadableWritableProperties()
	{
		return GetTargetProperties()
			.Where(x => x.Value.CanRead && x.Value.CanWrite)
			.Select(x => x.Value)
			.ToList();
	}

	internal override Validator GetValidator(string name)
	{
		return _validators.GetOrAdd(name, _ => new Validator<PartialUpdate<T>>(Dispatcher));
	}

	internal override string ToAssemblyName()
	{
		return typeof(T).ToAssemblyName();
	}

	/// <summary>
	/// Configure a validation for an update value.
	/// </summary>
	private PropertyValidator<TProperty> InternalValidateProperty<TProperty>(Expression<Func<T, TProperty>> expression, params string[] groupNames)
	{
		// Always add to the default group
		var propertyValidator = GetValidator(DefaultGroupName).Property(expression);

		foreach (var group in groupNames)
		{
			if (group == DefaultGroupName)
			{
				continue;
			}

			return GetValidator(group).Add(propertyValidator);
		}

		return propertyValidator;
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
	#region Constants

	/// <summary>
	/// The default name for updates if a group name is not provided.
	/// </summary>
	protected const string DefaultGroupName = "1191C53B-2580-4726-A4EE-4EAA9E01483A";

	#endregion

	#region Fields

	private readonly ConcurrentDictionary<string, Validator<PartialUpdate>> _validators;

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
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public PartialUpdate(IDispatcher dispatcher) : this(null, dispatcher)
	{
	}

	/// <summary>
	/// Instantiates a partial update.
	/// </summary>
	/// <param name="options"> The options for the partial update. </param>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	public PartialUpdate(PartialUpdateOptions options, IDispatcher dispatcher) : base(dispatcher)
	{
		_validators = new ConcurrentDictionary<string, Validator<PartialUpdate>>();

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
	/// Add or update the value with the type.
	/// </summary>
	/// <param name="name"> The name of the update to add. </param>
	/// <param name="value"> The value of the update. </param>
	public void AddOrUpdate(string name, object value)
	{
		var properties = GetTargetProperties();
		var property = properties.Values.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
		var type = value == null ? property?.PropertyType ?? typeof(object) : value.GetType();
		AddOrUpdate(name, type, value);
	}

	/// <summary>
	/// Add or update the value with the type.
	/// </summary>
	/// <param name="name"> The name of the update to add. </param>
	/// <param name="type"> The type of the value. </param>
	/// <param name="value"> The value of the update. </param>
	public void AddOrUpdate(string name, Type type, object value)
	{
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
	/// <param name="including"> Properties to be included. </param>
	/// <param name="excluding"> Properties to be excluded. </param>
	public void Apply(object entity, IEnumerable<string> including, IEnumerable<string> excluding)
	{
		Apply(entity, new PartialUpdateOptions(including, excluding));
	}

	/// <summary>
	/// Applies the updates to the entity.
	/// </summary>
	/// <param name="entity"> Entity to be updated. </param>
	/// <param name="options"> Options for the partial update. </param>
	public void Apply(object entity, PartialUpdateOptions options)
	{
		if ((entity == null) || (Updates == null))
		{
			return;
		}

		var updateOptions = (PartialUpdateOptions) options?.ShallowClone()
			?? new PartialUpdateOptions();

		if (entity is ISyncEntity syncEntity)
		{
			var exclusions = syncEntity.GetSyncExclusions(true, true, false);
			updateOptions.ExcludedProperties.AddRange(exclusions);
		}

		var propertyInfos = entity.GetType().GetCachedPropertyDictionary();

		foreach (var update in Updates)
		{
			// Ensure the property should be processed
			if (!updateOptions.ShouldProcessProperty(update.Key))
			{
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

			if ((update.Value == null) && !propertyInfo.PropertyType.IsNullable())
			{
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
	/// Applies the updates to the sync entity.
	/// </summary>
	/// <param name="entity"> Sync entity to be updated. </param>
	/// <param name="excludePropertiesForIncomingSync"> If true excluded properties will not be set during incoming sync. </param>
	/// <param name="excludePropertiesForOutgoingSync"> If true excluded properties will not be set during outgoing sync. </param>
	/// <param name="excludePropertiesForSyncUpdate"> If true excluded properties will not be set during update. </param>
	public void ApplyToSyncEntity(ISyncEntity entity, bool excludePropertiesForIncomingSync, bool excludePropertiesForOutgoingSync, bool excludePropertiesForSyncUpdate)
	{
		Apply(entity, new PartialUpdateOptions(null, entity.GetSyncExclusions(excludePropertiesForIncomingSync, excludePropertiesForOutgoingSync, excludePropertiesForSyncUpdate)));
	}

	/// <summary>
	/// Applies the updates to the entity including only properties from the default validation group.
	/// </summary>
	/// <param name="entity"> Entity to be updated. </param>
	public void ApplyValidationGroup(object entity)
	{
		ApplyValidationGroup(entity, DefaultGroupName);
	}

	/// <summary>
	/// Applies the updates to the entity including only properties from a validation group.
	/// </summary>
	/// <param name="entity"> Entity to be updated. </param>
	/// <param name="groupName"> The name of the validation group to apply. </param>
	public void ApplyValidationGroup(object entity, string groupName)
	{
		Apply(entity, GetValidationGroupOptions(groupName));
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
			return new PartialUpdate(options);
		}

		using var reader = new JsonTextReader(new StringReader(json));
		reader.Read();
		return LoadJson(new PartialUpdate(), reader, options);
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

		return (PartialUpdate<T>) FromJson(typeof(T), json, options);
	}

	/// <summary>
	/// Gets a partial update from a JSON string.
	/// </summary>
	/// <param name="type"> The type the partial update is for. </param>
	/// <param name="json"> The JSON containing the partial update. </param>
	/// <param name="options"> The options for the partial update. </param>
	/// <returns> The partial update. </returns>
	public static PartialUpdate FromJson(Type type, string json, PartialUpdateOptions options = null)
	{
		if (string.IsNullOrWhiteSpace(json))
		{
			return CreatePartialUpdateInstance(type);
		}

		using var reader = new JsonTextReader(new StringReader(json));
		reader.Read();
		return FromJson(type, reader, options);
	}

	/// <summary>
	/// Gets a partial update from a JSON string.
	/// </summary>
	/// <param name="type"> The type the partial update is for. </param>
	/// <param name="reader"> The JSON containing the partial update. </param>
	/// <param name="options"> The options for the partial update. </param>
	/// <returns> The partial update. </returns>
	public static PartialUpdate FromJson(Type type, JsonReader reader, PartialUpdateOptions options = null)
	{
		var update = CreatePartialUpdateInstance(type);
		return LoadJson(update, reader, options);
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
	/// Calculate the group names to for validations.
	/// </summary>
	/// <param name="propertyName"> The name of the property. </param>
	/// <returns> The names of the groups for the property. </returns>
	public virtual string[] GetGroupNames(string propertyName)
	{
		return Array.Empty<string>();
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
		var properties = GetTargetProperties();

		foreach (var key in collection.AllKeys)
		{
			if (key == null)
			{
				continue;
			}

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
		InternalSet(name, value);
	}

	/// <summary>
	/// Set a full set of updates.
	/// </summary>
	/// <param name="value"> The value that contains a full set of updates. </param>
	public void Set(object value)
	{
		var targetProperties = GetTargetProperties();
		var valueProperties = value.GetCachedProperties();

		foreach (var property in targetProperties.Values)
		{
			//
			// Do NOT use property type in this filter, this will be handled by Set
			// 
			var valueProperty = valueProperties
				.FirstOrDefault(x => x.Name == property.Name);

			if (valueProperty == null)
			{
				continue;
			}

			var propertyValue = valueProperty.GetValue(value);
			AddOrUpdate(property.Name, propertyValue);
		}
	}

	/// <summary>
	/// Check to see if a property should be processed.
	/// </summary>
	/// <param name="propertyName"> The name of the property to test. </param>
	/// <returns> True if the property should be processed otherwise false. </returns>
	public bool ShouldProcessProperty(string propertyName)
	{
		return Options.ShouldProcessProperty(propertyName);
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
		try
		{
			if (Updates.ContainsKey(name))
			{
				value = ConvertUpdate<T>(Updates[name]);
				return true;
			}

			value = default;
			return false;
		}
		catch
		{
			value = default;
			return false;
		}
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
		try
		{
			if (Updates.ContainsKey(name))
			{
				value = ConvertUpdate(Updates[name], type);
				return true;
			}

			value = default;
			return false;
		}
		catch
		{
			value = default;
			return false;
		}
	}

	/// <summary>
	/// Runs the validator to check the parameter.
	/// </summary>
	public virtual bool TryValidate(out IList<IValidation> failures)
	{
		return TryValidate(DefaultGroupName, out failures);
	}

	/// <summary>
	/// Runs the validator to check the parameter.
	/// </summary>
	public virtual bool TryValidate(string name, out IList<IValidation> failures)
	{
		GetValidator(name).TryValidate(this, out failures);
		return failures.Count <= 0;
	}

	/// <summary>
	/// Runs the validator to check the partial update.
	/// </summary>
	public virtual void Validate()
	{
		Validate(DefaultGroupName);
	}

	/// <summary>
	/// Runs the validator to check the partial update.
	/// </summary>
	public virtual void Validate(string groupName)
	{
		GetValidator(groupName).Validate(this);
	}

	/// <summary>
	/// Configure a validation for an update value.
	/// </summary>
	public PropertyValidator<TProperty> ValidateProperty<TClass, TProperty>(Expression<Func<TClass, TProperty>> expression)
	{
		var propertyExpression = (MemberExpression) expression.Body;
		var propertyName = (PropertyInfo) propertyExpression.Member;
		var groupNames = GetGroupNames(propertyName.Name);
		return InternalValidateProperty(expression, groupNames);
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
	/// A set of updates have been loaded so refresh object.
	/// </summary>
	protected internal virtual void RefreshObject()
	{
	}

	/// <summary>
	/// Refresh the update collection for this partial update.
	/// </summary>
	protected internal virtual void RefreshUpdates()
	{
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
	/// Gets a list of property information for provided type that are readable and writable.
	/// The results are cached so the next query is much faster.
	/// </summary>
	/// <returns> The list of properties for the provided type. </returns>
	protected virtual IEnumerable<PropertyInfo> GetTargetReadableWritableProperties()
	{
		return GetTargetProperties()
			.Where(x => x.Value.CanRead && x.Value.CanWrite)
			.Select(x => x.Value)
			.ToList();
	}

	internal virtual Validator GetValidator(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			name = DefaultGroupName;
		}

		return _validators.GetOrAdd(name, _ => new Validator<PartialUpdate>());
	}

	/// <summary>
	/// Set a property for the update. The name must be available of the target value.
	/// </summary>
	/// <param name="name"> The name of the member to set. </param>
	/// <param name="value"> The value of the member. </param>
	internal void InternalSet(string name, object value)
	{
		var properties = GetTargetProperties();
		if (!properties.ContainsKey(name))
		{
			throw new SpeedyException("The property does not exist by the provided name.");
		}

		var property = properties[name];
		InternalSet(name, value, property);
	}

	internal void InternalSet(string name, object value, PropertyInfo property)
	{
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

		if (type.IsEnum)
		{
			if (StringConverter.TryParse(type, update.Value?.ToString(), out var value))
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

	private static PartialUpdate CreatePartialUpdateInstance(Type type)
	{
		if (type.IsSubclassOf(PartialUpdateConverter.TypeOfPartialUpdate))
		{
			return (PartialUpdate) Activator.CreateInstance(type);
		}

		return (PartialUpdate) typeof(PartialUpdate<>).CreateInstance(new[] { type });
	}

	private PartialUpdateOptions GetValidationGroupOptions(string groupName)
	{
		var validator = GetValidator(groupName);
		var validationNames = validator.Validations.Select(x => x.Name);
		var memberNames = validator.MemberValidators.Select(x => x.Name);
		var response = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		response.AddRange(validationNames);
		response.AddRange(memberNames);
		return new PartialUpdateOptions(response, null);
	}

	/// <summary>
	/// Configure a validation for an update value.
	/// </summary>
	private PropertyValidator<TProperty> InternalValidateProperty<TClass, TProperty>(Expression<Func<TClass, TProperty>> expression, params string[] groupNames)
	{
		// Always add to the default group
		var propertyValidator = GetValidator(DefaultGroupName).Property(expression);

		foreach (var group in groupNames)
		{
			if (group == DefaultGroupName)
			{
				continue;
			}

			return GetValidator(group).Add(propertyValidator);
		}

		return propertyValidator;
	}

	private static PartialUpdate LoadJson(PartialUpdate partialUpdate, JsonReader reader, PartialUpdateOptions options)
	{
		options ??= new PartialUpdateOptions();

		partialUpdate.Options.UpdateWith(options);

		if (reader.TokenType == JsonToken.StartArray)
		{
			return partialUpdate;
		}

		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}

		var jObject = JObject.Load(reader);
		var jProperties = jObject.Properties();
		var directProperties = partialUpdate.GetType().GetCachedProperties();
		var targetProperties = partialUpdate.GetTargetProperties();

		foreach (var property in jProperties)
		{
			if (!partialUpdate.ShouldProcessProperty(property.Name))
			{
				continue;
			}

			var directWritableProperty = directProperties.FirstOrDefault(x => string.Equals(x.Name, property.Name, StringComparison.OrdinalIgnoreCase) && x.CanWrite);
			var targetProperty = targetProperties.FirstOrDefault(x => string.Equals(x.Key, property.Name, StringComparison.OrdinalIgnoreCase)).Value;
			var type = directWritableProperty?.PropertyType
				?? targetProperty?.PropertyType
				?? PartialUpdateConverter.ConvertType(property.Value.Type);

			if ((property.Type == JTokenType.Null)
				|| (property.Value.Type == JTokenType.Null))
			{
				// Only keep "null" values for nullable property types
				if (type.IsNullable())
				{
					partialUpdate.AddOrUpdate(property.Name, type, null);
				}
				continue;
			}

			if (TryGetValue(property.Value, type, out var readValue))
			{
				var readValueType = readValue?.GetType();
				var readValueDefaultType = readValueType?.GetDefaultValue();

				if ((directWritableProperty != null)
					&& (readValueType != null)
					&& directWritableProperty.PropertyType.IsAssignableFrom(readValueType)
					&& !Equals(readValue, readValueDefaultType))
				{
					directWritableProperty.SetValue(partialUpdate, readValue);
				}
				else
				{
					partialUpdate.AddOrUpdate(property.Name, type, readValue);
				}
			}
		}

		partialUpdate.RefreshUpdates();

		return partialUpdate;
	}

	private static bool TryGetObject(JObject jObject, Type type, out object value)
	{
		var directProperties = type.GetCachedProperties();
		var response = Activator.CreateInstance(type);

		foreach (var jValue in jObject)
		{
			var p = directProperties.FirstOrDefault(x => string.Equals(x.Name, jValue.Key, StringComparison.OrdinalIgnoreCase));
			if (p == null)
			{
				continue;
			}

			if (TryGetValue(jValue.Value, p.PropertyType, out var pValue))
			{
				p.SetValue(response, pValue);
			}
		}

		value = response;
		return true;
	}

	private static bool TryGetValue(JToken token, Type type, out object value)
	{
		// Property of array must be IEnumerable (ignoring some types like string)
		if (token is JArray jArray && type.IsEnumerable())
		{
			var genericType = type.GenericTypeArguments.FirstOrDefault() ?? typeof(object);
			var genericListType = typeof(List<>).MakeGenericType(genericType);
			var genericList = (IList) Activator.CreateInstance(genericListType);

			foreach (var jArrayValue in jArray)
			{
				if (TryGetValue(jArrayValue, genericType, out var arrayValue))
				{
					genericList.Add(arrayValue);
				}
			}

			value = genericList;
			return true;
		}

		if (token is JObject jObject)
		{
			return TryGetObject(jObject, type, out value);
		}

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

	#endregion
}