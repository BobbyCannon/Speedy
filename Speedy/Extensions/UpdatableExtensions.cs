#region References

using System.Linq;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for the IUpdatable interface
/// </summary>
public static class UpdatableExtensions
{
	#region Methods

	/// <summary>
	/// Try to apply an update to the provided value.
	/// </summary>
	/// <typeparam name="T"> The type of the update. </typeparam>
	/// <param name="value"> The value to be updated. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional list of members to exclude during updating. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	public static bool TryUpdateWith<T>(this IUpdatable<T> value, T update, params string[] exclusions)
	{
		return value.ShouldUpdate(update)
			&& value.UpdateWith(update, exclusions);
	}
	
	/// <summary>
	/// Try to apply an update to the provided value.
	/// </summary>
	/// <param name="value"> The value to be updated. </param>
	/// <param name="update"> The update to be applied. </param>
	/// <param name="exclusions"> An optional list of members to exclude during updating. </param>
	/// <returns> True if the update was applied otherwise false. </returns>
	public static bool TryUpdateWith(this IUpdatable value, object update, params string[] exclusions)
	{
		return value.ShouldUpdate(update)
			&& value.UpdateWith(update, exclusions);
	}

	/// <summary>
	/// Allows updating of one type to another based on member Name and Type.
	/// </summary>
	/// <typeparam name="T"> The type to be updated. </typeparam>
	/// <typeparam name="T2"> The source type of the provided update. </typeparam>
	/// <param name="value"> The value to be updated. </param>
	/// <param name="update"> The source of the updates. </param>
	/// <param name="exclusions"> An optional list of members to exclude. </param>
	public static bool UpdateWithUsingReflection<T, T2>(this T value, T2 update, params string[] exclusions)
	{
		if ((value == null) || (update == null))
		{
			return false;
		}

		var destinationType = value.GetRealType();
		var sourceType = update.GetRealType();
		var destinationProperties = destinationType.GetCachedProperties();
		var sourceProperties = sourceType.GetCachedProperties();

		foreach (var thisProperty in destinationProperties)
		{
			// Ensure the source can read this property
			var canRead = thisProperty.CanRead && thisProperty.GetMethod.IsPublic;
			if (!canRead)
			{
				continue;
			}

			// Ensure the destination can write this property
			var canWrite = thisProperty.CanWrite && thisProperty.SetMethod.IsPublic;
			if (!canWrite)
			{
				continue;
			}

			var isPropertyExcluded = exclusions?.Contains(thisProperty.Name) == true;
			if (isPropertyExcluded)
			{
				continue;
			}

			// Check to see if the update source entity has the property
			var updateProperty = sourceProperties.FirstOrDefault(x => (x.Name == thisProperty.Name) && (x.PropertyType == thisProperty.PropertyType));
			if (updateProperty == null)
			{
				// Skip this because target type does not have correct property name and / or type.
				continue;
			}

			var updateValue = updateProperty.GetValue(update);
			var thisValue = thisProperty.GetValue(value);

			if (!Equals(updateValue, thisValue))
			{
				thisProperty.SetValue(value, updateValue);
			}
		}

		return true;
	}

	#endregion
}