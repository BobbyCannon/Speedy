#region References

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Speedy.Exceptions;
using static Speedy.PartialUpdateOptions;

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
			Options = options;
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
		public override void Apply(object entity)
		{
			Apply((T) entity);
		}

		/// <summary>
		/// Applies the updates to the entity.
		/// </summary>
		/// <param name="entity"> Entity to be updated. </param>
		public void Apply(T entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException(nameof(entity));
			}

			if (Updates == null)
			{
				return;
			}

			foreach (var update in Updates)
			{
				if ((Options.IncludedProperties != null) && !Options.IncludedProperties.Contains(update.Key, StringComparer.OrdinalIgnoreCase))
				{
					// Ignore this property because we only want to include it
					continue;
				}

				if ((Options.ExcludedProperties != null) && Options.ExcludedProperties.Contains(update.Key, StringComparer.OrdinalIgnoreCase))
				{
					// Ignore this property because we only want to exclude it
					continue;
				}

				try
				{
					update.Value.Update.DynamicInvoke(entity);
				}
				catch (TargetInvocationException tie)
				{
					if (tie.InnerException is NullReferenceException)
					{
						throw new SpeedyException("Failed to apply the patch.", tie.InnerException);
					}

					throw;
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
		public T GetInstance()
		{
			var response = Activator.CreateInstance<T>();
			Apply(response);
			return response;
		}

		/// <summary>
		/// Validates the partial update is good.
		/// </summary>
		public override void Validate()
		{
			if (Options.Validations == null)
			{
				return;
			}

			foreach (var validation in Options.Validations)
			{
				var key = validation.Key;
				var value = validation.Value;
				var uKey = Updates.Keys.FirstOrDefault(x => x.Equals(key, StringComparison.OrdinalIgnoreCase));

				if (uKey == null)
				{
					if (value.Required)
					{
						throw new ValidationException($"Update for {key} is required.");
					}

					// The update is not required so just go to the next validation.
					continue;
				}

				var u = Updates[uKey].TypeValue;
				value.Process(u);
			}
		}

		internal override Type GetGenericType()
		{
			return typeof(T);
		}

		internal override PartialUpdateOptions GetOptions()
		{
			return Options;
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
		public Dictionary<string, PartialUpdateValue> Updates { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Applies the updates to the entity.
		/// </summary>
		/// <param name="entity"> Entity to be updated. </param>
		public abstract void Apply(object entity);

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
		/// <typeparam name="T"> The type the partial update is for. </typeparam>
		/// <param name="reader"> The JSON reader containing the partial update. </param>
		/// <param name="options"> An optional set of options to use during parsing. </param>
		/// <returns> The partial update. </returns>
		public static PartialUpdate<T> FromJson<T>(JsonReader reader, PartialUpdateOptions options = null)
		{
			return (PartialUpdate<T>) FromJson(reader, typeof(T), options);
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
				var partialType = typeof(PartialUpdate<>);
				var typeWithGeneric = partialType.MakeGenericType(type);
				var response = (PartialUpdate) Activator.CreateInstance(typeWithGeneric, options);
				return response;
			}

			var serializer = new JsonSerializer();
			using var reader = new JsonTextReader(new StringReader(json));
			reader.Read();
			return FromJson(reader, type, serializer, options);
		}

		/// <summary>
		/// Gets a partial update from a JSON string.
		/// </summary>
		/// <param name="reader"> The JSON reader containing the partial update. </param>
		/// <param name="type"> The type the partial update is for. </param>
		/// <param name="options"> An optional set of options to use during parsing. </param>
		/// <returns> The partial update. </returns>
		public static PartialUpdate FromJson(JsonReader reader, Type type, PartialUpdateOptions options = null)
		{
			var serializer = new JsonSerializer();
			return FromJson(reader, type, serializer, options);
		}

		/// <summary>
		/// Get the property value.
		/// </summary>
		/// <typeparam name="T"> </typeparam>
		/// <param name="name"> </param>
		/// <returns> </returns>
		public T GetPropertyValue<T>(string name)
		{
			return (T) Updates
				.First(u => u.Key == name)
				.Value
				.Update
				.DynamicInvoke(this);
		}

		/// <summary>
		/// Validate an update.
		/// </summary>
		public abstract void Validate();

		internal static PartialUpdate FromJson(JsonReader reader, Type genericType, JsonSerializer serializer, PartialUpdateOptions options = null)
		{
			var partialType = typeof(PartialUpdate<>);
			var isPartialType = typeof(PartialUpdate).IsAssignableFrom(genericType);
			var typeWithGeneric = isPartialType ? genericType : partialType.MakeGenericType(genericType);
			var response = isPartialType
				? (PartialUpdate) Activator.CreateInstance(typeWithGeneric)
				: (PartialUpdate) Activator.CreateInstance(typeWithGeneric, options ?? Create(genericType));

			if (reader.TokenType == JsonToken.StartArray)
			{
				return response;
			}

			var root = JObject.Load(reader);
			var leafs = GetMembers(root);

			return LoadOptions(response, leafs, serializer);
		}

		internal abstract Type GetGenericType();

		internal abstract PartialUpdateOptions GetOptions();

		private static IList<PartialUpdateValue> GetMembers(JToken token)
		{
			if (!token.HasValues)
			{
				if (token.Type is JTokenType.Object or JTokenType.Array)
				{
					return new List<PartialUpdateValue>();
				}

				return new List<PartialUpdateValue>
				{
					new PartialUpdateValue
					{
						Path = token.Path,
						Value = token.Value<IConvertible>()
					}
				};
			}

			var children = token.Children();
			var retVal = new List<PartialUpdateValue>();

			foreach (var child in children)
			{
				retVal.AddRange(GetMembers(child));
			}

			return retVal.Where(x => x != null).ToList();
		}

		private static PartialUpdate LoadOptions(PartialUpdate partialUpdate, IList<PartialUpdateValue> leafs, JsonSerializer serializer)
		{
			var options = partialUpdate.GetOptions();
			var genericType = partialUpdate.GetGenericType();

			if (options.ExcludedProperties?.Any() == true)
			{
				leafs = leafs.Where(x => !options.ExcludedProperties.Contains(x.Path, StringComparer.OrdinalIgnoreCase)).ToList();
			}

			if (options.IncludedProperties?.Any() == true)
			{
				leafs = leafs.Where(x => options.IncludedProperties.Contains(x.Path, StringComparer.OrdinalIgnoreCase)).ToList();
			}

			var lambdas = leafs
				.Select(x =>
				{
					UpdateMember(x, genericType, serializer);
					return x;
				})
				.Where(x => x.Update != null)
				.ToList();

			partialUpdate.Updates = lambdas.ToDictionary(x => x.Path, x => x);
			return partialUpdate;
		}

		private static void UpdateMember(PartialUpdateValue memberValue, Type type, JsonSerializer serializer)
		{
			if (memberValue == null)
			{
				throw new ArgumentNullException(nameof(memberValue));
			}
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}
			if (serializer == null)
			{
				throw new ArgumentNullException(nameof(serializer));
			}

			var param = Expression.Parameter(type, "x");
			Expression left = param;
			var pathTokens = memberValue.Path.Split('.');
			var currentType = type;

			foreach (var pathToken in pathTokens)
			{
				if (currentType == null)
				{
					break;
				}

				var contract = serializer.ContractResolver.ResolveContract(currentType) as JsonObjectContract;
				if (contract == null)
				{
					throw new InvalidOperationException("Only object types can be partially updated");
				}

				var matchingProperty = contract.Properties.GetClosestMatchProperty(pathToken);
				if (matchingProperty?.UnderlyingName == null)
				{
					continue;
				}

				left = Expression.Property(left, matchingProperty.UnderlyingName);
				currentType = matchingProperty.PropertyType;
			}

			if (currentType == null)
			{
				return;
			}

			try
			{
				memberValue.TypeValue = Convert.ChangeType(memberValue.Value, currentType);
				Expression right = Expression.Constant(memberValue.TypeValue);
				var myExpression = Expression.Assign(left, right);
				memberValue.Update = Expression.Lambda(myExpression, param).Compile();
			}
			catch (Exception)
			{
				// Ignore exception?
			}
		}

		#endregion
	}
}