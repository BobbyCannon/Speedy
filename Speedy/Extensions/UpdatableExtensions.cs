#region References

using System.Linq;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for the IUpdatable interface
	/// </summary>
	public static class UpdatableExtensions
	{
		#region Methods

		/// <summary>
		/// Allows updating of one type to another based on member Name and Type.
		/// </summary>
		/// <typeparam name="T"> The type to be updated. </typeparam>
		/// <typeparam name="T2"> The source type of the provided update. </typeparam>
		/// <param name="value"> The value to be updated. </param>
		/// <param name="update"> The source of the updates. </param>
		/// <param name="exclusions"> An optional list of members to exclude. </param>
		public static void UpdateWithUsingReflection<T, T2>(this T value, T2 update, params string[] exclusions)
		{
			if (value == null || update == null)
			{
				return;
			}

			var destinationType = value.GetRealType();
			var sourceType = update.GetRealType();
			var destinationProperties = destinationType.GetCachedProperties();
			var sourceProperties = sourceType.GetCachedProperties();

			foreach (var thisProperty in destinationProperties)
			{
				// Ensure the destination can write this property
				var canWrite = thisProperty.CanWrite && thisProperty.SetMethod.IsPublic;
				if (!canWrite)
				{
					continue;
				}

				var isPropertyExcluded = exclusions.Contains(thisProperty.Name);
				if (isPropertyExcluded)
				{
					continue;
				}

				// Check to see if the update source entity has the property
				var updateProperty = sourceProperties.FirstOrDefault(x => x.Name == thisProperty.Name && x.PropertyType == thisProperty.PropertyType);
				if (updateProperty == null)
				{
					// Skip this because target type does not have correct property.
					continue;
				}

				var updateValue = updateProperty.GetValue(update);
				var thisValue = thisProperty.GetValue(value);

				if (!Equals(updateValue, thisValue))
				{
					thisProperty.SetValue(value, updateValue);
				}
			}
		}

		#endregion
	}
}