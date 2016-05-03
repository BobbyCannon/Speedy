#region References

using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#endregion

namespace Speedy.Storage
{
	internal class IgnoreVirtualsSerializeContractResolver : DefaultContractResolver
	{
		#region Fields

		public static readonly IgnoreVirtualsSerializeContractResolver Instance = new IgnoreVirtualsSerializeContractResolver();

		#endregion

		#region Methods

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);
			var propertyInfo = member as PropertyInfo;
			if (propertyInfo == null)
			{
				return null;
			}

			var shouldSerialize = (propertyInfo.CanRead && !propertyInfo.GetAccessors()[0].IsVirtual)
				|| (propertyInfo.CanRead && propertyInfo.GetAccessors()[0].IsVirtual && propertyInfo.GetAccessors()[0].IsFinal);

			property.ShouldSerialize = instance => shouldSerialize;
			return property;
		}

		#endregion
	}
}