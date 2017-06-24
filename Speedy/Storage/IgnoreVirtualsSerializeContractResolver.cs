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

			var isVirtual = propertyInfo.CanRead && propertyInfo.GetMethod.IsVirtual && propertyInfo.GetMethod.Attributes.HasFlag(MethodAttributes.NewSlot) && !propertyInfo.GetMethod.IsFinal
				|| propertyInfo.CanWrite && propertyInfo.SetMethod.IsVirtual && propertyInfo.SetMethod.Attributes.HasFlag(MethodAttributes.NewSlot) && !propertyInfo.SetMethod.IsFinal;

			property.ShouldSerialize = instance => !isVirtual;
			return property;
		}

		#endregion
	}
}